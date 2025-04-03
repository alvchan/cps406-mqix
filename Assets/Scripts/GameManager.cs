using System.Collections;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Scripts
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ButtonManager buttonManager;

    // GameObjects
    [SerializeField] private GameObject PauseScreenPopUp;
    [SerializeField] private GameObject claimedPercentText;
    [SerializeField] private GameObject totalPercentText;

    // Game Loop States
    private enum GameState
    {
        Initialize,
        Playing,
        TransitionScreen,
        GameOver
    }

    // Variables
    private bool isGameOver = false;
    private GameState currentState = GameState.Initialize;
    private string totalPercent = "75%";

    

    // Start
    void Start()
    {
        InitializeAll();
    }

    // Dump all external Initialize methods in here
    private void InitializeAll()
    {
        playerMovement.Initialize();
        Initialize();
    }

    // Update
    void Update()
    {
        gameStateMachine();
    }

    private void gameStateMachine()
    {
        if (!isGameOver)
        {
            switch (currentState)
            {
                case GameState.Initialize:
                    gameInitialize();
                    break;
                case GameState.Playing:
                    playerMovement.playerMove();
                    // all other game related methods during playtime
                    break;
                case GameState.TransitionScreen:
                    gameTransition();
                    break;
                case GameState.GameOver:
                    gameOver();
                    break;
            }
        }
        else
        {
            buttonManager.MainMenu();
        }
    }

    // Helper Methods

    private void gameInitialize()
    {
        // setup the game scene 
        // i.e. set the score to 0 and all that good beautiful stuff
        claimedPercentText.GetComponent<TMP_Text>().text = "0%";
        totalPercentText.GetComponent<TMP_Text>().text = totalPercent;
        currentState = GameState.Playing;
    }

    private void gameTransition()
    {
        // transition screen
    }

    private void gameOver()
    {
        isGameOver = true;
    }
    private void Initialize()
    {
        PauseScreenPopUp.SetActive(false);
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
}
