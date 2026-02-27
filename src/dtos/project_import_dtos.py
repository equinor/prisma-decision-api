from pydantic import BaseModel

from src.dtos.edge_dtos import EdgeIncomingDto
from src.dtos.issue_dtos import IssueIncomingDto
from src.dtos.project_dtos import ProjectIncomingDto


class ProjectImportDto(BaseModel):
    projects: ProjectIncomingDto
    issues: list[IssueIncomingDto] = []
    edges: list[EdgeIncomingDto] = []
