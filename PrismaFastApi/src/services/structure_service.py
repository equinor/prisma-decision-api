import uuid
from typing import Optional
from src.services.decision_tree.decision_tree_creator_v3 import DecisionTreeCreator_v3
from src.dtos.decision_tree_dtos import DecisionTreeDto, PartialOrderDto, TreeNodeDto2
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.edge_dtos import EdgeOutgoingDto
from src.services.decision_tree.decision_tree_creator import DecisionTreeCreator


class StructureService:
    def __init__(self):
        pass

    async def create_decision_tree_from_dtos(
        self,
        project_id: uuid.UUID,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
    ) -> Optional[DecisionTreeDto]:
        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id=project_id, nodes=issues, edges=edges
        )
        dt = await decision_tree_creator.create_decision_tree()
        return await dt.to_issue_dtos()

    async def create_partial_order_from_dtos(
        self,
        project_id: uuid.UUID,
        issues: list[IssueOutgoingDto],
        edges: list[EdgeOutgoingDto],
    ) -> Optional[PartialOrderDto]:
        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id=project_id, nodes=issues, edges=edges
        )
        uuid_list = await decision_tree_creator.calculate_partial_order_issues()
        return PartialOrderDto(issue_ids=uuid_list)

    def create_decision_tree_from_dtos_optimal(
        self,
        project_id: uuid.UUID,
        issues: list[IssueOutgoingDto] = [],
        edges: list[EdgeOutgoingDto] = [],
    ) -> Optional[TreeNodeDto2]:
        decision_tree_creator = DecisionTreeCreator_v3.initialize(
            project_id=project_id, nodes=issues, edges=edges
        )
        dt = decision_tree_creator.create_decision_tree()
        return dt.to_issue_dtos()
