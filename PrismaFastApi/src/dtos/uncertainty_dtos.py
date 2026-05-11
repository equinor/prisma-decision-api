import uuid
from pydantic import BaseModel, Field
from typing import List

from src.dtos.outcome_dtos import (
    OutcomeIncomingDto,
    OutcomeOutgoingDto,
)

from src.dtos.discrete_probability_dtos import (
    DiscreteProbabilityIncomingDto,
    DiscreteProbabilityOutgoingDto,
)


class UncertaintyDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    issue_id: uuid.UUID
    is_key: bool = True


class UncertaintyIncomingDto(UncertaintyDto):
    discrete_probabilities: list[DiscreteProbabilityIncomingDto] = []
    outcomes: List[OutcomeIncomingDto] = []


class UncertaintyOutgoingDto(UncertaintyDto):
    discrete_probabilities: list[DiscreteProbabilityOutgoingDto] = []
    outcomes: List[OutcomeOutgoingDto] = []
