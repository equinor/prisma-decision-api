import uuid
from typing import Annotated
from pydantic import BaseModel, Field
from src.constants import DatabaseConstants


class OptionDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    name: Annotated[str, Field(max_length=DatabaseConstants.MAX_SHORT_STRING_LENGTH.value)] = ""
    decision_id: uuid.UUID
    utility: float = 0.0


class OptionIncomingDto(OptionDto):
    pass


class OptionOutgoingDto(OptionDto):
    pass
