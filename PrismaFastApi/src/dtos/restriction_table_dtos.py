import uuid
from typing import Optional

from pydantic import BaseModel


class RestrictionEntryOutgoingDto(BaseModel):
    id: uuid.UUID
    project_id: uuid.UUID
    restriction_value: float
    parent_state_id: Optional[uuid.UUID] = None
    is_parent_uncertainty: bool
    child_state_id: Optional[uuid.UUID] = None
    is_child_uncertainty: bool
    restriction_table_id: uuid.UUID


class RestrictionTableOutgoingDto(BaseModel):
    id: uuid.UUID
    project_id: uuid.UUID
    edge_id: uuid.UUID
    name: str
    restriction_entries: list[RestrictionEntryOutgoingDto]