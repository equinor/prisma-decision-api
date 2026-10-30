import uuid
from pydantic import BaseModel
from src.dtos.edge_dtos import EdgeOutgoingDto


class ValueOfPerfectInformationDto(BaseModel):
    uncertainty_issue_id: uuid.UUID
    base_meu: float
    meu_with_information: float
    value_of_information: float
    information_arcs: list[EdgeOutgoingDto]    