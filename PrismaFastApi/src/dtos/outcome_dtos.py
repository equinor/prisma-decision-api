import uuid
from typing import Annotated
from pydantic import BaseModel, Field
from src.constants import DatabaseConstants


class OutcomeDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    name: Annotated[str, Field(max_length=DatabaseConstants.MAX_SHORT_STRING_LENGTH.value)] = ""
    uncertainty_id: uuid.UUID
    utility: float = 0.0


class OutcomeIncomingDto(OutcomeDto):
    pass


class OutcomeOutgoingDto(OutcomeDto):
    pass
