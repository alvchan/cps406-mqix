using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Scripts
    [SerializeField] private PlayerMovement playerMovement;



    void Start()
    {
        playerMovement.Initialize();
    }
    void Update()
    {
        
    }
}
