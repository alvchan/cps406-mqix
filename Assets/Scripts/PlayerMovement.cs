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

    [SerializeField] private GameManager gameManager;

    public bool isOnEdge = true; // this will be used for unsnapping the player from the main lines so they can cut the board
    private Directions currentDirection = Directions.Up;
    private enum Directions
    {
        Left,
        Right,
        Up,
        Down
    }
    public LinkedList<GameObject> currentEdge = new LinkedList<GameObject>();
    public void Initialize()
    {
        // leave anything we need to initialize in here
    }
    public void PlayerMove()
    {


        if (currentEdge.ElementAt(0).layer == 7)
        {
            // top
            if (currentEdge.Contains(topLine.gameObject))
            {
                snapPlayerOnEdge(topLine);
                currentDirection = Directions.Down;
                moveLeft();
                moveRight();
            }
            // bottom
            if (currentEdge.Contains(bottomLine.gameObject))
            {
                snapPlayerOnEdge(bottomLine);
                currentDirection = Directions.Up;
                moveLeft();
                moveRight();
            }
            // left
            if (currentEdge.Contains(leftLine.gameObject))
            {
                snapPlayerOnEdge(leftLine);
                currentDirection = Directions.Right;
                moveUp();
                moveDown();
            }
            // right
            if (currentEdge.Contains(rightLine.gameObject))
            {
                snapPlayerOnEdge(rightLine);
                currentDirection = Directions.Left;
                moveUp();
                moveDown();
            }
        }

    }

    



    // Player Snapping Tech
    private void snapPlayerOnEdge(EdgeCollider2D edge)
    {
        if (isOnEdge && edge != null)
        {
            Vector2 closest = GetClosestPointOnEdge(transform.position, edge);
            transform.position = closest;
        }
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

    private void OnCollisionEnter2D (Collision2D collision)
    {
        if (collision.gameObject.layer == 3) gameManager.GameOver();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!currentEdge.Contains(collision.gameObject) && currentEdge.Count < 2) currentEdge.AddLast(collision.gameObject);
        else if (currentEdge.Count > 1) currentEdge.RemoveLast();
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


