import uuid
from dataclasses import dataclass, field
from typing import Any, Optional
from sqlalchemy.ext.asyncio import AsyncSession

from src.services.uncertainty_service import UncertaintyService
from src.services.utility_service import UtilityService
from src.services.decision_service import DecisionService
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
from src.dtos.project_dtos import ProjectCreateDto, ProjectOutgoingDto, ObjectiveViaProjectDto
from src.dtos.project_roles_dtos import ProjectRoleCreateDto
from src.dtos.uncertainty_dtos import UncertaintyIncomingDto
from src.dtos.user_dtos import UserIncomingDto
from src.dtos.utility_dtos import UtilityIncomingDto
from src.models import Project, Issue
from src.repositories.project_repository import ProjectRepository
from src.services.discrete_probability_service import DiscreteProbabilityService
from src.services.edge_service import EdgeService
from src.services.issue_service import IssueService
from src.services.project_service import ProjectService


@dataclass
class IdMappings:
    """Container for all ID mappings during project duplication."""

    issue: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})
    node: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})
    outcome: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})
    option: dict[uuid.UUID, uuid.UUID] = field(default_factory=lambda: {})


class ProjectDuplicationService:
    """Service for duplicating projects with all related entities."""

    async def project_duplication(
        self, session: AsyncSession, id: uuid.UUID, current_user: UserIncomingDto
    ) -> Optional[ProjectOutgoingDto]:
        """Duplicate a project with all its issues, nodes, and edges."""
        projects: list[Project] = await ProjectRepository(session).get([id])
        if not projects:
            return None

        original_project: Project = projects[0]
        new_project_id = uuid.uuid4()
        mappings = IdMappings()

        # Create duplicate project
        duplicated_project = await self.create_duplicate_project(
            session, original_project, new_project_id, current_user
        )
        self.generate_id_mappings(list(original_project.issues), mappings)
        await self.create_duplicate_issues(
            session,
            new_project_id,
            original_project,
            current_user,
            mappings,
        )

        return duplicated_project

    async def create_duplicate_project(
        self,
        session: AsyncSession,
        original_project: Project,
        new_project_id: uuid.UUID,
        current_user: UserIncomingDto,
    ) -> Optional[ProjectOutgoingDto]:
        """Create a duplicate project with roles."""

        duplicate_project_dto = ProjectCreateDto(
            id=new_project_id,
            name=original_project.name,
            parent_project_id=original_project.id,
            parent_project_name=original_project.name,
            objectives=[
                ObjectiveViaProjectDto(
                    description=objective.description,
                    name=objective.name,
                )
                for objective in original_project.objectives
            ],
            opportunityStatement=original_project.opportunityStatement,
            public=original_project.public,
            end_date=original_project.end_date,
            users=[
                ProjectRoleCreateDto(
                    user_name="",
                    azure_id="",
                    user_id=role.user_id,
                    project_id=new_project_id,
                    role=ProjectRoleType(role.role),
                )
                for role in original_project.project_role
            ],
        )

        duplicated_project = await ProjectService().create(
            session,
            [duplicate_project_dto],
            UserIncomingDto(id=None, name=current_user.name, azure_id=current_user.azure_id),
            is_duplicate=True,
        )

        return duplicated_project[0]

    async def create_duplicate_issues(
        self,
        session: AsyncSession,
        new_project_id: uuid.UUID,
        original_project: Project,
        current_user: UserIncomingDto,
        mappings: IdMappings,
    ) -> None:
        """Create duplicate issues for the new project, prioritizing decision issues first, then uncertainty, then utility."""
        # Sort issues to process decision issues first, then uncertainty, then utility

        original_issues = list(original_project.issues)
        decision_type_issues = [
            issue for issue in original_issues if issue.type == Type.DECISION.value
        ]
        uncertainity_issues = [
            issue for issue in original_issues if issue.type == Type.UNCERTAINTY.value
        ]
        utility_issues = [issue for issue in original_issues if issue.type == Type.UTILITY.value]

        await self.duplicate_unassigned_issues(
            session, original_project, new_project_id, mappings, current_user
        )
        await self.duplicate_fact_issues(
            session, original_project, new_project_id, mappings, current_user
        )

        # Duplicate all issue types
        await self.duplicate_decision_issues(
            session, decision_type_issues, new_project_id, mappings, current_user
        )

        discrete_prob_dtos = await self.duplicate_uncertainty_issues(
            session, uncertainity_issues, new_project_id, mappings, current_user
        )
        await DiscreteProbabilityService().create(session, discrete_prob_dtos)

        await self.duplicate_utility_issues(
            session, utility_issues, new_project_id, mappings, current_user
        )
        await self.duplicate_edges(session, original_project, new_project_id, mappings)

        optional_fact_and_unassigned_issues = [
            issue
            for issue in original_issues
            if issue.type == Type.FACT.value or issue.type == Type.UNASSIGNED.value
        ]
        optional_decision_issues = [
            issue for issue in original_issues if issue.type != Type.DECISION.value
        ]
        optional_uncertainty_issues = [
            issue for issue in original_issues if issue.type != Type.UNCERTAINTY.value
        ]
        optional_utility_issues = [
            issue for issue in original_issues if issue.type != Type.UTILITY.value
        ]
        for optional_fact_unassigned_issue in optional_fact_and_unassigned_issues:
            if optional_fact_unassigned_issue.decision:
                optional_decision_issues.append(optional_fact_unassigned_issue)
            if optional_fact_unassigned_issue.uncertainty:
                optional_uncertainty_issues.append(optional_fact_unassigned_issue)
            if optional_fact_unassigned_issue.utility:
                optional_utility_issues.append(optional_fact_unassigned_issue)

        decision_dtos: list[DecisionIncomingDto] = []
        for issue in [issue for issue in optional_decision_issues]:
            decision = issue.decision
            if decision:
                decision_dto = self.create_decision_incoming_dto(issue, mappings)
                if decision_dto:
                    decision_dtos.append(decision_dto)
                else:
                    continue
        await DecisionService().create(session, decision_dtos)

        uncertainty_dtos: list[UncertaintyIncomingDto] = []
        discrete_probabilities_dtos: list[DiscreteProbabilityIncomingDto] = []
        for issue in optional_uncertainty_issues:
            result = self.create_uncertainty_incoming_dto(issue, mappings)
            if result is not None:
                uncertainty_dto, discrete_prob_list = result
                uncertainty_dtos.append(uncertainty_dto)
                discrete_probabilities_dtos.extend(discrete_prob_list)

        await UncertaintyService().create(session, uncertainty_dtos)
        if len(discrete_probabilities_dtos) > 0:
            await DiscreteProbabilityService().create(session, discrete_probabilities_dtos)

        utility_dtos: list[UtilityIncomingDto] = []
        for issue in optional_utility_issues:
            utility_dto = self.create_utility_incoming_dto(issue, mappings)
            if utility_dto:
                utility_dtos.append(utility_dto)
        await UtilityService().create(session, utility_dtos)

    def generate_id_mappings(self, issues: list[Issue], mappings: IdMappings) -> None:
        """Pre-generate all ID mappings for issues, nodes, outcomes, and options."""
        optional_decision_issues = [issue for issue in issues if issue.type != Type.DECISION.value]
        optional_uncertainty_issues = [
            issue for issue in issues if issue.type != Type.UNCERTAINTY.value
        ]
        for optional_issue in optional_decision_issues:
            if optional_issue.decision:
                for option in optional_issue.decision.options:
                    mappings.option[option.id] = uuid.uuid4()
        for optional_issue in optional_uncertainty_issues:
            if optional_issue.uncertainty:
                for outcome in optional_issue.uncertainty.outcomes:
                    mappings.outcome[outcome.id] = uuid.uuid4()
        for issue in issues:
            mappings.issue[issue.id] = uuid.uuid4()

            if issue.node:
                mappings.node[issue.node.id] = uuid.uuid4()

            if issue.uncertainty and issue.type == Type.UNCERTAINTY.value:
                for outcome in issue.uncertainty.outcomes:
                    mappings.outcome[outcome.id] = uuid.uuid4()

            if issue.decision and issue.type == Type.DECISION.value:
                for option in issue.decision.options:
                    mappings.option[option.id] = uuid.uuid4()

    def create_decision_incoming_dto(
        self,
        issue: Issue,
        mappings: IdMappings,
    ) -> Optional[DecisionIncomingDto]:
        """Create a DecisionIncomingDto if the issue has a decision."""
        if not issue.decision:
            return None

        new_decision_id = uuid.uuid4()
        return DecisionIncomingDto(
            id=new_decision_id,
            issue_id=mappings.issue[issue.id],
            type=DecisionHierarchy(issue.decision.type),
            options=(
                [
                    OptionIncomingDto(
                        id=mappings.option[option.id],
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
        self, issue: Issue, mappings: IdMappings
    ) -> Optional[tuple[UncertaintyIncomingDto, list[DiscreteProbabilityIncomingDto]]]:
        """Create an UncertaintyIncomingDto if the issue has an uncertainty."""
        if not issue.uncertainty:
            return None

        new_uncertainty_id = uuid.uuid4()
        uncertainty = issue.uncertainty
        discrete_probabilities_dtos: list[DiscreteProbabilityIncomingDto] = []
        outcomes_dtos: list[OutcomeIncomingDto] = [
            OutcomeIncomingDto(
                id=mappings.outcome[outcome.id],
                name=outcome.name,
                uncertainty_id=new_uncertainty_id,
                utility=outcome.utility,
            )
            for outcome in uncertainty.outcomes
        ]
        if uncertainty.discrete_probabilities and len(uncertainty.discrete_probabilities) > 0:
            for dp in uncertainty.discrete_probabilities:
                mapped_outcome_ids, mapped_option_ids = self.map_parent_ids(
                    list(dp.parent_outcomes), list(dp.parent_options), mappings
                )
                outcome_id = mappings.outcome.get(dp.outcome_id)
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
            issue_id=mappings.issue[issue.id],
            is_key=uncertainty.is_key,
            outcomes=outcomes_dtos,
            discrete_probabilities=[],
        )
        return uncertainty_dto, discrete_probabilities_dtos

    def create_utility_incoming_dto(
        self, issue: Issue, mappings: IdMappings
    ) -> Optional[UtilityIncomingDto]:
        """Create a UtilityIncomingDto if the issue has a utility."""
        if not issue.utility:
            return None

        new_utility_id = uuid.uuid4()
        discrete_utilities: list[DiscreteUtilityIncomingDto] = []
        for du in issue.utility.discrete_utilities:
            mapped_outcome_ids, mapped_option_ids = self.map_parent_ids(
                list(du.parent_outcomes), list(du.parent_options), mappings
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

        return UtilityIncomingDto(
            id=new_utility_id,
            issue_id=mappings.issue[issue.id],
            discrete_utilities=discrete_utilities,
        )

    def create_node_dto(
        self, issue: Issue, new_project_id: uuid.UUID, mappings: IdMappings
    ) -> Optional[NodeIncomingDto]:
        """Create a NodeIncomingDto if the issue has a node with style."""

        new_node_id = mappings.node[issue.node.id]
        return NodeIncomingDto(
            id=new_node_id,
            project_id=new_project_id,
            issue_id=mappings.issue[issue.id],
            name=issue.node.name or issue.name,
            node_style=NodeStyleIncomingDto(
                node_id=new_node_id,
                x_position=issue.node.node_style.x_position,
                y_position=issue.node.node_style.y_position,
            ),
        )

    def create_issue_dto(
        self,
        issue: Issue,
        new_project_id: uuid.UUID,
        mappings: IdMappings,
        decision: Optional[DecisionIncomingDto] = None,
        uncertainty: Optional[UncertaintyIncomingDto] = None,
        utility: Optional[UtilityIncomingDto] = None,
    ) -> IssueIncomingDto:
        """Create an IssueIncomingDto with common fields."""
        return IssueIncomingDto(
            id=mappings.issue[issue.id],
            project_id=new_project_id,
            name=issue.name,
            description=issue.description,
            order=issue.order,
            type=Type(issue.type),
            boundary=Boundary(issue.boundary),
            node=self.create_node_dto(issue, new_project_id, mappings),
            decision=decision,
            uncertainty=uncertainty,
            utility=utility,
        )

    def map_parent_ids(
        self, parent_outcomes: list[Any], parent_options: list[Any], mappings: IdMappings
    ) -> tuple[list[uuid.UUID], list[uuid.UUID]]:
        """Map parent outcome and option IDs to new IDs."""
        mapped_outcome_ids: list[uuid.UUID] = [
            mappings.outcome[po.parent_outcome_id]
            for po in parent_outcomes
            if po.parent_outcome_id in mappings.outcome
        ]
        mapped_option_ids: list[uuid.UUID] = [
            mappings.option[po.parent_option_id]
            for po in parent_options
            if po.parent_option_id in mappings.option
        ]
        return (mapped_outcome_ids, mapped_option_ids)

    async def create_issue(
        self, session: AsyncSession, issue_dto: IssueIncomingDto, current_user: UserIncomingDto
    ) -> None:
        """Helper to create an issue with the current user."""
        await IssueService().create(
            session,
            [issue_dto],
            UserIncomingDto(id=None, name=current_user.name, azure_id=current_user.azure_id),
        )

    async def duplicate_decision_issues(
        self,
        session: AsyncSession,
        decision_issues: list[Issue],
        new_project_id: uuid.UUID,
        mappings: IdMappings,
        current_user: UserIncomingDto,
    ) -> None:
        """Duplicate all decision issues from the original project."""
        for issue in decision_issues:
            if not (issue.decision and issue.type == Type.DECISION.value):
                continue
            decision_dto = self.create_decision_incoming_dto(issue, mappings)

            issue_dto = self.create_issue_dto(
                issue,
                new_project_id,
                mappings,
                decision=decision_dto,
            )
            await self.create_issue(session, issue_dto, current_user)

    async def duplicate_fact_issues(
        self,
        session: AsyncSession,
        original_project: Project,
        new_project_id: uuid.UUID,
        mappings: IdMappings,
        current_user: UserIncomingDto,
    ) -> None:
        """Duplicate all fact issues from the original project."""
        await self._duplicate_basic_issue_type(
            session, original_project, new_project_id, mappings, current_user, Type.FACT
        )

    async def duplicate_unassigned_issues(
        self,
        session: AsyncSession,
        original_project: Project,
        new_project_id: uuid.UUID,
        mappings: IdMappings,
        current_user: UserIncomingDto,
    ) -> None:
        """Duplicate all unassigned issues from the original project."""
        await self._duplicate_basic_issue_type(
            session, original_project, new_project_id, mappings, current_user, Type.UNASSIGNED
        )

    async def _duplicate_basic_issue_type(
        self,
        session: AsyncSession,
        original_project: Project,
        new_project_id: uuid.UUID,
        mappings: IdMappings,
        current_user: UserIncomingDto,
        issue_type: Type,
    ) -> None:
        """Duplicate issues of a basic type (FACT or UNASSIGNED) from the original project."""
        for issue in original_project.issues:
            if issue.type == issue_type.value:
                issue_dto = self.create_issue_dto(
                    issue,
                    new_project_id,
                    mappings,
                )
                await self.create_issue(session, issue_dto, current_user)

    async def duplicate_uncertainty_issues(
        self,
        session: AsyncSession,
        uncertainity_issues: list[Issue],
        new_project_id: uuid.UUID,
        mappings: IdMappings,
        current_user: UserIncomingDto,
    ) -> list[DiscreteProbabilityIncomingDto]:
        """Duplicate all uncertainty issues and return discrete probability DTOs."""
        discrete_probabilities_dtos: list[DiscreteProbabilityIncomingDto] = []
        for issue in uncertainity_issues:
            if not (issue.uncertainty and issue.type == Type.UNCERTAINTY.value):
                continue
            result = self.create_uncertainty_incoming_dto(issue, mappings)
            if result is not None:
                uncertainty_dto, discrete_prob_list = result
                issue_dto = self.create_issue_dto(
                    issue,
                    new_project_id,
                    mappings,
                    uncertainty=uncertainty_dto,
                    decision=None,
                    utility=None,
                )
                await self.create_issue(session, issue_dto, current_user)
                discrete_probabilities_dtos.extend(discrete_prob_list)
        return discrete_probabilities_dtos

    async def duplicate_utility_issues(
        self,
        session: AsyncSession,
        utility_issues: list[Issue],
        new_project_id: uuid.UUID,
        mappings: IdMappings,
        current_user: UserIncomingDto,
    ) -> None:
        """Duplicate all utility issues from the original project."""
        for issue in utility_issues:
            if not (issue.utility and issue.type == Type.UTILITY.value):
                continue

            new_utility_id = uuid.uuid4()

            discrete_utility_dtos: list[DiscreteUtilityIncomingDto] = []
            for du in issue.utility.discrete_utilities:
                mapped_outcome_ids, mapped_option_ids = self.map_parent_ids(
                    list(du.parent_outcomes), list(du.parent_options), mappings
                )
                discrete_utility_dtos.append(
                    DiscreteUtilityIncomingDto(
                        id=uuid.uuid4(),
                        utility_id=new_utility_id,
                        utility_value=du.utility_value,
                        parent_option_ids=mapped_option_ids,
                        parent_outcome_ids=mapped_outcome_ids,
                        value_metric_id=du.value_metric_id,
                    )
                )

            utility_dto = UtilityIncomingDto(
                id=new_utility_id,
                issue_id=mappings.issue[issue.id],
                discrete_utilities=discrete_utility_dtos,
            )

            issue_dto = self.create_issue_dto(
                issue,
                new_project_id,
                mappings,
                utility=utility_dto,
                decision=None,
                uncertainty=None,
            )
            await self.create_issue(session, issue_dto, current_user)

    async def duplicate_edges(
        self,
        session: AsyncSession,
        original_project: Project,
        new_project_id: uuid.UUID,
        mappings: IdMappings,
    ) -> None:
        """Duplicate all edges from the original project."""

        duplicate_edge_dtos: list[EdgeIncomingDto] = [
            EdgeIncomingDto(
                id=uuid.uuid4(),
                tail_id=mappings.node[edge.tail_id],
                head_id=mappings.node[edge.head_id],
                project_id=new_project_id,
            )
            for edge in original_project.edges
        ]

        if duplicate_edge_dtos:
            await EdgeService().create(session, duplicate_edge_dtos)
