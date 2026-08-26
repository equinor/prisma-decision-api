import uuid
from typing import Optional
from src.dtos.discrete_probability_dtos import DiscreteProbabilityOutgoingDto
from src.dtos.discrete_utility_dtos import DiscreteUtilityOutgoingDto
from src.services.decision_tree.decision_tree_creator_v3 import DecisionTreeCreator_v3
from src.dtos.decision_tree_dtos import DecisionTreeDto, PartialOrderDto, TreeNodeDto2
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.edge_dtos import EdgeOutgoingDto
from src.services.decision_tree.decision_tree_creator import DecisionTreeCreator
from src.utils.visit_tree_node_and_populate import visit_tree_node_and_populate
from src.services.pyagrum_solver import PyagrumSolver

class StructureService:
    def __init__(self):
        pass

    async def create_decision_tree_from_dtos(
        self,
        project_id: uuid.UUID,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
    ) -> Optional[DecisionTreeDto]:
        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id=project_id, nodes=issues, edges=edges, discrete_probabilities=discrete_probabilities, discrete_utilities=discrete_utilities
        )
        dt = await decision_tree_creator.create_decision_tree()
        return await dt.to_issue_dtos()

    async def create_partial_order_from_dtos(
        self,
        project_id: uuid.UUID,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto],
        discrete_utilities: list[DiscreteUtilityOutgoingDto],
    ) -> Optional[PartialOrderDto]:
        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id=project_id, nodes=issues, edges=edges, discrete_probabilities=discrete_probabilities, discrete_utilities=discrete_utilities
        )
        uuid_list = await decision_tree_creator.calculate_partial_order_issues()
        return PartialOrderDto(issue_ids=uuid_list)

    def create_decision_tree_from_dtos_optimal(
        self,
        project_id: uuid.UUID,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto] = [],
        discrete_utilities: list[DiscreteUtilityOutgoingDto] = [],
    ) -> Optional[TreeNodeDto2]:
        decision_tree_creator = DecisionTreeCreator_v3.initialize(
            project_id=project_id, nodes=issues, edges=edges, discrete_probabilities=discrete_probabilities, discrete_utilities=discrete_utilities
        )
        dt = decision_tree_creator.create_decision_tree()
        return dt.to_issue_dtos()

    async def create_partial_decision_tree_from_dtos_optimal(
        self,
        project_id: uuid.UUID,
        issues: Optional[list[IssueOutgoingDto]] = None,
        edges: Optional[list[EdgeOutgoingDto]] = None,
        discrete_probabilities: list[DiscreteProbabilityOutgoingDto] = [],
        discrete_utilities: list[DiscreteUtilityOutgoingDto] = [],
        paths: Optional[list[list[uuid.UUID]]] = None,
        risk_tolerance: Optional[float] = None,
    ) -> Optional[TreeNodeDto2]:
        if issues is None:
            issues = []
        if edges is None:
            edges = []
        if paths is None:
            paths = []
        decision_tree_creator = DecisionTreeCreator_v3.initialize(
            project_id=project_id, nodes=issues, edges=edges, discrete_probabilities=discrete_probabilities, discrete_utilities=discrete_utilities
        )
        solver = PyagrumSolver(risk_tolerance=risk_tolerance)
        await solver.build_inference_engine(issues=issues, edges=edges, discrete_probabilities=discrete_probabilities, discrete_utilities=discrete_utilities)

        dt = decision_tree_creator.create_decision_tree_partial(paths=paths)
        res: Optional[TreeNodeDto2] = dt.to_issue_dtos(backwards_calc=False)
        if res is None:
            raise ValueError("Failed to create partial decision tree from DTOs")
        
        visit_tree_node_and_populate(solver, [], res)
        return res