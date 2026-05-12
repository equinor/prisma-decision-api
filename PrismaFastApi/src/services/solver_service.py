import uuid
from src.utils.visit_tree_node_and_populate_with_expected_utility import visit_tree_node_and_populate_with_expected_utility
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
from src.utils.discrete_probability_array_manager import DiscreteProbabilityArrayManager

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
        
    async def get_decision_tree_for_optimal_decisions_from_dtos(
            self, 
            project_id: uuid.UUID, 
            issues: list[IssueOutgoingDto] = [], 
            edges: list[EdgeOutgoingDto] = []
        ):
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

        dt_dtos = decision_tree.to_issue_dtos(backwards_calc_expected_values=False)

        visit_tree_node_and_populate_with_expected_utility(solver, [], dt_dtos)
        return dt_dtos
        
    
    def construct_paths_from_solution(
        self,
        solution: SolutionDto,
        partial_order: list[uuid.UUID],
        issues: list[IssueOutgoingDto],
    ) -> list[list[uuid.UUID]]:
        if not partial_order:
            return []
        
        # decision id to a dictionary of the parent state ids to the optimal option id
        optimal_option_lookup = solution.get_lookup()

        discrete_probability_tables: dict[str, DiscreteProbabilityArrayManager] = {}
        [
            discrete_probability_tables.__setitem__(str(x.id), DiscreteProbabilityArrayManager(x.uncertainty.discrete_probabilities)) 
            for x in issues if x.type == Type.UNCERTAINTY.value
        ]

        # iterate through the partial order and use the lookups to construct the possible paths, 
        # at a decision select the optimal option based on the current path using the optimal_option_lookup
        # at an uncertainty use the discrete_probability_tables to find the probabilities based on the current path and only add to the path the outcomes that are not 0 probability
        paths: list[list[uuid.UUID]] = []

        def add_to_paths(issue_id: uuid.UUID, current_path: list[uuid.UUID]):
            issue = [x for x in issues if x.id == issue_id][0]
            issue_position = partial_order.index(issue_id)
            next_issue_id = partial_order[issue_position + 1] if issue_position + 1 < len(partial_order) else None

            if issue.type == Type.DECISION.value:
                path_to_options = optimal_option_lookup[str(issue.id)]
                for key in path_to_options.keys():
                    if set(key).issubset(set([str(x) for x in current_path])):
                        optimal_option_id = path_to_options[key]
                        current_path = current_path + [uuid.UUID(optimal_option_id)]
                        if next_issue_id is not None:
                            add_to_paths(issue_id=next_issue_id, current_path=current_path)
                        else:
                            paths.append(current_path)
                        break

            elif issue.type == Type.UNCERTAINTY.value:
                # dpt = discrete_probability_tables[str(issue_id)]
                # probabilities: list[float] = dpt.get_probabilities_for_combination([str(x) for x in current_path])
                for outcome in issue.uncertainty.outcomes:
                    new_path = current_path + [outcome.id]
                    if next_issue_id is not None:
                            add_to_paths(issue_id=next_issue_id, current_path=new_path)
                    else:
                        paths.append(new_path)

                # sorted_outcomes = sorted(issue.uncertainty.outcomes, key=lambda x: x.id)
                # for i, prob in enumerate(probabilities):
                #     if prob > 0:
                #         outcome_id = sorted_outcomes[i].id
                #         new_path = current_path + [outcome_id]
                #         if next_issue_id is not None:
                #             add_to_paths(issue_id=next_issue_id, current_path=new_path)
                #         else:
                #             paths.append(new_path)
            

        add_to_paths(issue_id=partial_order[0], current_path=[])
        return paths