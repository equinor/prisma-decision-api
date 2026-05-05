import uuid
from pydantic import BaseModel, Field, field_validator

from src.constants import DtoConstants


class NodeStyleDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    node_id: uuid.UUID
    x_position: float = 0.0
    y_position: float = 0.0

    @field_validator("x_position")
    @classmethod
    def x_position_validator(cls, value: float) -> float:
        return round(value, DtoConstants.DECIMAL_PLACES.value)

    @field_validator("y_position")
    @classmethod
    def y_position_validator(cls, value: float) -> float:
        return round(value, DtoConstants.DECIMAL_PLACES.value)


class NodeStyleIncomingDto(NodeStyleDto):
    pass


class NodeStyleOutgoingDto(NodeStyleDto):
    pass
