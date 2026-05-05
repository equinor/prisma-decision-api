import uuid
from pydantic import BaseModel, Field
from typing import List
from src.dtos.option_dtos import (
    OptionIncomingDto,
    OptionOutgoingDto,
)
from src.constants import DecisionHierarchy


class DecisionDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    issue_id: uuid.UUID


class DecisionIncomingDto(DecisionDto):
    options: List[OptionIncomingDto]
    type: DecisionHierarchy = DecisionHierarchy.FOCUS


class DecisionOutgoingDto(DecisionDto):
    options: List[OptionOutgoingDto]
    type: str
