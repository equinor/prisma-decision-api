import uuid
from pydantic import BaseModel, Field
from src.dtos.node_dtos import NodeOutgoingDto


class EdgeDto(BaseModel):
    id: uuid.UUID = Field(default_factory=uuid.uuid4)
    tail_id: uuid.UUID
    head_id: uuid.UUID
    project_id: uuid.UUID


class EdgeIncomingDto(EdgeDto):
    pass


class EdgeOutgoingDto(EdgeDto):
    head_node: NodeOutgoingDto
    tail_node: NodeOutgoingDto
    head_issue_id: uuid.UUID
    tail_issue_id: uuid.UUID
