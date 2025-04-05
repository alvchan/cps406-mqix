using System.Threading;
using UnityEngine;

public class ballBounce : MonoBehaviour
{

    private Rigidbody2D rb;
    Vector3 lastVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
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
