using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SpeedTree.Importer;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class SparcMovement : MonoBehaviour
{
    [SerializeField] public float speed = 5;
    private Node currentNode;
    private Node targetNode;
    private bool isMoving = false;
    private Node previousNode;


    // Turn these Edge Colliders into Lists of EdgeCollider2Ds so when we add new edges to the board we know which is which
    [SerializeField] private EdgeCollider2D topLine;
    [SerializeField] private EdgeCollider2D bottomLine;
    [SerializeField] private EdgeCollider2D leftLine;
    [SerializeField] private EdgeCollider2D rightLine;

    //[SerializeField] private Material material;


    //[SerializeField] private GameManager gameManager;


    private GameObject pendingEdge = null;

    // Used for movement and tracking which edge the player is currently on
    private List<GameObject> currentEdge = new List<GameObject>(); // [0, 1] [currentEdge, pendingEdge]


    private enum Directions
    {
        Left,
        Right,
        Up,
        Down
    }

    // ---------------------
    // Public Methods
    // ---------------------
    private void Start()
    {


        topLine = GameObject.FindWithTag("top").GetComponent<EdgeCollider2D>();
        bottomLine = GameObject.FindWithTag("bottom").GetComponent<EdgeCollider2D>();
        leftLine = GameObject.FindWithTag("left").GetComponent<EdgeCollider2D>();
        rightLine = GameObject.FindWithTag("right").GetComponent<EdgeCollider2D>();
        currentEdge.Add(rightLine.gameObject);
        currentEdge.Add(null);

        currentNode = FindClosestNode();

    }
    private void Update()
    {

        Debug.Log($"Moving toward Node.");


        if (targetNode != null && isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetNode.transform.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetNode.transform.position) < 0.1f)
            {
                Debug.Log("Reached target node");
                currentNode = targetNode;
                targetNode = null;
                isMoving = false;
                PickNextNode();
            }
        }

        else if (!isMoving && currentNode != null)
        {
            PickNextNode();
        }

        pendingEdgeSwap();
    }

    private void PickNextNode()
    {
        Debug.Log("Picking next node...");
        Collider2D currentNodeCollider = currentNode.GetComponent<Collider2D>();
        currentNodeCollider.enabled = false; // temporarily disable



        currentNode.CheckAvailableDirections(); //Constantly Refreshesh the directions of Nodes In Case It Changes

        if (currentNode.availableDirections.Count == 0){
            Debug.LogWarning("No directions available!");
            return; //If we're not at a Node then we nothing
        }
        // STEP 3: Copy and shuffle available directions to randomize choice
        var directions = new List<Vector2>(currentNode.availableDirections);
        directions = directions.OrderBy(x => Random.value).ToList(); // shuffling for randomness

        // STEP 4: Loop through each direction to find a valid target node
        foreach (Vector2 direction in directions)
        {
            // STEP 4.1: Cast a ray in the current direction to check for a connected node
            RaycastHit2D hit = Physics2D.Raycast(currentNode.transform.position, direction, 8.1f, LayerMask.GetMask("TriggerLine"));

            if (hit.collider != null)
            {
                Node hitNode = hit.collider.GetComponent<Node>();

                // STEP 4.2: Check if the node hit is NOT the current or previous node
                if (hitNode != null && hitNode != currentNode && hitNode != previousNode)
                {
                    Debug.Log("Found next node: " + hitNode.name);

                    // STEP 4.3: Save current node as previous before moving
                    previousNode = currentNode;

                    // STEP 4.4: Set the new node as the movement target
                    targetNode = hitNode;
                    isMoving = true;
                    currentNodeCollider.enabled = true; // temporarily disable

                    return; // done — stop the loop
                }
            }
        }

        // STEP 5: If no valid new node was found, fall back to previousNode (optional)
        Debug.LogWarning("No valid new node found. Fallback to previousNode (if allowed).");

        if (previousNode != null && previousNode != currentNode)
        {
            targetNode = previousNode;
            previousNode = currentNode; // still update trail
            isMoving = true;

            Debug.Log("Fallback to previous node: " + targetNode.name);
        }
    
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        pendingEdge = collision.gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == pendingEdge)
        {
            pendingEdge = null;
        }

        // TODO: Comment this properly Frank
        if (currentEdge[0] == collision.gameObject)
        {
            currentEdge[0] = currentEdge[1];
            currentEdge[1] = null;
        }
        else if (currentEdge[1] == collision.gameObject)
        {
            currentEdge[1] = null;
        }
    }



    // ---------------------
    // Pending Edge Handling
    // ---------------------

    // Handle pending edge swap only if player is intentionally trying to move

    private void pendingEdgeSwap()
    {
        if (pendingEdge != null)
        {
            bool moveIntent = false;

            if ((pendingEdge == topLine.gameObject || pendingEdge == bottomLine.gameObject) &&
                (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)))
            {
                moveIntent = true;
            }
            else if ((pendingEdge == leftLine.gameObject || pendingEdge == rightLine.gameObject) &&
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)))
            {
                moveIntent = true;
            }

            if (moveIntent)
            {
                currentEdge[1] = currentEdge[0];
                currentEdge[0] = pendingEdge;
                pendingEdge = null;
            }
        }
    }

 /*   private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Qix") gameManager.GameOver();
        if (collision.gameObject.layer == 11) Debug.Log("GG");//gameManager.LoseLife();
    }

    */

    /*private void FixedUpdate()
    {
        SnapPlayerOnEdges(currentEdge);
    }
    */


    // ---------------------
    // Player Snapping Tech
    // ---------------------

    private void SnapPlayerOnEdges(List<GameObject> edges)
    {
        float minDist = Mathf.Infinity;
        Vector2 closestPoint = transform.position;

        foreach (GameObject edge in edges)
        {
            if (edge == null) continue;

            Vector2 pointOnEdge = GetClosestPointOnEdge(transform.position, edge.GetComponent<EdgeCollider2D>());
            float dist = Vector2.Distance(transform.position, pointOnEdge);

            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = pointOnEdge;
            }
        }

        transform.position = closestPoint;
    }

    private Vector2 GetClosestPointOnEdge(Vector2 playerPos, EdgeCollider2D edge)
    {
        float minDist = Mathf.Infinity;
        Vector2 closestPoint = Vector2.zero;


        var points = edge.points; // array of edge collider points

        // check every edge to see which is the closest to the player's position
        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 p1 = edge.transform.TransformPoint(points[i]);
            Vector2 p2 = edge.transform.TransformPoint(points[i + 1]);

            Vector2 projected = Proj_ab_aplayer(playerPos, p1, p2);
            float dist = Vector2.Distance(playerPos, projected);
            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = projected;
            }
        }
        return closestPoint;
    }

    private Vector2 Proj_ab_aplayer(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a; // edge we are projecting onto
        float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t); // ensures the player doesn't move beyond the length of the edge it is currently on
        return a + t * ab;
    }

    // ----------------
    // Movement Code
    // ----------------
    private Node FindClosestNode()
    {
        Node[] allNodes = GameObject.FindObjectsOfType<Node>(); //Grab All Existing Nodes In The Scene
        float minDistance = Mathf.Infinity; //Set Min Distance To Infinity So We Can Compare
        Node closest = null; //We are looking for the closest node so we set it to null

        foreach (Node node in allNodes) //Iterate through allNodes array
        {
            float dist = Vector2.Distance(transform.position, node.transform.position); //Get the distance between the current placement of the sparc and how far away the distance it is away from the Node
            if (dist < minDistance) //If the distance is less than the minDistance
            {
                minDistance = dist;
                closest = node;
            }
        }

        return closest;
    }


}
