import uuid
from pydantic import BaseModel


class MarginTableRowDto(BaseModel):
    uncertainty_id: uuid.UUID
    outcome_id: uuid.UUID
    probability: float
