import uuid
from typing import Optional
from pydantic import BaseModel, Field
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.node_dtos import NodeOutgoingDto


class EdgeDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    tail_id: uuid.UUID
    head_id: uuid.UUID
    project_id: uuid.UUID


class EdgeIncomingDto(EdgeDto):
    pass


class EdgeOutgoingDto(EdgeDto):
    head_node: Optional[NodeOutgoingDto]
    tail_node: Optional[NodeOutgoingDto]
    head_issue_id: uuid.UUID
    tail_issue_id: uuid.UUID

def build_information_arc(issues: list[IssueOutgoingDto], tail_issue_id: uuid.UUID, head_issue_id: uuid.UUID) -> EdgeOutgoingDto:
    tail_issue = next(i for i in issues if i.id == tail_issue_id)
    head_issue = next(i for i in issues if i.id == head_issue_id)
    return EdgeOutgoingDto(
        project_id=tail_issue.project_id,
        tail_issue_id=tail_issue.id,
        head_issue_id=head_issue.id,
        tail_id=tail_issue.get_node_id(),
        head_id=head_issue.get_node_id(),
        head_node=None,
        tail_node=None,
    )