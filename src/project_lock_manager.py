import uuid
import asyncio

class ProjectQueueManager:
    project_locks: dict[uuid.UUID, asyncio.Lock] = {}

    def add_project_lock(self, scenario_id: uuid.UUID):
        self.project_locks[scenario_id] = asyncio.Lock()

    def aquire_project_lock(self, project_id: uuid.UUID) -> asyncio.Lock:
        lock = self.project_locks.get(project_id)
        if lock is None:
            self.add_project_lock(project_id)
            lock = self.project_locks.get(project_id)
            if lock is None:
                raise Exception("Scenario lock could not be aquired")
        return lock




