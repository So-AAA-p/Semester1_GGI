using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PongMenu : MonoBehaviour
{
    public Button NormalMode;
    public Button HardMode;
    public Button OIIAMode;
    public Button BackToMainMenu;

	void Start()
    {
        NormalMode.onClick.AddListener(() => SceneManager.LoadScene(3));
        HardMode.onClick.AddListener(() => SceneManager.LoadScene(4));
        OIIAMode.onClick.AddListener(() => SceneManager.LoadScene(5));
        BackToMainMenu.onClick.AddListener(() => SceneManager.LoadScene(0));
	}
}
