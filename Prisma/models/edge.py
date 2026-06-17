import uuid
from pydantic import BaseModel

class Edge(BaseModel):
    tail_node_id: uuid.UUID
    hea_node_id: uuid.UUID