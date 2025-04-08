using System.Collections;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    // Scripts
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ButtonManager buttonManager;
    [SerializeField] private QixSpawner qixSpawner;
    [SerializeField] private Progression progression;
    



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
    [SerializeField] private string totalPercent;
    [SerializeField] private float qixSpeed;
    [SerializeField] private int qixNumber; // Keep this to one Qix for now and only adjust the speed.
    private bool isGameOver = false;
    private GameState currentState = GameState.Initialize;

    // Start
    void Start()
    {
        InitializeAll();
    }

    // Dump all external Initialize methods in here

    private void InitializeAll()
    {
        Initialize();
        playerMovement.Initialize();
    }

    private void Initialize()
    {
        PauseScreenPopUp.SetActive(false);
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
                    gamePlaying();
                    break;
                case GameState.TransitionScreen:
                    gameTransition();
                    break;
                case GameState.GameOver:
                    gameOverScreen();
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
        AudioManager.Instance.Play("GameSong");
        qixSpawner.SetQixSpeed(qixSpeed);
        claimedPercentText.GetComponent<TMP_Text>().text = "0%";
        totalPercentText.GetComponent<TMP_Text>().text = totalPercent;
        currentState = GameState.Playing;
    }

    // all other game related methods during playtime
    private void gamePlaying()
    {
        playerMovement.playerMove();
        qixSpawner.SpawnQix(qixNumber);
        qixSpawner.UpdateVelocity();
}

    private void gameTransition()
    {
        qixSpawner.DestroyQix();
        qixSpawner.SetQixSpeed(qixSpeed);
    }

    private void gameOverScreen()
    {
        isGameOver = true;
    }
    public void GameOver()
    {
        currentState = GameState.GameOver;
    }




    public void PauseMenu()
    {
        Time.timeScale = 0;
        AudioManager.Instance.Pause("MovingPlayer");
        PauseScreenPopUp.SetActive(true);
    }

    public void UnPauseMenu()
    {
        Time.timeScale = 1;
        AudioManager.Instance.UnPause("MovingPlayer");
        PauseScreenPopUp.SetActive(false);
    }


}
