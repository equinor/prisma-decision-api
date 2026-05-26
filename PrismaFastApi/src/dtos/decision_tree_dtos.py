import uuid
from typing import List, Optional
from pydantic import BaseModel, Field
from src.constants import Type
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
    cumulative_probability: float = 0


class ProbabilityDto(BaseModel):
    outcome_name: str
    outcome_id: uuid.UUID
    probability_value: float
    discrete_probability_id: uuid.UUID


class ProbabilityDto2(BaseModel):
    outcome_id: uuid.UUID
    probability_value: float


class UtilityDTDto(BaseModel):
    option_name: Optional[str] = None
    option_id: Optional[uuid.UUID] = None
    outcome_name: Optional[str] = None
    outcome_id: Optional[uuid.UUID] = None
    utility_value: float


class UtilityDTDto2(BaseModel):
    option_id: Optional[uuid.UUID] = None
    outcome_id: Optional[uuid.UUID] = None
    utility_value: float


class TreeNodeDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    expected_value: Optional[float] = None
    issue: IssueOutgoingDto | EndPointNodeDto
    probabilities: Optional[list[ProbabilityDto]] = None
    utilities: Optional[list[UtilityDTDto]] = None
    children: Optional[List["DecisionTreeDto"]] = None


class TreeNodeDto2(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    issue_id: uuid.UUID
    parent_state_id: Optional[str] = None
    type: str = Type.UNASSIGNED.value
    expected_value: Optional[float] = None  # only for decision and uncertainty nodes
    endpoint_value: Optional[float] = None  # only for endpoint nodes
    cumulative_probability: Optional[float] = None  # only for endpoint nodes
    probabilities: Optional[list[ProbabilityDto2]] = None
    utilities: Optional[list[UtilityDTDto2]] = None
    children: Optional[List["TreeNodeDto2"]] = None


class DecisionTreeDto(BaseModel):
    tree_node: TreeNodeDto


class PartialOrderDto(BaseModel):
    # list of issue ids
    issue_ids: Optional[List[uuid.UUID]] = None


# 26 jan: tmp for backward comaptibility, to be removed
class DecisionTreeDtoOld(BaseModel):
    tree_node: TreeNodeDto
    children: Optional[List["DecisionTreeDtoOld"]] = None
