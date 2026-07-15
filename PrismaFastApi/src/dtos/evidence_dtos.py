from pydantic import BaseModel
from typing import Optional
import uuid

class EvidenceIncomingDto(BaseModel):
    evidence_id: uuid.UUID
    state_ids: list[uuid.UUID]

class EvidenceOutgoingDto(BaseModel):
    evidence_id: uuid.UUID
    state_ids: list[uuid.UUID]
    expected_utility: Optional[float] = None
