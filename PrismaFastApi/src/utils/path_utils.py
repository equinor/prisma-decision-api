import uuid
from typing import Optional
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.model_solution_dtos import SolutionDto
from src.constants import Type

def is_path_valid_for_optimal_options(
        path: list[uuid.UUID],
        solution: Optional[SolutionDto],
    ) -> bool:
    if not solution:
        return True
    valid_subsets = solution.get_valid_subsets()
    return  any(set(valid_subset).issubset(path) for valid_subset in valid_subsets)

def filter_paths_by_solution(
    solution: SolutionDto,
    issues: list[IssueOutgoingDto],
    paths: list[list[uuid.UUID]],
) -> list[list[uuid.UUID]]:
    raise NotImplementedError("Cannot handle paths that contain outcomes that are not decision parents, but are correct")
    optimal_option_lookup = solution.get_lookup()
    issue_by_id = {i.id: i for i in issues}
    return list(
        filter(
            lambda x: is_path_valid_for_optimal_options(x, issue_by_id, solution), 
            paths,
        )
    )

def expand_all_paths_by_one_depth(
    partial_order: list[uuid.UUID],
    issues: list[IssueOutgoingDto],
    paths: list[list[uuid.UUID]],
    solution: Optional[SolutionDto] = None,
) -> list[list[uuid.UUID]]:
    issue_by_id = {i.id: i for i in issues}
    if len(paths) == 0:
        root = partial_order[0]
        issue = issue_by_id[root]

        if issue.type == Type.UNCERTAINTY.value and issue.uncertainty is not None:
            [paths.append([x.id]) for x in issue.uncertainty.outcomes]
        elif issue.type == Type.DECISION.value and issue.decision is not None:
            [
                paths.append([x.id]) 
                for x in issue.decision.options 
                if is_path_valid_for_optimal_options([x.id], solution)
            ]
    else:
        paths_to_add: list[list[uuid.UUID]] = []
        for path in paths:
            ind = len(path)
            if ind == len(partial_order):
                # already expanded to the end
                continue
            issue = issue_by_id[partial_order[ind]]

            if issue.type == Type.UNCERTAINTY.value and issue.uncertainty is not None:
                [paths_to_add.append(path+[x.id]) for x in issue.uncertainty.outcomes]
            elif issue.type == Type.DECISION.value and issue.decision is not None:
                [
                    paths_to_add.append(path+[x.id]) 
                    for x in issue.decision.options 
                    if is_path_valid_for_optimal_options(path+[x.id], solution)
                ]
        # override the paths to the expanded ones
        paths = paths_to_add
    return paths
