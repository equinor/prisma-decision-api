import uuid
from pydantic import BaseModel


class PolicyTableRowDto(BaseModel):
    decision_id: uuid.UUID
    parent_state_ids: list[uuid.UUID]
    option_id: uuid.UUID
    value: int
