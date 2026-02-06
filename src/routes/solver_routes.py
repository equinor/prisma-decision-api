import uuid
from typing import Optional
from fastapi import APIRouter, Depends, HTTPException
from src.services.solver_service import SolverService
from src.dependencies import get_solver_service
from src.services.user_service import get_current_user
from src.dtos.user_dtos import UserIncomingDto
from src.dtos.evidence_dto import EvidenceDto
from src.services.decision_tree_pruning_service import DecisionTreePruningException

router = APIRouter(tags=["solvers"])


@router.get("/solvers/project/{project_id}")
async def get_optimal_decisions_for_project(
    project_id: uuid.UUID,
    solver_service: SolverService = Depends(get_solver_service),
    evidence: Optional[EvidenceDto] = None,
):
    try:
        return await solver_service.find_optimal_decision_pyagrum(project_id, evidence = evidence)
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
    evidence: Optional[EvidenceDto] = None,
):
    try:
        return await solver_service.get_decision_tree_for_optimal_decisions(project_id, evidence=evidence)
    except DecisionTreePruningException as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
