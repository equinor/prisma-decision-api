from src.services.solver_service import SolverService
from src.services.structure_service import StructureService
from src.project_lock_manager import ProjectQueueManager

queue_manager = None


async def get_project_lock_manager() -> ProjectQueueManager:
    global queue_manager
    if queue_manager is None:
        queue_manager = ProjectQueueManager()
    return queue_manager


async def get_solver_service() -> SolverService:
    return SolverService()


async def get_structure_service() -> StructureService:
    return StructureService()
