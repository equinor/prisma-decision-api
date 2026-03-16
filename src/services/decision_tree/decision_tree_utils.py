import uuid
from typing import Dict, Optional, List
from src.dtos.decision_tree_dtos import TreeNodeDto, EndPointNodeDto
from src.dtos.issue_dtos import IssueOutgoingDto

# original id: id when treenode is created
# updated id: fixed id created from name of branch elements for treenode 
# original_id_to_node: map between treenodes and their original ids
# original_id_to_updated_id: map between original ids and updated ids
class TreeNodeLookup:
    def __init__(self):
        self.original_id_to_updated_id: Dict[uuid.UUID, uuid.UUID] = {}
        self.original_id_to_node: Dict[uuid.UUID, TreeNodeDto] = {}

    def add_with_original_id(self, original_id: uuid.UUID, node: TreeNodeDto):
        if original_id in self.original_id_to_node:
            raise ValueError("Original id must be unique.")
        self.original_id_to_node[original_id] = node

    def set_updated_node(self, original_id: uuid.UUID, updated_dto: TreeNodeDto):
        updated_id = updated_dto.id
        if original_id not in self.original_id_to_node:
            raise KeyError("Original id must exist before setting updated id.")
        if updated_id == original_id:
            raise ValueError("Updated id must be different from original id.")
        self.original_id_to_updated_id[original_id] = updated_id
        self.original_id_to_node[original_id] = updated_dto

    def get_node_by_original_id(self, id: uuid.UUID) -> Optional[TreeNodeDto]:
        return self.original_id_to_node.get(id) 

    def get_original_id(self, id: uuid.UUID) -> Optional[uuid.UUID]:
        for original_id, updated_id in self.original_id_to_updated_id.items():
            if updated_id == id:
                return original_id
        return None
    
    def get_list_of_nodes(self):
        return self.original_id_to_node.values()


class NodeTreeNodeLookup:
    def __init__(self):
        self.node_dtos: Dict[uuid.UUID, IssueOutgoingDto | EndPointNodeDto] = {}  # dto_id -> DTO instance
        #self.node_id_to_treenode_ids: Dict[uuid.UUID, List[uuid.UUID]] = {}  # dto_id -> list of uuids
        self.treenode_id_to_node_id: Dict[uuid.UUID, uuid.UUID] = {}   # treenode_id -> dto_id

    def add_dto(self, node_dto: IssueOutgoingDto | EndPointNodeDto):
        self.node_dtos[node_dto.id] = node_dto

    def add_node_dto_and_treenode_id(self, node_dto: IssueOutgoingDto | EndPointNodeDto, treenode_id: uuid.UUID):
        self.node_dtos[node_dto.id] = node_dto
        self.treenode_id_to_node_id[treenode_id] = node_dto.id

    def associate_uuid(self, node_id: uuid.UUID, treenode_id: uuid.UUID):
        # Add mapping in uuid_to_dto_id
        self.treenode_id_to_node_id[treenode_id] = node_id

    def get_node_dto(self, node_id: uuid.UUID) -> IssueOutgoingDto | EndPointNodeDto | None:
        return self.node_dtos.get(node_id)

    def get_dto_id_for_treenode_id(self, treenode_id: uuid.UUID) -> uuid.UUID | None:
        return self.treenode_id_to_node_id.get(treenode_id)

    def get_dto_for_treenode_id(self, treenode_id: uuid.UUID) -> IssueOutgoingDto | EndPointNodeDto | None:
        node_id = self.get_dto_id_for_treenode_id(treenode_id)
        return self.get_node_dto(node_id)
    
    def get_list_of_nodes(self) -> List[IssueOutgoingDto | EndPointNodeDto]:
        dtos: list[IssueOutgoingDto | EndPointNodeDto] = [self.node_dtos[v] for v in self.treenode_id_to_node_id.values()]
        return dtos
    
    def get_treenode_ids_for_dto(self, node_id: uuid.UUID) -> List[uuid.UUID]:
        treenode_ids = [k for k, v in self.treenode_id_to_node_id.items() if v == node_id]
        return treenode_ids



class NodeTreeNodeLookup_OLD:
    def __init__(self):
        self.node_dtos: Dict[uuid.UUID, IssueOutgoingDto | EndPointNodeDto] = {}  # dto_id -> DTO instance
        self.node_id_to_treenode_ids: Dict[uuid.UUID, List[uuid.UUID]] = {}  # dto_id -> list of uuids
        self.treenode_id_to_node_id: Dict[uuid.UUID, uuid.UUID] = {}   # treenode_id -> dto_id

    def add_dto(self, node_dto: IssueOutgoingDto | EndPointNodeDto):
        self.node_dtos[node_dto.id] = node_dto

    def add_node_dto_and_treenode_id(self, node_dto: IssueOutgoingDto | EndPointNodeDto, treenode_id: uuid.UUID):
        self.node_dtos[node_dto.id] = node_dto
        self.associate_uuid(node_dto.id, treenode_id) 

    def associate_uuid(self, node_id: uuid.UUID, treenode_id: uuid.UUID):
        # Add uuid to dto_id_to_uuids
        self.node_id_to_treenode_ids.setdefault(node_id, []).append(treenode_id)
        # Add mapping in uuid_to_dto_id
        self.treenode_id_to_node_id[treenode_id] = node_id

    def get_node_dto(self, node_id: uuid.UUID) -> IssueOutgoingDto | EndPointNodeDto | None:
        return self.node_dtos.get(node_id)

    def get_treenode_ids_for_dto(self, node_id: uuid.UUID) -> List[uuid.UUID]:
        return self.node_id_to_treenode_ids.get(node_id, [])

    def get_dto_id_for_treenode_id(self, treenode_id: uuid.UUID) -> uuid.UUID | None:
        return self.treenode_id_to_node_id.get(treenode_id)

    def get_dto_for_treenode_id(self, treenode_id: uuid.UUID) -> IssueOutgoingDto | EndPointNodeDto | None:
        node_id = self.get_dto_id_for_treenode_id(treenode_id)
        return self.get_node_dto(node_id)
    