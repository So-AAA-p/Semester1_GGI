using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace BreakOut
{
    public class BreakOutManager : MonoBehaviour
    {

        public enum GameState
        {
            Start,
            Playing,
            GameDone
        }
        private GameState currentState = GameState.Start;

        public static BreakOutManager instance;

        public GameObject BallPrefab;
        public GameObject gameOverObject;
        public GameObject gameStartObject;
        public GameObject enter;
        public GameObject Heart1, Heart2, Heart3, Heart4, Heart5;

        public float ballstartvelocity = 20;

        public TextMeshProUGUI scoretext;
        public TextMeshProUGUI losertext;

        private uint deaths;                                                                // uint wie normales int, aber ohne negativen Zahlen
        public int lives = 5;

        // private GameState currentState;



        private void Awake()                                                                // Awake() wird vor Start() ausgeführt; nimmt man, wenn Reihenfolgen eine Rolle spielen
        {
            if (instance == null)
                instance = this;
            else
            {
                Destroy(gameObject);
                Debug.LogWarning("BreakOutManager already exists. Destroying LOGIC");
            }

        }

        void Start()
        {
            gameOverObject.SetActive(false);
            gameStartObject.SetActive(true);
        }

        void Update()
        {
            if (currentState == GameState.Start && Input.GetKeyDown(KeyCode.Return))        //GetKeyDown gilt nur für einen frame, also passiert nichts beim halten der taste
            {
                StartGame();
            }
        }

        void StartGame()
        {
            currentState = GameState.Playing;
            gameStartObject.SetActive(false);
            ResetBall();
        }

        public void OnDeath()
        {
            if (currentState != GameState.Playing)                                          // beugt vor, das ein Tor gemacht werden kann, wenn das spiel nicht läuft
                return;
            deaths++;

            UpdateDeathCount();

            if (deaths == lives)
                GameOver();

            else
                ResetBall();


        }

        public void ResetBall()
        {
            if (currentState != GameState.Playing) return;

            Instantiate(BallPrefab);               // Quaternion.identity sorgt dafür, dass der Ball keine Rotation hat
        }

        public void UpdateDeathCount()
        {
            scoretext.text = "Leben: " + (lives - deaths);
        }

        void GameOver()
        {
            gameOverObject.SetActive(true);
            losertext.text = "You lost :(";
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene(0);
        }

    }
}
