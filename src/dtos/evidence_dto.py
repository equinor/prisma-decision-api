import uuid
from pydantic import BaseModel
from typing import List, Dict

class IssueStateDto(BaseModel):
    issue_id: uuid.UUID
    state_id: uuid.UUID

class EvidenceDto(BaseModel):
    issue_state_dtos: List[IssueStateDto] = []

    def to_dict_as_uuid(self) -> Dict[uuid.UUID, uuid.UUID]:
        var: Dict[uuid.UUID, uuid.UUID] = {}
        for state in self.issue_state_dtos:
            var[state.issue_id] = state.state_id
        return var
    
    def to_dict_as_str(self) -> Dict[str, str]:
        var: Dict[str, str] = {}
        for state in self.issue_state_dtos:
            var[state.issue_id.__str__()] = state.state_id.__str__()
        return var