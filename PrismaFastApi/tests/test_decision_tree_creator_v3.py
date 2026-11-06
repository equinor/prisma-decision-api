import uuid

from src.constants import Type
from src.dtos.edge_dtos import EdgeOutgoingDto
from src.dtos.issue_dtos import IssueOutgoingDto
from src.services.decision_tree.decision_tree_creator_v3 import DecisionTreeCreator_v3


def _issue(issue_id: str, issue_type: Type) -> IssueOutgoingDto:
    return IssueOutgoingDto.model_construct(
        id=uuid.UUID(issue_id),
        type=issue_type.value,
    )


def _edge(tail_issue_id: str, head_issue_id: str) -> EdgeOutgoingDto:
    return EdgeOutgoingDto.model_construct(
        tail_issue_id=uuid.UUID(tail_issue_id),
        head_issue_id=uuid.UUID(head_issue_id),
    )


def test_calculate_partial_order_is_independent_of_input_order() -> None:
    """Test partial order is independent of input order"""
    project_id = uuid.UUID("00000000-0000-0000-0000-000000000001")
    first_decision_id = "00000000-0000-0000-0000-000000000010"
    second_decision_id = "00000000-0000-0000-0000-000000000020"
    first_uncertainty_id = "00000000-0000-0000-0000-000000000030"
    second_uncertainty_id = "00000000-0000-0000-0000-000000000040"
    issues = [
        _issue(first_decision_id, Type.DECISION),
        _issue(second_uncertainty_id, Type.UNCERTAINTY),
        _issue(second_decision_id, Type.DECISION),
        _issue(first_uncertainty_id, Type.UNCERTAINTY),
    ]
    edges = [
        _edge(first_decision_id, first_uncertainty_id),
        _edge(first_uncertainty_id, second_decision_id),
        _edge(first_decision_id, second_uncertainty_id),
    ]

    forward_creator = DecisionTreeCreator_v3.initialize(project_id, issues, edges)
    reversed_creator = DecisionTreeCreator_v3.initialize(
        project_id,
        list(reversed(issues)),
        list(reversed(edges)),
    )

    assert (
        forward_creator.calculate_partial_order_issue_ids()
        == reversed_creator.calculate_partial_order_issue_ids()
    )