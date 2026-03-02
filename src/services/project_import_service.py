import uuid
from sqlalchemy.ext.asyncio import AsyncSession
from src.dtos.project_import_dtos import ProjectImportDto
from src.services.project_duplication_service import ProjectDuplicationService, IdMappings
from src.dtos.project_dtos import ProjectOutgoingDto
from src.dtos.user_dtos import UserIncomingDto


class ProjectImportService:
    """Service for importing projects from JSON data using project duplication logic."""

    async def import_from_json_with_duplication_logic(
        self,
        session: AsyncSession,
        dtos: list[ProjectImportDto],
        user_dto: UserIncomingDto,
    ) -> list[ProjectOutgoingDto]:
        """
        Import projects using the full duplication service logic for complex scenarios
        with discrete probabilities and utilities that require parent ID mapping.
        """
        if not dtos:
            return []

        duplication_service = ProjectDuplicationService()
        duplication_service.mappings = IdMappings()

        created_projects: list[ProjectOutgoingDto] = []
        # Map old project ID to new project ID
        for dto in dtos:
            project_id = uuid.uuid4()
            duplication_service.mappings.project[
                dto.projects.id if hasattr(dto.projects, "id") else uuid.uuid4()
            ] = project_id

            # Create project using duplication service
            created_project = await duplication_service.duplicate_project(
                session=session,
                original_project=dto.projects,
                current_user=user_dto,
                new_project_id=project_id,
            )
            duplication_service.generate_id_mappings(dto.issues)
            await duplication_service.duplicate_issues(
                session=session, issues=dto.issues, current_user=user_dto
            )
            await duplication_service.duplicate_strategies(
                session=session,
                strategies=dto.projects.strategies,
                new_project_id=project_id,
                user_dto=user_dto,
            )
            await duplication_service.duplicate_edges(
                session=session, original_edges=dto.edges, new_project_id=project_id
            )
            if not created_project:
                return []
            created_projects.append(created_project)

        return created_projects
