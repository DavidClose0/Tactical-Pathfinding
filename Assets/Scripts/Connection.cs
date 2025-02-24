using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Connection : MonoBehaviour, IPointerClickHandler
{
    public Node fromNode;
    public Node toNode;
    public float cost;
    private float originalCost; // Store the original cost
    private LineRenderer lineRenderer;
    private Color defaultColor = Color.grey;
    private Color selectedColor = Color.blue;
    private Graph graph;
    private BoxCollider boxCollider;
    private bool isBlue = false; // Track if the connection is blue
    public bool IsBlue => isBlue; // Public getter for isBlue

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        SetupLineRenderer();
        boxCollider = GetComponent<BoxCollider>();
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.4f;
        lineRenderer.endWidth = 0.4f;
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lineRenderer.startColor = defaultColor;
        lineRenderer.endColor = defaultColor;
    }

    public void Initialize(Node fromNode, Node toNode, float cost, Graph graph)
    {
        this.fromNode = fromNode;
        this.toNode = toNode;
        this.cost = cost;
        this.originalCost = cost; // Store the original cost
        this.graph = graph;
        UpdateLinePositions();
    }

    private void UpdateLinePositions()
    {
        if (lineRenderer != null && fromNode != null && toNode != null)
        {
            lineRenderer.SetPosition(0, fromNode.transform.position);
            lineRenderer.SetPosition(1, toNode.transform.position);
        }
    }

    public Node GetFromNode() { return fromNode; }
    public Node GetToNode() { return toNode; }
    public float GetCost() { return cost; }
    public float GetOriginalCost() { return originalCost; } // Public getter for originalCost


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick CALLED for connection: " + fromNode.name + " to " + toNode.name); // ADDED DEBUG LOG

        if (!isBlue)
        {
            SetColor(selectedColor);
            isBlue = true;
            cost *= 3f;

            if (graph != null)
            {
                Connection reverseConnection = graph.FindReverseConnection(toNode, fromNode);
                if (reverseConnection != null && reverseConnection != this)
                {
                    reverseConnection.SetColor(selectedColor);
                    reverseConnection.isBlue = true;
                    reverseConnection.cost *= 3f;
                    Debug.Log("Reverse Connection also colored (PointerClick): " + reverseConnection.fromNode.name + " to " + reverseConnection.toNode.name);
                }
            }
            Debug.Log("Connection Clicked and turned Blue: " + fromNode.name + " to " + toNode.name + ", Cost: " + cost);

        }
        else
        {
            ResetColor();
            isBlue = false;
            cost = originalCost;

            if (graph != null)
            {
                Connection reverseConnection = graph.FindReverseConnection(toNode, fromNode);
                if (reverseConnection != null && reverseConnection != this)
                {
                    reverseConnection.ResetColor();
                    reverseConnection.isBlue = false;
                    reverseConnection.cost = reverseConnection.originalCost;
                    Debug.Log("Reverse Connection also reset (PointerClick): " + reverseConnection.fromNode.name + " to " + reverseConnection.toNode.name);
                }
            }
            Debug.Log("Connection Clicked and reset to Grey: " + fromNode.name + " to " + toNode.name + ", Cost: " + cost);
        }
    }

    public void ResetColor()
    {
        SetColor(defaultColor);
    }

    public void SetColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private void OnDrawGizmos()
    {
        if (boxCollider != null)
        {
            Gizmos.color = Color.green;
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);

            Gizmos.matrix = originalMatrix;

            // Visualize local axes - ADD THIS FOR DEBUGGING
            Gizmos.color = Color.red;   // X axis is red
            Gizmos.DrawRay(transform.position, transform.right * 0.5f);
            Gizmos.color = Color.green; // Y axis is green
            Gizmos.DrawRay(transform.position, transform.up * 0.5f);
            Gizmos.color = Color.blue;  // Z axis is blue
            Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
        }
    }
}