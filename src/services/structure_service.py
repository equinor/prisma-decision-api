import uuid
from typing import Optional
from src.services.project_service import ProjectService
from src.dtos.decision_tree_dtos import DecisionTreeDto, PartialOrderDto, DecisionTreeDtoOld
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.edge_dtos import EdgeOutgoingDto
from src.services.decision_tree.decision_tree_creator import DecisionTreeCreator, DecisionTreeGraph
from src.session_manager import sessionmanager


class StructureService:
    def __init__(self, project_service: ProjectService):
        self.project_service = project_service

    async def create_decision_tree(self, project_id: uuid.UUID) -> DecisionTreeGraph:
        issues = []
        edges = []
        async for session in sessionmanager.get_session():
            (
                issues,
                edges,
            ) = await self.project_service.get_influence_diagram_data(session, project_id)
        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id=project_id, nodes=issues, edges=edges
        )
        return await decision_tree_creator.create_decision_tree()

    async def create_decision_tree_dtos(self, project_id: uuid.UUID) -> Optional[DecisionTreeDto]:
        issues = []
        edges = []
        async for session in sessionmanager.get_session():
            (
                issues,
                edges,
            ) = await self.project_service.get_influence_diagram_data(session, project_id)
        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id=project_id, nodes=issues, edges=edges
        )
        dt = await decision_tree_creator.create_decision_tree()
        return await dt.to_issue_dtos()
    
    async def create_decision_tree_from_dtos(self, project_id: uuid.UUID, issues: list[IssueOutgoingDto] = [], edges: list[EdgeOutgoingDto] = []) -> Optional[DecisionTreeDto]:
        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id=project_id, nodes=issues, edges=edges
        )
        dt = await decision_tree_creator.create_decision_tree()
        return await dt.to_issue_dtos()

    async def create_partial_order(self, project_id: uuid.UUID) -> Optional[PartialOrderDto]:
        issues = []
        edges = []
        async for session in sessionmanager.get_session():
            (
                issues,
                edges,
            ) = await self.project_service.get_influence_diagram_data(session, project_id)
        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id=project_id, nodes=issues, edges=edges
        )
        uuid_list = await decision_tree_creator.calculate_partial_order_issues()
        return PartialOrderDto(issue_ids=uuid_list)

    # to be used for backward compatilibility, to be removed
    async def create_decision_tree_dtos_old(self, project_id: uuid.UUID) -> Optional[DecisionTreeDtoOld]:
        issues = []
        edges = []
        async for session in sessionmanager.get_session():
            (
                issues,
                edges,
            ) = await self.project_service.get_influence_diagram_data(session, project_id)
        decision_tree_creator = await DecisionTreeCreator.initialize(
            project_id=project_id, nodes=issues, edges=edges
        )
        dt = await decision_tree_creator.create_decision_tree()
        return await dt.to_issue_dtos_old()
