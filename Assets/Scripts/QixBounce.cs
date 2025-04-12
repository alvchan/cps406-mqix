using System.Threading;
using UnityEngine;

public class QixBounce : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 lastVelocity;

    [Header("Bounce Settings")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float angleJitter = 15f;
    [SerializeField] private float cornerDotThreshold = 0.2f;

    [Header("Stuck Detection")]
    [SerializeField] private float stuckVelocityThreshold = 0.5f;
    [SerializeField] private float stuckTimeThreshold = 1.5f;
    [SerializeField] private float forcedEscapeSpeed = 2f;

    private float stuckTimer = 0.5f;

    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void UpdateVelocity()
    {
        lastVelocity = rb.linearVelocity;

        // Track stuck time
        if (rb.linearVelocity.magnitude < stuckVelocityThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;

            if (stuckTimer >= stuckTimeThreshold)
            {
                ForceEscape();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f; // Reset if moving normally
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var speed = lastVelocity.magnitude;
        var direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);
        Vector2 randomOffset = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));

        direction += (Vector3)randomOffset;
        direction.Normalize();

        rb.linearVelocity = direction * Mathf.Max(speed, 0f);
    }
    private void ForceEscape()
    {
        // Random forceful direction
        float angle = Random.Range(0f, 360f);
        Vector2 escapeDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        rb.linearVelocity = escapeDir * forcedEscapeSpeed;
    }
}
