import uuid
from pydantic import BaseModel, Field
from typing import Annotated
from src.constants import DatabaseConstants

from src.dtos.node_style_dtos import (
    NodeStyleOutgoingDto,
)

from src.dtos.shared_issue_node_dtos import IssueViaNodeOutgoingDto


class NodeDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    project_id: uuid.UUID
    issue_id: uuid.UUID
    name: Annotated[str, Field(max_length=DatabaseConstants.MAX_SHORT_STRING_LENGTH.value)] = ""


class NodeOutgoingDto(NodeDto):
    issue: IssueViaNodeOutgoingDto
    node_style: NodeStyleOutgoingDto


class NodeViaIssueOutgoingDto(NodeDto):
    node_style: NodeStyleOutgoingDto
