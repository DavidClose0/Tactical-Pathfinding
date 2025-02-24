using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : Seek
{
    // list of waypoints
    public GameObject[] path;
    public int pathIndex = 0;
    public float targetThreshold = .1f;

    public override SteeringOutput getSteering()
    {
        // initialize target to first waypoint
        if (target == null)
        {
            if (path != null && path.Length > 0) // Check if path is valid before accessing
            {
                target = path[pathIndex];
            }
            else
            {
                return new SteeringOutput(); // Return empty steering if path is not set
            }
        }

        // increment path index when waypoint is reached
        if (target != null && (target.transform.position - character.transform.position).magnitude < targetThreshold)
        {
            pathIndex++;
            if (pathIndex >= path.Length) // Stop at the end of the path
            {
                pathIndex = path.Length - 1; // Keep index at the last node
                return new SteeringOutput(); // Stop steering after path end
            }
            target = path[pathIndex];
            Debug.Log("Waypoint Reached, New Target: " + target.name + ", Path Index: " + pathIndex); // DEBUG LOG
        }

        return base.getSteering();
    }
}