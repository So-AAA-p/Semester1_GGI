using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BreakOut
{
    public class BO_Manager : MonoBehaviour
    {

        public enum GameState
        {
            Start,
            Stage1,
            Stage2,
            Boss,
            Transition,
            GameOver
        }

        private GameState currentState = GameState.Start;

        public static BO_Manager instance;

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

        [Header("Ball Control")]
        public int maxBalls = 3;
        public float minBallScale = 0.6f;

        private List<BO_Ball> activeBalls = new List<BO_Ball>();

        public bool controlsReversed = false;

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
            currentState = GameState.Stage1;
            gameStartObject.SetActive(false);
            SpawnBall();
        }

        public void OnDeath()
        {
            if (!IsPlayableState())
                return;
            deaths++;

            UpdateDeathCount();

            if (deaths == lives)
                GameOver();

            else
                SpawnBall();
        }

        bool IsPlayableState()
        {
            return currentState == GameState.Stage1
                || currentState == GameState.Stage2
                || currentState == GameState.Boss;
        }

        public void SpawnBall()
        {
            if (currentState != GameState.Stage1 &&
                currentState != GameState.Stage2 &&
                currentState != GameState.Boss)
                return;

            if (activeBalls.Count >= maxBalls)
                return;

            GameObject ballObj = Instantiate(BallPrefab);
            BO_Ball ball = ballObj.GetComponent<BO_Ball>();

            activeBalls.Add(ball);
        }
        public void RemoveBall(BO_Ball ball)
        {
            if (activeBalls.Contains(ball))
                activeBalls.Remove(ball);

            Destroy(ball.gameObject);

            // Each ball costs a life
            deaths++;
            UpdateDeathCount();

            if (deaths >= lives)
            {
                GameOver();
                return;
            }

            // Ensure at least one ball exists
            if (activeBalls.Count == 0)
            {
                SpawnBall();
            }
        }

        public void UpdateDeathCount()
        {
            scoretext.text = "Leben: " + (lives - deaths);
        }

        void GameOver()
        {
            currentState = GameState.GameOver;
            gameOverObject.SetActive(true);
            losertext.text = "You lost :(";

            ClearAllBalls();
        }
        void ClearAllBalls()
        {
            foreach (BO_Ball ball in activeBalls)
            {
                if (ball != null)
                    Destroy(ball.gameObject);
            }

            activeBalls.Clear();
        }

        public int GetActiveBallCount()
        {
            return activeBalls.Count;
        }

        public void RegisterBall(BO_Ball ball)
        {
            if (!activeBalls.Contains(ball))
                activeBalls.Add(ball);
        }


        public void ToggleControls()
        {
            controlsReversed = !controlsReversed;
            Debug.Log($"[Salt] Controls reversed = {controlsReversed}");
        }

        public void OnStageCleared()
        {
            if (currentState != GameState.Stage1)
                return;

            Debug.Log("[Game] Stage 1 complete!");

            gameOverObject.SetActive(true);

            // Destroy all active balls
            GameObject[] balls = GameObject.FindGameObjectsWithTag("BreakOutBall");
            foreach (GameObject ball in balls)
            {
                Destroy(ball);
            }

            currentState = GameState.Transition;

            // TEMP: simple placeholder behavior
            // Later this will trigger the sliding background, stage 2 spawn, etc.
        }

    }
}
