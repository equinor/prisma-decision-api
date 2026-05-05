from datetime import datetime
from typing import Optional
from src.dtos.decision_dtos import (
    DecisionOutgoingDto,
)
from src.dtos.uncertainty_dtos import (
    UncertaintyOutgoingDto,
)
from src.dtos.utility_dtos import (
    UtilityOutgoingDto,
)
from src.dtos.node_dtos import (
    NodeViaIssueOutgoingDto,
)
from src.dtos.shared_issue_node_dtos import IssueDto


class IssueOutgoingDto(IssueDto):
    type: str
    boundary: str
    node: NodeViaIssueOutgoingDto
    decision: Optional[DecisionOutgoingDto]
    uncertainty: Optional[UncertaintyOutgoingDto]
    utility: Optional[UtilityOutgoingDto]
    created_at: datetime
    updated_at: datetime
