import uuid
from dataclasses import dataclass, field
from typing import Optional
from sqlalchemy.ext.asyncio import AsyncSession

from src.models.stragegy_option import StrategyOption
from src.models.discrete_probability import DiscreteProbability
from src.models.discrete_utility import DiscreteUtility
from src.models.project_role import ProjectRole
from src.models.edge import Edge
from src.models.strategy import Strategy
from src.services.discrete_utility_service import DiscreteUtilityService
from src.constants import Boundary, DecisionHierarchy, ProjectRoleType, Type
from src.dtos.decision_dtos import DecisionIncomingDto
from src.dtos.discrete_probability_dtos import DiscreteProbabilityIncomingDto
from src.dtos.discrete_utility_dtos import DiscreteUtilityIncomingDto
from src.dtos.edge_dtos import EdgeIncomingDto
from src.dtos.issue_dtos import IssueIncomingDto
from src.dtos.node_dtos import NodeIncomingDto
from src.dtos.node_style_dtos import NodeStyleIncomingDto
from src.dtos.option_dtos import OptionIncomingDto
from src.dtos.outcome_dtos import OutcomeIncomingDto
from src.dtos.project_dtos import (
    ProjectCreateDto,
    ProjectIncomingDto,
    ProjectOutgoingDto,
    ObjectiveViaProjectDto,
)
from src.dtos.project_roles_dtos import ProjectRoleCreateDto
from src.dtos.uncertainty_dtos import UncertaintyIncomingDto
from src.dtos.user_dtos import UserIncomingDto
from src.dtos.utility_dtos import UtilityIncomingDto
from src.dtos.strategy_dtos import StrategyIncomingDto
from src.models import Project, Issue
from src.repositories.project_repository import ProjectRepository
from src.services.discrete_probability_service import DiscreteProbabilityService
from src.services.edge_service import EdgeService
from src.services.issue_service import IssueService
from src.services.project_service import ProjectService
from src.services.strategy_service import StrategyService


@dataclass
class IdMappings:
    """Container for all ID mappings during project duplication."""

    project: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})
    issue: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})
    uncertainty: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})
    decision: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})
    utility: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})
    node: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})
    outcome: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})
    option: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})


class ProjectDuplicationService:
    """Service for duplicating projects with all related entities."""

    mappings = IdMappings()

    def generate_id_mappings(self, issues: list[Issue] | list[IssueIncomingDto]) -> None:
        """Pre-generate all ID mappings for issues, nodes, outcomes, and options."""
        for issue in issues:
            self.mappings.issue[issue.id] = uuid.uuid4()
            if issue.node:
                self.mappings.node[issue.node.id] = uuid.uuid4()

            if issue.uncertainty:
                self.mappings.uncertainty[issue.uncertainty.id] = uuid.uuid4()
                for outcome in issue.uncertainty.outcomes:
                    self.mappings.outcome[outcome.id] = uuid.uuid4()

            if issue.decision:
                self.mappings.decision[issue.decision.id] = uuid.uuid4()
                for option in issue.decision.options:
                    self.mappings.option[option.id] = uuid.uuid4()

    def map_parent_ids(
        self,
        parent_outcome_ids: list[uuid.UUID],
        parent_option_ids: list[uuid.UUID],
    ) -> tuple[list[uuid.UUID], list[uuid.UUID]]:
        """Map parent outcome and option IDs to new IDs."""
        mapped_outcome_ids: list[uuid.UUID] = [
            self.mappings.outcome[po] for po in parent_outcome_ids if po in self.mappings.outcome
        ]
        mapped_option_ids: list[uuid.UUID] = [
            self.mappings.option[po] for po in parent_option_ids if po in self.mappings.option
        ]
        return (mapped_outcome_ids, mapped_option_ids)

    async def project_duplication(
        self, session: AsyncSession, id: uuid.UUID, current_user: UserIncomingDto
    ) -> Optional[ProjectOutgoingDto]:
        """Duplicate a project with all its issues, nodes, and edges."""
        projects: list[Project] = await ProjectRepository(session).get([id])
        if not projects:
            return None

        original_project: Project = projects[0]
        new_project_id = uuid.uuid4()

        # Create duplicate project
        duplicated_project = await self.duplicate_project(
            session, original_project, new_project_id, current_user
        )

        self.mappings.project[original_project.id] = new_project_id
        self.generate_id_mappings(list(original_project.issues))
        await self.duplicate_incoming_issues(session, list(original_project.issues), current_user)

        await self.duplicate_strategies(
            session, original_project.strategies, new_project_id, current_user
        )

        await self.duplicate_edges(session, original_project.edges, new_project_id)
        return duplicated_project

    async def duplicate_project(
        self,
        session: AsyncSession,
        original_project: Project | ProjectIncomingDto,
        new_project_id: uuid.UUID,
        current_user: UserIncomingDto,
    ) -> Optional[ProjectOutgoingDto]:
        """Create a duplicate project with roles."""

        duplicate_project_dto = ProjectCreateDto(
            id=new_project_id,
            name=original_project.name,
            parent_project_id=(
                original_project.id if isinstance(original_project, Project) else None
            ),
            parent_project_name=(
                str(original_project.id) if isinstance(original_project, Project) else None
            ),
            objectives=[
                ObjectiveViaProjectDto(
                    description=objective.description,
                    name=objective.name,
                )
                for objective in original_project.objectives
            ],
            opportunity_statement=original_project.opportunity_statement,
            public=original_project.public,
            end_date=original_project.end_date,
            users=[
                ProjectRoleCreateDto(
                    user_name=(
                        role.user.name
                        if isinstance(role, ProjectRole) and role.user
                        else role.user_name
                    ),
                    azure_id=(
                        role.user.azure_id
                        if isinstance(role, ProjectRole) and role.user
                        else role.azure_id
                    ),
                    user_id=(
                        role.user.id
                        if isinstance(role, ProjectRole) and role.user
                        else role.user_id
                    ),
                    project_id=new_project_id,
                    role=ProjectRoleType(role.role),
                )
                for role in (
                    original_project.project_role
                    if isinstance(original_project, Project)
                    else original_project.users
                )
            ],
        )

        duplicated_project = await ProjectService().create(
            session,
            [duplicate_project_dto],
            UserIncomingDto(id=None, name=current_user.name, azure_id=current_user.azure_id),
            is_duplicate=True,
        )

        return duplicated_project[0]

    async def duplicate_incoming_issues(
        self,
        session: AsyncSession,
        issues: list[Issue] | list[IssueIncomingDto],
        current_user: UserIncomingDto,
    ):
        """Create duplicate issues for the new project, prioritizing decision issues first, then uncertainty, then utility."""
        duplicate_incoming_issues: list[IssueIncomingDto] = []
        duplicate_incoming_discrete_probabilities_dtos: list[DiscreteProbabilityIncomingDto] = []
        duplicate_incoming_discrete_utilities_dtos: list[DiscreteUtilityIncomingDto] = []
        for issue in issues:
            uncertainty_result = self.create_uncertainty_incoming_dto(issue)
            uncertainty_dto = uncertainty_result[0] if uncertainty_result else None
            utility_dto_result = self.create_utility_incoming_dto(issue)
            utility_dto = utility_dto_result[0] if utility_dto_result else None
            duplicate_incoming_discrete_probabilities_dtos.extend(
                uncertainty_result[1] if uncertainty_result else []
            )
            duplicate_incoming_discrete_utilities_dtos.extend(
                utility_dto_result[1] if utility_dto_result else []
            )
            issue_incoming_dto = IssueIncomingDto(
                id=self.mappings.issue[issue.id],
                project_id=self.mappings.project[issue.project_id],
                name=issue.name,
                description=issue.description,
                order=issue.order,
                type=Type(issue.type),
                boundary=Boundary(issue.boundary),
                node=(
                    self.create_node_dto(issue, self.mappings.project[issue.project_id])
                    if issue.node
                    else None
                ),
                decision=self.create_decision_incoming_dto(issue),
                uncertainty=uncertainty_dto,
                utility=utility_dto,
            )

            duplicate_incoming_issues.append(issue_incoming_dto)
        await IssueService().create(
            session,
            duplicate_incoming_issues,
            UserIncomingDto(id=None, name=current_user.name, azure_id=current_user.azure_id),
        )
        # Create duplicate discrete probabilities
        if duplicate_incoming_discrete_probabilities_dtos:
            await DiscreteProbabilityService().create(
                session,
                duplicate_incoming_discrete_probabilities_dtos,
            )
        # Create duplicate discrete utilities
        if duplicate_incoming_discrete_utilities_dtos:
            await DiscreteUtilityService().create(
                session,
                duplicate_incoming_discrete_utilities_dtos,
            )

    def create_decision_incoming_dto(
        self,
        issue: Issue,
    ) -> Optional[DecisionIncomingDto]:
        """Create a DecisionIncomingDto if the issue has a decision."""
        if not issue.decision:
            return None

        new_decision_id = self.mappings.decision[issue.decision.id]
        return DecisionIncomingDto(
            id=new_decision_id,
            issue_id=self.mappings.issue[issue.id],
            type=DecisionHierarchy(issue.decision.type),
            options=(
                [
                    OptionIncomingDto(
                        id=self.mappings.option[option.id],
                        decision_id=new_decision_id,
                        name=option.name,
                        utility=option.utility,
                    )
                    for option in issue.decision.options
                ]
                if issue.decision.options and len(issue.decision.options) > 0
                else []
            ),
        )

    def create_uncertainty_incoming_dto(
        self, issue: Issue | IssueIncomingDto
    ) -> Optional[tuple[UncertaintyIncomingDto, list[DiscreteProbabilityIncomingDto]]]:
        """Create an UncertaintyIncomingDto if the issue has an uncertainty."""
        if not issue.uncertainty:
            return None

        new_uncertainty_id = self.mappings.uncertainty[issue.uncertainty.id]
        uncertainty = issue.uncertainty
        discrete_probabilities_dtos: list[DiscreteProbabilityIncomingDto] = []
        outcomes_dtos: list[OutcomeIncomingDto] = [
            OutcomeIncomingDto(
                id=self.mappings.outcome[outcome.id],
                name=outcome.name,
                uncertainty_id=new_uncertainty_id,
                utility=outcome.utility,
            )
            for outcome in uncertainty.outcomes
        ]
        if uncertainty.discrete_probabilities and len(uncertainty.discrete_probabilities) > 0:
            for dp in uncertainty.discrete_probabilities:
                mapped_outcome_ids, mapped_option_ids = self.map_parent_ids(
                    list(
                        [pout.parent_outcome_id for pout in dp.parent_outcomes]
                        if isinstance(dp, DiscreteProbability)
                        else dp.parent_outcome_ids
                    ),
                    list(
                        [pot.parent_option_id for pot in dp.parent_options]
                        if isinstance(dp, DiscreteProbability)
                        else dp.parent_option_ids
                    ),
                )

                outcome_id = self.mappings.outcome.get(dp.outcome_id)
                if outcome_id is None:
                    outcome_id = dp.outcome_id

                discrete_probabilities_dtos.append(
                    DiscreteProbabilityIncomingDto(
                        id=uuid.uuid4(),
                        uncertainty_id=new_uncertainty_id,
                        outcome_id=outcome_id,
                        probability=dp.probability,
                        parent_outcome_ids=mapped_outcome_ids,
                        parent_option_ids=mapped_option_ids,
                    )
                )

        uncertainty_dto = UncertaintyIncomingDto(
            id=new_uncertainty_id,
            issue_id=self.mappings.issue[issue.id],
            is_key=uncertainty.is_key,
            outcomes=outcomes_dtos,
            discrete_probabilities=[],
        )
        return uncertainty_dto, discrete_probabilities_dtos

    def create_utility_incoming_dto(
        self, issue: Issue | IssueIncomingDto
    ) -> Optional[tuple[Optional[UtilityIncomingDto], list[DiscreteUtilityIncomingDto]]]:
        """Create a UtilityIncomingDto if the issue has a utility."""
        if not issue.utility:
            return None

        new_utility_id = uuid.uuid4()
        discrete_utilities: list[DiscreteUtilityIncomingDto] = []
        for du in issue.utility.discrete_utilities:

            mapped_outcome_ids, mapped_option_ids = self.map_parent_ids(
                list(
                    [pout.parent_outcome_id for pout in du.parent_outcomes]
                    if isinstance(du, DiscreteUtility)
                    else du.parent_outcome_ids
                ),
                list(
                    [pot.parent_option_id for pot in du.parent_options]
                    if isinstance(du, DiscreteUtility)
                    else du.parent_option_ids
                ),
            )
            discrete_utilities.append(
                DiscreteUtilityIncomingDto(
                    id=uuid.uuid4(),
                    utility_id=new_utility_id,
                    utility_value=du.utility_value,
                    parent_option_ids=mapped_option_ids,
                    parent_outcome_ids=mapped_outcome_ids,
                    value_metric_id=du.value_metric_id,
                )
            )

        utility_incoming_dto = UtilityIncomingDto(
            id=new_utility_id,
            issue_id=self.mappings.issue[issue.id],
            discrete_utilities=[],
        )
        return utility_incoming_dto, discrete_utilities

    def create_node_dto(
        self, issue: Issue | IssueIncomingDto, new_project_id: uuid.UUID
    ) -> Optional[NodeIncomingDto]:
        """Create a NodeIncomingDto if the issue has a node with style."""

        if not issue.node or not issue.node.node_style:
            return None

        new_node_id = self.mappings.node[issue.node.id]
        return NodeIncomingDto(
            id=new_node_id,
            project_id=new_project_id,
            issue_id=self.mappings.issue[issue.id],
            name=issue.node.name or issue.name,
            node_style=NodeStyleIncomingDto(
                node_id=new_node_id,
                x_position=issue.node.node_style.x_position,
                y_position=issue.node.node_style.y_position,
            ),
        )

    async def duplicate_edges(
        self,
        session: AsyncSession,
        original_edges: list[Edge] | list[EdgeIncomingDto],
        new_project_id: uuid.UUID,
    ) -> None:
        """Duplicate all edges from the original project."""

        duplicate_edge_dtos: list[EdgeIncomingDto] = [
            EdgeIncomingDto(
                id=uuid.uuid4(),
                tail_id=self.mappings.node[edge.tail_id],
                head_id=self.mappings.node[edge.head_id],
                project_id=new_project_id,
            )
            for edge in original_edges
        ]

        if duplicate_edge_dtos:
            await EdgeService().create(session, duplicate_edge_dtos)

    async def duplicate_strategies(
        self,
        session: AsyncSession,
        strategies: list[Strategy] | list[StrategyIncomingDto],
        new_project_id: uuid.UUID,
        user_dto: UserIncomingDto,
    ) -> None:
        strategies_to_create = [
            StrategyIncomingDto(
                id=uuid.uuid4(),
                name=strategy.name,
                description=strategy.description,
                rationale=strategy.rationale,
                project_id=new_project_id,
                options=[
                    OptionIncomingDto(
                        id=self.mappings.option[
                            (
                                strategy_option.option.id
                                if isinstance(strategy_option, StrategyOption)
                                else strategy_option.id
                            )
                        ],
                        name=(
                            strategy_option.option.name
                            if isinstance(strategy_option, StrategyOption)
                            else strategy_option.name
                        ),
                        decision_id=self.mappings.decision[
                            (
                                strategy_option.option.decision_id
                                if isinstance(strategy_option, StrategyOption)
                                else strategy_option.decision_id
                            )
                        ],
                        utility=(
                            strategy_option.option.utility
                            if isinstance(strategy_option, StrategyOption)
                            else strategy_option.utility
                        ),
                    )
                    for strategy_option in (
                        strategy.strategy_options
                        if isinstance(strategy, Strategy)
                        else strategy.options
                    )
                ],
            )
            for strategy in strategies
        ]
        if strategies_to_create:
            await StrategyService().create(session, strategies_to_create, user_dto)
