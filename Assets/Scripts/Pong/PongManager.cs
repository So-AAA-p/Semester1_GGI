using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Pong
{
    public class PongManager : MonoBehaviour
    {
        public enum Player
        {
            Player1,
            Player2
        }

        public enum GameState
        {
            Start,
            Playing,
            GameDone
        }
        
        public enum LevelType
        {
            Classic,
            UnderWater,
            BallTypes
        }

        public GameState currentState = GameState.Start;
        public LevelType currentLevel = LevelType.Classic;

        public static PongManager instance;
        public GameObject gameOverObject;
        public GameObject gameStartObject;
        public GameObject enter;
        public float ballstartvelocity = 5;
        public TextMeshProUGUI winnertext;

        [Header("UI References")]
        public TMP_Text mainScoreText;      // Your current single text field (Level 1 & 3)
        public TMP_Text player1ScoreText;   // New field for Level 2
        public TMP_Text player2ScoreText;   // New field for Level 2

        private uint player1Score;                                                          // uint wie normales int, aber ohne negativen Zahlen
        private uint player2Score;

        public int maxPoints = 5;

        public Transform leftPaddle;
        public Transform rightPaddle;

        public float paddleSpawnOffset = 0.25f;
        public float serveDelay = 1.0f;

        [Header("Ball Prefabs")]
        public GameObject classicBallPrefab;
        public GameObject underwaterBallPrefab;
        public GameObject[] ballTypePrefabs;



        private void Awake()                                                                // Awake() wird vor Start() ausgeführt; nimmt man, wenn Reihenfolgen eine Rolle spielen
        {
            if (instance == null)
                instance = this;
            else
            {
                Destroy(gameObject);
                Debug.LogWarning("PongManager already exists. Destroying LOGIC");
            }

        }


        void Start()
        {
            gameOverObject.SetActive(false);
            gameStartObject.SetActive(true);
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
                //Debug.Log("ENTER PRESSED");

            if (currentState == GameState.Start && Input.GetKeyDown(KeyCode.Return))        //GetKeyDown gilt nur für einen frame, also passiert nichts beim halten der taste
            {
                StartGame();
            }
        }


        IEnumerator SpawnBallWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetBall();
        }


        void StartGame()
        {
            currentState = GameState.Playing;
            gameStartObject.SetActive(false);

            player1Score = 0;
            player2Score = 0;
            UpdateScoreUI();

            StartCoroutine(SpawnBallWithDelay(1.5f));
            ConfigureLevel();
        }

        public void SpawnBallAtPaddle(GameObject ballPrefab)
        {
            Transform chosenPaddle;
            Vector2 launchDirection;

            if (Random.value < 0.5f)
            {
                chosenPaddle = leftPaddle;
                launchDirection = Vector2.right;
            }
            else
            {
                chosenPaddle = rightPaddle;
                launchDirection = Vector2.left;
            }

            Vector3 spawnPos = chosenPaddle.position +
                (Vector3)(launchDirection * paddleSpawnOffset);

            GameObject ballObj = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
            PongBall ball = ballObj.GetComponent<PongBall>();

            ball.PrepareServe(launchDirection);
        }


        public void OnGoalScored(Player player)
        {
            if (currentState != GameState.Playing)                                          // beugt vor, das ein Tor gemacht werden kann, wenn das spiel nicht läuft
                return;

            if (player == Player.Player1)
                player1Score++;
            else
                player2Score++;

            UpdateScoreUI();

            if (player1Score == maxPoints)
                GameOver(Player.Player1);
            else if (player2Score == maxPoints)
                GameOver(Player.Player2);
            else
                ResetBall();
        }



        public void ResetBall()
        {
            if (currentState != GameState.Playing)
                return;

            GameObject prefabToSpawn = null;

            switch (currentLevel)
            {
                case LevelType.Classic:
                    prefabToSpawn = classicBallPrefab;
                    break;

                case LevelType.UnderWater:
                    prefabToSpawn = underwaterBallPrefab;
                    break;

                case LevelType.BallTypes:
                    prefabToSpawn = GetRandomBallTypePrefab();

                    if (prefabToSpawn != null)
                    {
                        Debug.Log($"[LEVEL 3] Spawned ball prefab: {prefabToSpawn.name}");
                        SpawnBallAtPaddle(prefabToSpawn);
                    }
                    return; // ⬅ IMPORTANT: stop here, don't Instantiate below
            }

            if (prefabToSpawn == null)
            {
                Debug.LogWarning("No ball prefab assigned for level " + currentLevel);
                return;
            }

            Instantiate(prefabToSpawn);
        }



        //if (!(currentState == GameState.Playing)) return;                             // ! ist einfach verneinung; !GameStart = game hat noch nicht begonnen
        // Instantiate(BallPrefab);                                                     // instantiate immer notwendig, wenn man etwas in der Szene erscheinen lassen will, was davor noch nicht da war
        // hinter dem BallPrefab, kann man noch eine Position mit angeben

        GameObject GetRandomBallTypePrefab()
        {
            if (ballTypePrefabs == null || ballTypePrefabs.Length == 0)
            {
                Debug.LogWarning("No ballTypePrefabs assigned!");
                return null;
            }

            int index = Random.Range(0, ballTypePrefabs.Length);
            return ballTypePrefabs[index];
        }


        void UpdateScoreUI()
        {
            // SAFETY: If mainScoreText is missing in the Inspector, don't crash the game!
            if (mainScoreText == null || player1ScoreText == null || player2ScoreText == null)
            {
                Debug.LogError("UI Text references are missing in the PongManager Inspector!");
                return;
            }

            if (currentLevel == LevelType.UnderWater)
            {
                mainScoreText.gameObject.SetActive(false);
                player1ScoreText.gameObject.SetActive(true);
                player2ScoreText.gameObject.SetActive(true);

                player1ScoreText.text = player1Score.ToString();
                player2ScoreText.text = player2Score.ToString();
            }
            else
            {
                mainScoreText.gameObject.SetActive(true);
                player1ScoreText.gameObject.SetActive(false);
                player2ScoreText.gameObject.SetActive(false);

                mainScoreText.text = player1Score + " - " + player2Score;
            }
        }


        void GameOver(Player winner)
        {
            gameOverObject.SetActive(true);
            winnertext.text = winner.ToString() + "won! Congrats :]";
        }


            //  void ConfigureBall(PongBall ball)
            //  {
            //      ball.canShrink = false;
            //      ball.isLiquid = false;
            //      ball.SetLiquidVisual(false);

            //      switch (currentLevel)
            //          {
            //              case LevelType.Classic:
            //              ball.canShrink = true;
            //              break;

            //              case LevelType.UnderWater:
            //              ball.isLiquid = true;
            //              ball.SetLiquidVisual(true);
            //              break;

            //              case LevelType.BallTypes:
            //              break;
            //          }
            //   }


        void ConfigureLevel()
        {
            switch (currentLevel)
            {
                case LevelType.Classic:
                    ConfigurePaddles(vertical: true);
                    break;

                case LevelType.UnderWater:
                    ConfigurePaddles(vertical: false);
                    break;

                case LevelType.BallTypes:
                    ConfigurePaddles(vertical: true);
                    break;
            }
        }

        void ConfigurePaddles(bool vertical)
        {
            PongPaddle[] allPaddles = FindObjectsOfType<PongPaddle>();
            int paddleLayer = LayerMask.NameToLayer("PongPaddles");

            foreach (PongPaddle paddle in allPaddles)
            {
                if (paddle.gameObject.layer != paddleLayer)
                    continue;

                paddle.moveHorizontally = !vertical;
            }
        }
    }
}
