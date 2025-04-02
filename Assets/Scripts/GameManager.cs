using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Scripts
    [SerializeField] private PlayerMovement playerMovement;

    // GameObjects
    [SerializeField] GameObject PauseScreenPopUp;


    void Start()
    {
        Initialize();
        playerMovement.Initialize();
    }
    void Update()
    {
        playerMovement.playerMove();
    }
    public void PauseMenu()
    {
        Time.timeScale = 0;
        PauseScreenPopUp.SetActive(true);
    }

    public void UnPauseMenu()
    {
        Time.timeScale = 1;
        PauseScreenPopUp.SetActive(false);
    }
    private void Initialize()
    {
        PauseScreenPopUp.SetActive(false);
    }
}
