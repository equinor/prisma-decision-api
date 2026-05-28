from pydantic import BaseModel
import uuid

class EvidenceIncommingDto(BaseModel):
    evidence_id: uuid.UUID
    state_ids: list[uuid.UUID]

class EvidenceOutgoingDto(BaseModel):
    evidence_id: uuid.UUID
    state_ids: list[uuid.UUID]
    expected_utility: float = 0
