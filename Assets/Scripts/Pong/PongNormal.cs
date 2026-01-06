using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PongNormal : MonoBehaviour
{
    public Button BTMenu;
    
    void Start()
    {
       BTMenu.onClick.AddListener(() => SceneManager.LoadScene(2));
	}

}
