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
        // This holds the current state of the game logic
        private GameState currentState = GameState.Start;

        public enum BakingResult 
        { 
            Undercooked, 
            Perfect, 
            Burnt 
        }
        public BakingResult lastBakingResult;

        public static BO_Manager instance;

        [Header("Powerup Unlocks")]
        public bool isBerriesUnlocked = false;
        public bool isShieldUnlocked = false;
        public bool isJamUnlocked = false;

        [Header("Game Settings")]
        public int maxHealth = 10;
        private int currentHealth; // Internal tracker

        [Header("References")]
        public GameObject BallPrefab;
        public GameObject gameOverObject;
        public TextMeshProUGUI scoretext; // This will now show HP
        public TextMeshProUGUI losertext;

        [Header("Ball Control")]
        public int maxBalls = 3;
        public float minBallScale = 0.6f; // Kept this as Ball script might need it
        public bool controlsReversed = false;

        private List<BO_Ball> activeBalls = new List<BO_Ball>();

        // (Optional) Hearts - if you add visual heart logic later, uncomment these
        // public GameObject Heart1, Heart2, Heart3, Heart4, Heart5;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            if (gameOverObject != null) gameOverObject.SetActive(false);

            currentHealth = maxHealth; // Initialize HP
            UpdateHealthDisplay();     // Draw the hearts
        }

        // Call this to sync the Text and the Hearts at once
        public void UpdateHealthDisplay()
        {
            // Check if the GameObject is active and the reference isn't null
            if (scoretext != null && scoretext.gameObject.activeInHierarchy)
            {
                scoretext.text = "Health: " + currentHealth;
            }

            if (BO_HealthDisplay.Instance != null && BO_HealthDisplay.Instance.gameObject.activeInHierarchy)
            {
                BO_HealthDisplay.Instance.UpdateDisplay(currentHealth);
            }
        }

        public void Heal(int amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth); // Don't exceed 10 HP
            UpdateHealthDisplay();
        }

        public void ResetLives() // Keeping the name so other scripts don't break
        {
            currentHealth = maxHealth;
            UpdateHealthDisplay();
            controlsReversed = false;
            Debug.Log("[Manager] Health reset for new stage.");
        }

        public void SetState(GameState s)
        {
            currentState = s;
        }
        public void SetBakingResult(BakingResult result)
        {
            lastBakingResult = result;
            Debug.Log($"[Manager] Baking result stored: {lastBakingResult}");
        }

        public void UnlockBerries()
        {
            isBerriesUnlocked = true;
            // Enable the cannon GameObject on the power-up controller if present
            if (BO_PowerUpController.instance != null && BO_PowerUpController.instance.canon != null)
            {
                BO_PowerUpController.instance.canon.SetActive(true);
            }
            Debug.Log("<color=orange>[Manager] Berry Shots Unlocked!</color>");
        }

        public void UnlockShield()
        {
            isShieldUnlocked = true;
            Debug.Log("<color=cyan>[Manager] Shield Powerup Unlocked!</color>");
        }

        public void UnlockJam()
        {
            isJamUnlocked = true;
            Debug.Log("<color=magenta>[Manager] Jam Mode Unlocked!</color>");
        }

        // --- BALL SPAWNING & DEATH LOGIC ---

        public void SpawnBall()
        {
            // Clean out any "null" bails that might be stuck in the list
            activeBalls.RemoveAll(b => b == null);

            if (currentState == GameState.Transition ||
                currentState == GameState.BakingMiniGame ||
                activeBalls.Count >= maxBalls)
            {
                Debug.Log($"[Manager] Spawn blocked. State: {currentState}, Ball Count: {activeBalls.Count}");
                return;
            }

            GameObject ballObj = Instantiate(BallPrefab);
            BO_Ball ball = ballObj.GetComponent<BO_Ball>();
            activeBalls.Add(ball);
        }

        public void RemoveBall(BO_Ball ball)
        {
            if (activeBalls.Contains(ball)) activeBalls.Remove(ball);
            if (ball != null) Destroy(ball.gameObject);

            currentHealth -= 1;
            UpdateHealthDisplay();

            if (currentHealth <= 0)
            {
                GameOver();
                return;
            }

            if (activeBalls.Count == 0 && IsPlayableState())
            {
                SpawnBall();
            }
        }

        public void OnStageCleared()
        {
            Debug.Log("[Manager] Stage Cleared Signal Received.");

            // Stop the ball and gameplay logic
            SetState(GameState.Transition);

            // Tell the StageController to show the "You Win" screen
            if (BO_StageController.Instance != null)
            {
                BO_StageController.Instance.HandleStageWin();
            }
        }

        // --- HELPER FUNCTIONS ---

        bool IsPlayableState()
        {
            return currentState == GameState.Stage1 ||
                   currentState == GameState.Stage2 ||
                   currentState == GameState.Boss;
        }

        void GameOver()
        {
            currentState = GameState.GameOver;
            if (gameOverObject != null) gameOverObject.SetActive(true);
            if (losertext != null) losertext.text = "You lost :(";

            ClearAllBalls();
        }

        public void ClearAllBalls()
        {
            foreach (BO_Ball ball in activeBalls)
            {
                if (ball != null) Destroy(ball.gameObject);
            }
            activeBalls.Clear();
        }

        public void ToggleControls()
        {
            controlsReversed = !controlsReversed;
            Debug.Log($"[Manager] Controls reversed = {controlsReversed}");
        }

        // Used by other scripts to check ball count
        public int GetActiveBallCount()
        {
            return activeBalls.Count;
        }

        // Used if a ball is created by something else (like a powerup)
        public void RegisterBall(BO_Ball ball)
        {
            if (!activeBalls.Contains(ball))
                activeBalls.Add(ball);
        }
    }
}