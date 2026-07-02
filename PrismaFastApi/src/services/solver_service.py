import uuid
from typing import Optional
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
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.edge_dtos import EdgeOutgoingDto
from src.dtos.model_solution_dtos import SolutionDto
from src.constants import Type

executor = ThreadPoolExecutor()


class SolverService:
    def __init__(
        self,
    ):
        pass

    async def find_optimal_decision_pyagrum(
        self, issues: list[IssueOutgoingDto], edges: list[EdgeOutgoingDto]
    ) -> SolutionDto:

        solution = await PyagrumSolver().find_optimal_decisions(issues=issues, edges=edges)

        return solution

    async def find_optimal_decision_pyagrum_from_dtos(
        self, issues: list[IssueOutgoingDto], edges: list[EdgeOutgoingDto]
    ):
        solution = await PyagrumSolver().find_optimal_decisions(issues=issues, edges=edges)

        return solution
    
    async def find_optimal_decision_pyagrum_from_with_evidence(
        self, issues: list[IssueOutgoingDto], edges: list[EdgeOutgoingDto], evidence: list[list[uuid.UUID]] = []
    ) -> list[SolutionDto]:
        
        solver = PyagrumSolver()
        return await solver.get_solutions_given_evidence(issues=issues, edges=edges, evidence=evidence)
    
    async def get_MEU_given_evidence(
        self, issues: list[IssueOutgoingDto], edges: list[EdgeOutgoingDto], evidence: list[list[uuid.UUID]] = []
    ) -> list[Optional[float]]:
        
        solver = PyagrumSolver()
        return await solver.get_MEU_given_evidence(issues=issues, edges=edges, evidence=evidence)

    async def get_decision_tree_for_optimal_decisions_old(
        self, project_id: uuid.UUID, issues: list[IssueOutgoingDto], edges: list[EdgeOutgoingDto]
    ):

        solution = await PyagrumSolver().find_optimal_decisions(issues=issues, edges=edges)

        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id, nodes=issues, edges=edges
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
        self, project_id: uuid.UUID, issues: list[IssueOutgoingDto], edges: list[EdgeOutgoingDto]
    ):

        solution = await PyagrumSolver().find_optimal_decisions(issues=issues, edges=edges)

        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id, nodes=issues, edges=edges
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
    ):
        solution = await PyagrumSolver().find_optimal_decisions(issues=issues, edges=edges)

        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id, nodes=issues, edges=edges
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
            paths: list[list[uuid.UUID]] = [],
        ):
        if not issues:
            raise ValueError("issues must be provided and non-empty")
        if edges is None:
            edges = []

        solver = PyagrumSolver()
        solution = await solver.find_optimal_decisions(issues=issues, edges=edges)

        paths = self.filter_paths_from_solution(solution, paths, issues)

        decision_tree_creator = DecisionTreeCreator_v3.initialize(project_id, nodes = issues, edges = edges)

        decision_tree = decision_tree_creator.convert_to_decision_tree_partial(project_id=project_id, paths=paths)

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
                (option_id for key, option_id in path_to_options.items()
                 if set(key).issubset(path_str_set)),
                None,
            )
            return str(state_id) == optimal_option_id

        return [
            path for path in paths
            if all(is_valid_state(state_id, path[:i]) for i, state_id in enumerate(path))
        ]
    