import asyncio
import uuid
from datetime import datetime
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.orm import InstrumentedAttribute
from sqlalchemy.sql import ColumnElement, Select, select, desc
from sqlalchemy.orm.strategy_options import _AbstractLoad  # type: ignore
from typing import (
    Type,
    TypeVar,
    Generic,
    List,
    Protocol,
    Callable,
    Union,
    Optional,
    Tuple,
    cast,
    Coroutine,
    Any
)
from odata_query.sqlalchemy.shorthand import apply_odata_query
from src.constants import PageSize
from src.models import (
    Uncertainty,
    Utility,
    Decision,
    Node,
    NodeStyle,
    StrategyOption,
    Strategy,
    Outcome,
    Option,
    DiscreteProbability,
    ProjectRole,
)

LoadOptions = List[_AbstractLoad]


class AlchemyModel(Protocol):
    id: InstrumentedAttribute[Union[int, uuid.UUID]]
    created_at: InstrumentedAttribute[datetime]
    updated_at: InstrumentedAttribute[datetime]


T = TypeVar("T", bound=AlchemyModel)
K = TypeVar("K", bound=AlchemyModel)
IDType = TypeVar("IDType", int, uuid.UUID)


class BaseRepository(Generic[T, IDType]):
    def __init__(
        self,
        session: AsyncSession,
        model: Type[T],
        query_extension_method: Callable[[], LoadOptions],
    ):
        self.session = session
        self.model = model
        self.query_extension_method = query_extension_method

    async def create(self, entities: List[T]) -> List[T]:
        self.session.add_all(entities)
        await self.session.flush()
        return entities

    async def create_single(self, entity: T) -> T:
        self.session.add(entity)
        await self.session.flush()
        return entity

    async def get(self, ids: List[IDType]) -> List[T]:
        query = (
            select(self.model).where(self.model.id.in_(ids)).options(*self.query_extension_method())
        )
        return list((await self.session.scalars(query)).unique().all())

    async def get_all(
        self,
        model_filter: List[ColumnElement[bool]] = [],
        odata_query: Optional[str] = None,
        skip: int = 0,
        take: int = PageSize.DEFAULT,
    ) -> List[T]:
        query = select(self.model).options(*self.query_extension_method())
        if len(model_filter) != 0:
            query = query.filter(*model_filter)
        if odata_query is not None:
            query = cast(Select[Tuple[T]], apply_odata_query(query, odata_query))
        query = query.order_by(desc(self.model.created_at)).offset(skip).limit(take)
        return list((await self.session.scalars(query)).unique().all())

    async def delete(self, ids: List[IDType]) -> None:
        entities = await self.get(ids)
        for entity in entities:
            await self.session.delete(entity)

        await self.session.flush()

    async def update(self, entities: List[T]) -> List[T]:
        raise NotImplementedError("Subclasses must implement update_entity method.")

    @staticmethod
    def _check_update_entity_ids_match(entity_lists: List[List[T]]):
        """
        For update operations: checks that all entity lists have the same ids in the same order.
        Raises ValueError if not. Only intended for use in update methods.
        """
        if len(entity_lists) < 2:
            return
        ids_list = [[entity.id for entity in entity_list] for entity_list in entity_lists]
        if not all(ids == ids_list[0] for ids in ids_list):
            raise ValueError(
                "Entity lists do not have matching ids. Possible missing or extra entities during update."
            )

    @classmethod
    def prepare_entities_for_update(cls, entity_lists: List[List[T]]):
        """
        For update operations: sorts all entity lists by id and checks that their ids match.
        Only intended for use in update methods.
        """
        cls.sort_entity_collections_by_id(entity_lists)
        cls._check_update_entity_ids_match(entity_lists)

    @staticmethod
    def sort_entity_collections_by_id(entity_lists: List[List[T]]):
        for entity_list in entity_lists:
            entity_list.sort(key=lambda entity: entity.id)

    async def _update_entities(
            self, 
            incoming_entities: List[K], 
            existing_entities: List[K],
            update_method: Callable[[K, K], Coroutine[Any, Any, K]]
        ) -> List[K]:
        """
        Updates existing_entities to match incoming_entities by:
        - Updating entities that exist in both lists (matched by id)
        - Creating new entities from incoming that don't exist in existing
        - Deleting entities from existing that aren't in incoming
        """
        incoming_by_id = {entity.id: entity for entity in incoming_entities}
        existing_by_id = {entity.id: entity for entity in existing_entities}

        # Update existing entities that are in incoming
        common_ids = set(existing_by_id.keys()) & set(incoming_by_id.keys())
        await asyncio.gather(
            *[
                update_method(incoming_by_id[entity_id], existing_by_id[entity_id])
                for entity_id in common_ids
            ]
        )

        # Delete entities that are in existing but not in incoming
        entities_to_delete = [
            entity for entity in existing_entities if entity.id not in incoming_by_id
        ]
        if entities_to_delete:
            # delete discrete probabilities using ORM to trigger cascade delete operation
            entity_ids_to_delete = [entity.id for entity in entities_to_delete]
            if all([isinstance(x, Outcome) for x in entities_to_delete]):
                discrete_probs = await self.session.scalars(
                    select(DiscreteProbability).where(
                        DiscreteProbability.outcome_id.in_(entity_ids_to_delete)
                    )
                )
                await asyncio.gather(*[self.session.delete(dp) for dp in discrete_probs])

            # Remove entities from existing_entities list and delete from session
            [existing_entities.remove(entity) for entity in entities_to_delete]
            await asyncio.gather(*[self.session.delete(entity) for entity in entities_to_delete])

        # Create new entities that are in incoming but not in existing
        new_entities = [
            entity for entity in incoming_entities if entity.id not in existing_by_id
        ]
        if new_entities:
            existing_entities.extend(new_entities)
            [self.session.add(entity) for entity in new_entities]
            if all([isinstance(x, ProjectRole) for x in new_entities]):
                [self.session.expunge(role.user) for role in new_entities]

        return existing_entities

    async def _update_uncertainty(
        self, incoming_entity: Uncertainty, existing_entity: Uncertainty
    ) -> Uncertainty:
        """
        Selective update of an existing Uncertainty entity with data from an incoming Uncertainty entity.
        """

        existing_entity.is_key = incoming_entity.is_key
        if incoming_entity.issue_id:
            existing_entity.issue_id = incoming_entity.issue_id

        if existing_entity.outcomes != incoming_entity.outcomes:
            existing_entity.outcomes = await self._update_outcomes(
                incoming_entity.outcomes, existing_entity.outcomes
            )

        # Create a map of incoming discrete probabilities by ID for efficient lookup
        incoming_dps_by_id = {dp.id: dp for dp in incoming_entity.discrete_probabilities}
        if (
            existing_entity.discrete_probabilities.__len__()
            != incoming_entity.discrete_probabilities.__len__()
        ):
            existing_entity.discrete_probabilities = incoming_entity.discrete_probabilities
        else:
            for existing_dp in existing_entity.discrete_probabilities:
                if existing_dp.id in incoming_dps_by_id:
                    incoming_dp = incoming_dps_by_id[existing_dp.id]
                    # Only update the probability field
                    if existing_dp.probability != incoming_dp.probability:
                        existing_dp.probability = incoming_dp.probability
            # If no match, ignore and leave existing discrete probability unchanged

        return existing_entity

    async def _update_utility(self, incoming_entity: Utility, existing_entity: Utility) -> Utility:
        """
        Selective update of an existing Utility entity with data from an incoming Utility entity.
        """

        if incoming_entity.issue_id:
            existing_entity.issue_id = incoming_entity.issue_id

        # Create a map of incoming discrete utilities by ID for efficient lookup
        incoming_dps_by_id = {dp.id: dp for dp in incoming_entity.discrete_utilities}

        for existing_dp in existing_entity.discrete_utilities:
            if existing_dp.id in incoming_dps_by_id:
                incoming_dp = incoming_dps_by_id[existing_dp.id]
                # Only update the utility_value field
                if existing_dp.utility_value != incoming_dp.utility_value:
                    existing_dp.utility_value = incoming_dp.utility_value
            # If no match, ignore and leave existing discrete utility unchanged

        return existing_entity

    async def _update_outcome(self, incoming_entity: Outcome, existing_entity: Outcome) -> Outcome:
        existing_entity.name = incoming_entity.name
        existing_entity.utility = incoming_entity.utility
        return existing_entity

    @staticmethod
    async def _update_option(incoming_entity: Option, existing_entity: Option) -> Option:
        existing_entity.name = incoming_entity.name
        existing_entity.utility = incoming_entity.utility
        return existing_entity

    async def _update_outcomes(
        self, incoming_entities: List[Outcome], existing_entities: List[Outcome]
    ):
        """
        Updates existing_entities to match incoming_entities by:
        - Updating outcomes that exist in both lists (matched by id)
        - Creating new outcomes from incoming that don't exist in existing
        - Deleting outcomes from existing that aren't in incoming
        """
        return await self._update_entities(incoming_entities, existing_entities, self._update_outcome)

    async def _update_options(
        self, incoming_entities: List[Option], existing_entities: List[Option]
    ):
        """
        Updates existing_entities to match incoming_entities by:
        - Updating options that exist in both lists (matched by id)
        - Creating new options from incoming that don't exist in existing
        - Deleting options from existing that aren't in incoming
        """
        return await self._update_entities(incoming_entities, existing_entities, self._update_option)

    async def _update_decision(
        self, incoming_entity: Decision, existing_entity: Decision
    ) -> Decision:
        existing_entity.type = incoming_entity.type
        if existing_entity.options != incoming_entity.options:
            existing_entity.options = await self._update_options(
                incoming_entity.options, existing_entity.options
            )
        return existing_entity
    
    async def _replace_strategy_options(self, strategy_to_update: Strategy, new_strategy_options: list[StrategyOption]) -> None:
        """
        Safely replace strategy options by managing only the StrategyOption join table relationships.
        This approach prevents any updates to Option or Strategy entities themselves.
        """
        strategy_to_update.strategy_options.clear()
        
        for new_strategy_option in new_strategy_options:
            strategy_option_to_add = StrategyOption(
                strategy_id=strategy_to_update.id,
                option_id=new_strategy_option.option_id
            )
            self.session.add(strategy_option_to_add)
            strategy_to_update.strategy_options.append(strategy_option_to_add)
    
    async def _update_strategy(self, incoming_entity: Strategy, existing_entity: Strategy):
        existing_entity.project_id = incoming_entity.project_id
        existing_entity.name = incoming_entity.name
        existing_entity.description = incoming_entity.description
        existing_entity.rationale = incoming_entity.rationale
        existing_entity.updated_by_id = incoming_entity.updated_by_id

        # Clear existing strategy options and create new ones
        # This ensures we only manage the relationship without updating Option or Strategy entities
        await self._replace_strategy_options(existing_entity, incoming_entity.strategy_options)
    
    async def _update_strategies(
        self, incoming_entities: list[Strategy], existing_entities: list[Strategy]
    ):
        """
        Updates existing_entities to match incoming_entities by:
        - Updating strategies that exist in both lists (matched by id)
        - Creating new strategies from incoming that don't exist in existing
        - Deleting strategies from existing that aren't in incoming
        """

        return await self._update_entities(incoming_entities, existing_entities, self._update_strategy)

    def _update_node(self, incoming_entity: Node, existing_entity: Node):
        existing_entity.name = incoming_entity.name
        if incoming_entity.node_style and (
            existing_entity.node_style != incoming_entity.node_style
        ):
            existing_entity.node_style = self._update_node_style(
                incoming_entity.node_style, existing_entity.node_style
            )
        return existing_entity
    
    async def _update_project_role(self, incoming_entity: ProjectRole, existing_entity: ProjectRole):
        existing_entity.role = incoming_entity.role
        existing_entity.user_id = incoming_entity.user_id
        if incoming_entity.user:
            try:
                self.session.expunge(incoming_entity.user)
            except:
                pass
        return existing_entity
    
    async def _update_project_roles(
        self, incoming_entities: list[ProjectRole], existing_entities: list[ProjectRole]
    ):
        """
        Updates existing_entities to match incoming_entities by:
        - Updating project roles that exist in both lists (matched by id)
        - Creating new project roles from incoming that don't exist in existing
        - Deleting project roles from existing that aren't in incoming
        """
        return await self._update_entities(incoming_entities, existing_entities, self._update_project_role)

    @staticmethod
    def _update_node_style(incoming_entity: NodeStyle, existing_entity: NodeStyle):
        existing_entity.x_position = incoming_entity.x_position
        existing_entity.y_position = incoming_entity.y_position
        return existing_entity
