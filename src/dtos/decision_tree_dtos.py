import uuid
from typing import List, Optional
from pydantic import BaseModel, Field
from src.dtos.issue_dtos import IssueOutgoingDto


class EdgeUUIDDto(BaseModel):
    tail: uuid.UUID
    head: uuid.UUID | None
    name: str = ""


class EndPointNodeDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    project_id: uuid.UUID
    type: str = "EndPoint"
    value: float = 0


class ProbabilityDto(BaseModel):
    outcome_name: str
    outcome_id: uuid.UUID
    probability_value: float
    discrete_probability_id: uuid.UUID


class UtilityDTDto(BaseModel):
    option_name: Optional[str] = None
    option_id: Optional[uuid.UUID] = None
    outcome_name: Optional[str] = None
    outcome_id: Optional[uuid.UUID] = None
    utility_value: float


class TreeNodeDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    issue: IssueOutgoingDto | EndPointNodeDto
    probabilities: Optional[list[ProbabilityDto]] = None
    utilities: Optional[list[UtilityDTDto]] = None
    children: Optional[List["DecisionTreeDto"]] = None


class DecisionTreeDto(BaseModel):
    tree_node: TreeNodeDto

class DecisionTreeDtoOld(BaseModel):
    tree_node: TreeNodeDto
    children: Optional[List["DecisionTreeDtoOld"]] = None

class PartialOrderDto(BaseModel):
    # list of issue ids
    issue_ids: Optional[List[uuid.UUID]] = None
