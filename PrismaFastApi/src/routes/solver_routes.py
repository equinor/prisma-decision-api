import uuid
from fastapi import APIRouter, Depends, HTTPException
from src.project_lock_manager import ProjectQueueManager
from src.services.solver_service import SolverService
from src.dependencies import get_solver_service, get_project_lock_manager
from src.services.decision_tree_pruning_service import DecisionTreePruningException
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
    try:
        return await solver_service.find_optimal_decision_pyagrum_from_dtos(issues, edges)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.get("/solvers/project/{project_id}/decision_tree/v2")
async def get_optimal_decisions_for_project_as_tree_tmp(
    project_id: uuid.UUID,
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    solver_service: SolverService = Depends(get_solver_service),
    lock_manager: ProjectQueueManager = Depends(get_project_lock_manager),
):
    try:
        async with lock_manager.acquire_project_lock(project_id):
            return await solver_service.get_decision_tree_for_optimal_decisions(
                project_id, issues, edges
            )
    except DecisionTreePruningException as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.post("/solvers/project/{project_id}/decision_tree/v2")
async def get_optimal_decisions_for_project_as_tree_tmp_from_dtos(
    project_id: uuid.UUID,
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    solver_service: SolverService = Depends(get_solver_service),
    lock_manager: ProjectQueueManager = Depends(get_project_lock_manager),
):
    try:
        async with lock_manager.acquire_project_lock(project_id):
            return await solver_service.get_decision_tree_for_optimal_decisions_from_dtos(
                project_id, issues, edges
            )
    except DecisionTreePruningException as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    
@router.post("/solvers/project/{project_id}/decision_tree/v3")
async def get_optimal_decisions_for_project_as_tree_tmp_from_dtos_v3(
    project_id: uuid.UUID,
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    solver_service: SolverService = Depends(get_solver_service),
    lock_manager: ProjectQueueManager = Depends(get_project_lock_manager),
):
    try:
        async with lock_manager.acquire_project_lock(project_id):
            return await solver_service.get_decision_tree_for_optimal_decisions_from_dtos_by_constructing_paths(
                project_id, issues, edges
            )
    except DecisionTreePruningException as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

