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
    [SerializeField] private float speed = 5;
    // Turn these Edge Colliders into Lists of EdgeCollider2Ds so when we add new edges to the board we know which is which
    [SerializeField] private EdgeCollider2D topLine;
    [SerializeField] private EdgeCollider2D bottomLine;
    [SerializeField] private EdgeCollider2D leftLine;
    [SerializeField] private EdgeCollider2D rightLine;

    //[SerializeField] private Material material;

    [SerializeField] private LineRenderer playerTrail;

    [SerializeField] private GameManager gameManager;


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
    }
    private void Update()
    {
        pendingEdgeSwap();
        lineMovement();
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Qix") gameManager.GameOver();
        if (collision.gameObject.layer == 11) Debug.Log("GG");//gameManager.LoseLife();
    }

    private void FixedUpdate()
    {
        SnapPlayerOnEdges(currentEdge);
    }


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



    // Moving On Lines
    // ----------------

    private void lineMovement()
    {
        // Direction logic — 8 possible movement combos
        if (currentEdge.Contains(topLine.gameObject))
        {
            moveLeft();
            moveRight();
        }
        if (currentEdge.Contains(bottomLine.gameObject))
        {
            moveLeft();
            moveRight();
        }
        if (currentEdge.Contains(leftLine.gameObject))
        {
            moveUp();
            moveDown();
        }
        if (currentEdge.Contains(rightLine.gameObject))
        {
            moveUp();
            moveDown();
        }
    }

    // Movement Helpers
    // ---------------------

    private void moveUp()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(0, 1 * speed, 0) * Time.deltaTime;
        }
    }

    private void moveDown()
    {
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(0, -1 * speed, 0) * Time.deltaTime;
        }
    }

    private void moveLeft()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(-1 * speed, 0, 0) * Time.deltaTime;
        }
    }

    private void moveRight()
    {
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(1 * speed, 0, 0) * Time.deltaTime;
        }
    }



}
