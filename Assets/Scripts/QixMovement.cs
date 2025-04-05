using UnityEngine;

public class QixMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    public void Initialize(float speed)
    {
        rb = GetComponent<Rigidbody2D>();
        SetVelocity(speed);
    }

    private void SetVelocity(float speed)
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);

        Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

        rb.linearVelocity = randomDirection * speed;
    }



}
