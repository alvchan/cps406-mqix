using System.Threading;
using UnityEngine;

public class QixBounce : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 lastVelocity;


    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void UpdateVelocity()
    {
        lastVelocity = rb.linearVelocity;
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
}
