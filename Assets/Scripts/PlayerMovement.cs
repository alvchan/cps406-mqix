using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private EdgeCollider2D topLine;
    [SerializeField] private EdgeCollider2D bottomLine;
    [SerializeField] private EdgeCollider2D leftLine;
    [SerializeField] private EdgeCollider2D rightLine;

    [SerializeField] private EdgeCollider2D playerLine;
    [SerializeField] private LineRenderer playerTrail;

    [SerializeField] private GameManager gameManager;

    private GameObject pendingEdge = null;

    public bool isOnEdge = true; // this will be used for unsnapping the player from the main lines so they can cut the board
    private Directions currentDirection = Directions.Up;
    private enum Directions
    {
        Left,
        Right,
        Up,
        Down
    }
    private List<GameObject> currentEdge = new List<GameObject>();
    
    public void Initialize()
    {
        currentEdge.Add(bottomLine.gameObject);
        currentEdge.Add(null);
    }
    public void PlayerMove()
    {
        // Don’t move unless touching an edge
        if (!isOnEdge || currentEdge[0] == null || currentEdge[0].layer != 7)
            return;

        // Handle pending edge swap only if player is intentionally trying to move
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

                Debug.Log("Switched to edge: " + currentEdge[0].name);
            }
        }

        SnapPlayerOnEdges(currentEdge);

        // Direction logic — 8 possible movement combos
        if (currentEdge.Contains(topLine.gameObject))
        {
            currentDirection = Directions.Down;
            moveLeft();
            moveRight();
        }
        if (currentEdge.Contains(bottomLine.gameObject))
        {
            currentDirection = Directions.Up;
            moveLeft();
            moveRight();
        }
        if (currentEdge.Contains(leftLine.gameObject))
        {
            currentDirection = Directions.Right;
            moveUp();
            moveDown();
        }
        if (currentEdge.Contains(rightLine.gameObject))
        {
            currentDirection = Directions.Left;
            moveUp();
            moveDown();
        }
    }






    // Player Snapping Tech
    private void SnapPlayerOnEdges(List<GameObject> edges)
    {
        if (!isOnEdge || edges == null || edges.Count == 0)
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3) gameManager.GameOver();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentEdge.Contains(collision.gameObject)) return;

        // Buffer the new edge, don't apply immediately
        if (collision.gameObject.layer == 7)
        {
            pendingEdge = collision.gameObject;
            Debug.Log("Buffered edge: " + pendingEdge.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == pendingEdge)
        {
            pendingEdge = null;
            Debug.Log("Canceled buffered edge: " + collision.gameObject.name);
        }

        if (currentEdge[0] == collision.gameObject)
        {
            currentEdge[0] = currentEdge[1];
            currentEdge[1] = null;
        }
        else if (currentEdge[1] == collision.gameObject)
        {
            currentEdge[1] = null;
        }

        Debug.Log("Exited: " + collision.gameObject.name);
        Debug.Log("Edge 0: " + currentEdge[0]?.name);
        Debug.Log("Edge 1: " + currentEdge[1]?.name);
    }






    // helpers 

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


    public void playMoveSound()
    {
        if (AudioManager.Instance.isPlaying("MovingPlayer"))
        {
            AudioManager.Instance.Play("MovingPlayer");
        }
    }



}

