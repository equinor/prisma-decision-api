import uuid
from pydantic import BaseModel
from typing import List
from src.dtos.option_dtos import OptionOutgoingDto
from src.dtos.outcome_dtos import OutcomeOutgoingDto


class ParentState(BaseModel):
    parent_id: uuid.UUID
    state: OptionOutgoingDto | OutcomeOutgoingDto


class OptimalOption(BaseModel):
    parent_states: List[ParentState]
    decision_id: uuid.UUID
    state: OptionOutgoingDto


class DecisionSolution(BaseModel):
    optimal_decisions: List[OptimalOption]
    mean: float


class SolutionDto(BaseModel):
    decision_solutions: list[DecisionSolution]

    def get_all_optimal_decisions(self):
        a: List[OptimalOption] = []
        [a.extend(x.optimal_decisions) for x in self.decision_solutions]
        return a
        
    def get_lookup(self) -> dict[str, dict[tuple[str, ...], str]]:
        lookup: dict[str, dict[tuple[str, ...], str]] = {}
        for option in self.get_all_optimal_decisions():
            decision_key = str(option.decision_id)
            parent_key = tuple(str(x.state.id) for x in option.parent_states)
            lookup.setdefault(decision_key, {})[parent_key] = str(option.state.id)
        return lookup
    
    def get_valid_subsets(self) -> list[list[uuid.UUID]]:
        res: list[list[uuid.UUID]] = []
        for option in self.get_all_optimal_decisions():
            parents = [x.state.id for x in option.parent_states]
            res.append(parents + [option.state.id])
        return res