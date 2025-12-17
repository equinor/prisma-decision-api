import asyncio
import uuid
from typing import Optional
from sqlalchemy.ext.asyncio import AsyncSession
from src.services.issue_service import IssueService
from src.dtos.discrete_utility_dtos import DiscreteUtilityIncomingDto
from src.dtos.utility_dtos import UtilityIncomingDto
from src.dtos.node_style_dtos import NodeStyleIncomingDto
from src.domain.influence_diagram import InfluenceDiagramDOT
from src.dtos.edge_dtos import EdgeMapper, EdgeOutgoingDto
from src.dtos.issue_dtos import IssueIncomingDto, IssueMapper, IssueOutgoingDto
from src.models.filters.edge_filter import EdgeFilter
from src.models.filters.issues_filter import IssueFilter
from src.repositories.edge_repository import EdgeRepository
from src.repositories.issue_repository import IssueRepository
from src.constants import Boundary, DecisionHierarchy, ProjectRoleType, Type
from src.dtos.project_roles_dtos import ProjectRoleCreateDto, ProjectRoleMapper
from src.models.project_role import ProjectRole
from src.repositories.project_role_repository import ProjectRoleRepository
from src.models import (
    Project,
)
from src.dtos.project_dtos import (
    ProjectMapper,
    ProjectIncomingDto,
    ProjectOutgoingDto,
    ProjectCreateDto,
    PopulatedProjectDto,
)
from src.dtos.user_dtos import (
    UserMapper,
    UserIncomingDto,
)
from src.repositories.project_repository import ProjectRepository
from src.repositories.user_repository import UserRepository
from src.models.filters.project_filter import ProjectFilter


class ProjectService:

    async def _create_role_for_project(
        self,
        session: AsyncSession,
        project_role_dtos: list[ProjectRoleCreateDto],
    ):
        # Ensure this method is always called within an async session context
        project_user_roles = await ProjectRoleRepository(session).create(
            ProjectRoleMapper.from_create_via_project_to_entities(project_role_dtos)
        )
        project_user_role = await ProjectRoleRepository(session).get(
            [role.id for role in project_user_roles]
        )
        return project_user_role

    async def create(
        self,
        session: AsyncSession,
        dtos: list[ProjectCreateDto],
        user_dto: UserIncomingDto,
    ) -> list[ProjectOutgoingDto]:
        user = await UserRepository(session).get_or_create(UserMapper.to_entity(user_dto))
        for dto in dtos:
            owner_role = ProjectRoleCreateDto(
                user_name=user.name,
                azure_id=user.azure_id,
                user_id=user.id,
                project_id=dto.id,
                role=ProjectRoleType.FACILITATOR,
            )
            dto.users.append(owner_role)

        project_entities: list[Project] = await ProjectRepository(session).create(
            ProjectMapper.from_create_to_project_entities(dtos, user.id)
        )
        for project_entity, dto in zip(project_entities, dtos):
            if len(dto.users) > 0:
                project_role_entities: list[ProjectRole] = await self._create_role_for_project(
                    session, dto.users
                )
                project_entity.project_role = project_role_entities

        result: list[ProjectOutgoingDto] = ProjectMapper.to_outgoing_dtos(project_entities)
        return result

    async def update(
        self,
        session: AsyncSession,
        dtos: list[ProjectIncomingDto],
        user_dto: UserIncomingDto,
    ) -> list[ProjectOutgoingDto]:
        user = await UserRepository(session).get_or_create(UserMapper.to_entity(user_dto))
        entities_project: list[Project] = await ProjectRepository(session).update(
            ProjectMapper.to_project_entities(dtos, user.id)
        )
        result: list[ProjectOutgoingDto] = ProjectMapper.to_outgoing_dtos(entities_project)
        return result

    async def delete(
        self,
        session: AsyncSession,
        ids: list[uuid.UUID],
        user_dto: UserIncomingDto,
    ) -> None:
        user = await UserRepository(session).get_by_azure_id(azure_id=user_dto.azure_id)
        if user is None or len(user.project_role) == 0:
            return
        ids_to_delete = [
            project_role.project_id
            for project_role in user.project_role
            if (
                project_role.role == ProjectRoleType.FACILITATOR
                or project_role.role == ProjectRoleType.DECISIONMAKER
            )
            and project_role.project_id in ids
        ]
        await ProjectRepository(session).delete(ids=ids_to_delete)

    async def get(self, session: AsyncSession, ids: list[uuid.UUID]) -> list[ProjectOutgoingDto]:
        if not ids:
            return []
        projects: list[Project] = await ProjectRepository(session).get(ids)
        result = ProjectMapper.to_outgoing_dtos(projects)
        return result

    async def get_all(
        self,
        session: AsyncSession,
        user_dto: UserIncomingDto,
        filter: Optional[ProjectFilter] = None,
        odata_query: Optional[str] = None,
    ) -> list[ProjectOutgoingDto]:
        user = await UserRepository(session).get_or_create(UserMapper.to_entity(user_dto))
        if not user:
            return []
        if filter is None:
            filter = ProjectFilter()

        filter.accessing_user_id = user.id
        project_access_filter = filter.construct_access_conditions()

        # Construct model filters
        model_filter = filter.construct_filters() if filter else []
        model_filter.append(project_access_filter)
        projects: list[Project] = await ProjectRepository(session).get_all(
            model_filter=model_filter, odata_query=odata_query
        )
        result = ProjectMapper.to_outgoing_dtos(projects)
        return result

    async def get_populated_projects(
        self, session: AsyncSession, ids: list[uuid.UUID]
    ) -> list[PopulatedProjectDto]:
        projects: list[Project] = await ProjectRepository(session).get(ids)
        result = ProjectMapper.to_populated_dtos(projects)
        return result

    async def get_all_populated_projects(
        self,
        session: AsyncSession,
        filter: Optional[ProjectFilter] = None,
        odata_query: Optional[str] = None,
    ) -> list[PopulatedProjectDto]:
        model_filter = filter.construct_filters() if filter else []
        projects: list[Project] = await ProjectRepository(session).get_all(
            model_filter=model_filter, odata_query=odata_query
        )
        result = ProjectMapper.to_populated_dtos(projects)
        return result

    async def project_dupilcation(
        self, session: AsyncSession, id: uuid.UUID, current_user: UserIncomingDto
    ) -> ProjectOutgoingDto | None:
        project = await ProjectRepository(session).get([id])
        if not project:
            return None
        # Get original project
        original_project = project[0]

        # Create duplicate project with new ID
        new_project_id = uuid.uuid4()
        duplicate_project_dto = ProjectCreateDto(
            id=new_project_id,
            name=original_project.name,
            parent_project_id=project[0].id,
            opportunityStatement=original_project.opportunityStatement,
            public=original_project.public,
            end_date=original_project.end_date,
            users=[],
        )

        # Duplicate project roles
        for role in original_project.project_role:
            duplicate_role = ProjectRoleCreateDto(
                user_name="",
                azure_id="",
                user_id=role.user_id,
                project_id=new_project_id,
                role=ProjectRoleType(role.role),
            )
            duplicate_project_dto.users.append(duplicate_role)

        # Create the duplicated project with roles
        await self.create(
            session,
            [duplicate_project_dto],
            UserIncomingDto(id=None, name=current_user.name, azure_id=current_user.azure_id),
        )
        # Create ID mapping for relationships
        id_mapping: dict[uuid.UUID, uuid.UUID] = {}
        outcome_id_mapping: dict[uuid.UUID, uuid.UUID] = {}
        option_id_mapping: dict[uuid.UUID, uuid.UUID] = {}

        # Duplicate issues with proper DTO conversion
        duplicate_project_issue_dtos: list[IssueIncomingDto] = []

        for issue in original_project.issues:
            new_issue_id = uuid.uuid4()
            new_node_id = uuid.uuid4()
            id_mapping[issue.id] = new_issue_id
            if issue.node:
                id_mapping[issue.node.id] = new_node_id

            # Convert decision to DTO if exists
            decision_dto = None
            if issue.decision:
                from src.dtos.decision_dtos import DecisionIncomingDto
                from src.dtos.option_dtos import OptionIncomingDto

                new_decision_id = uuid.uuid4()
                options_dtos: list[OptionIncomingDto] = []
                for option in issue.decision.options:
                    new_option_id = uuid.uuid4()
                    option_id_mapping[option.id] = new_option_id
                    option_dto = OptionIncomingDto(
                        id=new_option_id,
                        decision_id=new_decision_id,
                        name=option.name,
                        utility=option.utility,
                    )
                    options_dtos.append(option_dto)

                decision_dto = DecisionIncomingDto(
                    id=new_decision_id,
                    issue_id=new_issue_id,
                    type=DecisionHierarchy(issue.decision.type),
                    options=options_dtos,
                )
            # Convert uncertainty to DTO if exists
            uncertainty_dto = None
            if issue.uncertainty:
                from src.dtos.uncertainty_dtos import UncertaintyIncomingDto
                from src.dtos.outcome_dtos import OutcomeIncomingDto
                from src.dtos.discrete_probability_dtos import DiscreteProbabilityIncomingDto

                new_uncertainty_id = uuid.uuid4()
                outcomes_dtos: list[OutcomeIncomingDto] = []
                discrete_probability_dtos: list[DiscreteProbabilityIncomingDto] = []
                for outcome in issue.uncertainty.outcomes:
                    new_outcome_id = uuid.uuid4()
                    outcome_id_mapping[outcome.id] = new_outcome_id
                    outcomes_dto = OutcomeIncomingDto(
                        id=new_outcome_id,
                        name=outcome.name,
                        uncertainty_id=new_uncertainty_id,
                        utility=outcome.utility,
                    )
                    outcomes_dtos.append(outcomes_dto)

                for discrete_probability in issue.uncertainty.discrete_probabilities:
                    discrete_probability_dto = DiscreteProbabilityIncomingDto(
                        id=uuid.uuid4(),
                        uncertainty_id=new_uncertainty_id,
                        outcome_id=outcome_id_mapping.get(
                            discrete_probability.outcome_id, discrete_probability.outcome_id
                        ),
                        probability=discrete_probability.probability,
                        parent_outcome_ids=[
                            outcome_id_mapping.get(
                                parent_outcome.parent_outcome_id, parent_outcome.parent_outcome_id
                            )
                            for parent_outcome in discrete_probability.parent_outcomes
                        ],
                        parent_option_ids=[
                            option_id_mapping.get(
                                parent_option.parent_option_id, parent_option.parent_option_id
                            )
                            for parent_option in discrete_probability.parent_options
                        ],
                    )

                    discrete_probability_dtos.append(discrete_probability_dto)

                uncertainty_dto = UncertaintyIncomingDto(
                    id=new_uncertainty_id,
                    issue_id=new_issue_id,
                    is_key=issue.uncertainty.is_key,
                    outcomes=outcomes_dtos,
                    discrete_probabilities=discrete_probability_dtos,
                )
                # Convert node to DTO with node_style
            from src.dtos.node_dtos import NodeIncomingDto

            # Create node style DTO (copy from original if exists)
            node_style_dto = None
            node_dto = None
            if issue.node and hasattr(issue.node, "node_style") and issue.node.node_style:
                node_style_dto = NodeStyleIncomingDto(
                    node_id=new_node_id,
                    x_position=issue.node.node_style.x_position,
                    y_position=issue.node.node_style.y_position,
                )

                node_dto = NodeIncomingDto(
                    id=new_node_id,
                    project_id=new_project_id,
                    issue_id=new_issue_id,
                    name=issue.node.name if issue.node else issue.name,
                    node_style=node_style_dto,  # Add the missing field
                )

            utility_dto = None
            if issue.utility:
                new_utility_id = uuid.uuid4()
                discrete_utility_dtos: list[DiscreteUtilityIncomingDto] = []
                for discrete_utility in issue.utility.discrete_utilities:
                    discrete_utility_dto = DiscreteUtilityIncomingDto(
                        id=uuid.uuid4(),
                        utility_id=new_utility_id,
                        utility_value=discrete_utility.utility_value,
                        parent_option_ids=[
                            option_id_mapping.get(
                                parent_option.parent_option_id, parent_option.parent_option_id
                            )
                            for parent_option in discrete_utility.parent_options
                        ],
                        parent_outcome_ids=[
                            outcome_id_mapping.get(
                                parent_outcome.parent_outcome_id, parent_outcome.parent_outcome_id
                            )
                            for parent_outcome in discrete_utility.parent_outcomes
                        ],
                        value_metric_id=discrete_utility.value_metric_id,
                    )
                    discrete_utility_dtos.append(discrete_utility_dto)
                utility_dto = UtilityIncomingDto(
                    id=new_utility_id,
                    issue_id=new_issue_id,
                    discrete_utilities=discrete_utility_dtos,
                )
            duplicate_project_issue = IssueIncomingDto(
                id=uuid.uuid4(),
                project_id=new_project_id,
                name=issue.name,
                description=issue.description,
                order=issue.order,
                type=Type(issue.type),
                boundary=Boundary(issue.boundary),
                node=node_dto,
                decision=decision_dto,
                uncertainty=uncertainty_dto,
                utility=utility_dto,
            )
            duplicate_project_issue_dtos.append(duplicate_project_issue)

        await IssueService().create(
            session,
            duplicate_project_issue_dtos,
            UserIncomingDto(id=None, name=current_user.name, azure_id=current_user.azure_id),
        )
        # Duplicate edges with updated node references
        from src.dtos.edge_dtos import EdgeIncomingDto

        duplicate_edge_dtos: list[EdgeIncomingDto] = []

        for edge in original_project.edges:
            # Map old node IDs to new node IDs
            new_tail_id = id_mapping.get(edge.tail_id)
            new_head_id = id_mapping.get(edge.head_id)

            if new_tail_id and new_head_id:
                edge_dto = EdgeIncomingDto(
                    id=uuid.uuid4(),
                    tail_id=new_tail_id,
                    head_id=new_head_id,
                    project_id=new_project_id,
                )
                duplicate_edge_dtos.append(edge_dto)
            # Create the duplicated edges (if you have EdgeService)
        from src.services.edge_service import EdgeService

        await EdgeService().create(session, duplicate_edge_dtos)

    async def get_influence_diagram_data(
        self, session: AsyncSession, project_id: uuid.UUID
    ) -> tuple[list[IssueOutgoingDto], list[EdgeOutgoingDto]]:
        issue_filter = IssueFilter(
            project_ids=[project_id],
            boundaries=[Boundary.ON.value, Boundary.IN.value],
            types=[Type.DECISION.value, Type.UNCERTAINTY.value],
            decision_types=[DecisionHierarchy.FOCUS.value],
            is_key_uncertainties=[True],
        )
        edge_filter = EdgeFilter(
            project_ids=[project_id],
            issue_boundaries=[Boundary.ON.value, Boundary.IN.value],
            issue_types=[Type.DECISION.value, Type.UNCERTAINTY.value],
            decision_types=[DecisionHierarchy.FOCUS.value],
            is_key_uncertainties=[True],
        )

        issues_entities = await IssueRepository(session).get_all(
            model_filter=issue_filter.construct_filters()
        )
        edges_entities = await EdgeRepository(session).get_all(
            model_filter=edge_filter.construct_filters()
        )

        issue_dtos = IssueMapper.to_outgoing_dtos(issues_entities)
        edge_dtos = EdgeMapper.to_outgoing_dtos(edges_entities)

        # Run influence diagram creation and validation in a separate thread
        influence_diagram = await asyncio.to_thread(
            lambda: InfluenceDiagramDOT(edge_dtos, issue_dtos)
        )
        await asyncio.to_thread(influence_diagram.validate_diagram)

        # Return the validated (potentially filtered) issues and edges
        return influence_diagram.issues, influence_diagram.edges
