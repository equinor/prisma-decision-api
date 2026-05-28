import uuid
from fastapi import APIRouter, Depends
from src.project_lock_manager import ProjectQueueManager
from src.services.solver_service import SolverService
from src.dependencies import get_solver_service, get_project_lock_manager
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.edge_dtos import EdgeOutgoingDto
from src.dtos.model_solution_dtos import SolutionDto
from src.dtos.evidence_dtos import EvidenceIncommingDto, EvidenceOutgoingDto

router = APIRouter(tags=["solvers"])


@router.post("/solvers/project/{project_id}")
async def get_optimal_decisions_for_project_from_dtos(
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    solver_service: SolverService = Depends(get_solver_service),
) -> SolutionDto:
    return await solver_service.find_optimal_decision_pyagrum_from_dtos(issues, edges)

@router.post("/solvers/project/{project_id}/with_evidence")
async def get_optimal_decisions_for_project_with_evidence(
    issues: list[IssueOutgoingDto],
    edges: list[EdgeOutgoingDto],
    evidence: list[EvidenceIncommingDto] = [],
    solver_service: SolverService = Depends(get_solver_service),
) -> list[EvidenceOutgoingDto]:
    evidence_lists = [e.state_ids for e in evidence]
    results: list[SolutionDto] = await solver_service.find_optimal_decision_pyagrum_from_with_evidence(issues, edges, evidence_lists)
    # decision_solutions[0].mean is the expected utility for the first optimal decision, i.e. the root node which represents the expected utility for the model
    return [
        EvidenceOutgoingDto(
            evidence_id=evidence.evidence_id,
            state_ids=evidence.state_ids,
            expected_utility=results[n].decision_solutions[0].mean if results[n].decision_solutions else None,
        )
        for n, evidence in enumerate(evidence)
    ]


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

