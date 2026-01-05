import uuid
from sqlalchemy.orm import joinedload, Session
from sqlalchemy.sql import select 
from sqlalchemy.ext.asyncio import AsyncSession
from src.repositories.query_extensions import QueryExtensions
from src.repositories.base_repository import BaseRepository
from src.constants import Type
from src.models import (
    Node, 
    Issue,
)
from src.utils.session_info_handler import SessionInfo

class NodeRepository(BaseRepository[Node, uuid.UUID]):
    def __init__(self, session: AsyncSession):
        super().__init__(
            session,
            Node,
            query_extension_method=QueryExtensions.load_node_with_relationships,
        )

    async def update(self, entities: list[Node]) -> list[Node]:
        entities_to_update = await self.get([node.id for node in entities])
        # sort the entity lists to share the same order according to the entity.id
        self.prepare_entities_for_update([entities, entities_to_update])

        for n, entity_to_update in enumerate(entities_to_update):
            entity = entities[n]
            entity_to_update.scenario_id = entity.scenario_id
            if entity.issue_id:
                entity_to_update.issue_id = entity.issue_id
            if entity.node_style and (entity.node_style != entity_to_update.node_style):
                entity_to_update.node_style = self._update_node_style(entity.node_style, entity_to_update.node_style)

        await self.session.flush()
        return entities_to_update
    
    async def clear_discrete_probability_tables(self, ids: list[uuid.UUID]):
        
        entities = await self.get(ids)

        for entity in entities:
            if entity.issue.uncertainty is None: continue
            entity.issue.uncertainty.discrete_probabilities = []

        await self.session.flush()

def add_effected_session_entities(session: Session, ids: set[uuid.UUID]) -> SessionInfo:
    session_info = SessionInfo()
    query = select(Node).where(Node.id.in_(ids)).options(
        joinedload(Node.issue).options(
            joinedload(Issue.utility),
            joinedload(Issue.uncertainty),
        )
    )
    entities = list((session.scalars(query)).unique().all())

    for entity in entities:
        if (entity.issue.type == Type.UNCERTAINTY.value and entity.issue.uncertainty):
            session_info.affected_uncertainties.add(entity.issue.uncertainty.id)

        if (entity.issue.type == Type.UTILITY.value and entity.issue.utility):
            session_info.affected_utilities.add(entity.issue.utility.id)


    return session_info
