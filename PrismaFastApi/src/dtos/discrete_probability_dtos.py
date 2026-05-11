import uuid
from typing import List, Optional
from pydantic import BaseModel, Field, field_validator
from src.constants import DtoConstants


class DiscreteProbabilityDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    uncertainty_id: uuid.UUID
    outcome_id: uuid.UUID
    probability: Optional[float] = None
    parent_outcome_ids: List[uuid.UUID] = []
    parent_option_ids: List[uuid.UUID] = []

    @field_validator("probability")
    @classmethod
    def probability_validator(cls, value: Optional[float]) -> Optional[float]:
        """
        Ensures that the probability values fulfill the following conditions:
        1) Probabilities cannot be negative
        2) Probabilities have a decimal precision set by the global constant
        """
        if value is not None:
            return abs(round(value, DtoConstants.DECIMAL_PLACES.value))
        return value


class DiscreteProbabilityIncomingDto(DiscreteProbabilityDto):
    pass


class DiscreteProbabilityOutgoingDto(DiscreteProbabilityDto):
    pass
