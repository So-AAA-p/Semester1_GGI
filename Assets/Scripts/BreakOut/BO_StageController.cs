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

        [Header("Stage 3 Screens (Boss)")]
        public CanvasGroup Stage3Intro;
        public CanvasGroup Stage3Win;
        public CanvasGroup ShieldTutorial;
        public CanvasGroup JamTutorial;

        [Header("Minigame Transitions")]
        public CanvasGroup MiniGameScreensGroup;
        public CanvasGroup BakingResultScreen;
        public Animator minigameAnimator;
        public Animator backgroundAnimator;

        private CanvasGroup currentActiveScreen;

        private void Awake() => Instance = this;

        private void Start()
        {
            HideAllScreens();
            StartStage(currentStage);
        }

        private void HideAllScreens()
        {
            if (Stage1Intro) Stage1Intro.gameObject.SetActive(false);
            if (Stage1Win) Stage1Win.gameObject.SetActive(false);
            if (Stage2Intro) Stage2Intro.gameObject.SetActive(false);
            if (Stage2Win) Stage2Win.gameObject.SetActive(false);
            if (Stage3Intro) Stage3Intro.gameObject.SetActive(false);
            if (Stage3Win) Stage3Win.gameObject.SetActive(false);
            if (BlueberryCounter) BlueberryCounter.gameObject.SetActive(false);
            if (ShieldTutorial) ShieldTutorial.gameObject.SetActive(false);
            if (JamTutorial) JamTutorial.gameObject.SetActive(false);
        }

        private void Update()
        {
            // We use unscaledDeltaTime here just in case, though Input works anyway
            if (currentActiveScreen != null && Input.GetKeyDown(KeyCode.Return))
            {
                CanvasGroup screenToDismiss = currentActiveScreen;
                currentActiveScreen = null;

                // Use a slightly faster fade out for responsiveness
                StartCoroutine(FadeOutRoutine(screenToDismiss, 3.0f));
                OnScreenDismissed(screenToDismiss);
            }
        }

        // --- THE NEW TUTORIAL LOGIC ---

        public void TriggerPhase2Tutorial()
        {
            StartCoroutine(SlowMotionTutorialRoutine(ShieldTutorial));
        }

        private IEnumerator SlowMotionTutorialRoutine(CanvasGroup tutorialScreen)
        {
            Debug.Log("[Tutorial] Slowing down time...");

            // 1. Slow down time over 2 seconds
            float duration = 2.0f;
            float elapsed = 0f;
            float startScale = Time.timeScale;
            float targetScale = 0.1f; // Slow down to 10% speed

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                Time.timeScale = Mathf.Lerp(startScale, targetScale, elapsed / duration);
                yield return null;
            }
            Time.timeScale = targetScale;

            // 2. Show the screen
            ShowScreen(tutorialScreen);
        }

        private IEnumerator RestoreTimeScale()
        {
            Debug.Log("[Tutorial] Returning to normal speed...");

            float duration = 1.0f; // Return to normal faster (1 second)
            float elapsed = 0f;
            float startScale = Time.timeScale;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                Time.timeScale = Mathf.Lerp(startScale, 1.0f, elapsed / duration);
                yield return null;
            }
            Time.timeScale = 1.0f;
        }

        // --- EXISTING LOGIC ---

        public void StartStage(StageType stage)
        {
            currentStage = stage;
            switch (stage)
            {
                case StageType.Stage1:
                    BO_Manager.instance.SetState(BO_Manager.GameState.Stage1);
                    if (BO_BlockSpawner.Instance != null) BO_BlockSpawner.Instance.GenerateGrid(BO_BlockSpawner.GridMode.IngredientBlocks);
                    ShowScreen(Stage1Intro);
                    break;
                case StageType.Stage2:
                    BO_Manager.instance.SetState(BO_Manager.GameState.Stage2);
                    StartCoroutine(SetupStage2());
                    break;
                case StageType.Stage3:
                    BO_Manager.instance.SetState(BO_Manager.GameState.Boss);
                    if (BO_BlockSpawner.Instance != null) BO_BlockSpawner.Instance.ClearGrid();
                    if (BO_BlueberryManager.Instance != null) BO_BlueberryManager.Instance.ClearRemainingLeaves();
                    ShowScreen(Stage3Intro);
                    break;
                case StageType.BakingMinigame:
                    BO_Manager.instance.SetState(BO_Manager.GameState.BakingMiniGame);
                    break;
            }
        }

        public void ShowScreen(CanvasGroup screen)
        {
            if (screen == null) return;
            if (currentActiveScreen != null) currentActiveScreen.gameObject.SetActive(false);

            currentActiveScreen = screen;
            currentActiveScreen.alpha = 0; // Start transparent for fade in
            currentActiveScreen.blocksRaycasts = true;
            currentActiveScreen.gameObject.SetActive(true);

            StartCoroutine(FadeInRoutine(screen));
        }

        private IEnumerator FadeInRoutine(CanvasGroup screen)
        {
            while (screen.alpha < 1)
            {
                // Use unscaledDeltaTime so it fades in smoothly even if Time.timeScale is near 0
                screen.alpha += Time.unscaledDeltaTime * 2.0f;
                yield return null;
            }
            screen.alpha = 1;
        }

        private void OnScreenDismissed(CanvasGroup screen)
        {
            if (screen == Stage1Intro) BO_Manager.instance.SpawnBall();
            else if (screen == Stage1Win) StartCoroutine(TransitionElevator(StageType.Stage2));
            else if (screen == Stage2Intro) BO_Manager.instance.SpawnBall();
            else if (screen == Stage2Win) OnStage2Complete();
            else if (screen == Stage3Intro) BO_Manager.instance.SpawnBall();

            // --- TUTORIAL DISMISSAL ---
            else if (screen == ShieldTutorial)
            {
                BO_Manager.instance.UnlockShield(); // Unlock the power!
                StartCoroutine(RestoreTimeScale()); // Speed time back up
            }

            else if (screen == BakingResultScreen)
            {
                StartCoroutine(FadeOutMiniGameAndStartStage3());
            }

            else if (screen == JamTutorial)
            {
                BO_Manager.instance.UnlockJam(); // Unlock Jam & Shots!
                StartCoroutine(RestoreTimeScale());
            }
        }

        private IEnumerator FadeOutRoutine(CanvasGroup targetScreen, float speed)
        {
            targetScreen.blocksRaycasts = false;
            while (targetScreen.alpha > 0)
            {
                // IMPORTANT: Use unscaledDeltaTime here too
                targetScreen.alpha -= Time.unscaledDeltaTime * speed;
                yield return null;
            }
            targetScreen.gameObject.SetActive(false);
        }

        public void TriggerPhase3Tutorial()
        {
            // Reuse your cool slow-mo routine!
            StartCoroutine(SlowMotionTutorialRoutine(JamTutorial));
        }

        private IEnumerator FadeOutMiniGameAndStartStage3()
        {
            yield return StartCoroutine(FadeOutRoutine(MiniGameScreensGroup, 1.5f));
            MiniGameScreensGroup.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);

            FindObjectOfType<BO_Paddle>().ApplyBakingModifiers();
            StartStage(StageType.Stage3);
            yield return new WaitForEndOfFrame();

            BO_Ball activeBall = FindObjectOfType<BO_Ball>();
            if (activeBall != null) activeBall.ApplyBakingModifiers();
        }

        private IEnumerator SetupStage2()
        {
            if (BO_BlockSpawner.Instance != null) BO_BlockSpawner.Instance.GenerateGrid(BO_BlockSpawner.GridMode.LeafBlocks);
            yield return new WaitForSeconds(0.1f);
            if (BO_BlueberryManager.Instance != null) BO_BlueberryManager.Instance.InitializeBlueberries();
            if (BlueberryCounter != null) BlueberryCounter.gameObject.SetActive(true);
            ShowScreen(Stage2Intro);
        }

        public void HandleStageWin()
        {
            BO_Manager.instance.SetState(BO_Manager.GameState.Transition);
            BO_Manager.instance.ClearAllBalls();
            BO_Manager.instance.ResetLives();

            if (currentStage == StageType.Stage1) ShowScreen(Stage1Win);
            else if (currentStage == StageType.Stage2) ShowScreen(Stage2Win);
        }

        private IEnumerator TransitionElevator(StageType nextStage)
        {
            if (backgroundAnimator != null) backgroundAnimator.SetTrigger("SlideDown");
            yield return new WaitForSeconds(2f);
            StartStage(nextStage);
        }

        public void OnStage2Complete()
        {
            currentStage = StageType.BakingMinigame;
            StartCoroutine(TransitionToBaking());
        }

        private IEnumerator TransitionToBaking()
        {
            if (BO_BlockSpawner.Instance != null) BO_BlockSpawner.Instance.ClearGrid();
            if (BO_BlueberryManager.Instance != null) BO_BlueberryManager.Instance.ClearRemainingLeaves();
            if (BlueberryCounter != null) BlueberryCounter.gameObject.SetActive(false);

            if (MiniGameScreensGroup != null)
            {
                MiniGameScreensGroup.alpha = 1;
                MiniGameScreensGroup.gameObject.SetActive(true);
            }

            if (minigameAnimator != null) minigameAnimator.SetTrigger("StartBaking");
            yield return new WaitForSeconds(1.0f);
            BO_Manager.instance.SetState(BO_Manager.GameState.BakingMiniGame);
            if (BO_BakingMinigame.Instance != null) BO_BakingMinigame.Instance.PrepareToStart();
        }
    }
}