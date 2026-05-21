import uuid
import asyncio
from typing import Optional
from fastapi import APIRouter, Depends
from src.project_lock_manager import ProjectQueueManager
from src.dtos.edge_dtos import EdgeOutgoingDto
from src.dtos.issue_dtos import IssueOutgoingDto
from src.services.structure_service import StructureService
from src.dependencies import (
    get_project_lock_manager,
    get_structure_service,
)
from src.domain.influence_diagram import InfluenceDiagramDOT
from src.dtos.decision_tree_dtos import DecisionTreeDto, PartialOrderDto, TreeNodeDto2


router = APIRouter(tags=["structure"])


@router.post("/structure/{project_id}/influence_diagram")
async def get_influence_diagram_from_dtos(
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
) -> tuple[list[IssueOutgoingDto], list[EdgeOutgoingDto]]:
    influence_diagram = await asyncio.to_thread(lambda: InfluenceDiagramDOT(edges, issues))
    await asyncio.to_thread(influence_diagram.validate_diagram)
    return influence_diagram.issues, influence_diagram.edges


@router.post("/structure/{project_id}/partial_order")
async def get_partial_order_from_dtos(
    project_id: uuid.UUID,
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    structure_service: StructureService = Depends(get_structure_service),
) -> Optional[PartialOrderDto]:
    return await structure_service.create_partial_order_from_dtos(project_id, issues, edges)


@router.post("/structure/{project_id}/decision_tree/v2")
async def build_decision_tree_from_dtos(
    project_id: uuid.UUID,
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    structure_service: StructureService = Depends(get_structure_service),
) -> Optional[DecisionTreeDto]:
    return await structure_service.create_decision_tree_from_dtos(project_id, issues, edges)


@router.post("/structure/{project_id}/decision_tree/v3")
async def build_decision_tree_from_dtos_optimal(
    project_id: uuid.UUID,
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    structure_service: StructureService = Depends(get_structure_service),
    lock_manager: ProjectQueueManager = Depends(get_project_lock_manager),
) -> Optional[TreeNodeDto2]:
    async with lock_manager.acquire_project_lock(project_id):
        return structure_service.create_decision_tree_from_dtos_optimal(
            project_id, issues, edges
        )
        
@router.post("/structure/{project_id}/partial_decision_tree/v3")
async def build_partial_decision_tree_from_dtos_optimal(
    project_id: uuid.UUID, issues: list[IssueOutgoingDto], edges: list[EdgeOutgoingDto],
    paths: list[list[uuid.UUID]],
    structure_service: StructureService = Depends(get_structure_service),
    lock_manager: ProjectQueueManager = Depends(get_project_lock_manager),
) -> Optional[TreeNodeDto2]:
    async with lock_manager.acquire_project_lock(project_id):
        return await structure_service.create_partial_decision_tree_from_dtos_optimal(project_id, issues, edges, paths=paths)
        