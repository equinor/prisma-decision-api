import uuid
from src.dtos.decision_tree_dtos import TreeNodeDto2, ProbabilityDto2
from src.services.pyagrum_solver import PyagrumSolver
from src.constants import Type


def _populate_uncertainty_probabilities(
    solver: PyagrumSolver,
    node: TreeNodeDto2,
    path_to_node: list[str],
) -> None:
    posterior = solver.get_posterior_given_path(
        issue_id=node.issue_id.__str__(),
        state_ids=path_to_node,
    )
    if node.probabilities is None:
        node.probabilities = []
    if len(node.probabilities) == 0:  # arc reversal required
        for prob in posterior.keys():
            node.probabilities.append(
                ProbabilityDto2(outcome_id=uuid.UUID(prob), probability_value=posterior[prob])
            )
        # sort probabilities to match children order
        order_lookup = {str(c.parent_state_id): i for i, c in enumerate(node.children or [])}
        node.probabilities.sort(
            key=lambda prob: order_lookup.get(str(prob.outcome_id), len(order_lookup))
        )
    else:
        for probability in node.probabilities:
            probability.probability_value = posterior[str(probability.outcome_id)]

def visit_tree_node_and_populate(
    solver: PyagrumSolver,
    current_path: list[str],
    res: TreeNodeDto2,
    cumulative_probability: float = 1.0,
) -> None:
    # From the utility dto we can find the outcome/option id
    if res.expected_value is None:  # for handling root
        expected_utility = solver.get_expected_utility_given_path(
            issue_id=res.issue_id.__str__(),
            state_ids=current_path,
        )
        res.expected_value = expected_utility

    if res.type == Type.UNCERTAINTY.value:
        _populate_uncertainty_probabilities(solver, res, current_path)

    if res.type == Type.END.value:
        res.cumulative_probability = cumulative_probability
        return

    if not res.children:
        return

    for child in res.children:
        # util = [x for x in res.utilities if str(x.option_id) or str(x.outcome_id) == child.parent_state_id][0]
        child: TreeNodeDto2
        # util: UtilityDTDto2
        # state_id = util.outcome_id if util.outcome_id is not None else util.option_id
        state_id = child.parent_state_id
        if state_id is None:
            raise ValueError("State id is None for child node, cannot calculate expected utility")
        next_path = current_path + [state_id.__str__()]

        # handle cumulative probability
        branch_probability = 1.0
        if res.type == Type.UNCERTAINTY.value and res.probabilities:
            matched = next(
                (p for p in res.probabilities if p.outcome_id.__str__() == state_id.__str__()),
                None,
            )
            if matched is not None:
                branch_probability = matched.probability_value
        child_cumulative_probability = cumulative_probability * branch_probability

        if child.type == Type.END.value:
            child.cumulative_probability = child_cumulative_probability
            continue

        if child.type == Type.UNCERTAINTY.value:
            _populate_uncertainty_probabilities(solver, child, next_path)

        expected_utility = solver.get_expected_utility_given_path(
            issue_id=child.issue_id.__str__(),
            state_ids=next_path,
        )
        child.expected_value = expected_utility
        visit_tree_node_and_populate(
            solver,
            next_path,
            child,
            cumulative_probability=child_cumulative_probability,
        )