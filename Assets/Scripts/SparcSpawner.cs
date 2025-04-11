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
        SpawnSparcs(5); // or however many you want
    }

    public void SpawnSparcs(int num)
    {
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
            movement.SetCurrentNode(spawnNode); // Set node so movement starts correctly
        }
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
