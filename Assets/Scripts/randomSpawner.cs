using UnityEngine;

public class randomSpawner : MonoBehaviour
{

    public GameObject Qix;


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // Spawn a new Qix object at a random position
            Vector3 randomSpawnPosition = new Vector3(Random.Range(-8, 9),Random.Range(-3, 4), (float)0.04952531);
            Instantiate(Qix, randomSpawnPosition, Quaternion.identity);



        }
    }
}
