using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private EdgeCollider2D edgeCollider; // Drag in your perimeter object
    [SerializeField] private GameManager gameManager;

    public bool isOnEdge = true; // this will be used for unsnapping the player from the main lines so they can cut the board


    public void Initialize()
    {
        // leave anything we need to initialize in here
    }
    public void playerMove()
    {
        snapPlayerOnEdge();

        // scuffed movement code can be fixed later
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            playMoveSound();
            gameObject.transform.position = gameObject.transform.position + new Vector3(0, 1 * speed, 0) * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            playMoveSound();
            gameObject.transform.position = gameObject.transform.position + new Vector3(-1 * speed, 0, 0) * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            playMoveSound();
            gameObject.transform.position = gameObject.transform.position + new Vector3(0, -1 * speed, 0) * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            playMoveSound();
            gameObject.transform.position = gameObject.transform.position + new Vector3(1 * speed, 0, 0) * Time.deltaTime;
        }
        else {
            FindFirstObjectByType<AudioManager>().Stop("MovingPlayer");
        }
        
    }

    public void playMoveSound() {
        if (!FindFirstObjectByType<AudioManager>().isPlaying("MovingPlayer"))
        {
            FindFirstObjectByType<AudioManager>().Play("MovingPlayer");
        }
    }



    // Player Snapping Tech
    private void snapPlayerOnEdge()
    {
        if (isOnEdge && edgeCollider != null)
        {
            Vector2 closest = GetClosestPointOnEdge(transform.position, edgeCollider);
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
        if (collision.gameObject.layer == 3)
            gameManager.GameOver();
    }

}
