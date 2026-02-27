import uuid
import asyncio
from threading import Lock as ThreadLock

class ProjectQueueManager:
    project_locks: dict[uuid.UUID, asyncio.Lock] = {}
    # use to ensure that several locks cannot be created by simultaneous requests
    _manager_lock: ThreadLock = ThreadLock()

    def add_project_lock(self, project_id: uuid.UUID):
        self.project_locks[project_id] = asyncio.Lock()

    def acquire_project_lock(self, project_id: uuid.UUID) -> asyncio.Lock:
        lock = self.project_locks.get(project_id)
        if lock is None:
            with self._manager_lock:
                self.add_project_lock(project_id)
                lock = self.project_locks.get(project_id)
                if lock is None:
                    raise Exception("Project lock could not be acquired")
        return lock