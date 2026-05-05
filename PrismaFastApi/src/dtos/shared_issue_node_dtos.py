import uuid
from pydantic import BaseModel, Field
from typing import Optional, Annotated
from src.dtos.decision_dtos import (
    DecisionOutgoingDto,
)
from src.dtos.uncertainty_dtos import (
    UncertaintyOutgoingDto,
)
from src.dtos.utility_dtos import (
    UtilityOutgoingDto,
)
from src.constants import DatabaseConstants


class IssueDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    project_id: uuid.UUID
    name: Annotated[str, Field(max_length=DatabaseConstants.MAX_SHORT_STRING_LENGTH.value)] = ""
    description: Annotated[
        str, Field(max_length=DatabaseConstants.MAX_LONG_STRING_LENGTH.value)
    ] = ""
    order: int


class IssueViaNodeOutgoingDto(IssueDto):
    type: str
    boundary: str
    decision: Optional[DecisionOutgoingDto]
    uncertainty: Optional[UncertaintyOutgoingDto]
    utility: Optional[UtilityOutgoingDto]
