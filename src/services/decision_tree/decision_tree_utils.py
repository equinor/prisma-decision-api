import uuid
from typing import Dict, Optional
from src.dtos.decision_tree_dtos import TreeNodeDto

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
    