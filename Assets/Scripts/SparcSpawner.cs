using System.Collections.Generic;
using UnityEngine;

public class SparcSpawner : MonoBehaviour
{
    [SerializeField] private GameObject sparcPrefab;
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float maxSpeed = 6f;

    private List<GameObject> currentSparcs = new List<GameObject>();
    private Node[] allNodes;

    private void Start()
    {
        allNodes = FindObjectsOfType<Node>();
        SpawnSparcs(1); // Spawn 1 by default
    }

    public void SpawnSparcs(int num)
    {

        allNodes = FindObjectsOfType<Node>();

        if (allNodes == null || allNodes.Length == 0)
        {
            Debug.LogError("SparcSpawner: No Nodes found in the scene!");
            return;
        }

        for (int i = 0; i < num; i++)
        {
            Node spawnNode = allNodes[Random.Range(0, allNodes.Length)];
            Vector2 spawnPosition = spawnNode.transform.position;

            GameObject sparc = Instantiate(sparcPrefab, spawnPosition, Quaternion.identity);
            currentSparcs.Add(sparc);

            SparcMovement movement = sparc.GetComponent<SparcMovement>();
            movement.speed = Random.Range(minSpeed, maxSpeed);
            // movement.SetCurrentNode(spawnNode); // Uncomment if you want to assign starting node
        }
    }

    public void RespawnSparc(GameObject oldSparc)
    {
        if (oldSparc != null)
        {
            Destroy(oldSparc);
            currentSparcs.Remove(oldSparc);
        }

        // Reuse SpawnSparcs(1) to spawn a new one
        SpawnSparcs(1);
    }

    public void DestroyAllSparcs()
    {
        foreach (GameObject sparc in currentSparcs)
        {
            Destroy(sparc);
        }
        currentSparcs.Clear();
    }
}
