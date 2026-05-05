import uuid
from typing import List, Optional
from pydantic import BaseModel, Field
from src.constants import default_value_metric_id


class DiscreteUtilityDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    utility_id: uuid.UUID
    value_metric_id: uuid.UUID = default_value_metric_id
    utility_value: Optional[float] = None
    parent_outcome_ids: List[uuid.UUID] = []
    parent_option_ids: List[uuid.UUID] = []


class DiscreteUtilityIncomingDto(DiscreteUtilityDto):
    pass


class DiscreteUtilityOutgoingDto(DiscreteUtilityDto):
    pass
