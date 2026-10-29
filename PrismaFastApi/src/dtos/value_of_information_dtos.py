import uuid

from pydantic import BaseModel


class ValueOfPerfectInformationDto(BaseModel):
    uncertainty_issue_id: uuid.UUID
    base_meu: float
    perfect_information_meu: float
    value_of_information: float
