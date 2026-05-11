from src.dtos.decision_tree_dtos import TreeNodeDto2
from src.services.pyagrum_solver import PyagrumSolver
from src.constants import Type

def visit_tree_node_and_populate_with_expected_utility(
    solver: PyagrumSolver,
    current_path: list[str],
    res: TreeNodeDto2,
) -> None:
    # From the utility dto we can find the outcome/option id
    if res.expected_value is None: # for handling root
        expected_utility = solver.get_expected_utility_given_path(
            issue_id=res.issue_id.__str__(),
            state_ids=current_path,
        )
        res.expected_value = expected_utility

    if res.type == Type.END.value or not res.children:
        return

    for child in res.children:
        if child.type == Type.END.value:
            continue
        # util = [x for x in res.utilities if str(x.option_id) or str(x.outcome_id) == child.parent_state_id][0]
        child: TreeNodeDto2
        # util: UtilityDTDto2
        # state_id = util.outcome_id if util.outcome_id is not None else util.option_id
        state_id = child.parent_state_id
        if state_id is None:
            raise ValueError("State id is None for child node, cannot calculate expected utility")
        temp_path = current_path + [state_id.__str__()]
        try:
            expected_utility = solver.get_expected_utility_given_path(
            issue_id=child.issue_id.__str__(),
            state_ids=temp_path,
        )
        except Exception as e:
            raise e
        child.expected_value = expected_utility
        visit_tree_node_and_populate_with_expected_utility(solver, temp_path, child)