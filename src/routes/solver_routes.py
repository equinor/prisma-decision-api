import uuid
from fastapi import APIRouter, Depends, HTTPException
from src.services.solver_service import SolverService
from src.dependencies import get_solver_service
from src.services.user_service import get_current_user
from src.dtos.user_dtos import UserIncomingDto
from src.services.decision_tree_pruning_service import DecisionTreePruningException
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.edge_dtos import EdgeOutgoingDto

router = APIRouter(tags=["solvers"])


@router.get("/solvers/project/{project_id}")
async def get_optimal_decisions_for_project(
    project_id: uuid.UUID,
    solver_service: SolverService = Depends(get_solver_service),
):
    try:
        return await solver_service.find_optimal_decision_pyagrum(project_id)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    
@router.post("/solvers/project/{project_id}")
async def get_optimal_decisions_for_project_from_dtos(
    project_id: uuid.UUID,
    issues: list[IssueOutgoingDto], 
    edges: list[EdgeOutgoingDto],
    solver_service: SolverService = Depends(get_solver_service),
):
    try:
        return await solver_service.find_optimal_decision_pyagrum_from_dtos(issues, edges)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/solvers/project/{project_id}/decision_tree")
async def get_optimal_decisions_for_project_as_tree(
    project_id: uuid.UUID,
    solver_service: SolverService = Depends(get_solver_service),
    current_user: UserIncomingDto = Depends(get_current_user),
):
    try:
        return await solver_service.get_decision_tree_for_optimal_decisions_old(project_id)
    except DecisionTreePruningException as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/solvers/project/{project_id}/decision_tree/v2")
async def get_optimal_decisions_for_project_as_tree_tmp(
    project_id: uuid.UUID,
    solver_service: SolverService = Depends(get_solver_service),
    current_user: UserIncomingDto = Depends(get_current_user),
):
    try:
        return await solver_service.get_decision_tree_for_optimal_decisions(project_id)
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
    current_user: UserIncomingDto = Depends(get_current_user),
):
    try:
        return await solver_service.get_decision_tree_for_optimal_decisions_from_dtos(project_id, issues, edges)
    except DecisionTreePruningException as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
