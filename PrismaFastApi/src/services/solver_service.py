import uuid
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
