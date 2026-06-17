import uuid
from pydantic import BaseModel

class Node(BaseModel):
    id: uuid.UUID

class DecisionNode(Node):
    pass

class Utility(BaseModel):
    parents: list[uuid.UUID]
    utility: float

class UtilityNode(Node):
    utilties: list[Utility]

class Probability(BaseModel):
    parents: list[uuid.UUID]

class UncertaintyNode(Node):
    probabilities: list[Probability]