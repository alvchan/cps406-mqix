using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SpeedTree.Importer;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
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
    private List<GameObject> currentEdge = new List<GameObject>();

    // Tracks the points of the edges the player creates when cutting
    private LinkedList<Vector3> edges = new LinkedList<Vector3>();

    // Holds the Colliders that the player is currently placing while cutting
    private List<GameObject> tempColliders = new List<GameObject>();
    private List<Vector3> points = new List<Vector3>();

    private bool isOnEdge = true; // this will be used for unsnapping the player from the main lines so they can cut the board
    private bool isCutting = false;
    private bool startedCutting = false;

    // TODO: move this crap to progression or something
    private float area = 0.0f;
    private float turnsies = 0.0f;
    private const float TOTAL_AREA = 64.0f;
    private const float GOAL = 0.75f * TOTAL_AREA;

    private Directions olderDirection = Directions.Right;
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

        for (int i = 0; i < edges.ToList<Vector3>().Count; i++)
        {
            print(edges.ToString());
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

    private void createTrail()
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





    private void createLine()
    {
        GameObject go = new GameObject();
        go.layer = 11;
        EdgeCollider2D edgeCollider = go.AddComponent<EdgeCollider2D>();
        edgeCollider.points = new Vector2[] { edges.First(), edges.Skip(1).First() };
        tempColliders.Add(go);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetWidth(0.05f, 0.05f);
        Vector2[] newEdges = edgeCollider.points;
        lr.SetPosition(0, newEdges[0]);
        lr.SetPosition(1, newEdges[1]);
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    private void tempToMoveable()
    {
        foreach (GameObject collider in tempColliders)
        {
            collider.layer = 7;
            EdgeCollider2D edge = collider.GetComponent<EdgeCollider2D>();
            edge.isTrigger = true;
        }
    }






    private void beginCutting()
    {
        isOnEdge = false;

        if (!startedCutting)
        {
            startedCutting = true;
            edges.AddFirst(transform.position);
            points.Add(transform.position);
            createTrail();
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


    private void snapBackToEdge(Collider2D collision)
    {
        isCutting = false;
        isOnEdge = true;
        edges.AddFirst(transform.position);
        createLine();
        tempToMoveable();

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
        resetLine();
        tempColliders.Clear();

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
        // Continue drawing the line and store the old direction.
        createTrail();
        oldDirection = cutDirection;

        // Gather input booleans (true if that key is down).
        bool upPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool downPressed = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool leftPressed = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rightPressed = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        // Count how many directions are pressed, and propose a new direction.
        int pressedCount = 0;
        Directions newDirection = cutDirection;

        // Check UP (ignore if current direction is DOWN to prevent instant 180° flip).
        if (upPressed && cutDirection != Directions.Down)
        {
            pressedCount++;
            newDirection = Directions.Up;
        }

        // Check DOWN (ignore if current direction is UP).
        if (downPressed && cutDirection != Directions.Up)
        {
            pressedCount++;
            newDirection = Directions.Down;
        }

        // Check LEFT (ignore if current direction is RIGHT).
        if (leftPressed && cutDirection != Directions.Right)
        {
            pressedCount++;
            newDirection = Directions.Left;
        }

        // Check RIGHT (ignore if current direction is LEFT).
        if (rightPressed && cutDirection != Directions.Left)
        {
            pressedCount++;
            newDirection = Directions.Right;
        }

        // Only update cutDirection if exactly ONE direction key was pressed.
        if (pressedCount == 1)
        {
            cutDirection = newDirection;
        }

        // Now move in the chosen direction ONLY if the corresponding key is still pressed.
        switch (cutDirection)
        {
            case Directions.Up:
                if (upPressed) moveUp();
                break;
            case Directions.Down:
                if (downPressed) moveDown();
                break;
            case Directions.Left:
                if (leftPressed) moveLeft();
                break;
            case Directions.Right:
                if (rightPressed) moveRight();
                break;
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
            edges.AddFirst(transform.position);
            createLine();
            points.Add(transform.position);

            if (points.Count >= 3)
            {
                float distance1 = Vector2.Distance(points[points.Count - 1], points[points.Count - 2]);
                float distance2 = Vector2.Distance(points[points.Count - 2], points[points.Count - 3]);

                // TODO: check olderDirection to figure out which side is
                // additive
                // omg i never set older direction, but it already sort of works rn
                if (oldDirection == cw(olderDirection)) {
                    if (cutDirection == cw(oldDirection)) {
                        turnsies += distance1 * distance2;
                    } else if (cutDirection == ccw(oldDirection)) {
                        turnsies -= distance1 * distance2;
                    }
                } else if (oldDirection == ccw(oldDirection)) {
                    if (cutDirection == ccw(oldDirection)) {
                        turnsies += distance1 * distance2;
                    } else if (cutDirection == cw(oldDirection)) {
                        turnsies -= distance1 * distance2;
                    }
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
