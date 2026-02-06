using System.Collections;
using UnityEngine;
using TMPro;

namespace BreakOut
{
    public class BO_StageController : MonoBehaviour
    {
        public static BO_StageController Instance;

        public enum StageType { Stage1, Stage2, BakingMinigame, Stage3, WinScreen }
        public StageType currentStage;

        [Header("Stage 1 Screens")]
        public CanvasGroup Stage1Intro;
        public CanvasGroup Stage1Win;

        [Header("Stage 2 Screens")]
        public CanvasGroup Stage2Intro;
        public CanvasGroup Stage2Win;
        public CanvasGroup BlueberryCounter;

        [Header("Stage 3 Screens")]
        public CanvasGroup Stage3Intro;
        public CanvasGroup Stage3Win;

        [Header("Minigame Transitions")] 
        public CanvasGroup MiniGameScreensGroup;
        public RectTransform minigamePanel;
        public CanvasGroup BakingResultScreen;
        public Animator minigameAnimator;
        public Animator backgroundAnimator; // The "Elevator"

        // Internal State
        private CanvasGroup currentActiveScreen;



        private void Awake() => Instance = this;

        private void Start()
        {
            // 1. CLEAN SLATE: Force every single screen to turn OFF immediately
            HideAllScreens();

            // 2. Start the game loop
            StartStage(currentStage);
        }

        // Helper to make sure no "ghost" screens are visible at launch
        private void HideAllScreens()
        {
            if (Stage1Intro) Stage1Intro.gameObject.SetActive(false);
            if (Stage1Win) Stage1Win.gameObject.SetActive(false);
            if (Stage2Intro) Stage2Intro.gameObject.SetActive(false);
            if (Stage2Win) Stage2Win.gameObject.SetActive(false);
            if (Stage3Intro) Stage3Intro.gameObject.SetActive(false);
            if (Stage3Win) Stage3Win.gameObject.SetActive(false);
            if (BlueberryCounter) BlueberryCounter.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (currentActiveScreen != null && Input.GetKeyDown(KeyCode.Return))
            {
                // 1. Capture the screen we are about to dismiss
                CanvasGroup screenToDismiss = currentActiveScreen;

                // 2. Clear the variable FIRST so no other logic can mess with it this frame
                currentActiveScreen = null;

                Debug.Log($"[Input] Dismissing: {screenToDismiss.name}");

                // 3. Run the exit logic
                StartCoroutine(FadeOutRoutine(screenToDismiss, 2.0f));
                OnScreenDismissed(screenToDismiss);
            }
        }

        public void StartStage(StageType stage)
        {
            currentStage = stage;
            Debug.Log($"[StageController] Entering {stage}");

            // --- FIX: Explicitly sync the Manager State ---
            switch (stage)
            {
                case StageType.Stage1:
                    BO_Manager.instance.SetState(BO_Manager.GameState.Stage1);
                    if (BO_BlockSpawner.Instance != null)
                        BO_BlockSpawner.Instance.GenerateGrid(BO_BlockSpawner.GridMode.IngredientBlocks);
                    ShowScreen(Stage1Intro);
                    break;

                case StageType.Stage2:
                    BO_Manager.instance.SetState(BO_Manager.GameState.Stage2);
                    StartCoroutine(SetupStage2());
                    break;

                case StageType.Stage3:
                    BO_Manager.instance.SetState(BO_Manager.GameState.Boss); // Tell manager it's Boss time!
                    if (BO_BlockSpawner.Instance != null) BO_BlockSpawner.Instance.ClearGrid();
                    if (BO_BlueberryManager.Instance != null) BO_BlueberryManager.Instance.ClearRemainingLeaves();
                    ShowScreen(Stage3Intro);
                    break;

                case StageType.BakingMinigame:
                    BO_Manager.instance.SetState(BO_Manager.GameState.BakingMiniGame);
                    break;
            }
        }

        // --- SCREEN LOGIC ---

        public void ShowScreen(CanvasGroup screen)
        {
            if (screen == null) return;

            // Hide old one if exists
            if (currentActiveScreen != null) currentActiveScreen.gameObject.SetActive(false);

            currentActiveScreen = screen;
            currentActiveScreen.alpha = 1;
            currentActiveScreen.blocksRaycasts = true;
            currentActiveScreen.gameObject.SetActive(true);

            // Pause physics while reading text? Optional:
            // Time.timeScale = 0; 
        }

        private void OnScreenDismissed(CanvasGroup screen)
        {
            if (screen == Stage1Intro) { BO_Manager.instance.SpawnBall(); }
            else if (screen == Stage1Win) { StartCoroutine(TransitionElevator(StageType.Stage2)); }
            else if (screen == Stage2Intro) { BO_Manager.instance.SpawnBall(); }
            else if (screen == Stage2Win) { OnStage2Complete(); }
            else if (screen == Stage3Intro) { BO_Manager.instance.SpawnBall(); }

            // CONSOLIDATED MINIGAME RESET:
            else if (screen == BakingResultScreen)
            {
                Debug.Log("Result Screen Dismissed! Starting Fade Out...");
                StartCoroutine(FadeOutMiniGameAndStartStage3());
            }
        }

        private IEnumerator FadeOutMiniGameAndStartStage3()
        {
            Debug.Log("Starting Master Fade for Minigame...");

            // 1. Fade the whole group
            yield return StartCoroutine(FadeOutRoutine(MiniGameScreensGroup, 1.5f));

            // 2. FORCE SHUTDOWN: Make sure the object is actually inactive
            MiniGameScreensGroup.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.5f);

            // 1. Tell the paddle to check the baking result
            FindObjectOfType<BO_Paddle>().ApplyBakingModifiers();

            // 2. Start the stage
            StartStage(StageType.Stage3);

            // 3. Since StartStage usually spawns the ball, 
            // we find the ball AFTER it's spawned to shrink it
            yield return new WaitForEndOfFrame(); // Wait for spawn to finish
            BO_Ball activeBall = FindObjectOfType<BO_Ball>();
            if (activeBall != null) activeBall.ApplyBakingModifiers();
        }

        private IEnumerator FadeOutRoutine(CanvasGroup targetScreen, float speed)
        {
            targetScreen.blocksRaycasts = false;
            while (targetScreen.alpha > 0)
            {
                targetScreen.alpha -= Time.deltaTime * speed;
                yield return null;
            }
            targetScreen.gameObject.SetActive(false);
        }

        // --- SPECIFIC STAGE LOGIC ---

        private IEnumerator SetupStage2()
        {
            // 1. Clear Stage 1 and Spawn Leaf Grid
            if (BO_BlockSpawner.Instance != null)
            {
                Debug.Log("[StageController] Spawning Leaf Grid...");
                BO_BlockSpawner.Instance.GenerateGrid(BO_BlockSpawner.GridMode.LeafBlocks);
            }

            yield return new WaitForSeconds(0.1f); // Brief pause for Unity to register new objects

            // 2. Initialize Blueberry logic on the new leaves
            if (BO_BlueberryManager.Instance != null)
            {
                BO_BlueberryManager.Instance.InitializeBlueberries();
            }

            if (BlueberryCounter != null) BlueberryCounter.gameObject.SetActive(true);

            ShowScreen(Stage2Intro);
        }


        // Call this when BO_Manager says "Level Cleared"
        public void HandleStageWin()
        {
            BO_Manager.instance.SetState(BO_Manager.GameState.Transition);

            // --- NEW: CLEANUP BALLS AND RESET LIVES ---
            BO_Manager.instance.ClearAllBalls(); // We'll create this method in a second
            BO_Manager.instance.ResetLives();    // We'll create this too!

            if (currentStage == StageType.Stage1)
            {
                ShowScreen(Stage1Win);
            }
            else if (currentStage == StageType.Stage2)
            {
                ShowScreen(Stage2Win);
            }
        }

        private IEnumerator TransitionElevator(StageType nextStage)
        {
            if (backgroundAnimator != null) backgroundAnimator.SetTrigger("SlideDown");

            yield return new WaitForSeconds(2f); // Wait for elevator

            StartStage(nextStage);
        }

        public void OnStage2Complete()
        {
            currentStage = StageType.BakingMinigame;
            StartCoroutine(TransitionToBaking());
        }

        private IEnumerator TransitionToBaking()
        {
            // Clean up Stage 2
            if (BO_BlockSpawner.Instance != null) BO_BlockSpawner.Instance.ClearGrid();
            if (BO_BlueberryManager.Instance != null) BO_BlueberryManager.Instance.ClearRemainingLeaves();
            if (BlueberryCounter != null) BlueberryCounter.gameObject.SetActive(false);

            // --- NEW: Reset the Master Group Alpha ---
            if (MiniGameScreensGroup != null)
            {
                MiniGameScreensGroup.alpha = 1;
                MiniGameScreensGroup.gameObject.SetActive(true);
            }

            // Trigger your "StartBaking" animation (the sliding elevator/background)
            if (minigameAnimator != null) minigameAnimator.SetTrigger("StartBaking");

            yield return new WaitForSeconds(1.0f);

            BO_Manager.instance.SetState(BO_Manager.GameState.BakingMiniGame);
            if (BO_BakingMinigame.Instance != null) BO_BakingMinigame.Instance.PrepareToStart();
        }
    }
}