using UnityEngine;

public class SparcMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float speed;
    [SerializeField] public EdgeCollider2D edgeCollider; // Drag in your perimeter object
    [SerializeField] private GameManager gameManager;
    private Rigidbody2D rb;

    public bool isOnEdge = true; // this will be used for unsnapping the player from the main lines so they can cut the board

    private Vector2[] worldEdgePoints;
    private int currentTargetIndex = 1; // Start by moving from 0 -> 1
    private float segmentProgress = 0f; // 0 to 1 along the edge
    private bool useRandomMovement = true; // Toggle random behavior



    private void Start()
    {
        if (edgeCollider != null)
        {
            var localPoints = edgeCollider.points;
            worldEdgePoints = new Vector2[localPoints.Length];
            for (int i = 0; i < localPoints.Length; i++)
            {
                worldEdgePoints[i] = edgeCollider.transform.TransformPoint(localPoints[i]);
            }
        }

        // Snap to the edge
        snapSparcOnEdge();

        // Now set which edge segment to start moving along
        SetClosestEdgeSegment();
    }


    private void Update()
    {
        sparcMove();
    }

    public void Initialize(float speed)
    {
        rb = GetComponent<Rigidbody2D>();
        this.speed = speed;
    }

    public void sparcMove()
    {
        if (!useRandomMovement || worldEdgePoints == null || worldEdgePoints.Length < 2)
            return;

        Vector2 start = worldEdgePoints[currentTargetIndex - 1];
        Vector2 end = worldEdgePoints[currentTargetIndex];

        segmentProgress += Time.deltaTime * speed / Vector2.Distance(start, end);
        transform.position = Vector2.Lerp(start, end, segmentProgress);

        if (segmentProgress >= 1f)
        {
            segmentProgress = 0f;

            // Move to next edge — wrap around if at the end
            currentTargetIndex++;
            if (currentTargetIndex >= worldEdgePoints.Length)
            {
                currentTargetIndex = 1; // Loop back to beginning (0 -> 1)
            }

        }

        snapSparcOnEdge(); // optional: re-snap to ensure alignment
    }


    public void playMoveSound()
    {
        if (AudioManager.Instance.isPlaying("MovingPlayer"))
        {
            AudioManager.Instance.Play("MovingPlayer");
        }
    }

    // Sparc Snapping Tech
    private void snapSparcOnEdge()
    {
        if (isOnEdge && edgeCollider != null)
        {
            Vector2 closest = GetClosestPointOnEdge(transform.position, edgeCollider);
            transform.position = closest;
        }
    }


    //Same code as the player movement
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

    private void SetClosestEdgeSegment()
    {
        float minDist = Mathf.Infinity;
        int closestIndex = 1; // start at 1 because we use [i - 1] and [i]

        for (int i = 1; i < worldEdgePoints.Length; i++)
        {
            Vector2 start = worldEdgePoints[i - 1];
            Vector2 end = worldEdgePoints[i];
            Vector2 proj = Proj_ab_aplayer(transform.position, start, end);
            float dist = Vector2.Distance(transform.position, proj);
            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }

        currentTargetIndex = closestIndex;
        segmentProgress = 0f;
    }


    //Same code as the player movement just to make sure its on the 
    private Vector2 Proj_ab_aplayer(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a; // edge we are projecting onto
        float t = Vector2.Dot(point - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t); // ensures the player doesn't move beyond the length of the edge it is currently on
        return a + t * ab;
    }


}
