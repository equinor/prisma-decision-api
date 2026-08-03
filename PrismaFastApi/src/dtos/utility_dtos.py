import uuid
from pydantic import BaseModel, Field

class UtilityDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    issue_id: uuid.UUID