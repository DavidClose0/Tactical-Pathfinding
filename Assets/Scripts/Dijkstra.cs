using System;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra
{
    // This structure is used to keep track of the information we need
    // for each node.
    private class NodeRecord : System.IComparable<NodeRecord>
    {
        public Node node;
        public Connection connection;
        public float costSoFar;

        public NodeRecord(Node node)
        {
            this.node = node;
        }

        public int CompareTo(NodeRecord other)
        {
            if (other == null) return 1; // Handle null case
            return costSoFar.CompareTo(other.costSoFar);
        }
    }

    private class PathfindingList
    {
        private List<NodeRecord> nodeRecords = new List<NodeRecord>();

        public int Length()
        {
            return nodeRecords.Count;
        }

        public void Add(NodeRecord n)
        {
            nodeRecords.Add(n);
        }

        public void Remove(NodeRecord n)
        {
            nodeRecords.Remove(n);
        }

        public bool Contains(Node node)
        {
            foreach (var n in nodeRecords)
            {
                if (n.node.Equals(node))
                {
                    return true;
                }
            }
            return false;
        }

        public NodeRecord Find(Node node)
        {
            foreach (var n in nodeRecords)
            {
                if (n.node == node)
                {
                    return n;
                }
            }
            return null;
        }

        public NodeRecord SmallestElement()
        {
            nodeRecords.Sort();
            return nodeRecords[0];
        }
    }

    public static List<Connection> pathfind(Graph graph, Node start, Node end)
    {
        // Initialize the record for the start node.
        NodeRecord startRecord = new NodeRecord(start);
        startRecord.connection = null;
        startRecord.costSoFar = 0;

        // Initialize the open and closed lists.
        PathfindingList open = new PathfindingList();
        open.Add(startRecord);
        PathfindingList closed = new PathfindingList();

        NodeRecord current = null;

        // Iterate through processing each node.
        while (open.Length() > 0)
        {
            // Find the smallest element in the open list.
            current = open.SmallestElement();

            // If it is the goal node, then terminate.
            if (current.node == end)
            {
                break;
            }

            // Otherwise get its outgoing connections.
            List<Connection> connections = graph.GetConnections(current.node);

            // Loop through each connection in turn.
            foreach (var connection in connections)
            {
                // Get the cost estimate for the end node.
                Node endNode = connection.GetToNode();
                float endNodeCost = current.costSoFar + connection.GetCost();

                // Skip if the node is closed.
                if (closed.Contains(endNode))
                {
                    continue;
                }

                NodeRecord endNodeRecord = null;

                // .. or if it is open and we’ve found a worse route.
                if (open.Contains(endNode))
                {
                    // Here we find the record in the open list
                    // corresponding to the endNode.
                    endNodeRecord = open.Find(endNode);
                    if (endNodeRecord != null && endNodeRecord.costSoFar <= endNodeCost)
                    {
                        continue;
                    }
                }
                // Otherwise we know we’ve got an unvisited node, so make a
                // record for it.
                else
                {
                    endNodeRecord = new NodeRecord(endNode);
                }

                // We’re here if we need to update the node. Update the
                // cost and connection.
                endNodeRecord.costSoFar = endNodeCost;
                endNodeRecord.connection = connection;


                // And add it to the open list
                if (!open.Contains(endNode))
                {
                    open.Add(endNodeRecord);
                }
            }

            // We’ve finished looking at the connections for the current node,
            // so add it to the closed list and remove it from the open list.
            open.Remove(current);
            closed.Add(current);
        }

        // We’re here if we’ve either found the goal, or if we’ve no more
        // nodes to search, find which.
        if (current == null) // Check if current is null in case open list becomes empty and goal is not found
        {
            // We’ve run out of nodes without finding the goal, so there’s
            // no solution.
            return null;
        }
        else
        {
            // Compile the list of connections in the path.
            List<Connection> path = new List<Connection>();

            // Work back along the path, accumulating connections.
            while (current.connection != null) // Stop when we reach the start node (no connection)
            {
                path.Add(current.connection);
                Node fromNode = current.connection.GetFromNode();
                current = closed.Find(fromNode);
            }

            // Reverse the path, and return it.
            path.Reverse();
            return path;
        }
    }
}