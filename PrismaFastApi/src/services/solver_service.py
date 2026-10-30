import asyncio
import uuid
import numpy as np
from typing import Optional
from src.dtos.margin_table_dtos import MarginTableRowDto
from src.config import config
from src.utils.visit_tree_node_and_populate import visit_tree_node_and_populate
from src.services.decision_tree.decision_tree_creator_v3 import DecisionTreeCreator_v3
from concurrent.futures import ThreadPoolExecutor
from src.services.pyagrum_solver import PyagrumSolver
from src.services.decision_tree.decision_tree_creator import DecisionTreeCreator
from src.services.decision_tree_pruning_service import (
    DecisionTreePruningService,
    OptimalDecisionTreePruner,
)
from src.services.decision_tree_pruning_service import (
    DecisionTreePruningServiceOld,
    OptimalDecisionTreePrunerOld,
)
from src.dtos.policy_table_dtos import PolicyTableRowDto
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.edge_dtos import EdgeOutgoingDto, build_information_arc
from src.dtos.discrete_probability_dtos import DiscreteProbabilityOutgoingDto
from src.dtos.discrete_utility_dtos import DiscreteUtilityOutgoingDto
from src.dtos.model_solution_dtos import SolutionDto
from src.dtos.value_of_information_dtos import ValueOfPerfectInformationDto
from src.constants import Type

executor = ThreadPoolExecutor()


class SolverService:
    def __init__(
        self,
    ):
        pass

    async def get_margins_pyagrum(
        self,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
    ) -> dict[str, list[MarginTableRowDto]]:
        solver = PyagrumSolver()
        await solver.build_inference_engine(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )
        res = solver.get_margins()  
        return res

    async def export_pyagrum_model(
        self,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
    ) -> dict[str, object]:
        solver = PyagrumSolver()
        solver.build_influence_diagram(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )
        return await asyncio.to_thread(solver.export_pyagrum_model)

    async def find_optimal_decision_pyagrum(
        self,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
    ) -> SolutionDto:

        solution = await PyagrumSolver().find_optimal_decisions(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )

        return solution

    async def find_optimal_decision_pyagrum_from_dtos(
        self,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
    ):
        solution = await PyagrumSolver().find_optimal_decisions(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )

        return solution

    async def get_value_of_perfect_information(
        self,
        uncertainty_issue_ids: list[uuid.UUID],
        evidence: list[uuid.UUID],
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
    ) -> list[ValueOfPerfectInformationDto]:
        uncertainty_ids = {
            issue.id
            for issue in issues
            if issue.type == Type.UNCERTAINTY.value
        }
        existing_arcs = {
            (edge.tail_issue_id, edge.head_issue_id)
            for edge in edges
        }
        valid_information_arcs = self._get_valid_information_arcs(
            uncertainty_issue_ids, uncertainty_ids, issues, edges, existing_arcs
        )

        base_meu = await PyagrumSolver().get_mean_expected_utility(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
            evidence=evidence,
        )
        results: list[ValueOfPerfectInformationDto] = []
        for uncertainty_issue_id, information_arcs in valid_information_arcs:
            perfect_information_meu = (
                await PyagrumSolver().get_mean_expected_utility(
                    issues=issues,
                    edges=edges,
                    discrete_probabilities=discrete_probabilities,
                    discrete_utilities=discrete_utilities,
                    additional_arcs=information_arcs,
                    evidence=evidence,
                )
            )
            # TODO: make a decision on handling non-finite MEU values
            if (
                not np.isfinite(perfect_information_meu) or 
                not np.isfinite(base_meu)
            ):
                continue
            results.append(
                ValueOfPerfectInformationDto(
                    uncertainty_issue_id=uncertainty_issue_id,
                    base_meu=base_meu,
                    meu_with_information=perfect_information_meu,
                    value_of_information=max(
                        0.0,
                        perfect_information_meu - base_meu,
                    ),
                    information_arcs=[build_information_arc(issues, x[0], x[1]) for x in information_arcs]
                )
            )
        return results

    def _get_valid_information_arcs(
        self,
        uncertainty_issue_ids: list[uuid.UUID],
        uncertainty_ids: set[uuid.UUID],
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        existing_arcs: set[tuple[uuid.UUID, uuid.UUID]],
    ) -> list[tuple[uuid.UUID, list[tuple[uuid.UUID, uuid.UUID]]]]:
        """
        Get valid info arcs for each uncertainty
        Returns list of valid info arcs for each uncertainty issue
        Each item is (uncertainty_issue_id, list of arcs)
        """
        valid_information_arcs: list[
            tuple[uuid.UUID, list[tuple[uuid.UUID, uuid.UUID]]]
        ] = []
        for uncertainty_issue_id in uncertainty_issue_ids:
            if uncertainty_issue_id not in uncertainty_ids:
                continue
            valid_decision_ids = self._get_valid_decision_ids_for_perfect_information(
                uncertainty_issue_id=uncertainty_issue_id,
                issues=issues,
                edges=edges,
            )
            if not valid_decision_ids:
                continue

            information_arcs = [
                (uncertainty_issue_id, decision_id)
                for decision_id in valid_decision_ids
                if (uncertainty_issue_id, decision_id) not in existing_arcs
            ]
            valid_information_arcs.append(
                (uncertainty_issue_id, information_arcs)
            )
        if not valid_information_arcs:
           return []
        return valid_information_arcs

    def _get_valid_decision_ids_for_perfect_information(
        self,
        uncertainty_issue_id: uuid.UUID,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
    ) -> list[uuid.UUID]:
        """
        Get valid decision ids that can't reach the uncertainty issue.
        This means that we can add information arcs from the uncertainty to the decisions.
        Args:
            uncertainty_issue_id: UUID of the uncertainty
            issues: list of issues
            edges: list of edges
        """
        descendants_by_issue_id: dict[uuid.UUID, set[uuid.UUID]] = {}
        for edge in edges:
            descendants_by_issue_id.setdefault(edge.tail_issue_id, set()).add(
                edge.head_issue_id
            )
        return [
            issue.id
            for issue in issues
            if issue.type == Type.DECISION.value
            and not self._can_reach_uncertainty(
                issue.id,
                uncertainty_issue_id,
                descendants_by_issue_id,
            )
        ]

    def _can_reach_uncertainty(
        self,
        issue_id: uuid.UUID,
        uncertainty_issue_id: uuid.UUID,
        descendants_by_issue_id: dict[uuid.UUID, set[uuid.UUID]],
    ) -> bool:
        """
        Check if issue_id can reach uncertainty_issue_id in the graph.
        Uses DFS with max depth to avoid cycles.
        Returns True if reachable, False otherwise.
        Args:
            issue_id: UUID
                the start issue
            uncertainty_issue_id: UUID
                the issue to check reachability for
            descendants_by_issue_id: dict
                Key: issue id
                Value: set of descendant ids
        """
        pending = list(descendants_by_issue_id.get(issue_id, set()))
        visited: set[uuid.UUID] = set()
        while pending:
            descendant_id = pending.pop()
            if descendant_id == uncertainty_issue_id:
                return True
            if descendant_id in visited:
                continue
            visited.add(descendant_id)
            pending.extend(descendants_by_issue_id.get(descendant_id, set()))
        return False

    async def find_optimal_decision_pyagrum_from_with_evidence(
        self,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
        evidence: list[list[uuid.UUID]] = [],
    ) -> list[SolutionDto]:

        solver = PyagrumSolver()
        return await solver.get_solutions_given_evidence(
            issues=issues,
            edges=edges,
            evidence=evidence,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )

    async def get_MEU_given_evidence(
        self,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
        evidence: list[list[uuid.UUID]] = [],
    ) -> list[Optional[float]]:

        solver = PyagrumSolver()
        result = await solver.get_mean_expected_utilities_given_evidence(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
            evidence=evidence,
        )
        # for debugging
        if config.SAVE_INFLUENCE_DIAGRAM:
            solver.export_pyagrum_model()
        return result

    async def get_decision_tree_for_optimal_decisions_old(
        self,
        project_id: uuid.UUID,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
    ):

        solution = await PyagrumSolver().find_optimal_decisions(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )

        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id,
            nodes=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )
        DT_partial_order = await decision_tree_creator.calculate_partial_order()
        decision_tree = await decision_tree_creator.convert_to_decision_tree(
            project_id=issues[0].project_id, partial_order=DT_partial_order
        )

        dt_dtos = await decision_tree.to_issue_dtos_old()
        if dt_dtos is None:
            raise ValueError("Failed to generate decision tree")

        pruning_service = DecisionTreePruningServiceOld(pruner=OptimalDecisionTreePrunerOld())
        return pruning_service.prune_tree_for_optimal_decisions(dt_dtos, solution)

    async def get_decision_tree_for_optimal_decisions(
        self,
        project_id: uuid.UUID,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
    ):

        solution = await PyagrumSolver().find_optimal_decisions(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )

        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id,
            nodes=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )
        DT_partial_order = await decision_tree_creator.calculate_partial_order()
        decision_tree = await decision_tree_creator.convert_to_decision_tree(
            project_id=issues[0].project_id, partial_order=DT_partial_order
        )

        dt_dtos = await decision_tree.to_issue_dtos()
        if dt_dtos is None:
            raise ValueError("Failed to generate decision tree")

        pruning_service = DecisionTreePruningService(pruner=OptimalDecisionTreePruner())
        return pruning_service.prune_tree_for_optimal_decisions(dt_dtos, solution)

    async def get_decision_tree_for_optimal_decisions_from_dtos(
        self,
        project_id: uuid.UUID,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
    ):
        solution = await PyagrumSolver().find_optimal_decisions(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )

        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id,
            nodes=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )
        DT_partial_order = await decision_tree_creator.calculate_partial_order()
        decision_tree = await decision_tree_creator.convert_to_decision_tree(
            project_id=issues[0].project_id, partial_order=DT_partial_order
        )

        dt_dtos = await decision_tree.to_issue_dtos()
        if dt_dtos is None:
            raise ValueError("Failed to generate decision tree")

        pruning_service = DecisionTreePruningService(pruner=OptimalDecisionTreePruner())
        return pruning_service.prune_tree_for_optimal_decisions(dt_dtos, solution)

    async def get_decision_tree_for_optimal_decisions_from_dtos_by_constructing_paths(
        self,
        project_id: uuid.UUID,
        issues: list[IssueOutgoingDto] | None = None,
        edges: list[EdgeOutgoingDto] | None = None,
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto] = [],
        discrete_utilities: list[DiscreteUtilityOutgoingDto] = [],
        paths: list[list[uuid.UUID]] = [],
    ):
        if not issues:
            raise ValueError("issues must be provided and non-empty")
        if edges is None:
            edges = []

        solver = PyagrumSolver()
        solution = await solver.find_optimal_decisions(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )

        paths = self.filter_paths_from_solution(solution, paths, issues)

        decision_tree_creator = DecisionTreeCreator_v3.initialize(
            project_id,
            nodes=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )

        decision_tree = decision_tree_creator.convert_to_decision_tree_partial(
            project_id=project_id, paths=paths
        )

        dt_dtos = decision_tree.to_issue_dtos(backwards_calc=False)

        visit_tree_node_and_populate(solver, [], dt_dtos, solution=solution)
        return dt_dtos

    def filter_paths_from_solution(
        self,
        solution: SolutionDto,
        paths: list[list[uuid.UUID]],
        issues: list[IssueOutgoingDto],
    ) -> list[list[uuid.UUID]]:
        optimal_option_lookup = solution.get_lookup()

        decision_state_to_issue: dict[uuid.UUID, IssueOutgoingDto] = {
            option.id: issue
            for issue in issues
            if issue.type == Type.DECISION.value and issue.decision
            for option in issue.decision.options
        }
        uncertainty_state_ids: set[uuid.UUID] = {
            outcome.id
            for issue in issues
            if issue.type == Type.UNCERTAINTY.value and issue.uncertainty
            for outcome in issue.uncertainty.outcomes
        }

        def is_valid_state(state_id: uuid.UUID, current_path: list[uuid.UUID]) -> bool:
            if state_id in uncertainty_state_ids:
                return True
            issue = decision_state_to_issue.get(state_id)
            if issue is None:
                return False
            path_str_set = set(str(x) for x in current_path)
            path_to_options = optimal_option_lookup.get(str(issue.id), {})
            optimal_option_id = next(
                (
                    option_id
                    for key, option_id in path_to_options.items()
                    if set(key).issubset(path_str_set)
                ),
                None,
            )
            return str(state_id) == optimal_option_id

        return [
            path
            for path in paths
            if all(is_valid_state(state_id, path[:i]) for i, state_id in enumerate(path))
        ]

    async def get_policy_table(
        self,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
        evidence: Optional[list[uuid.UUID]] = None,
    ) -> list[PolicyTableRowDto]:
        solver = PyagrumSolver()
        ie = await solver.build_inference_engine(
            issues=issues,
            edges=edges,
            discrete_probabilities=discrete_probabilities,
            discrete_utilities=discrete_utilities,
        )

        if evidence:
            solver.set_evidence(ie, [str(x) for x in evidence])

        decision_ids = [str(issue.id) for issue in issues if issue.type == Type.DECISION.value]
        return [row for decision_id in decision_ids for row in solver.get_policy_table(decision_id)]
