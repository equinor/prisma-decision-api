import uuid

import pyagrum as gum  # type: ignore

from src.constants import Type
from src.dtos.decision_dtos import DecisionOutgoingDto
from src.dtos.discrete_utility_dtos import DiscreteUtilityOutgoingDto
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.option_dtos import OptionOutgoingDto
from src.dtos.utility_dtos import UtilityDto
from src.services.pyagrum_solver import PyagrumSolver


def test_combine_utility_nodes_replaces_and_sums_explicit_and_virtual_nodes() -> None:
    solver = PyagrumSolver()
    decision_issue_id = uuid.uuid4()
    decision_id = uuid.uuid4()
    utility_issue_id = uuid.uuid4()
    utility_id = uuid.uuid4()
    value_metric_id = uuid.uuid4()
    options = [
        OptionOutgoingDto(
            id=uuid.uuid4(), decision_id=decision_id, name="First", utility=1.0
        ),
        OptionOutgoingDto(
            id=uuid.uuid4(), decision_id=decision_id, name="Second", utility=2.0
        ),
    ]
    options = sorted(options, key=lambda option: str(option.id))
    decision_issue = IssueOutgoingDto.model_construct(
        id=decision_issue_id,
        type=Type.DECISION.value,
        decision=DecisionOutgoingDto(
            id=decision_id,
            issue_id=decision_issue_id,
            options=options,
            type="Focus",
        ),
        uncertainty=None,
        utility=None,
    )
    utility_issue = IssueOutgoingDto.model_construct(
        id=utility_issue_id,
        type=Type.UTILITY.value,
        decision=None,
        uncertainty=None,
        utility=UtilityDto(id=utility_id, issue_id=utility_issue_id),
    )

    decision_node_id = solver.diagram.addDecisionNode(  # type: ignore
        gum.LabelizedVariable(
            str(decision_issue_id),
            str(decision_issue_id),
            [str(option.id) for option in options],
        )
    )
    explicit_utility_node_id = solver.diagram.addUtilityNode(  # type: ignore
        gum.LabelizedVariable(str(utility_issue_id), str(utility_issue_id), 1)
    )
    virtual_utility_node_id = solver.diagram.addUtilityNode(  # type: ignore
        gum.LabelizedVariable("virtual_utility", "virtual_utility", 1)
    )
    solver.diagram.addArc(decision_node_id, explicit_utility_node_id)  # type: ignore
    solver.diagram.addArc(decision_node_id, virtual_utility_node_id)  # type: ignore
    solver.node_lookup = {
        str(decision_issue_id): decision_node_id,
        str(utility_issue_id): explicit_utility_node_id,
    }
    solver.discrete_utilities = [
        DiscreteUtilityOutgoingDto(
            utility_id=utility_id,
            value_metric_id=value_metric_id,
            utility_value=explicit_value,
            parent_option_ids=[option.id],
        )
        for option, explicit_value in zip(options, [10.0, 20.0])
    ]

    solver.combine_utility_nodes([decision_issue, utility_issue])

    assert solver.diagram.utilityNodeSize() == 1  # type: ignore
    combined_node_id = solver.diagram.idFromName("combined_utility")  # type: ignore
    assert solver.diagram.parents(combined_node_id) == {decision_node_id}  # type: ignore
    assert solver.diagram.sizeArcs() == 1  # type: ignore
    assert solver.diagram.utility(combined_node_id)[  # type: ignore
        {str(decision_issue_id): str(options[0].id)}
    ] == 11.0
    assert solver.diagram.utility(combined_node_id)[  # type: ignore
        {str(decision_issue_id): str(options[1].id)}
    ] == 22.0