from typing import Optional, List, Any
from Prisma.models.node import Node
from Prisma.models.edge import Edge

class InfluenceDiagram:
    def __init__(self, nodes: List[Node], arcs: List[Edge]) -> None:
        self.nodes = nodes
        self.arcs = arcs
        self.network = self._construct_network()

    def _construct_network(self):
        # networkx implementation
        pass

    def _has_loop(self):
        pass

    def _validate_influence_diagram(self):
        pass

    def to_decision_tree(self):
        pass

    def solve_with_pyagrum(self):
        pass