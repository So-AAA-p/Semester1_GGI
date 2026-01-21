using UnityEngine;
using UnityEngine.SceneManagement;
// using UnityEngine.UI;

public class ScenesManager : MonoBehaviour
{
    // public Button TTB;
    //  public Button Pong;
    // public Button Exit;

    // private void Start()
    // {
    // TTB.onClick.AddListener(() => SceneManager.LoadScene(1));
    //  Pong.onClick.AddListener(() => SceneManager.LoadScene(2));
    //Exit.onClick.AddListener(() => Application.Quit());
    // }

    // [MENU]
    public void OpenTTB()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenPongMenu()
    {
        SceneManager.LoadScene(2);
    }

    public void OpenBreakOut()
    {
        SceneManager.LoadScene(6);
    }

    public void Quit()
    {
        Application.Quit();
    }

    // [PONGDIFS]
    public void OpenPongClassic()
    {
        SceneManager.LoadScene(3);
    }

    public void OpenPongUnderWater()
    {
        SceneManager.LoadScene(4);
    }

    public void OpenPongBallTypes()
    {
        SceneManager.LoadScene(5);
    }

    // [PONG]
    public void BackToPongMenu()
    {
        SceneManager.LoadScene(2);
    }

    // [COMMON]
    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
