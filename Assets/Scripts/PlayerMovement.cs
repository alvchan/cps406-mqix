using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SpeedTree.Importer;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5; 
    // Turn these Edge Colliders into Lists of EdgeCollider2Ds so when we add new edges to the board we know which is which
    [SerializeField] private EdgeCollider2D topLine;
    [SerializeField] private EdgeCollider2D bottomLine;
    [SerializeField] private EdgeCollider2D leftLine;
    [SerializeField] private EdgeCollider2D rightLine;

    [SerializeField] private LineRenderer playerTrail;

    [SerializeField] private GameManager gameManager;


    private GameObject pendingEdge = null;

    private List<GameObject> currentEdge = new List<GameObject>();
    private List<Vector3> edges = new List<Vector3>();
    private List<Vector3> points = new List<Vector3>();

    private bool isOnEdge = true; // this will be used for unsnapping the player from the main lines so they can cut the board
    private bool isCutting = false;
    private bool startedCutting = false;

    // TODO: move this crap to progression or something
    private float area = 0.0f;
    private float turnsies = 0.0f;
    private const float TOTAL_AREA = 64.0f;
    private const float GOAL = 0.75f * TOTAL_AREA;

    private Directions oldDirection = Directions.Right;
    private Directions cutDirection = Directions.Right;
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
    public void Initialize()
    {
        currentEdge.Add(rightLine.gameObject);
        currentEdge.Add(null);

    }
    public void PlayerMove()
    {
        //printEdges();
        if (Input.GetKey(KeyCode.Space))
        {
            beginCutting();
        }
        else if (!isCutting)
        {
            resetLine();
            isOnEdge = true;
            startedCutting = false; // Reset when not cutting
        }


        // Don’t move unless touching an edge
        if (isCutting)
        {
            freelyCuttingMovement();
        }
        else
        {
            pendingEdgeSwap();
            lineMovement();
        }
    }















    


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isOnEdge)
        {
            cutTowardsMiddle();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // snapbaaaaack mechanic
        if (!isOnEdge)
        {
            snapBackToEdge(collision);
        }

        //if (currentEdge.Contains(collision.gameObject)) return;
        pendingEdge = collision.gameObject;

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isOnEdge)
        {
            if (collision.gameObject == pendingEdge)
            {
                print("pending = null");
                pendingEdge = null;
            }

            // TODO: Comment this properly Frank
            if (currentEdge[0] == collision.gameObject)
            {
                print("staying on same edge");
                currentEdge[0] = currentEdge[1];
                currentEdge[1] = null;
            }
            else if (currentEdge[1] == collision.gameObject)
            {
                print("switch [0] edge with new edge");
                currentEdge[1] = null;
            }
        }
        else
        {
            isCutting = true;
        }
    }

















    private void printEdges()
    {
        for (int i = 0; i < edges.Count; i++)
        {
            print(edges[i].ToString());
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

    // ---------------------
    // Direction Handling
    // ---------------------

    private void setDirection(Collider2D collision)
    {
        if (topLine.gameObject == collision.gameObject)
        {
            currentEdge[0] = topLine.gameObject;
        }
        else if (bottomLine.gameObject == collision.gameObject)
        {
            currentEdge[0] = bottomLine.gameObject;
        }
        else if (leftLine.gameObject == collision.gameObject)
        {
            currentEdge[0] = leftLine.gameObject;
        }
        else if (rightLine.gameObject == collision.gameObject)
        {
            currentEdge[0] = rightLine.gameObject;
        }
    }


    // --------------------------
    // Line Tracking (Tail/Trail)
    // --------------------------

    private void createLine()
    {
        if (playerTrail.positionCount == 0)
        {
            playerTrail.positionCount = 1;
            playerTrail.SetPosition(0, transform.position);
        }
        Vector3[] temp = new Vector3[playerTrail.positionCount];
        playerTrail.GetPositions(temp);
        List<Vector3> linePoints = temp.ToList();

        // If trail just started, seed it with current position
        if (linePoints.Count == 0)
        {
            linePoints.Add(transform.position);
        }

        // Add current position
        linePoints.Add(transform.position);

        playerTrail.positionCount = linePoints.Count;
        playerTrail.SetPositions(linePoints.ToArray());
    }



    private void resetLine()
    {
        playerTrail.positionCount = 0;
    }

    // ----------------------
    // Random methods for now
    // ----------------------

    private void beginCutting()
    {
        isOnEdge = false;

        if (!startedCutting)
        {
            startedCutting = true;
            edges.Add(transform.position);
            points.Add(transform.position);
            createLine();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Qix") gameManager.GameOver();
    }

    private void FixedUpdate()
    {
        SnapPlayerOnEdges(currentEdge);
    }


    private void snapBackToEdge(Collider2D collision)
    {
        isCutting = false;
        isOnEdge = true;
        resetLine();
        edges.Add(transform.position);

        Vector3 topleft = new Vector3(-8, 4, 0);
        Vector3 ideal = new Vector3(topleft.x, transform.position.y, 0);
        float dist1 = Vector3.Distance(ideal, topleft);
        float dist2 = Vector3.Distance(points[0], ideal);
        area = dist1 * dist2 - turnsies;
        print(area/TOTAL_AREA);
        area = 0.0f;
        turnsies = 0.0f;
        points.Clear();

        setDirection(collision);
    }

    // ---------------------
    // Player Snapping Tech
    // ---------------------

    private void SnapPlayerOnEdges(List<GameObject> edges)
    {
        if (!isOnEdge)
            return;

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


    // Cutting Movement
    // ----------------
    private void freelyCuttingMovement()
    {
        createLine();
        oldDirection = cutDirection;
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && cutDirection != Directions.Down)
        {
            cutDirection = Directions.Up;
            moveUp();
        }
        else if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && cutDirection != Directions.Up)
        {
            cutDirection = Directions.Down;
            moveDown();
        }
        else if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && cutDirection != Directions.Right)
        {
            cutDirection = Directions.Left;
            moveLeft();
        }
        else if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && cutDirection != Directions.Left)
        {
            cutDirection = Directions.Right;
            moveRight();
        }

        calculateArea();
    }

    private Directions cw(Directions dir) {
        switch (dir) {
            case Directions.Up: 
                return Directions.Right;
            case Directions.Right:
                return Directions.Down;
            case Directions.Down:
                return Directions.Left;
            case Directions.Left:
                return Directions.Up;
            default:
                print("??? cw broke");
                return Directions.Left;
        }
    }

    private Directions ccw(Directions dir) {
        switch (dir) {
            case Directions.Up: 
                return Directions.Left;
            case Directions.Left:
                return Directions.Down;
            case Directions.Down:
                return Directions.Right;
            case Directions.Right:
                return Directions.Up;
            default:
                print("??? ccw broke");
                return Directions.Right;
        }
    }

    private void calculateArea()
    {
        if (cutDirection != oldDirection)
        {
            edges.Add(transform.position);
            points.Add(transform.position);

            if (points.Count >= 3)
            {
                float distance1 = Vector2.Distance(points[points.Count - 1], points[points.Count - 2]);
                float distance2 = Vector2.Distance(points[points.Count - 2], points[points.Count - 3]);

                // TODO: check olderDirection to figure out which side is
                // additive
                if (cutDirection == cw(oldDirection)) {
                    turnsies += distance1 * distance2;
                } else if (cutDirection == ccw(oldDirection)) {
                    turnsies -= distance1 * distance2;
                }
            }
        }
    }

    private void cutTowardsMiddle()
    {
        switch (cutDirection)
        {
            case Directions.Left:
                moveLeft();
                break;
            case Directions.Right:
                moveRight();
                break;
            case Directions.Up:
                moveUp();
                break;
            case Directions.Down:
                moveDown();
                break;
        }
    }

    // Moving On Lines
    // ----------------

    private void lineMovement()
    {
        // Direction logic — 8 possible movement combos
        if (currentEdge.Contains(topLine.gameObject))
        {
            cutDirection = Directions.Down;
            moveLeft();
            moveRight();
        }
        if (currentEdge.Contains(bottomLine.gameObject))
        {
            cutDirection = Directions.Up;
            moveLeft();
            moveRight();
        }
        if (currentEdge.Contains(leftLine.gameObject))
        {
            cutDirection = Directions.Right;
            moveUp();
            moveDown();
        }
        if (currentEdge.Contains(rightLine.gameObject))
        {
            cutDirection = Directions.Left;
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

    // ---------------------
    // Play Move Sound
    // ---------------------
    public void playMoveSound()
    {
        if (!AudioManager.Instance.isPlaying("MovingPlayer"))
        {
            AudioManager.Instance.Play("MovingPlayer");
        }
    }



}

