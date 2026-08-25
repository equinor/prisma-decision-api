import uuid
import math
from typing import Optional
from src.dtos.decision_tree_dtos import TreeNodeDto2, ProbabilityDto2
from src.services.pyagrum_solver import PyagrumSolver
from src.dtos.model_solution_dtos import SolutionDto
from src.dtos.decision_tree_dtos import UtilityDTDto2
from src.constants import Type
from src.constants import PrecisionConstants


def _populate_uncertainty_probabilities(
    solver: PyagrumSolver,
    node: TreeNodeDto2,
    path_to_node: list[str],
) -> None:
    posterior = solver.get_posterior_given_path(
        issue_id=node.issue_id.__str__(),
        state_ids=path_to_node,
    )
    for key in posterior.keys():
        if math.isnan(posterior[key]):
            posterior[key] = 0
    if node.probabilities is None:
        node.probabilities = []
    if len(node.probabilities) == 0:  # arc reversal required
        for prob in posterior.keys():
            node.probabilities.append(
                ProbabilityDto2(outcome_id=uuid.UUID(prob), probability_value=round(posterior[prob], ndigits=PrecisionConstants.PROBABILITY_PRECISION.value))
            )
        # sort probabilities to match children order
        order_lookup = {str(c.parent_state_id): i for i, c in enumerate(node.children or [])}
        node.probabilities.sort(
            key=lambda prob: order_lookup.get(str(prob.outcome_id), len(order_lookup))
        )
    else:
        for probability in node.probabilities:
            probability.probability_value = round(posterior[str(probability.outcome_id)], ndigits=PrecisionConstants.PROBABILITY_PRECISION.value)


def _populate_root_node_if_needed(
    solver: PyagrumSolver,
    tree_node: TreeNodeDto2,
    current_path: list[str],
) -> None:
    # From the utility dto we can find the outcome/option id
    if tree_node.expected_value is None:  # for handling root
        expected_utility = solver.get_expected_utility_given_path(
            issue_id=tree_node.issue_id.__str__(),
            state_ids=current_path,
        )
        if math.isnan(expected_utility):
            tree_node.expected_value = 0
        if expected_utility < -1e10:
            raise ValueError(f"Impossible state reached due to all possibilities being restricted")
        else:
            tree_node.expected_value = expected_utility

        if tree_node.type == Type.UNCERTAINTY.value:
            _populate_uncertainty_probabilities(solver, tree_node, current_path)


def _prune_to_optimal_child(
    tree_node: TreeNodeDto2,
    current_path: list[str],
    optimal_option_lookup: dict[str, dict[tuple[str, ...], str]],
) -> None:
    if not tree_node.utilities:
        return
    path_str_set = set(current_path)
    path_to_options = optimal_option_lookup.get(str(tree_node.issue_id), {})
    optimal_option_id = next(
        (option_id for key, option_id in path_to_options.items()
         if set(key).issubset(path_str_set)),
        None,
    )

    if optimal_option_id is not None:
        # utilities are always available
        optimal_idx = next(
            (i for i, c in enumerate(tree_node.utilities)
             if str(c.option_id) == optimal_option_id),
            None,
        )
        if optimal_idx is not None:
            # length missmatch can occur when some options are not present due to pruning in earlier nodes, so we check index before pruning
            if tree_node.children and optimal_idx < len(tree_node.children):
                tree_node.children = [tree_node.children[optimal_idx]]
            if tree_node.utilities and optimal_idx < len(tree_node.utilities):
                tree_node.utilities = [tree_node.utilities[optimal_idx]]

def _get_branch_probability(
    tree_node: TreeNodeDto2,
    state_id: str,
) -> float:
    if tree_node.type == Type.UNCERTAINTY.value and tree_node.probabilities:
        matched = next(
            (p for p in tree_node.probabilities if p.outcome_id.__str__() == state_id),
            None,
        )
        if matched is not None:
            return matched.probability_value
    return 1.0


def _visit_child(
    solver: PyagrumSolver,
    tree_node: TreeNodeDto2,
    child: TreeNodeDto2,
    current_path: list[str],
    cumulative_probability: float,
    solution: Optional[SolutionDto],
) -> None:
    state_id = child.parent_state_id
    if state_id is None:
        raise ValueError("State id is None for child node, cannot calculate expected utility")
    next_path = current_path + [state_id.__str__()]

    # handle cumulative probability
    branch_probability = _get_branch_probability(tree_node, state_id.__str__())
    child_cumulative_probability = cumulative_probability * branch_probability

    if child.type == Type.END.value:
        child.cumulative_probability = round(child_cumulative_probability, ndigits=PrecisionConstants.EXPECTED_UTILITY_PRECISION.value)
        return

    if child.type == Type.UNCERTAINTY.value:
        _populate_uncertainty_probabilities(solver, child, next_path)

    expected_utility = solver.get_expected_utility_given_path(
        issue_id=child.issue_id.__str__(),
        state_ids=next_path,
    )
    if math.isnan(expected_utility):
        child.expected_value = 0
    else:
        child.expected_value = round(expected_utility, ndigits=PrecisionConstants.EXPECTED_UTILITY_PRECISION.value)

    visit_tree_node_and_populate(
        solver,
        next_path,
        child,
        cumulative_probability=child_cumulative_probability,
        solution=solution,
    )


def visit_tree_node_and_populate(
    solver: PyagrumSolver,
    current_path: list[str],
    tree_node: TreeNodeDto2,
    cumulative_probability: float = 1.0,
    solution: Optional[SolutionDto] = None,
) -> None:
    optimal_option_lookup: Optional[dict[str, dict[tuple[str, ...], str]]] = None
    if solution is not None:
        optimal_option_lookup = solution.get_lookup()

    _populate_root_node_if_needed(solver, tree_node, current_path)

    if tree_node.type == Type.END.value:
        tree_node.cumulative_probability = cumulative_probability
        return

    state_ids_to_prune: set[str] = set()
    if tree_node.utilities and any([ut.utility_value < -1e10 for ut in tree_node.utilities if ut.utility_value is not None]):
        state_ids_to_prune.update({
            ut.option_id.__str__() if ut.option_id is not None else ut.outcome_id.__str__()
            for ut in tree_node.utilities if ut.utility_value is not None and ut.utility_value < -1e10})

    if tree_node.probabilities and any([p.probability_value == 0 for p in tree_node.probabilities]):
        state_ids_to_prune.update({p.outcome_id.__str__() for p in tree_node.probabilities if p.probability_value == 0})

    prune_tree_node_child(tree_node, state_ids_to_prune)
    if optimal_option_lookup is not None and tree_node.type == Type.DECISION.value:
        _prune_to_optimal_child(tree_node, current_path, optimal_option_lookup)

    if not tree_node.children:
        return

    # another pass to visit children and prune based on expected utility 
    # since the children need to be visited to calculate expected utility before pruning
    state_ids_to_prune = set()
    for child in tree_node.children:
        try:
            _visit_child(solver, tree_node, child, current_path, cumulative_probability, solution)
        except Exception as e:
            raise ValueError(f"Error visiting child node {child.issue_id} with parent state id {child.parent_state_id}: {str(e)}") from e
        if child.expected_value is not None and child.expected_value < -1e10:
            state_ids_to_prune.add(child.parent_state_id.__str__())

    if state_ids_to_prune:
        prune_tree_node_child(tree_node, state_ids_to_prune)
        
def prune_tree_node_child(
    tree_node: TreeNodeDto2,
    states_to_prune: set[str],
) -> None:
    # set utility pruned to true, also set the utility value to None, and remove the child from the children list if it exists
    # remove the probability from the probabilities list if it exists
    if tree_node.utilities:
        for utility in tree_node.utilities:
            state_id = utility.option_id.__str__() if utility.option_id is not None else utility.outcome_id.__str__()
            if state_id in states_to_prune:
                utility.pruned = True
                utility.utility_value = None
    if tree_node.children:
        tree_node.children = [
            child for child in tree_node.children
            if child.parent_state_id.__str__() not in states_to_prune
        ]
    if tree_node.probabilities:
        tree_node.probabilities = [
            prob for prob in tree_node.probabilities
            if prob.outcome_id.__str__() not in states_to_prune
        ]
    