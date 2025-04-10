using System.Collections.Generic;
using UnityEngine;

public class SparcSpawner : MonoBehaviour
{

    [SerializeField] private GameObject Sparc;
    [SerializeField] private EdgeCollider2D edgeCollider;
    private List<GameObject> currentSparc = new List<GameObject>();

    private float sparcSpeed = 6;
    private int i = 0;

    public void SetSparcSpeed(float speed)
    {
        sparcSpeed = speed;
    }

    public void Start()
    {
       SpawnSparc(1);
    }

    private Vector2 GetRandomPointOnEdge()
    {

        Vector2[] points = edgeCollider.points;
        int segmentIndex = Random.Range(0, points.Length - 1); // pick a random segment

        Vector2 p1 = edgeCollider.transform.TransformPoint(points[segmentIndex]);
        Vector2 p2 = edgeCollider.transform.TransformPoint(points[segmentIndex + 1]);

        float t = Random.Range(0f, 1f); // random point along the segment
        return Vector2.Lerp(p1, p2, t); // interpolate between p1 and p2
    }

    public List<GameObject> GetCurrentSparc()
    {
        return currentSparc;
    }
    public void SpawnSparc(int num)
    {
        // Spawn a new Sparc object at a random position
        for (; i < num; i++)
        {
            Vector2 randomSpawnPosition = GetRandomPointOnEdge();
            GameObject sparcClone = Instantiate(Sparc, randomSpawnPosition, Quaternion.identity);
            currentSparc.Add(sparcClone);
            SparcMovement enemyMovement = sparcClone.GetComponent<SparcMovement>();
            enemyMovement.edgeCollider = edgeCollider;
            enemyMovement.Initialize(sparcSpeed);

        }
    }

    public void DestroySparc()
    {
        foreach (GameObject sparc in currentSparc)
        {
            GameObject.Destroy(sparc);
        }
    }



}
