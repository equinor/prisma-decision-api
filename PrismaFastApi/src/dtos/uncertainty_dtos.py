import uuid
from pydantic import BaseModel, Field
from typing import List

from src.dtos.outcome_dtos import (
    OutcomeIncomingDto,
    OutcomeOutgoingDto,
)


class UncertaintyDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    issue_id: uuid.UUID
    is_key: bool = True


class UncertaintyIncomingDto(UncertaintyDto):
    outcomes: List[OutcomeIncomingDto] = []


class UncertaintyOutgoingDto(UncertaintyDto):
    outcomes: List[OutcomeOutgoingDto] = []
