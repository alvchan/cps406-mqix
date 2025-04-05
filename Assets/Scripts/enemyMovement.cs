using UnityEngine;

public class enemyMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
        SetVelocity();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void SetVelocity()
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);

        Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));

        rb.linearVelocity = randomDirection * speed;
    }



}
