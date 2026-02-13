using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Pong
{
public class PongMenu : MonoBehaviour
{
    public Button ClassicMode;
    public Button UnderWaterMode;
    public Button BallTypesMode;
    public Button BackToMainMenu;

	void Start()
    {
        ClassicMode.onClick.AddListener(() => SceneManager.LoadScene(3));
        UnderWaterMode.onClick.AddListener(() => SceneManager.LoadScene(4));
        BallTypesMode.onClick.AddListener(() => SceneManager.LoadScene(5));
        BackToMainMenu.onClick.AddListener(() => SceneManager.LoadScene(0));
	}
}

}
