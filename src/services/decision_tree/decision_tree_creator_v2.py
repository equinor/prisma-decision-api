from __future__ import annotations

import logging
import uuid
import copy
import itertools
import time
import numpy as np
import networkx as nx
from typing import Optional, Dict, Any, Union, List, Tuple, Iterator, cast
from pydantic import Field
from fastapi import HTTPException
from src.constants import Type
from src.seed_database import GenerateUuid
from src.dtos.issue_dtos import IssueOutgoingDto
from src.dtos.edge_dtos import EdgeOutgoingDto
from src.dtos.option_dtos import OptionOutgoingDto
from src.dtos.outcome_dtos import OutcomeOutgoingDto
from src.dtos.decision_tree_dtos import (
    EdgeUUIDDto,
    EndPointNodeDto,
    DecisionTreeDto,
    DecisionTreeDto2,
    TreeNodeDto,
    TreeNodeDto2,
    ProbabilityDto2,
    UtilityDTDto2,
)
from src.dtos.discrete_probability_dtos import DiscreteProbabilityOutgoingDto
from src.services.decision_tree.decision_tree_utils import TreeNodeLookup, NodeTreeNodeLookup

logger = logging.getLogger(__name__)


class DecisionTreeGraph_v2:
    """Decision tree class"""

    def __init__(self, root: Optional[Any] = None, **kwargs: Dict[str, Any]) -> None:
        self.nx: nx.DiGraph = nx.DiGraph(**kwargs)  # type: ignore
        self.root: Optional[Any] = root
        if self.root is not None:
            self.nx.add_node(self.root)  # type: ignore
        self.edge_names: Dict[Tuple[uuid.UUID, uuid.UUID], str] = {}
        self.utility_lookup : Dict[tuple[str, ...], List[float]] = {}
        self.treenode_lookup : TreeNodeLookup
        self.node_treenode_lookup : NodeTreeNodeLookup
        self.final_expected_value : float = 0

    async def add_edge(self, edge: EdgeUUIDDto) -> None:
        self.nx.add_edge(edge.tail, edge.head, name=edge.name)  # type: ignore

    async def get_parent(self, node: uuid.UUID) -> Optional[uuid.UUID]:
        parents = list(self.nx.predecessors(node))  # type: ignore
        return parents[0] if (parents and len(parents) > 0) else None  # type: ignore

    async def populate_utility_lookup(self) -> None:
        # for treenode_id in self.nx.nodes: # type: ignore
        #   treenode_id = cast(uuid.UUID, treenode_id)
        for node_id in self.node_treenode_lookup.treenode_id_to_node_id.values(): # type: ignore
            node = self.node_treenode_lookup.get_node_dto(node_id)
            if node:
                print(node.type)
                if isinstance(node, EndPointNodeDto) or node.type != Type.UTILITY.value:
                    continue
                utility = node.utility
                if utility and utility.discrete_utilities:
                    for discrete_utility in utility.discrete_utilities:
                        if discrete_utility.utility_value:
                            parents = tuple(
                                sorted(p.__str__() for p in discrete_utility.parent_outcome_ids + discrete_utility.parent_option_ids)
                            )
                            self.utility_lookup.setdefault(parents, []).append(discrete_utility.utility_value)

    async def populate_treenode_lookup(self, lookup: Dict[str, TreeNodeDto]) -> None:
        self.treenode_lookup = TreeNodeLookup()
        for id, node in lookup.items():
            self.treenode_lookup.add_with_original_id(uuid.UUID(id), node)

    async def populate_node_treenode_lookup(self, lookup: NodeTreeNodeLookup) -> None:
        self.node_treenode_lookup = lookup

    async def get_dto_map(self):
        dto_map: Dict[uuid.UUID, TreeNodeDto2] = {}
        for treenode_id in self.nx.nodes: # type: ignore
            treenode_id = cast(uuid.UUID, treenode_id)
            if (node := self.node_treenode_lookup.get_dto_for_treenode_id(treenode_id)):
                type = Type.END.value if isinstance(node, EndPointNodeDto) else node.type
                dto = TreeNodeDto2(
                    id=treenode_id,
                    issue_id = node.id,
                    type = type,
                    probabilities = await self.get_probability_dtos(treenode_id, node),
                    utilities = await self.get_utility_dtos(treenode_id, node),
                    children=[],
                )
                dto_map[treenode_id] = dto

        for parent_id in self.nx.nodes:  # type: ignore
            parent_id = cast(uuid.UUID, parent_id)
            parent_dto = dto_map.get(parent_id)
            if parent_dto is not None:
                # Get child node ids (outgoing edges from parent)
                child_ids = list(self.nx.successors(parent_id))
                # Set children as list of DTOs
                #parent_dto.children = [child_id for child_id in child_ids if child_id in dto_map]
                parent_dto.children = [dto_map[child_id] for child_id in child_ids if child_id in dto_map]
       
        return dto_map

    async def topological_sort(self, dto_map: Dict[uuid.UUID, TreeNodeDto2]):
        visited = set()
        order = []

        def visit(node_id: uuid.UUID):
            if node_id in visited:
                return
            visited.add(node_id)
            for child in dto_map[node_id].children:
                visit(child.id)
            order.append(node_id)

        for node_id in dto_map:
            visit(node_id)
        return order
    
    async def to_issue_dtos(self) -> Optional[DecisionTreeDto2]:
        time1 = time.time()
        await self.populate_utility_lookup() # create lookup for discrete utilities
        self.edge_names = nx.get_edge_attributes(self.nx, "name") # type: ignore
        dto_map = await self.get_dto_map()


        #tg = nx.readwrite.json_graph.tree_data(self.nx, self.root)  # type: ignore
        #tree_structure = await self.create_decision_tree_dto_from_treenode(tg)  # type: ignore
        # calculate endpoint values after all tree_nodes have got their utility values


        #await self.calculate_endpointnode_values(tree_structure)
        await self.calculate_endpoint_nodes(dto_map, self.root)
        # calculate expected values for the tree_nodes
        #self.final_expected_value = await self.calculate_expected_values(tree_structure)
        self.final_expected_value = await self.compute_expected_values(dto_map, self.root)
        dto_map = await self.calculate_treenode_ids_from_branches(dto_map)
        
        root_id = await self.find_root_id(dto_map)
        response = await self.build_decision_tree_dto2_wrapped(root_id, dto_map)

        time2 = time.time()
        print("Elapsed time opt:", time2 - time1, "seconds")
        return response
        # return DecisionTreeDto2(dto_map[self.root])
        #return tree_structure
    
    async def calculate_treenode_ids_from_branches(self, dto_map: Dict[uuid.UUID, TreeNodeDto2]) -> Dict[uuid.UUID, TreeNodeDto2]:
        new_map: Dict[uuid.UUID, TreeNodeDto2] = {}
        for old_key, dto in dto_map.items():
            new_id = await self.create_treenode_id(old_key)
            dto.id = new_id  # Update the dto's id
            new_map[new_id] = dto
        # dto_map.clear()
        # dto_map.update(new_map)
        return new_map




    # async def serialize_tree_node_from_map(
    #     node_id: uuid.UUID,
    #     dto_map: Dict[uuid.UUID, TreeNodeDto2]
    # ) -> dict:
    #     node = dto_map[node_id]
    #     node_dict = node.dict(by_alias=True)
    #     # Recursively resolve and serialize children
    #     if node.children:
    #         node_dict["children"] = [
    #             {"tree_node": serialize_tree_node_from_map(child_id, dto_map)}
    #             for child_id in node.children
    #         ]
    #     else:
    #         node_dict["children"] = []
    #     return node_dict


    # async def build_tree_response_from_map(self, 
    #     root_id: uuid.UUID,
    #     dto_map: Dict[uuid.UUID, TreeNodeDto2]
    # ) -> dict:
    #     return {"tree_node": serialize_tree_node_from_map(root_id, dto_map)}
    async def serialize_tree_node_wrapped(self, 
        node_id: uuid.UUID,
        dto_map: Dict[uuid.UUID, TreeNodeDto2]
    ) -> dict:
        node = dto_map[node_id]
        node_dict = node.model_dump()
        # Recursively wrap children
        if node.children:
            node_dict["children"] = [
                {"tree_node": await self.serialize_tree_node_wrapped(child.id, dto_map)}
                for child in node.children
            ]
        else:
            node_dict["children"] = []
        return node_dict

    async def build_decision_tree_dto2_wrapped(self, 
        root_id: uuid.UUID,
        dto_map: Dict[uuid.UUID, TreeNodeDto2]
    ) -> DecisionTreeDto2:
    # The top-level "tree_node" key is handled here
        return DecisionTreeDto2(tree_node=await self.serialize_tree_node_wrapped(root_id, dto_map))


    async def find_root_id(self, dto_map: Dict[uuid.UUID, TreeNodeDto2]) -> uuid.UUID:
        print(dto_map)
        all_ids = set(dto_map.keys())
        # child_ids = set(
        #     child_id
        #     for node in dto_map.values() if node.children
        #     for child_id in node.children
        # )
        child_ids = set(
            child.id
            for node in dto_map.values() if node.children
            for child in node.children
        )
        root_ids = list(all_ids - child_ids)
        if not root_ids:
            raise ValueError("No root node found")
        return root_ids[0]  # or return all roots if you expect a forest





    async def compute_expected_values(self, dto_map, root_id):
        order = await self.topological_sort(dto_map)
        for node_id in order:
            node = dto_map[node_id]
            if node.type == Type.END.value:
                node.expected_value = node.endpoint_value
            elif node.type == Type.UNCERTAINTY.value:
                # child_values = np.array([child.expected_value for child in node.children])
                child_values = np.array([child.endpoint_value if child.type == Type.END.value else child.expected_value for child in node.children])
                probabilities = np.array([prob.probability_value for prob in node.probabilities])
                node.expected_value = np.dot(probabilities, child_values)
            elif node.type == Type.DECISION.value:
                child_values = np.array([child.expected_value for child in node.children])
                node.expected_value = np.max(child_values)
        return dto_map[root_id].expected_value


    # async def calculate_expected_values(self, decision_tree: DecisionTreeDto | None) -> float:
    #     if not decision_tree:
    #         return 0
    #     return await self.update_expected_values(decision_tree.tree_node)

    # async def update_expected_values(self, node: TreeNodeDto) -> float:
    #     if node.issue and isinstance(node.issue, EndPointNodeDto):
    #         return node.issue.value

        # # Loop through children recursively
        # if node.children:
        #     if node.issue.type == Type.UNCERTAINTY.value:
        #         child_nodes = [child.tree_node for child in node.children]
        #         values = np.array([await self.update_expected_values(child) for child in child_nodes])
        #         probabilities = np.array([await self.get_probability_value(node, child) for child in child_nodes])
        #         total_value = np.dot(probabilities, values)
        #         node.expected_value = total_value
        #         return total_value
        #     if node.issue.type == Type.DECISION.value:
        #         values = np.array([await self.update_expected_values(child.tree_node) for child in node.children])
        #         max_value = np.max(values)
        #         node.expected_value = max_value
        #         return max_value
        # return 0

    async def calculate_endpointnode_values(self, decision_tree: DecisionTreeDto | None) -> None:
        if not decision_tree:
            return
        await self.update_endpoint_nodes(decision_tree.tree_node)

    # async def update_endpoint_nodes(self, node: TreeNodeDto) -> None:
    #     if node.issue and isinstance(node.issue, EndPointNodeDto):
    #         node.issue.value, node.issue.cumulative_probability = await self.calculate_endpoint_value(node)
    #         return

    #     # Loop through children recursively
    #     if node.children:
    #         for child in node.children:
    #             await self.update_endpoint_nodes(child.tree_node)

    async def update_endpoint_nodes(self, root: TreeNodeDto) -> None:
        stack = [root]
        while stack:
            node = stack.pop()
            if node.issue and isinstance(node.issue, EndPointNodeDto):
                node.issue.value, node.issue.cumulative_probability = await self.calculate_endpoint_value(node)
            elif node.children:
                stack.extend([child.tree_node for child in node.children])



    async def calculate_endpoint_nodes(self, dto_map, root_id) -> None:
        stack = [dto_map[root_id]]
        while stack:
            node = stack.pop()
            if node.type == Type.END.value:
                node.endpoint_value, node.cumulative_probability = await self.calculate_endpoint_value(node.id, dto_map)
            elif node.children:
                stack.extend(node.children)          
            


    async def calculate_endpoint_value(self, id: uuid.UUID, dto_map: Dict[uuid.UUID, TreeNodeDto2]) -> Tuple[float, float]:
        if not id:
            return 0, 0

        parent_id = await self.get_parent(id)
        if not parent_id:
            return 0, 0
        count = 0
        node_value = 0
        cumulative_probability = 1
        branch_id = self.edge_names[(parent_id, id)]
        while parent_id and count < 1000:
            #parent_node = self.treenode_lookup.get_node_by_original_id(parent_id) if id else None
            parent_node = dto_map[parent_id]
            node_value += await self.get_utility_for_branch(parent_node, branch_id) if parent_node else 0


            if parent_node and parent_node.type == Type.UNCERTAINTY.value:
                cumulative_probability *= await self.get_probability_for_branch(parent_node, branch_id)
            id = parent_id
            parent_id = await self.get_parent(id)
            if parent_id:
                branch_id = self.edge_names[(parent_id, id)]
            count += 1
        return node_value, cumulative_probability
    
    async def get_utility_for_branch(
        self, node: TreeNodeDto2, branch_id: str
    ) -> float:
        if node.utilities:
            for utility in node.utilities:
                if ((utility.option_id is not None and utility.option_id.__str__() == branch_id) or
                    (utility.outcome_id is not None and utility.outcome_id.__str__() == branch_id)):
                    return utility.utility_value
        return 0
    
    
    async def get_probability_value(
            self, parent_node: TreeNodeDto2, child_node: TreeNodeDto2
    ) -> float:
        probability = 0
        id = self.treenode_lookup.get_original_id(child_node.id)
        if id:
            parent_id = await self.get_parent(id)
            if parent_id:
                branch_id = self.edge_names[(parent_id, id)]
                probability = await self.get_probability_for_branch(parent_node, branch_id)
        return probability

    
    async def get_probability_for_branch(
        self, node: TreeNodeDto2, branch_id: str
    ) -> float:
        if node.probabilities:
            for probability in node.probabilities:
                if (probability.outcome_id.__str__() == branch_id):
                    return probability.probability_value
        return 0
    

    async def get_decision_tree_dto(
        self, issue: TreeNodeDto, children: list[DecisionTreeDto] | None = None
    ) -> DecisionTreeDto:
        issue.children = children
        return DecisionTreeDto(tree_node=issue)

    async def create_decision_tree_dto_from_treenode(
        self, tree_data: Dict[str, Any]
    ) -> Optional[DecisionTreeDto]:
        # Base case: if the tree data is empty, return None
        if not tree_data:
            return None

        # Create the DTO for the current node
        node = self.treenode_lookup.get_node_by_original_id(tree_data["id"])

        if node is None:
            return None

        # Recursively create DTOs for child nodes
        children_dtos: list[DecisionTreeDto] = []
        for child in tree_data.get("children", []):
            child_dto = await self.create_decision_tree_dto_from_treenode(child)
            if child_dto:
                children_dtos.append(child_dto)

        copy_node = copy.deepcopy(node)
        copy_node.probabilities = await self.get_probabiltiy_dtos(copy_node)
        copy_node.utilities = await self.get_utility_values(copy_node)
        original_id = copy_node.id
        copy_node.id = await self.create_treenode_id(copy_node)

        self.treenode_lookup.set_updated_node(original_id, copy_node)
        copy_node.children = children_dtos if children_dtos else None
        return DecisionTreeDto(tree_node=copy_node)

    async def create_treenode_id(self, treenode_id: uuid.UUID) -> uuid.UUID:
        id_string = ""
        # treenode_id = node.id

        parent_id = await self.get_parent(treenode_id)
        count = 0
        while parent_id and count < 1000:
            n = self.edge_names[(parent_id, treenode_id)]
            id_string = n if id_string == "" else n + " - " + id_string
            treenode_id = parent_id
            parent_id = await self.get_parent(treenode_id)
            count += 1

        id_string = "root" if id_string == "" else "root" + " - " + id_string
        return GenerateUuid.as_uuid(id_string)
    

    async def create_treenode_id_old(self, node: TreeNodeDto) -> uuid.UUID:
        id_string = ""
        treenode_id = node.id

        parent_id = await self.get_parent(treenode_id)
        count = 0
        while parent_id and count < 1000:
            n = self.edge_names[(parent_id, treenode_id)]
            id_string = n if id_string == "" else n + " - " + id_string
            treenode_id = parent_id
            parent_id = await self.get_parent(treenode_id)
            count += 1

        id_string = "root" if id_string == "" else "root" + " - " + id_string
        return GenerateUuid.as_uuid(id_string)

    async def find_matching_probabilities_dtos(
        self, object_uuids: list[uuid.UUID], in_dtos: list[DiscreteProbabilityOutgoingDto]
    ):
        out_dtos: list[DiscreteProbabilityOutgoingDto] = []
        for dto in in_dtos:
            combined_set = set(dto.parent_option_ids).union(set(dto.parent_outcome_ids))
            if set(combined_set).issubset(set(object_uuids)):
                out_dtos.append(dto)
        return out_dtos

    async def get_probability_dtos(
        self, treenode_id : uuid.UUID, node: IssueOutgoingDto | EndPointNodeDto
    ) -> Optional[list[ProbabilityDto2]]:
        
        probability_dtos : list[ProbabilityDto2] = []
        if (isinstance(node, EndPointNodeDto)):
            return probability_dtos

        if (node.type == Type.UNCERTAINTY.value
            and node.uncertainty is not None
            and len(node.uncertainty.discrete_probabilities) > 0
        ):
            parent_labels: list[uuid.UUID] = []
            parent_id = await self.get_parent(treenode_id)
            count = 0
            while parent_id and count < 1000:
                parent_labels.append(uuid.UUID(self.edge_names[(parent_id, treenode_id)]))
                treenode_id = parent_id
                parent_id = await self.get_parent(treenode_id)
                count += 1

            discrete_prob_dtos = await self.find_matching_probabilities_dtos(
                parent_labels, node.uncertainty.discrete_probabilities
            )

            for dto in discrete_prob_dtos:
                if dto.probability is not None:
                    probability_dto = ProbabilityDto2(
                        outcome_id=dto.outcome_id,
                        probability_value=dto.probability,
                    )
                    probability_dtos.append(probability_dto)

        return probability_dtos

    async def get_utility_dtos(
        self, treenode_id: uuid.UUID, node: IssueOutgoingDto | EndPointNodeDto
    ) -> Optional[list[UtilityDTDto2]]:
    
        utility_dtos : list[UtilityDTDto2] = []
        if (isinstance(node, EndPointNodeDto)):
            return utility_dtos

        if node.type == Type.UNCERTAINTY.value and node.uncertainty is not None:
            outcomes = node.uncertainty.outcomes
            for outcome in outcomes:
                discrete_utility_value = await self.get_discrete_utility_value(treenode_id, outcome)
                utility_dto = UtilityDTDto2(outcome_id=outcome.id,
                                           utility_value=outcome.utility + discrete_utility_value)
                utility_dtos.append(utility_dto)

        if node.type == Type.DECISION.value and node.decision is not None:
            options = node.decision.options
            for option in options:
                discrete_utility_value = await self.get_discrete_utility_value(treenode_id, option)
                utility_dto = UtilityDTDto2(option_id=option.id,
                                           utility_value=option.utility + discrete_utility_value)
                utility_dtos.append(utility_dto)

        return utility_dtos

    async def get_discrete_utility_value(
        self, treenode_id: uuid.UUID, dto : OptionOutgoingDto | OutcomeOutgoingDto
    ) -> float:
        branch_label = dto.id.__str__()
        if not any(branch_label in key for key in self.utility_lookup.keys()):
            return 0

        node_id = treenode_id
        branch_labels = [branch_label]

        # Add branch labels for predecessors to the treenode branch
        parent_id = await self.get_parent(node_id)
        count = 0
        while parent_id and count < 1000:
            branch_labels.append(self.edge_names[(parent_id, node_id)])
            node_id = parent_id
            parent_id = await self.get_parent(node_id)
            count += 1

        # Create list of branch combinations, must include branch label for treenode
        branch_combinations : list[tuple[str, ...]] = [
            combo for r in range(2, len(branch_labels) + 1)
            for combo in itertools.combinations(branch_labels, r)
            if branch_label in combo]

        # Loop through branch_combinations
        # and add discrete utility for current treenode (from combinations present in utility_lookup)
        total_discrete_utility_value = sum(
                sum(self.utility_lookup[key])
                for combination in branch_combinations
                if (key := tuple(sorted(combination))) in self.utility_lookup
            ) if len(branch_labels) > 1 else 0

        return total_discrete_utility_value



class DecisionTreeCreator_v2:
    def __init__(self) -> None:
        self.nx = nx.DiGraph()  # type: ignore
        self.project_id: uuid.UUID
        self.data: Dict[str, List[Union[uuid.UUID, EdgeUUIDDto]]] = {}
        self.treenode_ids: list[uuid.UUID] = []
        self.node_ids: list[uuid.UUID] = []
        self.treenode_edge_dtos: list[EdgeUUIDDto] = []
        self.treenode_lookup_dict: Dict[str, TreeNodeDto] = {}
        self.node_treenode_lookup : NodeTreeNodeLookup

    @classmethod
    async def initialize(
        cls, project_id: uuid.UUID, nodes: list[IssueOutgoingDto], edges: list[EdgeOutgoingDto]
    ) -> DecisionTreeCreator_v2:
        instance = cls()
        instance.project_id = project_id
        instance.node_treenode_lookup = NodeTreeNodeLookup()
        # create a lookup between treenode id and IssueOutgoingDto | EndPointNodeDto
        await instance.create_data_struct(nodes, edges)
        return instance
    
    async def create_decision_tree(
        self, partial_order: Optional[list[uuid.UUID]] = None
    ) -> DecisionTreeGraph_v2:
        return await self.convert_to_decision_tree(
            project_id=self.project_id, partial_order=partial_order
        )

    async def create_data_struct(
        self, nodes: list[IssueOutgoingDto], edges: list[EdgeOutgoingDto]
    ) -> None:
        for node in nodes:
            treenode_id = uuid.uuid4()
            self.node_treenode_lookup.add_node_dto_and_treenode_id(node, treenode_id)
            await self.add_node(treenode_id)

        for edge in edges:
            tail_node = [x for x in nodes if x.id == edge.tail_node.issue_id][0]
            head_node = [x for x in nodes if x.id == edge.head_node.issue_id][0]
            tail_treenode_id = self.node_treenode_lookup.get_treenode_ids_for_dto(tail_node.id)[0]
            head_treenode_id = self.node_treenode_lookup.get_treenode_ids_for_dto(head_node.id)[0]
            await self.add_edge(EdgeUUIDDto(tail=tail_treenode_id, head=head_treenode_id))

    async def add_node(self, node: uuid.UUID) -> None:
        try:
            self.nx.add_node(node)  # type: ignore
        except Exception as e:
            print("Exception Add_node", str(e))
            raise HTTPException(status_code=500, detail=str(e))

    async def add_edge(self, edge: EdgeUUIDDto) -> None:
        self.nx.add_edge(edge.tail, edge.head)  # type: ignore

    async def copy(self) -> DecisionTreeCreator_v2:
        new_id = type(self)()  # Need to instance from the concrete class
        new_id.nx = self.nx.copy()  # type: ignore
        new_id.project_id = self.project_id
        new_id.node_treenode_lookup = self.node_treenode_lookup
        return new_id

    async def get_parents(self, node: uuid.UUID) -> list[uuid.UUID]:
        return list(self.nx.predecessors(node))  # type: ignore

    async def get_children(self, node: uuid.UUID) -> list[uuid.UUID]:
        return list(self.nx.successors(node))  # type: ignore

    async def get_node_from_uuid(self, uuid: uuid.UUID) -> Optional[IssueOutgoingDto | EndPointNodeDto]:
        return self.node_treenode_lookup.get_dto_for_treenode_id(uuid)
    
    async def get_type_from_id(self, id: uuid.UUID) -> str:
        node = await self.get_node_from_uuid(id)
        return node.type if node is not None else "Undefined"

    async def get_nodes_from_type(self, node_type_string: str) -> list[uuid.UUID]:
        node_list: list[uuid.UUID] = []
        for node in list(self.nx.nodes(data=True)):  # type: ignore
            node_id = node[0]  # type: ignore
            if await self.get_type_from_id(node_id) == node_type_string:  # type: ignore
                node_list.append(node_id)  # type: ignore
        return node_list

    async def has_children(self, node: uuid.UUID) -> bool:
        return len(await self.get_children(node)) > 0

    async def get_decision_nodes(self) -> list[uuid.UUID]:
        return await self.get_nodes_from_type(Type.DECISION.value)

    async def get_uncertainty_nodes(self) -> list[uuid.UUID]:
        return await self.get_nodes_from_type(Type.UNCERTAINTY.value)

    async def get_utility_nodes(self) -> list[uuid.UUID]:
        return await self.get_nodes_from_type(Type.UTILITY.value)

    @property
    async def decision_count(self) -> int:
        return len(await self.get_decision_nodes())

    @property
    async def uncertainty_count(self) -> int:
        return len(await self.get_uncertainty_nodes())

    @property
    async def utility_count(self) -> int:
        return len(await self.get_utility_nodes())

    async def decision_elimination_order(self) -> list[uuid.UUID]:
        cid_copy = await self.copy()

        decisions: list[uuid.UUID] = []
        decisions_count = await cid_copy.decision_count
        while decisions_count > 0:
            nodes: list[uuid.UUID] = list(cid_copy.nx.nodes())  # type: ignore
            for node in nodes:
                if not await cid_copy.has_children(node):
                    if await self.get_type_from_id(node) == Type.DECISION.value:
                        decisions.append(node)
                        decisions_count -= 1
                    cid_copy.nx.remove_node(node)  # type: ignore
        return decisions

    async def calculate_partial_order_issues(self) -> List[uuid.UUID | None]:
        partial_order = await self.calculate_partial_order()
        partial_order_issues = [self.node_treenode_lookup.get_dto_id_for_treenode_id(id) for id in partial_order]
        return partial_order_issues

    async def calculate_partial_order(self) -> list[uuid.UUID]:
        """Partial order algorithm
        """

        # get all chance nodes and sort according to child/parent relationship
        uncertainty_subgraph = self.nx.subgraph(await self.get_uncertainty_nodes()) # type: ignore
        uncertainty_nodes = list(nx.topological_sort(uncertainty_subgraph))   # type: ignore

        elimination_order = await self.decision_elimination_order()
        # TODO: Add utility nodes
        partial_order: list[uuid.UUID] = []

        while elimination_order:
            decision = elimination_order.pop()
            parent_decision_nodes: list[uuid.UUID] = []
            for parent in await self.get_parents(decision):
                if not await self.get_type_from_id(parent) == Type.DECISION.value:
                    if parent in uncertainty_nodes:
                        parent_decision_nodes.append(parent)
                        uncertainty_nodes.remove(parent)

            if len(parent_decision_nodes) > 0:
                partial_order += parent_decision_nodes
            partial_order.append(decision)

        partial_order += uncertainty_nodes # type: ignore

        return partial_order

    async def output_branches_from_node(
        self, node_id: uuid.UUID, node_in_partial_order_id: uuid.UUID, flip: bool = True
    ) -> Iterator[Tuple[EdgeUUIDDto, uuid.UUID]]:
        tree_stack = []
        node = await self.get_node_from_uuid(node_id)
        if node is not None and isinstance(node, IssueOutgoingDto):
            if node.type == Type.DECISION:
                tree_stack = (
                    [
                        EdgeUUIDDto(tail=node_id, head=None, name=option.id.__str__())
                        for option in node.decision.options
                    ]
                    if node.decision
                    else []
                )
            elif node.type == Type.UNCERTAINTY:
                # This needs to be re-written according to the way we deal with probabilities
                tree_stack = (
                    [
                        EdgeUUIDDto(tail=node_id, head=None, name=outcome.id.__str__())
                        for outcome in node.uncertainty.outcomes
                    ]
                    if node.uncertainty
                    else []
                )
            if flip:
                tree_stack.reverse()

        return zip(tree_stack, [node_in_partial_order_id] * len(tree_stack), strict=False)

    async def convert_to_decision_tree(
        self, project_id: uuid.UUID, partial_order: Optional[list[uuid.UUID]] = None
    ) -> DecisionTreeGraph_v2:
        # TODO: Update ID2DT according to way we deal with probabilities
        if not partial_order:
            partial_order = await self.calculate_partial_order()
        root_node = partial_order[0]
        decision_tree = DecisionTreeGraph_v2(root=root_node)
        # tree_stack contains views of the partial order nodes
        # decision_tree contains copy of the nodes (as they appear several times)
        tree_stack = [(root_node, root_node)]

        while tree_stack:
            element = tree_stack.pop()

            if isinstance(element[0], uuid.UUID):  # type: ignore
                tree_stack += await self.output_branches_from_node(*element)  # type: ignore

            else:  # element is a branch
                endpoint_start_index = partial_order.index(element[1])

                if endpoint_start_index < len(partial_order) - 1:
                    endpoint_end = await self.copy_treenode(partial_order[endpoint_start_index + 1])
                    tree_stack.append((endpoint_end, partial_order[endpoint_start_index + 1]))
                else:
                    endpoint_end = await self.create_endpoint_node(project_id=project_id)

                element[0].head = endpoint_end
                await decision_tree.add_edge(element[0])  # node is added when the branch is added

        nodes = self.node_treenode_lookup.get_list_of_nodes()
        for node in nodes:
            print(node.type)
        #await decision_tree.populate_treenode_lookup(self.treenode_lookup_dict)
        await decision_tree.populate_node_treenode_lookup(self.node_treenode_lookup)
        return decision_tree

    async def copy_treenode(self, treenode_id: uuid.UUID) -> uuid.UUID:
        node_id = self.node_treenode_lookup.get_dto_id_for_treenode_id(treenode_id)
        copy_node_id = uuid.uuid4()
        if node_id:
            self.node_treenode_lookup.associate_uuid(node_id, copy_node_id)
        return copy_node_id

    async def create_endpoint_node(self, project_id: uuid.UUID) -> uuid.UUID:
        # create endpoint node which is added to treenode_lookup table, return id of the node
        node = EndPointNodeDto(project_id=project_id)
        treenode_id = uuid.uuid4()
        self.node_treenode_lookup.add_node_dto_and_treenode_id(node, treenode_id)
        return treenode_id
    
