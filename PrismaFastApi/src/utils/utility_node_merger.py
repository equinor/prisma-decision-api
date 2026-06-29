import pandas as pd
import uuid
from src.dtos.utility_dtos import UtilityOutgoingDto
from src.dtos.utility_dtos import DiscreteUtilityOutgoingDto
from src.dtos.issue_dtos import IssueOutgoingDto
from src.utils.combine_nodes import DataPoint, State, combine_nodes

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
                    
    def convert_utility_dtos_to_data_points(self, utility_dtos: list[UtilityOutgoingDto]) -> list[list[DataPoint]]:
        data_tables: list[list[DataPoint]] = []
        for utility in utility_dtos:
            data_points: list[DataPoint] = []
            for discrete_utility in utility.discrete_utilities:
                states: list[State] = []
                for outcome_id in discrete_utility.parent_outcome_ids:
                    issue_id = self.find_issue_id_from_state_id(str(outcome_id))
                    states.append(State(id=str(outcome_id), parent_id=issue_id))
                for option_id in discrete_utility.parent_option_ids:
                    issue_id = self.find_issue_id_from_state_id(str(option_id))
                    states.append(State(id=str(option_id), parent_id=issue_id))
                data_points.append(DataPoint(value=discrete_utility.utility_value, states=states))
            if data_points:
                data_tables.append(data_points)
        return data_tables
    
    def convert_virtual_utilities_to_data_points(self) -> list[list[DataPoint]]:
        # all uncertainties and decisions have outcomes/options that contain utilities. these should be converted to utility dtos for the utility node merger
        data_tables: list[list[DataPoint]] = []
        for issue in self.issues:
            data_points: list[DataPoint] = []
            if (issue.type == "Uncertainty" 
                and issue.uncertainty is not None 
                # and not all(outcome.utility == 0 for outcome in issue.uncertainty.outcomes)
            ):
                data_points.extend(
                    DataPoint(
                        value=outcome.utility,
                        states=[
                            State(id=str(outcome.id), parent_id=str(issue.id))
                        ]
                    ) for outcome in issue.uncertainty.outcomes
                )
            if (issue.type == "Decision" 
                and issue.decision is not None 
                # and not all(option.utility == 0 for option in issue.decision.options)
            ):
                data_points.extend(
                    DataPoint(
                        value=option.utility,
                        states=[
                            State(id=str(option.id), parent_id=str(issue.id))
                        ]
                    ) for option in issue.decision.options
                )
            if data_points:
                data_tables.append(data_points)
        return data_tables
    
    def combine_and_return_assignments(self, data_tables: list[list[DataPoint]]) -> tuple[list[str], list[dict[str, float|str]]]:
        # Convert DataPoints to xarray DataArray
        data_array = combine_nodes(data_tables)
        
        # Convert the DataArray to a pandas DataFrame
        df = data_array.to_dataframe(name='value').reset_index()
        # return issue ids so we know what nodes to connect to the master utility node
        issue_ids = df.columns[df.columns != 'value']
        
        # Create a dictionary to hold the assignments
        assignments: list[dict[str, float|str]] = []

        return issue_ids, [row.to_dict() for _, row in df.iterrows()]
        
        # assignments are the input for pyarum solver to fill the cpt

        for _, row in df.iterrows():
            # Create a key based on the parent states
            key = tuple((str(row[dim]) for dim in df.columns if dim != 'value'))
            assignments[key] = row['value']
        return issue_ids, assignments
    
                    
    # def convert_virtual_utilities_to_data_points(self):
    #     # all uncertainties and decisions have outcomes/options that contain utilities. these should be converted to utility dtos for the utility node merger
    #     utility_dtos: list[UtilityOutgoingDto] = []
    #     for issue in self.issues:
    #         if (issue.type == "Uncertainty" 
    #             and issue.uncertainty is not None 
    #             and not all(outcome.utility == 0 for outcome in issue.uncertainty.outcomes)
    #         ):
    #             utility_id = uuid.uuid4()
    #             utility_dto = UtilityOutgoingDto(
    #                 id=utility_id,
    #                 issue_id=issue.id,
    #                 discrete_utilities=[
    #                     DiscreteUtilityOutgoingDto(
    #                         id=uuid.uuid4(),
    #                         utility_id=utility_id,
    #                         value_metric_id=uuid.uuid4(),
    #                         utility_value=outcome.utility,
    #                         parent_outcome_ids=[outcome.id],
    #                     ) for outcome in issue.uncertainty.outcomes
    #                 ]
    #             )
    #             utility_dtos.append(utility_dto)
    #         if (issue.type == "Decision" 
    #             and issue.decision is not None 
    #             and not all(option.utility == 0 for option in issue.decision.options)
    #         ):
    #             utility_id = uuid.uuid4()
    #             utility_dto = UtilityOutgoingDto(
    #                 id=utility_id,
    #                 issue_id=issue.id,
    #                 discrete_utilities=[
    #                     DiscreteUtilityOutgoingDto(
    #                         id=uuid.uuid4(),
    #                         utility_id=utility_id,
    #                         value_metric_id=uuid.uuid4(),
    #                         utility_value=option.utility,
    #                         parent_option_ids=[option.id],
    #                     ) for option in issue.decision.options
    #                 ]
    #             )
    #             utility_dtos.append(utility_dto)
    #     return utility_dtos
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
            left = result.dropna(axis=1, how='all')
            right = df.dropna(axis=1, how='all')
            shared = left.columns.intersection(right.columns).difference(['value'])
            common_cols = list(shared[left[shared].notna().any() & right[shared].notna().any()])
            
            if common_cols:
                merged = left.merge(right, on=common_cols)
            else:
                merged = left.merge(right, how='cross')  # no shared keys → cartesian product
            
            result = (merged
                    .assign(value=lambda x: x['value_x'] + x['value_y'])
                    .drop(columns=['value_x', 'value_y']))
        return result