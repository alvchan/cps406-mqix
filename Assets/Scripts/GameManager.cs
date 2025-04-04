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

    // Text Mesh Pro
    [SerializeField] private TMP_Text claimedPercentText;
    [SerializeField] private TMP_Text totalPercentText;

    // Line Renderer
    [SerializeField] private LineRenderer lr; // line renderer
    [SerializeField] private LineRenderer plr; // player line renderer

    /* Game Board Calculations */
    // The Play Area game object is 8x8 World Space units.
    // The Player game object is scaled down by 0.4 so we need to account for this when calculating the Player Line Renderer calculations.
    // The LineRenderer game object is at a scale of 1 so we can just use the regular 1:1 World space unit for every calculation.


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
