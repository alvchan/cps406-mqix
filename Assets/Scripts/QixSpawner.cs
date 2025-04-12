using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class QixSpawner : MonoBehaviour
{

    [SerializeField] private GameObject Qix;
    private List<GameObject> currentQix = new List<GameObject>();
    
    private float qixSpeed;
    private int i = 0;

    public void SetQixSpeed(float speed)
    {
        qixSpeed = speed;
    }

    public List<GameObject> GetCurrentQix()
    {
        return currentQix;
    }

    public void UpdateVelocity()
    {
        foreach (GameObject qix in currentQix)
        {
            QixBounce ballBounce = qix.GetComponent<QixBounce>();
            ballBounce.UpdateVelocity();
        }
    }
    

    public void SpawnQix(int num)
    {
        // Spawn a new Qix object at a random position
        for (; i < num; i++)
        {
            Vector3 randomSpawnPosition = new Vector3(Random.Range(-7, 0), Random.Range(-3, 4), 0);
            GameObject qixClone = Instantiate(Qix, randomSpawnPosition, Quaternion.identity);
            currentQix.Add(qixClone);
            QixBounce ballBounce = qixClone.GetComponent<QixBounce>();
            QixMovement enemyMovement = qixClone.GetComponent<QixMovement>();
            ballBounce.Initialize();
            enemyMovement.Initialize(qixSpeed);
        }
    }

    public void DestroyQix()
    {
        i = 0;
        foreach (GameObject qix in currentQix)
        {
            GameObject.Destroy(qix);
        }
    }



}
