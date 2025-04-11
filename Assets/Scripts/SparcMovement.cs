using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls the movement of a Sparc (enemy) along a network of Nodes (path intersections).
/// The Sparc will start at the nearest Node and move continuously from node to node along the allowed directions defined by each Node.
/// Movement is kept along the designated Node network (e.g., paths in a maze) and will not stray off these paths.
/// Uses smooth interpolation (Vector2.Lerp) for movement between nodes to ensure clean transitions.
/// Supports an optional "scatter" mode where the Sparc can take random directions (including potentially reversing direction) at nodes.
/// In non-scatter (chase) mode, if a target (e.g., the player) is provided, the Sparc will try to choose directions that move it closer to the target.
/// </summary>
public class SparcMovement : MonoBehaviour
{
    [Tooltip("Movement speed of the Sparc in world units per second.")]
    public float speed = 5f;

    [Tooltip("Reference to the target (e.g., Player) to chase. If null or scatterMode is true, movement will be random at intersections.")]
    public Transform target;

    [Tooltip("Scatter mode flag. If true, the Sparc will move in random directions at each node (potentially even reversing direction).")]
    public bool scatterMode = false;

    /// <summary>
    /// Current movement direction (normalized). This is the direction from the last node toward the next node.
    /// </summary>
    public Vector2 currentDirection { get; private set; }

    // The current node the Sparc is at or last visited.
    private Node currentNode;

    // The next node that the Sparc is moving towards along the path.
    private Node targetNode;

    // A cached list of all nodes in the scene for pathfinding.
    private List<Node> allNodes = new List<Node>();

    // Variables for smooth movement interpolation between nodes.
    private Vector2 segmentStartPos;
    private float segmentTravelTime;
    private float segmentTimer;

    void Start()
    {
        // Find all Node objects in the scene and identify the nearest node to the Sparc's starting position.
        allNodes.AddRange(FindObjectsOfType<Node>());
        currentNode = FindNearestNode(transform.position);
        

        // Initialize the Sparc at the starting node position to align it exactly on the path.
        transform.position = currentNode.transform.position;

        // Reset currentDirection at start (no movement yet).
        currentDirection = Vector2.zero;

        // Choose the initial direction and target node to begin moving.
        SetNextTarget();
    }

    void Update()
    {
        if (targetNode != null)
        {
            // Increment the timer and interpolate between the start and target positions.
            segmentTimer += Time.deltaTime;
            float t = segmentTravelTime > 0 ? segmentTimer / segmentTravelTime : 1f;
            // Lerp from the segment start position to the target node position for smooth movement.
            Vector2 newPos = Vector2.Lerp(segmentStartPos, targetNode.transform.position, t);
            transform.position = newPos;

            // Check if we have reached or passed the target node (t >= 1 means the time to reach target has elapsed).
            if (t >= 1f)
            {
                // Snap to exact target node position (to avoid any floating-point error).
                transform.position = targetNode.transform.position;
                // Update the current node to this target node since we've arrived.
                currentNode = targetNode;
                targetNode = null;

                currentNode.CheckAvailableDirections(); // ← THIS LINE is the fix!

                // Now choose the next direction and target node to continue movement.
                SetNextTarget();
            }
        }
    }

    /// <summary>
    /// Determines the next direction and target node for the Sparc to move to from the current node.
    /// Sets the currentDirection and targetNode, and prepares interpolation variables for smooth movement.
    /// </summary>
    private void SetNextTarget()
    {
        if (currentNode == null) return;

        // Choose a direction to move in from the current node's available directions.
        Vector2 newDirection = ChooseDirection(currentNode);

        Debug.Log($"📍 Node at {currentNode.transform.position} has {currentNode.availableDirections.Count} directions.");

        // Find the neighboring node in this direction.
        Node nextNode = GetNodeInDirection(currentNode, newDirection);

        // If no neighbor node is found in that direction (should not happen if availableDirections is correct), try a different direction.
        if (nextNode == null)
        {
            // In case of an unexpected missing node, do not move.
            // You could add additional logic here to handle dead-ends or turn around.
            currentDirection = Vector2.zero;
            return;
        }

        // Set the new direction and target node.
        currentDirection = newDirection;
        targetNode = nextNode;

        Debug.Log($"🟢 Moving from {currentNode.transform.position} → {targetNode.transform.position} via {currentDirection}");


        // Prepare for smooth movement toward the target node.
        segmentStartPos = currentNode.transform.position;
        segmentTimer = 0f;
        float distance = Vector2.Distance(segmentStartPos, targetNode.transform.position);
        segmentTravelTime = distance / speed;
    }

    /// <summary>
    /// Chooses the next movement direction from the given node, based on the current mode (scatter or chase).
    /// In scatter mode or if no target is provided, a random available direction is chosen (allowing possible reversals).
    /// In chase mode (scatterMode == false and target is set), the direction that brings the Sparc closest to the target is chosen (excluding immediate reversal unless it's the only way).
    /// </summary>
    /// <param name="node">The node from which to choose the next direction.</param>
    /// <returns>A normalized Vector2 representing the chosen direction.</returns>
    private Vector2 ChooseDirection(Node node)
    {
        // Get the list of allowed directions from this node.
        List<Vector2> possibleDirections = new List<Vector2>(node.availableDirections);


        // If not in scatter mode, avoid immediately reversing direction unless there is no other option.
        if (!scatterMode && currentDirection != Vector2.zero && possibleDirections.Count > 1)
        {
            // Remove the opposite of the current direction to prevent a 180-degree turn at an intersection (unless it's a dead-end).
            possibleDirections.Remove(-currentDirection);
        }

        // ⚠️ Safety check to prevent crashing
        if (possibleDirections.Count == 0)
        {
            Debug.LogWarning($"No directions available from node at {node.transform.position}.");
            return Vector2.zero;
        }

        // Decide on a direction based on whether we have a target to chase and the scatter mode.
        Vector2 chosenDirection;
        if (scatterMode || target == null)
        {
            // Scatter mode (or no target to chase): pick a random direction from the available ones.
            int randIndex = Random.Range(0, possibleDirections.Count);
            chosenDirection = possibleDirections[randIndex];
        }
        else
        {
            // Chase mode: choose the direction that minimizes distance to the target.
            float shortestDistance = Mathf.Infinity;
            Vector2 bestDirection = Vector2.zero;
            foreach (Vector2 dir in possibleDirections)
            {
                // Find the neighbor node if we go in this direction.
                Node neighborNode = GetNodeInDirection(node, dir);
                if (neighborNode == null) continue;
                // Calculate distance from that neighbor node to the target.
                float d = Vector2.Distance(neighborNode.transform.position, target.position);
                if (d < shortestDistance)
                {
                    shortestDistance = d;
                    bestDirection = dir;
                }
            }
            // If for some reason no direction was found (which shouldn't happen), default to a random direction.
            if (bestDirection == Vector2.zero)
            {
                bestDirection = possibleDirections[Random.Range(0, possibleDirections.Count)];
            }
            chosenDirection = bestDirection;
        }

        return chosenDirection;
    }

    /// <summary>
    /// Finds the neighboring Node in a given direction from the starting node.
    /// Assumes nodes are connected in cardinal directions (up, down, left, right) along the paths.
    /// </summary>
    /// <param name="node">The starting node.</param>
    /// <param name="direction">The direction to look for a neighbor (normalized Vector2, e.g., (1,0), (-1,0), (0,1), (0,-1)).</param>
    /// <returns>The adjacent Node in that direction, or null if none exists.</returns>
    private Node GetNodeInDirection(Node node, Vector2 direction)
    {
        Vector2 origin = node.transform.position;
        Vector2 targetPos = origin + direction;

        // Round target position to whole numbers to match tilemap grid
        targetPos = new Vector2(Mathf.Round(targetPos.x), Mathf.Round(targetPos.y));

        foreach (Node n in allNodes)
        {
            Vector2 nodePos = new Vector2(Mathf.Round(n.transform.position.x), Mathf.Round(n.transform.position.y));

            if (nodePos == targetPos)
            {
                return n;
            }
        }

        return null; // No neighbor found exactly in that direction
    }




    /// <summary>
    /// Finds the nearest Node to a given position (used to initialize the Sparc on the network).
    /// </summary>
    /// <param name="position">The position from which to find the closest node.</param>
    /// <returns>The Node closest to the given position.</returns>
    private Node FindNearestNode(Vector2 position)
    {
        Node nearest = null;
        float minDistance = 0.1f; // tighter threshold to snap

        foreach (Node n in allNodes)
        {
            float dist = Vector2.Distance(position, n.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = n;
            }
        }

        return nearest;
    }


    public void SetCurrentNode(Node node)
    {
        currentNode = node;
        transform.position = node.transform.position; // snap onto the node
        targetNode = null;
        currentDirection = Vector2.zero;

        SetNextTarget(); // ← start movement immediately
    }



}
