using UnityEngine;

public class RaycastTester : MonoBehaviour
{
    public float checkDistance = 10f;
    public LayerMask edgeLayer;

    private void Start()
    {
        Debug.Log("RaycastTester running...");
        Debug.DrawRay(transform.position, Vector2.right * checkDistance, Color.green, 5f);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, checkDistance, edgeLayer);

        if (hit.collider != null)
        {
            Debug.Log($"✅ Ray hit: {hit.collider.name}");
        }
        else
        {
            Debug.LogWarning("❌ Raycast failed — no collider hit.");
        }
    }
}
