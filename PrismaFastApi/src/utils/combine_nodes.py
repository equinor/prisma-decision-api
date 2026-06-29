import numpy as np
import xarray as xr
from functools import reduce
from pydantic import BaseModel

class State(BaseModel):
    id: str # a1
    parent_id: str # a

class DataPoint(BaseModel):
    value: int
    states: list[State]

def _build_node_array(datapoints: list[DataPoint]) -> xr.DataArray:
    dims: list[str] = []
    coords: dict[str, list[str]] = {}

    for dp in datapoints:
        for state in dp.states:
            if state.parent_id not in coords:
                dims.append(state.parent_id)
                coords[state.parent_id] = []
            if state.id not in coords[state.parent_id]:
                coords[state.parent_id].append(state.id)

    shape = tuple(len(coords[dim]) for dim in dims)
    data = np.zeros(shape, dtype=int)

    for dp in datapoints:
        idx = tuple(
            coords[dim].index(next(s.id for s in dp.states if s.parent_id == dim))
            for dim in dims
        )
        data[idx] = dp.value

    return xr.DataArray(data, dims=dims, coords={dim: coords[dim] for dim in dims})


def combine_nodes(nodes: list[list[DataPoint]]) -> xr.DataArray:
    """Sum DataArrays from each node, broadcasting over all dimensions."""
    return reduce(lambda a, b: a + b, (_build_node_array(dps) for dps in nodes))