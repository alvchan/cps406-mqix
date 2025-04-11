using UnityEngine;

public class SparcScatter : SparcBehavior
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Node node = collision.GetComponent<Node>();

        if (node != null && this.enabled)
        {
            int index = Random.Range(0, node.availableDirections.Count);

            if (node.availableDirections[index] == -this.sparc.movement.currentDirection && node.availableDirections.Count > 1) 
            {
                index++;

                if (index >= node.availableDirections.Count)
                {
                    index = 0;
                }

                this.sparc.movement.SetCurrentNode(node); // Optional: re-snap to the node
                this.sparc.movement.scatterMode = true;   // enable scatter
            }
        }
    }
}