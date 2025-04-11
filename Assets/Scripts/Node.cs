using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{

    [SerializeField] private float checkDistance;
    [SerializeField] private LayerMask edgeLayerMask;
    public List<Vector2> availableDirections {  get; private set; }
    private static readonly Vector2[] cardinalDirections = {
    Vector2.up,
    Vector2.down,
    Vector2.left,
    Vector2.right
    };



    private void Start()
    {
        this.availableDirections = new List<Vector2>();

       

        CheckAvailableDirections();
    }

    public void CheckAvailableDirections()
    {
        availableDirections.Clear();

        foreach (Vector2 dir in cardinalDirections)
        {
            // 🔁 Offset the origin slightly to avoid missing edge collider
            Vector2 rayOrigin = (Vector2)transform.position + dir * 0.05f;

            Debug.DrawRay(rayOrigin, dir * checkDistance, Color.red, 2f);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir, checkDistance, edgeLayerMask);

            if (hit.collider != null)
            {
                Debug.Log($"✅ Hit: {hit.collider.name} in direction {dir}");
                availableDirections.Add(dir);
            }
            else
            {
                Debug.LogWarning($"❌ No hit in direction {dir} from {rayOrigin}");
            }
        }


    }



}


