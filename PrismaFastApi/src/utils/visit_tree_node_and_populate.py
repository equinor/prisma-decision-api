import uuid
import math
from typing import Optional
from src.dtos.decision_tree_dtos import TreeNodeDto2, ProbabilityDto2
from src.services.pyagrum_solver import PyagrumSolver
from src.dtos.model_solution_dtos import SolutionDto
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

def visit_tree_node_and_populate(
    solver: PyagrumSolver,
    current_path: list[str],
    res: TreeNodeDto2,
    cumulative_probability: float = 1.0,
    solution: Optional[SolutionDto] = None,
) -> None:
    optimal_option_lookup: Optional[dict[str, dict[tuple[str, ...], str]]] = None
    if solution is not None:
        optimal_option_lookup = solution.get_lookup()

    # From the utility dto we can find the outcome/option id
    if res.expected_value is None:  # for handling root
        expected_utility = solver.get_expected_utility_given_path(
            issue_id=res.issue_id.__str__(),
            state_ids=current_path,
        )
        if math.isnan(expected_utility):
            res.expected_value = 0
        else:
            res.expected_value = expected_utility

        if res.type == Type.UNCERTAINTY.value:
            _populate_uncertainty_probabilities(solver, res, current_path)

    if res.type == Type.END.value:
        res.cumulative_probability = cumulative_probability
        return

    if not res.children:
        return

    # Prune non-optimal children for decision nodes when solution is provided
    if optimal_option_lookup is not None and res.type == Type.DECISION.value:
        path_str_set = set(current_path)
        path_to_options = optimal_option_lookup.get(str(res.issue_id), {})
        optimal_option_id = next(
            (option_id for key, option_id in path_to_options.items()
             if set(key).issubset(path_str_set)),
            None,
        )
        
        if optimal_option_id is not None:
            optimal_idx = next(
                (i for i, c in enumerate(res.children)
                 if str(c.parent_state_id) == optimal_option_id),
                None,
            )
            if optimal_idx is not None:
                res.children = [res.children[optimal_idx]]
                if res.utilities:
                    res.utilities = [res.utilities[optimal_idx]]

    for child in res.children:
        child: TreeNodeDto2
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
            child.cumulative_probability = round(child_cumulative_probability, ndigits=PrecisionConstants.EXPECTED_UTILITY_PRECISION.value)
            continue

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