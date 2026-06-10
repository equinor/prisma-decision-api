import uuid
from fastapi import APIRouter, Depends
from src.project_lock_manager import ProjectQueueManager
from src.services.solver_service import SolverService
from src.dependencies import get_solver_service, get_project_lock_manager
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.edge_dtos import EdgeOutgoingDto
from src.dtos.model_solution_dtos import SolutionDto


router = APIRouter(tags=["solvers"])


@router.post("/solvers/project/{project_id}")
async def get_optimal_decisions_for_project_from_dtos(
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    solver_service: SolverService = Depends(get_solver_service),
) -> SolutionDto:
    return await solver_service.find_optimal_decision_pyagrum_from_dtos(issues, edges)

@router.post("/solvers/project/{project_id}/export")
async def export_diagram_as_string(
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    solver_service: SolverService = Depends(get_solver_service),
) -> str:
    return await solver_service.export_diagram_from_dtos(issues, edges)

@router.get("/solvers/project/{project_id}/decision_tree/v2")
async def get_optimal_decisions_for_project_as_tree_tmp(
    project_id: uuid.UUID,
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    solver_service: SolverService = Depends(get_solver_service),
    lock_manager: ProjectQueueManager = Depends(get_project_lock_manager),
):
    async with lock_manager.acquire_project_lock(project_id):
        return await solver_service.get_decision_tree_for_optimal_decisions(
            project_id, issues, edges
        )


@router.post("/solvers/project/{project_id}/decision_tree/v2")
async def get_optimal_decisions_for_project_as_tree_tmp_from_dtos(
    project_id: uuid.UUID,
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    solver_service: SolverService = Depends(get_solver_service),
    lock_manager: ProjectQueueManager = Depends(get_project_lock_manager),
):
    async with lock_manager.acquire_project_lock(project_id):
        return await solver_service.get_decision_tree_for_optimal_decisions_from_dtos(
            project_id, issues, edges
        )
    
@router.post("/solvers/project/{project_id}/partial_decision_tree/v3")
async def get_optimal_decisions_for_project_as_tree_tmp_from_dtos_v3(
    project_id: uuid.UUID,
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    paths: list[list[uuid.UUID]] = [],
    solver_service: SolverService = Depends(get_solver_service),
    lock_manager: ProjectQueueManager = Depends(get_project_lock_manager),
):
    async with lock_manager.acquire_project_lock(project_id):
        return await solver_service.get_decision_tree_for_optimal_decisions_from_dtos_by_constructing_paths(
            project_id, issues, edges, paths,
        )

