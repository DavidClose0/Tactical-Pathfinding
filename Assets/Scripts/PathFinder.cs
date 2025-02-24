using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Dijkstra;

public class Pathfinder : Kinematic
{
    public Node start;
    public Node goal;
    public Material goalMaterial;
    public float maxAngularAcceleration = 100f;

    Graph myGraph;

    FollowPath myMoveType;
    LookWhereGoing myRotateType;

    GameObject[] myPath;

    private List<Node> allNodes;
    private Material defaultMaterial;

    private float originalMaxSpeed;

    // Start is called before the first frame update
    void Start()
    {
        myRotateType = new LookWhereGoing();
        myRotateType.character = this;
        myRotateType.target = myTarget;
        myRotateType.maxAngularAcceleration = maxAngularAcceleration;

        myMoveType = new FollowPath();
        myMoveType.character = this;

        myGraph = new Graph();
        myGraph.Build();

        allNodes = new List<Node>(GameObject.FindObjectsByType<Node>(FindObjectsSortMode.None));
        defaultMaterial = allNodes[0].GetComponent<Renderer>().material;

        goal = GetRandomNodeExcluding(start);
        goal.gameObject.GetComponent<Renderer>().material = goalMaterial;

        ComputePath();
        myMoveType.path = myPath;

        originalMaxSpeed = maxSpeed;
    }

    // Update is called once per frame
    protected override void Update()
    {
        steeringUpdate = new SteeringOutput();
        steeringUpdate.angular = myRotateType.getSteering().angular;
        steeringUpdate.linear = myMoveType.getSteering().linear;
        base.Update();

        if (myMoveType.path != null && myMoveType.pathIndex < myMoveType.path.Length)
        {
            GameObject currentNodeObject = myMoveType.path[myMoveType.pathIndex];
            if (currentNodeObject != null)
            {
                Node currentNode = currentNodeObject.GetComponent<Node>();
                if (currentNode != null && Vector3.Distance(transform.position, currentNodeObject.transform.position) < myMoveType.targetThreshold) // Use FollowPath's threshold
                {
                    Debug.Log("Reached Node: " + currentNode.name + ", Recomputing Path."); // DEBUG LOG
                    if (currentNode == goal)
                    {
                        ChooseNewGoal();
                    }
                    else
                    {
                        start = currentNode;
                        ComputePath(); // Recompute path when reaching any node
                        myMoveType.pathIndex = 0; // Reset path index to start from the beginning of the new path
                        if (myPath != null && myPath.Length > 0)
                        {
                            myMoveType.target = myPath[0];
                            Debug.Log("New Path Computed, Target Set to: " + myMoveType.target.name); // DEBUG LOG
                        }
                        else
                        {
                            Debug.LogWarning("ComputePath returned no path!");
                        }
                    }
                }
            }
        }
    }

    // Computes the path from the current start to the current goal
    private void ComputePath()
    {
        List<Connection> pathConnections = Dijkstra.pathfind(myGraph, start, goal);
        if (pathConnections == null) // Handle case where no path is found
        {
            Debug.LogWarning("No path found!");
            myPath = null;
            myMoveType.path = null;
            myMoveType.target = null;
            return;
        }

        myPath = new GameObject[pathConnections.Count + 1];
        int i = 0;
        foreach (Connection c in pathConnections)
        {
            myPath[i] = c.GetFromNode().gameObject;
            i++;
        }
        myPath[i] = goal.gameObject;

        myMoveType.path = myPath;
        myMoveType.pathIndex = 0;
        if (myPath.Length > 0)
        {
            myMoveType.target = myPath[0];
        }
        Debug.Log("Path Computed. Path Length: " + (myPath != null ? myPath.Length : 0)); // DEBUG LOG
        if (myPath != null)
        {
            string pathNodeNames = "";
            foreach (var nodeObj in myPath)
            {
                pathNodeNames += nodeObj.name + " -> ";
            }
            Debug.Log("Path Nodes: " + pathNodeNames);
        }
    }

    // Chooses a new random goal: the previous goal becomes the new start,
    // its special material is removed, and a new goal is picked and marked.
    private void ChooseNewGoal()
    {
        goal.gameObject.GetComponent<Renderer>().material = defaultMaterial;
        start = goal;
        goal = GetRandomNodeExcluding(start);
        goal.gameObject.GetComponent<Renderer>().material = goalMaterial;
        ComputePath();
        Debug.Log("New Goal Chosen: " + goal.name + ", New Start: " + start.name); // DEBUG LOG
    }

    // Returns a random node from allNodes that is not equal to the provided node
    private Node GetRandomNodeExcluding(Node exclude)
    {
        List<Node> candidates = new List<Node>();
        foreach (Node n in allNodes)
        {
            if (n != exclude)
            {
                candidates.Add(n);
            }
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    // Trigger collision detection for speed penalty
    private void OnTriggerEnter(Collider other)
    {
        Connection connection = other.GetComponent<Connection>();
        if (connection != null && connection.IsBlue)
        {
            maxSpeed = originalMaxSpeed / 3;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Connection connection = other.GetComponent<Connection>();
        if (connection != null && connection.IsBlue)
        {
            maxSpeed = originalMaxSpeed;
        }
    }
}