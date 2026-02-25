from fastapi import APIRouter, Depends, HTTPException

from sqlalchemy.ext.asyncio import AsyncSession
from src.dtos.project_dtos import ProjectOutgoingDto
from src.dtos.project_import_dtos import ProjectImportDto
from src.services.project_import_service import ProjectImportService
from src.dependencies import get_project_import_service
from src.services.user_service import get_current_user
from src.dtos.user_dtos import UserIncomingDto
from src.dependencies import get_db
from src.constants import SessionInfoParameters

router = APIRouter(tags=["project-import"])


@router.post("/project/import")
async def import_project_with_duplication(
    dtos: list[ProjectImportDto],
    project_import_service: ProjectImportService = Depends(get_project_import_service),
    session: AsyncSession = Depends(get_db),
    current_user: UserIncomingDto = Depends(get_current_user),
) -> list[ProjectOutgoingDto]:
    """s
    Endpoint for importing Projects using full duplication service logic.
    Recommended for complex scenarios with discrete probabilities and utilities.
    """
    try:
        session.info[SessionInfoParameters.IS_EVENT_DISABLED.value] = True
        result = await project_import_service.import_from_json_with_duplication_logic(
            session, dtos, current_user
        )
        await session.commit()
        return result
    except Exception as e:
        await session.rollback()
        raise HTTPException(status_code=500, detail=str(e))
