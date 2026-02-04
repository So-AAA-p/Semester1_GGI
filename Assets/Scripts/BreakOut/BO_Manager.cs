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
            BakingMiniGame,
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

            // If we are testing Stage 2+ in the Inspector, skip the start screen
            if (BO_StageController.Instance != null &&
                BO_StageController.Instance.currentStage != BO_StageController.StageType.Stage1)
            {
                gameStartObject.SetActive(false);
                // StageController.Start() already calls SetState, so we just need to spawn the ball
                SpawnBall();
            }
            else
            {
                currentState = GameState.Start;
                gameStartObject.SetActive(true);
            }
        }

        public void SetState(GameState s) 
        { 
            currentState = s; 
        
       }

        void Update()
        {
            // ONLY allow Enter to work if we are in the very first Start state
            if (currentState == GameState.Start && Input.GetKeyDown(KeyCode.Return))
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
            // Safety check: Don't spawn if we are in transition or have enough balls
            if (currentState == GameState.Transition || activeBalls.Count >= maxBalls)
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
            // 1. HARD GATE: If we aren't in a playable stage, ignore this entirely.
            if (currentState != GameState.Stage1 && currentState != GameState.Stage2)
                return;

            Debug.Log("OnStageCleared called! Current Stage: " + BO_StageController.Instance.currentStage);

            // 2. Set state to Transition immediately to block further calls
            currentState = GameState.Transition;

            if (BO_StageController.Instance.currentStage == BO_StageController.StageType.Stage2 ||
                BO_StageController.Instance.currentStage == BO_StageController.StageType.BakingMinigame)
            {
                BO_StageController.Instance.OnStage2Complete();
            }

            else if (BO_StageController.Instance.currentStage == BO_StageController.StageType.Stage1)
            {
                Debug.Log("[Game] Stage 1 complete!");

                // gameOverObject.SetActive(true);

                // Destroy all active balls
                GameObject[] balls = GameObject.FindGameObjectsWithTag("BreakOutBall");
                foreach (GameObject ball in balls)
                {
                    Destroy(ball);
                }

                Debug.Log("[Game] Stage 1 complete!");
                currentState = GameState.Transition;

                // TEMP: simple placeholder behavior
                // Later this will trigger the sliding background, stage 2 spawn, etc.

            }
        }
    }
}
