import uuid

import pytest

from src.dtos.discrete_utility_dtos import DiscreteUtilityOutgoingDto
from src.utils.discrete_utility_array_manager import DiscreteUtilityArrayManager


def create_utility(
    utility_id: uuid.UUID,
    value_metric_id: uuid.UUID,
    utility_value: float,
    parent_outcome_ids: list[uuid.UUID],
    parent_option_ids: list[uuid.UUID],
) -> DiscreteUtilityOutgoingDto:
    return DiscreteUtilityOutgoingDto(
        utility_id=utility_id,
        value_metric_id=value_metric_id,
        utility_value=utility_value,
        parent_outcome_ids=parent_outcome_ids,
        parent_option_ids=parent_option_ids,
    )


def test_get_utility_for_combination_returns_scalar_for_single_value_metric() -> None:
    utility_id = uuid.uuid4()
    value_metric_id = uuid.uuid4()
    outcome_id = uuid.uuid4()
    option_id = uuid.uuid4()
    unrelated_id = uuid.uuid4()
    manager = DiscreteUtilityArrayManager(
        [
            create_utility(utility_id, value_metric_id, 12.5, [outcome_id], [option_id]),
        ],
        {"weather": [str(outcome_id)], "decision": [str(option_id)]},
    )

    assert manager.array.dims == ("weather", "decision", manager.VALUE_METRICS_DIM)
    assert manager.array.sizes[manager.VALUE_METRICS_DIM] == 1
    assert manager.get_utility_for_combination(
        [str(outcome_id), str(option_id), str(unrelated_id)]
    ) == 12.5


def test_multiple_value_metrics_are_rejected() -> None:
    utility_id = uuid.uuid4()
    parent_id = uuid.uuid4()

    with pytest.raises(ValueError, match="exactly one value metric"):
        DiscreteUtilityArrayManager(
            [
                create_utility(utility_id, uuid.uuid4(), 1.0, [parent_id], []),
                create_utility(utility_id, uuid.uuid4(), 2.0, [parent_id], []),
            ],
            {"parent": [str(parent_id)]},
        )


def test_empty_utilities_create_empty_array() -> None:
    manager = DiscreteUtilityArrayManager([], {})

    assert manager.array.size == 0
    assert manager.get_utility_for_combination([]) == 0.0


def test_overlapping_parent_dimensions_align_when_tables_are_added() -> None:
    utility_id = uuid.uuid4()
    value_metric_id = uuid.uuid4()
    decision_states = [uuid.uuid4(), uuid.uuid4()]
    weather_states = [uuid.uuid4(), uuid.uuid4()]
    ground_states = [uuid.uuid4(), uuid.uuid4()]

    operating_cost = DiscreteUtilityArrayManager(
        [
            create_utility(utility_id, value_metric_id, float(index), [weather], [decision])
            for index, (decision, weather) in enumerate(
                (decision, weather)
                for decision in decision_states
                for weather in weather_states
            )
        ],
        {
            "decision": [str(state) for state in decision_states],
            "weather": [str(state) for state in weather_states],
        },
    )
    safety_value = DiscreteUtilityArrayManager(
        [
            create_utility(utility_id, value_metric_id, 10.0 + index, [weather, ground], [])
            for index, (weather, ground) in enumerate(
                (weather, ground)
                for weather in weather_states
                for ground in ground_states
            )
        ],
        {
            "weather": [str(state) for state in weather_states],
            "ground": [str(state) for state in ground_states],
        },
    )

    combined = operating_cost.array + safety_value.array

    assert set(combined.dims) == {
        "decision",
        "weather",
        "ground",
        operating_cost.VALUE_METRICS_DIM,
    }
    assert combined.size == 8
    assert combined.item(0) == 10.0