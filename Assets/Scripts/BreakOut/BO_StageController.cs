using System.Collections;
using UnityEngine;
using TMPro;

namespace BreakOut
{
    public class BO_StageController : MonoBehaviour
    {
        public enum StageType { Stage1, Stage2, BakingMinigame, Stage3, WinScreen }

        public static BO_StageController Instance;
        public StageType currentStage;

        [Header("Transition Settings")]
        public GameObject instructionUI;
        public Animator backgroundAnimator; // Use an animator to handle the "Elevator" slide

        [Header("Minigame Transition")]
        public TextMeshProUGUI BlueberryCounter;          // Drag the Berry Count text here
        public RectTransform minigamePanel;    // Drag the baking UI panel here
        public float slideDuration = 1.0f;

        public Animator minigameAnimator;

        private void Awake() => Instance = this;


        private void Start()
        {
            // This allows you to set the stage in the Inspector for testing!
            StartStage(currentStage);
        }

        public void StartStage(StageType stage)
        {
            currentStage = stage;

            // SYNC the old manager's state so its "if" checks pass
            if (stage == StageType.Stage2)
                BO_Manager.instance.SetState(BO_Manager.GameState.Stage2);

            Debug.Log($"[Stage] Switching to {stage}");

            switch (stage)
            {
                case StageType.Stage1:
                    // Stage 1 setup
                    break;

                case StageType.Stage2:
                    // Use a Coroutine instead of a direct call to allow leaves to register
                    StartCoroutine(SetupStage2());
                    break;

                case StageType.BakingMinigame:
                    OnStage2Complete();
                    break;
            }
        }

        private IEnumerator SetupStage2()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            BO_BlueberryManager.Instance.InitializeBlueberries();

            if (BlueberryCounter != null)
            {
                BlueberryCounter.alpha = 1;
                BlueberryCounter.gameObject.SetActive(true);
            }

            // REMOVED: BO_Manager.instance.SpawnBall(); 
            // The Manager now handles this in its own Start()!
        }

        private IEnumerator TransitionElevator(StageType nextStage)
        {
            // 1. Fade out current stage blocks/text
            // 2. Play Background Slide Animation ("Elevator")
            if (backgroundAnimator != null) backgroundAnimator.SetTrigger("SlideDown");

            yield return new WaitForSeconds(2f); // Wait for slide to finish

            if (nextStage == StageType.Stage2)
            {
                BO_BlueberryManager.Instance.InitializeBlueberries();
                ShowInstructions("Collect Berries! \n Press ENTER");
            }
            else if (nextStage == StageType.Stage3)
            {
                // Spawn the Chihuahua!
            }
        }

        public void ShowInstructions(string text)
        {
            // Set your text and wait for Enter key to start the actual gameplay
        }

        public void OnStage2Complete()
        {
            // IMMEDIATELY change the stage type so OnStageCleared() 
            // won't pass the "if Stage2" check again.
            currentStage = StageType.BakingMinigame;
            StartCoroutine(TransitionToBaking());
        }

        private IEnumerator TransitionToBaking()
        {
            Debug.Log("TRANSITION START: Freezing game...");

            // 1. Freeze Paddle safely
            if (BO_Paddle.Instance != null) BO_Paddle.Instance.enabled = false;

            // 2. Clear ALL balls by tag (much safer than using Instance)
            GameObject[] balls = GameObject.FindGameObjectsWithTag("BreakOutBall");
            foreach (GameObject b in balls)
            {
                Destroy(b);
            }

            // 3. IMMEDIATELY clear leaves before doing the UI fade
            // This ensures they disappear the moment the last berry is hit
            if (BO_BlueberryManager.Instance != null)
            {
                Debug.Log("Clearing Leaves now!");
                BO_BlueberryManager.Instance.ClearRemainingLeaves();
            }

            // 4. Now do the slow UI fade and Animation
            if (BlueberryCounter != null)
            {
                float elapsed = 0;
                while (elapsed < 0.5f) // Speed up the fade a bit
                {
                    elapsed += Time.deltaTime;
                    BlueberryCounter.alpha = 1 - (elapsed / 0.5f);
                    yield return null;
                }
                BlueberryCounter.gameObject.SetActive(false);
            }

            if (minigameAnimator != null)
            {
                minigameAnimator.SetTrigger("StartBaking");
            }

            yield return new WaitForSeconds(1.0f);

            // NEW: Tell the Manager we are officially baking now!
            BO_Manager.instance.SetState(BO_Manager.GameState.BakingMiniGame);

            if (BO_BakingMinigame.Instance != null)
            {
                BO_BakingMinigame.Instance.PrepareToStart();
            }
        }

        public void JumpToStage(StageType stage)
        {
            // 1. Clean up existing objects
            GameObject[] balls = GameObject.FindGameObjectsWithTag("BreakOutBall");
            foreach (GameObject b in balls) Destroy(b);

            // 2. Clear the UI
            if (instructionUI != null) instructionUI.SetActive(false);

            // 3. Launch the stage
            StartStage(stage);
        }
    }
}
   