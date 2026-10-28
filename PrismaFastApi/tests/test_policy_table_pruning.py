import uuid

from src.dtos.policy_table_dtos import PolicyTableRowDto
from src.dtos.restriction_table_dtos import RestrictionEntryOutgoingDto
from src.services.solver_service import SolverService


def test_prune_policy_table_recalculates_determined_policies() -> None:
    first_decision_id = uuid.uuid4()
    second_decision_id = uuid.uuid4()
    third_decision_id = uuid.uuid4()
    first_option_id = uuid.uuid4()
    second_option_id = uuid.uuid4()
    third_option_id = uuid.uuid4()
    alternative_option_id = uuid.uuid4()
    first_impossible_outcome_id = uuid.uuid4()
    second_impossible_outcome_id = uuid.uuid4()

    policy_rows = [
        PolicyTableRowDto(decision_id=first_decision_id, parent_state_ids=[], option_id=first_option_id, value=1),
        PolicyTableRowDto(decision_id=second_decision_id, parent_state_ids=[], option_id=second_option_id, value=1),
        PolicyTableRowDto(decision_id=second_decision_id, parent_state_ids=[first_impossible_outcome_id], option_id=alternative_option_id, value=1),
        PolicyTableRowDto(decision_id=third_decision_id, parent_state_ids=[], option_id=third_option_id, value=1),
        PolicyTableRowDto(decision_id=third_decision_id, parent_state_ids=[second_impossible_outcome_id], option_id=alternative_option_id, value=1),
    ]
    restriction_entries = [
        _restriction(first_option_id, first_impossible_outcome_id),
        _restriction(second_option_id, second_impossible_outcome_id),
    ]

    result = SolverService().prune_policy_table(policy_rows, restriction_entries)

    assert result == [policy_rows[0], policy_rows[1], policy_rows[3]]


def _restriction(
    parent_option_id: uuid.UUID, child_outcome_id: uuid.UUID
) -> RestrictionEntryOutgoingDto:
    return RestrictionEntryOutgoingDto(
        id=uuid.uuid4(),
        project_id=uuid.uuid4(),
        restriction_value=0,
        parent_state_id=parent_option_id,
        is_parent_uncertainty=False,
        child_state_id=child_outcome_id,
        is_child_uncertainty=True,
        restriction_table_id=uuid.uuid4(),
    )