using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public void OpenTTB()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenPongDifs()
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
}
