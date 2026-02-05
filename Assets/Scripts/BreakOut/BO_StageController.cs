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
            // UNIVERSAL INPUT: If a screen is up, Enter dismisses it
            if (currentActiveScreen != null && Input.GetKeyDown(KeyCode.Return))
            {
                // 1. Fade out the screen
                StartCoroutine(FadeOutRoutine(currentActiveScreen, 2.0f));

                // 2. Decide what happens next based on which screen just closed
                OnScreenDismissed(currentActiveScreen);

                // 3. Clear variable so we don't trigger twice
                currentActiveScreen = null;
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
            // LOGIC: "The player just pressed Enter on [screen]. What happens next?"

            // --- STAGE 1 LOGIC ---
            if (screen == Stage1Intro)
            {
                // Intro is gone -> Spawn the ball to play Stage 1
                BO_Manager.instance.SpawnBall();
            }
            else if (screen == Stage1Win)
            {
                // Win screen is gone -> Start elevator to Stage 2
                StartCoroutine(TransitionElevator(StageType.Stage2));
            }

            // --- STAGE 2 LOGIC ---
            else if (screen == Stage2Intro)
            {
                // Intro is gone -> Spawn ball for Stage 2
                BO_Manager.instance.SpawnBall();
            }
            else if (screen == Stage2Win)
            {
                // Win screen is gone -> Start baking transition!
                OnStage2Complete();
            }

            else if (screen == BakingResultScreen)
            {
                // The player saw their score, pressed Enter, now we leave the kitchen!
                StartCoroutine(TransitionToStage3Sequence());
            }

            // --- STAGE 3 LOGIC (Placeholder for now) ---
            else if (screen == Stage3Intro)
            {
                BO_Manager.instance.SpawnBall();
            }

            else if (screen == BakingResultScreen)
            {
                // 1. Start fading out the entire minigame container
                StartCoroutine(FadeOutMiniGameAndStartStage3());
            }
        }

        private IEnumerator FadeOutMiniGameAndStartStage3()
        {
            // 1. Fade out the whole container (using your existing 2.0f speed or faster)
            yield return StartCoroutine(FadeOutRoutine(MiniGameScreensGroup, 1.5f));

            // 2. Small buffer to let the screen stay clear for a beat
            yield return new WaitForSeconds(0.5f);

            // 3. Launch Stage 3
            StartStage(StageType.Stage3);
        }

        private IEnumerator TransitionToStage3Sequence()
        {
            // 1. Trigger the "SlideUp" to move the kitchen/panel away
            if (backgroundAnimator != null) backgroundAnimator.SetTrigger("SlideUp");

            // 2. Wait for the animation to finish (adjust time to match your clip length)
            yield return new WaitForSeconds(1.5f);

            // 3. Finally, start Stage 3
            StartStage(StageType.Stage3);
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