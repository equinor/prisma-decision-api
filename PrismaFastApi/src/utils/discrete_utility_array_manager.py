import numpy as np
import xarray as xr

from src.dtos.discrete_utility_dtos import DiscreteUtilityOutgoingDto


class DiscreteUtilityArrayManager:
    """Manage discrete utility values indexed by parent-state combinations."""

    VALUE_METRICS_DIM = "value_metrics"
    UTILITY_GRID_NAME = "utility_grid"
    PARENT_SEPARATOR = ","

    def __init__(
        self,
        utilities: list[DiscreteUtilityOutgoingDto],
        parent_dimensions: dict[str, list[str]],
    ) -> None:
        self.all_parent_ids: set[str] = set()
        self.utility_by_parents: dict[str, float] = {}
        self.array = self.create_xarray_grid(utilities, parent_dimensions)

    def _create_parents_label(self, parents: list[str] | tuple[str, ...] | set[str]) -> str:
        return self.PARENT_SEPARATOR.join(sorted(parents))

    def create_xarray_grid(
        self,
        utilities: list[DiscreteUtilityOutgoingDto],
        parent_dimensions: dict[str, list[str]],
    ) -> xr.DataArray:
        self.all_parent_ids = set()
        self.utility_by_parents = {}
        if not utilities:
            return xr.DataArray([])

        if self.VALUE_METRICS_DIM in parent_dimensions:
            raise ValueError(f"Parent dimension cannot be named {self.VALUE_METRICS_DIM}")

        if any(not states for states in parent_dimensions.values()):
            raise ValueError("Each parent dimension must contain at least one state")

        value_metrics = sorted({str(utility.value_metric_id) for utility in utilities})
        if len(value_metrics) != 1:
            raise ValueError("Discrete utilities must use exactly one value metric")

        utility_by_parents: dict[str, float] = {}
        dimensions = list(parent_dimensions)
        coordinate_indexes = {
            dimension: {state_id: index for index, state_id in enumerate(state_ids)}
            for dimension, state_ids in parent_dimensions.items()
        }
        data = np.zeros(tuple(len(parent_dimensions[dimension]) for dimension in dimensions) + (1,))

        for utility in utilities:
            parent_ids = [str(parent_id) for parent_id in utility.parent_outcome_ids] + [
                str(parent_id) for parent_id in utility.parent_option_ids
            ]
            self.all_parent_ids.update(parent_ids)
            utility_value = utility.utility_value if utility.utility_value is not None else 0.0
            utility_by_parents[self._create_parents_label(parent_ids)] = utility_value

            indexes: list[int] = []
            for dimension in dimensions:
                matching_states = set(parent_ids).intersection(parent_dimensions[dimension])
                if len(matching_states) != 1:
                    raise ValueError(
                        f"Utility row must contain exactly one state for parent {dimension}"
                    )
                indexes.append(coordinate_indexes[dimension][matching_states.pop()])
            data[tuple(indexes) + (0,)] = utility_value

        self.utility_by_parents = utility_by_parents
        return xr.DataArray(
            data,
            dims=dimensions + [self.VALUE_METRICS_DIM],
            coords=parent_dimensions | {self.VALUE_METRICS_DIM: value_metrics},
            name=self.UTILITY_GRID_NAME,
        )

    def get_utility_for_combination(
        self, parents: list[str] | tuple[str, ...] | set[str]
    ) -> float:
        if self.array.size == 0:
            return 0.0

        relevant_parents = [parent for parent in set(parents) if parent in self.all_parent_ids]
        parent_label = self._create_parents_label(relevant_parents)
        return self.utility_by_parents.get(parent_label, 0.0)