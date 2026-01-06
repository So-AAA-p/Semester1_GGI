using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PongOIIA : MonoBehaviour
{
    public Button BTMenu;

    void Start()
    {
        BTMenu.onClick.AddListener(() => SceneManager.LoadScene(2));
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
