import uuid
from src.dtos.decision_tree_dtos import TreeNodeDto2, ProbabilityDto2
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

        if child.type == Type.UNCERTAINTY.value and res.probabilities is not None:
            posterior = solver.get_posterior_given_path(
                issue_id=child.issue_id.__str__(),
                state_ids=temp_path,
            )
            if child.probabilities is None:
                    child.probabilities = []
            if len(child.probabilities) == 0: # arc reversal required
                for prob in posterior.keys():
                    child.probabilities.append(ProbabilityDto2(outcome_id=uuid.UUID(prob), probability_value=posterior[prob]))
                # sort the probabilities so they fit the order of the children
                order_lookup = {str(c.parent_state_id): i for i, c in enumerate(child.children or [])}
                child.probabilities.sort(key=lambda prob: order_lookup.get(str(prob.outcome_id), float("inf")))
            else:
                for probability in child.probabilities:
                    probability.probability_value = posterior[str(probability.outcome_id)]
                
        expected_utility = solver.get_expected_utility_given_path(
            issue_id=child.issue_id.__str__(),
            state_ids=temp_path,
        )
        child.expected_value = expected_utility
        visit_tree_node_and_populate_with_expected_utility(solver, temp_path, child)