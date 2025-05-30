using System.Collections;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Scripts
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ButtonManager buttonManager;
    [SerializeField] private QixSpawner qixSpawner;
    [SerializeField] private Progression progression;
    

    private int Lives = 3;

    // GameObjects
    [SerializeField] private GameObject PauseScreenPopUp;

    // Text Mesh Pro
    [SerializeField] private TMP_Text claimedPercentText;
    [SerializeField] private TMP_Text totalPercentText;

    //Death Transition and Game Over transition animation
    [SerializeField] private Animator transition;
    [SerializeField] private Animator gameOverTransition;

    [SerializeField] public Image[] hearts;
    public Sprite lostHeart;

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

    [SerializeField] private int qixNumber;

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

        //TODO: set sparx speed and spawn sparx (just like qix)
        qixSpawner.SetQixSpeed(progression.getQixSpeed());
        qixSpawner.SpawnQix(qixNumber);

        claimedPercentText.GetComponent<TMP_Text>().text = "0%";
        totalPercentText.GetComponent<TMP_Text>().text = totalPercent;
        currentState = GameState.Playing;
    }

    // all other game related methods during playtime
    private void gamePlaying()
    {
        playerMovement.PlayerMove();
        qixSpawner.UpdateVelocity();
        //TODO: Update velocity for sparx
        //TODO: Call a method for checking if we've completed the level (or have this method called on completion of a "cut")
            //If we have completed it, call `completeLevel()`
    }

    private void gameTransition()
    {
        qixSpawner.DestroyQix();
        qixSpawner.SetQixSpeed(progression.getQixSpeed());
    }

    private void gameOverScreen()
    {
        isGameOver = true;
    }
    public void GameOver()
    {
        currentState = GameState.GameOver;
    }

    private void completeLevel() {
        //TODO: If we handle the "transition screen" logic in `gameTransition()` then this is probably all we need
        progression.incrementLevel(); //Note this returns the *new* level number in case we want to work that into GUI somewhere
        currentState = GameState.TransitionScreen; 
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

    public IEnumerator loseLife()
    {
        Time.timeScale = 0;
        if (Lives == 1)
        {
            AudioManager.Instance.Play("PlayerDeath");
            gameOverTransition.Play("EndTransition");
            AudioManager.Instance.Pause("MovingPlayer");
            AudioManager.Instance.Stop("GameSong");
            yield return new WaitForSecondsRealtime(4.3f);
            GameOver();
        }
        else {
            AudioManager.Instance.Play("PlayerHit");
            transition.Play("DeathTransition");
            AudioManager.Instance.Pause("MovingPlayer");
            yield return new WaitForSecondsRealtime(1.0f);
            playerMovement.SnapPlayerOnEdges(playerMovement.currentEdge);
            qixSpawner.DestroyQix();
            if (Lives == 3)
            {
                hearts[0].sprite = lostHeart;
            }
            else if (Lives == 2)
            {
                hearts[1].sprite = lostHeart;
            }
            qixSpawner.SpawnQix(qixNumber);
            yield return new WaitForSecondsRealtime(1.0f); 
            Lives -= 1;
            Time.timeScale = 1;
            AudioManager.Instance.UnPause("MovingPlayer");
        }
    }

}

