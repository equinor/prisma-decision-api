from src.dtos.utility_dtos import UtilityOutgoingDto
from src.dtos.issue_dtos import IssueOutgoingDto
from src.utils.combine_nodes import DataPoint, State, combine_nodes
from src.constants import Type, ComputationalNames

class UtilityNodeMerger:
    """
    A class to help merge utility nodes for a given set of issues.
    """
    def __init__(self, issues: list[IssueOutgoingDto]):
        self.issues = issues

    def find_issue_id_from_state_id(self, state_id: str) -> str:
        for issue in self.issues:
            if issue.type == Type.UNCERTAINTY.value and issue.uncertainty is not None:
                # look in outcomes
                for outcome in issue.uncertainty.outcomes:
                    if str(outcome.id) == state_id:
                        return str(issue.id)
            if issue.type == Type.DECISION.value and issue.decision is not None:
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
            if (issue.type == Type.UNCERTAINTY.value 
                and issue.uncertainty is not None 
            ):
                # do not filter out outcomes with utility 0, as this effects pyagrums MEU calculation
                data_points.extend(
                    DataPoint(
                        value=outcome.utility,
                        states=[
                            State(id=str(outcome.id), parent_id=str(issue.id))
                        ]
                    ) for outcome in issue.uncertainty.outcomes
                )
            if (issue.type == Type.DECISION.value
                and issue.decision is not None 
            ):
                # do not filter out options with utility 0, as this effects pyagrums MEU calculation
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
        df = data_array.to_dataframe(name=ComputationalNames.UTILITY_NODE_VALUE.value).reset_index()
        # return issue ids so we know what nodes to connect to the master utility node
        issue_ids = df.columns[df.columns != ComputationalNames.UTILITY_NODE_VALUE.value]
        
        # Create a dictionary to hold the assignments
        assignments: list[dict[str, float|str]] = [row.to_dict() for _, row in df.iterrows()]

        return issue_ids, assignments
