using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager: MonoBehaviour
{

    public void MainMenu()
    {
        AudioManager.Instance.Play("Button");
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            AudioManager.Instance.Stop("GameSong");
            AudioManager.Instance.Play("MenuSong");
        }
        SceneManager.LoadScene(0);
        Time.timeScale = 1;

    }
    public void PlayGame()
    {
        AudioManager.Instance.Play("Button");
        AudioManager.Instance.Stop("MenuSong");
        SceneManager.LoadScene(1);

    }
    public void Options()
    {
        AudioManager.Instance.Play("Button");
        SceneManager.LoadScene(2);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Mute()
    {
        AudioManager.Instance.getSound("GameSong").source.mute = true;
    }
    public void UnMute()
    {
        AudioManager.Instance.getSound("GameSong").source.mute = false;

    }

}
