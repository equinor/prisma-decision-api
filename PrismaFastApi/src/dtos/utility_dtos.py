import uuid
from pydantic import BaseModel, Field
from src.dtos.discrete_utility_dtos import (
    DiscreteUtilityIncomingDto,
    DiscreteUtilityOutgoingDto,
)


class UtilityDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    issue_id: uuid.UUID


class UtilityIncomingDto(UtilityDto):
    discrete_utilities: list[DiscreteUtilityIncomingDto]


class UtilityOutgoingDto(UtilityDto):
    discrete_utilities: list[DiscreteUtilityOutgoingDto]
