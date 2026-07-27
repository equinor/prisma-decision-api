from datetime import datetime
from typing import Optional
from src.dtos.decision_dtos import (
    DecisionOutgoingDto,
)
from src.dtos.discrete_probability_dtos import (
    DiscreteProbabilityOutgoingDto,
)
from src.dtos.outcome_dtos import (
    UtilityDto,
)
from src.dtos.uncertainty_dtos import (
    UncertaintyOutgoingDto,
)
from src.dtos.utility_dtos import (
    UtilityDto
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
    utility: Optional[UtilityDto]
    created_at: datetime
    updated_at: datetime
