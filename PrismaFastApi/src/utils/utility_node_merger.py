import pandas as pd
import uuid
from src.dtos.utility_dtos import UtilityOutgoingDto
from src.dtos.utility_dtos import DiscreteUtilityOutgoingDto
from src.dtos.issue_dtos import IssueOutgoingDto

class UtilityDataFrameConstructor:
    def __init__(self, issues: list[IssueOutgoingDto]):
        self.issues = issues

    def find_issue_id_from_state_id(self, state_id: str) -> str:
        for issue in self.issues:
            if issue.type == "Uncertainty" and issue.uncertainty is not None:
                # look in outcomes
                for outcome in issue.uncertainty.outcomes:
                    if str(outcome.id) == state_id:
                        return str(issue.id)
            if issue.type == "Decision" and issue.decision is not None:
                # look in options
                for option in issue.decision.options:
                    if str(option.id) == state_id:
                        return str(issue.id)
                    
    def convert_virtual_utilities_to_utility_dtos(self):
        # all uncertainties and decisions have outcomes/options that contain utilities. these should be converted to utility dtos for the utility node merger
        utility_dtos: list[UtilityOutgoingDto] = []
        for issue in self.issues:
            if (issue.type == "Uncertainty" 
                and issue.uncertainty is not None 
                and not all(outcome.utility == 0 for outcome in issue.uncertainty.outcomes)
            ):
                for outcome in issue.uncertainty.outcomes:
                    utility_dto = UtilityOutgoingDto(
                        id=uuid.uuid4(),
                        issue_id=issue.id,
                        discrete_utilities=[
                            DiscreteUtilityOutgoingDto(
                                
                                id=uuid.uuid4(),
                                utility_id=uuid.uuid4(),
                                value_metric_id=uuid.uuid4(),
                                utility_value=outcome.utility,
                                parent_outcome_ids=[outcome.id],
                            )
                        ]
                    )
                    utility_dtos.append(utility_dto)
            if (issue.type == "Decision" 
                and issue.decision is not None 
                and not all(option.utility == 0 for option in issue.decision.options)
            ):
                for option in issue.decision.options:
                    utility_dto = UtilityOutgoingDto(
                        id=uuid.uuid4(),
                        issue_id=issue.id,
                        discrete_utilities=[
                            DiscreteUtilityOutgoingDto(
                                id=uuid.uuid4(),
                                utility_id=uuid.uuid4(),
                                value_metric_id=uuid.uuid4(),
                                utility_value=option.utility,
                                parent_option_ids=[option.id],
                            )
                        ]
                    )
                    utility_dtos.append(utility_dto)
        return utility_dtos
    def construct_utility(self, utility: UtilityOutgoingDto) -> pd.DataFrame:
        rows = []
        for util in utility.discrete_utilities:
            row = {}
            for state_id in util.parent_outcome_ids + util.parent_option_ids:
                issue_id = self.find_issue_id_from_state_id(str(state_id))
                if issue_id is not None:
                    row[issue_id] = state_id
            row["value"] = util.utility_value
            rows.append(row)
        return pd.DataFrame(rows, columns=self.issue_columns + ["value"])

    @property
    def issue_columns(self):
        return [
            str(issue.id) for issue in self.issues
            if (issue.type == "Decision" and issue.decision is not None)
            or (issue.type == "Uncertainty" and issue.uncertainty is not None)
        ]
    
    def combine_and_sum(self, dataframes: list[pd.DataFrame]) -> pd.DataFrame:
        if not dataframes:
            return pd.DataFrame()
        
        result = dataframes[0]
        for df in dataframes[1:]:
            common_cols = list(result.columns[(result.notna().any() & df.notna().any())].drop('value'))
            result = (result.dropna(axis=1, how='all')
                      .merge(df.dropna(axis=1, how='all'), on=common_cols)
                      .assign(value=lambda x: x['value_x'] + x['value_y'])
                      .drop(columns=['value_x', 'value_y']))
        return result