import uuid
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
            edges: list[EdgeOutgoingDto] | None = None
        ):
        if not issues:
            raise ValueError("issues must be provided and non-empty")
        if edges is None:
            edges = []

        solver = PyagrumSolver()
        solution = await solver.find_optimal_decisions(issues=issues, edges=edges)

        # instead of pruning, construct expanded paths from the solution to make the decision tree.
        # the issue is that the solution only cares about parents
        # can get order of issues from partial order.
        # then need to construct paths from solution

        decision_tree_creator = DecisionTreeCreator_v3.initialize(project_id, nodes = issues, edges = edges)
        DT_partial_order = decision_tree_creator.calculate_partial_order_issue_ids()
        paths = self.construct_paths_from_solution(solution, DT_partial_order, issues)
        decision_tree = decision_tree_creator.convert_to_decision_tree_partial(project_id=issues[0].project_id, paths=paths)

        dt_dtos = decision_tree.to_issue_dtos(backwards_calc=False)

        visit_tree_node_and_populate(solver, [], dt_dtos)
        return dt_dtos
    
    def construct_paths_from_solution(
        self,
        solution: SolutionDto,
        partial_order: list[uuid.UUID],
        issues: list[IssueOutgoingDto],
    ) -> list[list[uuid.UUID]]:
        if not partial_order:
            return []
        optimal_option_lookup = solution.get_lookup()
        issue_by_id = {i.id: i for i in issues}
        order_index = {issue_id: idx for idx, issue_id in enumerate(partial_order)}
        paths: list[list[uuid.UUID]] = []

        stack: list[tuple[uuid.UUID, list[uuid.UUID]]] = [(partial_order[0], [])]

        while stack:
            issue_id, current_path = stack.pop()
            issue = issue_by_id[issue_id]
            issue_position = order_index[issue_id]
            next_issue_id = (
                partial_order[issue_position + 1]
                if issue_position + 1 < len(partial_order)
                else None
            )

            if issue.type == Type.DECISION.value:
                path_to_options = optimal_option_lookup[str(issue.id)]
                path_str_set = set(str(x) for x in current_path)

                for key, optimal_option_id in path_to_options.items():
                    if set(key).issubset(path_str_set):
                        new_path = current_path + [uuid.UUID(optimal_option_id)]
                        if next_issue_id is not None:
                            stack.append((next_issue_id, new_path))
                        else:
                            paths.append(new_path)
                        break

            elif issue.type == Type.UNCERTAINTY.value:
                for outcome in issue.uncertainty.outcomes:
                    new_path = current_path + [outcome.id]
                    if next_issue_id is not None:
                        stack.append((next_issue_id, new_path))
                    else:
                        paths.append(new_path)

        return paths
    