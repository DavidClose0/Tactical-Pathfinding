using System;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    private List<Connection> adjacencyList;
    private List<GameObject> connectionLineObjects; // To store line objects for cleanup if needed

    public Graph()
    {
        adjacencyList = new List<Connection>();
        connectionLineObjects = new List<GameObject>();
    }

    public List<Connection> GetConnections(Node fromNode)
    {
        List<Connection> connections = new List<Connection>();
        foreach (Connection c in adjacencyList)
        {
            if (c.GetFromNode() == fromNode)
            {
                connections.Add(c);
            }
        }
        return connections;
    }

    // New method to find the reverse connection
    public Connection FindReverseConnection(Node toNode, Node fromNode)
    {
        foreach (Connection c in adjacencyList)
        {
            if (c.GetFromNode() == toNode && c.GetToNode() == fromNode) // Corrected condition - REVERSED nodes
            {
                return c;
            }
        }
        return null;
    }


    public void Build()
    {
        // Clear existing connections and lines
        ClearConnections();

        // Populate graph
        adjacencyList = new List<Connection>();

        Node[] nodes = GameObject.FindObjectsByType<Node>(FindObjectsSortMode.None);
        if (nodes == null || nodes.Length < 2) return; // Need at least two nodes to connect

        for (int i = 0; i < nodes.Length; ++i)
        {
            for (int j = 0; j < nodes.Length; ++j)
            {
                if (i == j) continue; // Don't connect node to itself

                Node fromNode = nodes[i];
                Node toNode = nodes[j];

                float distance = Vector3.Distance(toNode.transform.position, fromNode.transform.position);
                if (distance <= 8f)
                {
                    float cost = distance;

                    // Create GameObject for Connection and add Connection script
                    GameObject connectionObject = new GameObject("ConnectionLine");
                    connectionObject.transform.position = fromNode.transform.position; // Position Connection object at fromNode
                    Connection connectionScript = connectionObject.AddComponent<Connection>();
                    connectionScript.Initialize(fromNode, toNode, cost, this); // Pass 'this' (the Graph)

                    // Add Box Collider and make it a trigger
                    BoxCollider boxCollider = connectionObject.AddComponent<BoxCollider>();
                    boxCollider.isTrigger = true; // Make it a trigger for collision detection in Pathfinder

                    // Adjust collider to fit the line (more precise now)
                    LineRenderer lineRenderer = connectionObject.GetComponent<LineRenderer>();
                    if (lineRenderer != null)
                    {
                        Vector3 startPoint = fromNode.transform.position; // Use fromNode.transform.position directly
                        Vector3 endPoint = toNode.transform.position;     // Use toNode.transform.position directly

                        // Calculate line direction
                        Vector3 lineDirection = (endPoint - startPoint).normalized;

                        // Rotate collider to align with the line direction - **MOVE ROTATION HERE, BEFORE CENTER CALCULATION**
                        if (lineDirection != Vector3.zero) // Avoid errors if nodes are at the same position
                        {
                            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.right, lineDirection); // Align X-axis with line direction
                            connectionObject.transform.rotation = targetRotation;
                        }

                        // Calculate midpoint for collider center in world space
                        Vector3 midPoint = (startPoint + endPoint) / 2f;

                        // Set collider center in LOCAL space - Calculated AFTER rotation
                        boxCollider.center = connectionObject.transform.InverseTransformPoint(midPoint);

                        float lineLength = Vector3.Distance(startPoint, endPoint);
                        // Set collider size - X-axis aligned with the line
                        boxCollider.size = new Vector3(lineLength, 0.3f, 0.4f); // Adjust width (0.3f) and depth (0.1f) as needed


                    }
                    else
                    {
                        Debug.LogError("LineRenderer not found on Connection Object, collider might not be positioned correctly.");
                    }

                    adjacencyList.Add(connectionScript);
                    connectionLineObjects.Add(connectionObject); // Keep track of line object

                    Debug.Log("Connection GameObject created between " + fromNode.name + " and " + toNode.name); // Debug log to confirm creation
                }
            }
        }
    }

    public void ClearConnections()
    {
        adjacencyList.Clear();
        // Destroy existing line objects
        foreach (GameObject line in connectionLineObjects)
        {
            GameObject.Destroy(line);
        }
        connectionLineObjects.Clear();
    }

    public void ResetAllConnectionColors()
    {
        foreach (Connection connection in adjacencyList)
        {
            connection.ResetColor();
        }
    }
}