using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BreakOut
{
    public class BO_BakingMinigame : MonoBehaviour
    {
        public static BO_BakingMinigame Instance;

        [Header("State")]
        private bool hasStarted = false;
        private bool isReady = false;
        public float timeLeft = 10f;

        [Header("Baking Physics")]
        [Range(0, 100)] public float currentHeat = 0f;
        public float heatPower = 60f;   // Increased power for better feel
        public float gravity = 30f;     // Increased gravity
        public float maxHeat = 100f;

        [Header("The Sweet Spot")]
        public float sweetSpotMin = 40f;
        public float sweetSpotMax = 60f;

        public float qualityGainRate = 15f;
        public float currentQuality = 0f;

        [Header("UI References")]
        public Slider heatSlider;
        public Image sliderFillImage;
        public TextMeshProUGUI timerText;
        public GameObject startPromptText;

        [Header("Result UI")]
        public TextMeshProUGUI resultAnnouncementText;

        [Header("Color stuff")]
        public Color coldColor = Color.cyan;
        public Color perfectColor = Color.green;
        public Color hotColor = Color.red;

        private void Awake()
        {
            Instance = this;
        }

        public void PrepareToStart()
        {
            isReady = true;
            hasStarted = false;
            currentHeat = 0;
            currentQuality = 0;

            if (startPromptText != null) startPromptText.SetActive(true);

            // IMPORTANT: Reset the slider max value to 1.0 just in case
            if (heatSlider != null)
            {
                heatSlider.minValue = 0f;
                heatSlider.maxValue = 1.0f;
                heatSlider.value = 0f;
            }

            UpdateTimerUI();
        }

        void Update()
        {
            if (!isReady) return;

            // Wait for Start
            if (!hasStarted)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    hasStarted = true;
                    //if (startPromptText != null) startPromptText.SetActive(false);
                    Debug.Log("Baking Started!");
                }
                return;
            }

            RunMinigameLogic();
        }

        void RunMinigameLogic()
        {
            // 1. Timer
            timeLeft -= Time.deltaTime;
            UpdateTimerUI();

            if (timeLeft <= 0)
            {
                EndMinigame();
                return;
            }

            // 2. Heat Physics
            if (Input.GetKey(KeyCode.Space))
            {
                currentHeat += heatPower * Time.deltaTime;
            }
            else
            {
                currentHeat -= gravity * Time.deltaTime;
            }

            currentHeat = Mathf.Clamp(currentHeat, 0, maxHeat);

            // Debug Log to prove the math is working!
            // Debug.Log($"Heat: {currentHeat}"); 

            // 3. Update Visuals (SAFE MODE)
            if (heatSlider != null)
            {
                // We divide by maxHeat to get a 0.0 to 1.0 value
                heatSlider.value = currentHeat / maxHeat;
            }

            // 4. Check "Sweet Spot" 
            CheckSweetSpot();
        }

        void CheckSweetSpot()
        {
            // Define our ranges based on your design
            float scoreDecreaseRate = 10f; // How fast it cools/undercooks
            float scoreIncreaseRate = 20f; // How fast it bakes in the sweet spot
            float burnRate = 15f;          // How fast it burns when too hot

            if (currentHeat < sweetSpotMin)
            {
                // ZONE: TOO COLD (Score drops)
                currentQuality -= scoreDecreaseRate * Time.deltaTime;
                if (sliderFillImage != null) sliderFillImage.color = coldColor;
            }
            else if (currentHeat >= sweetSpotMin && currentHeat <= sweetSpotMax)
            {
                // ZONE: SWEET SPOT (Bake up to 60)
                if (currentQuality < 70f)
                {
                    currentQuality += scoreIncreaseRate * Time.deltaTime;
                }
                else if (currentQuality > 80f)
                {
                    // If they were burning and came back to green, let it "cool" back to 60
                    currentQuality -= scoreDecreaseRate * Time.deltaTime;
                    currentQuality = Mathf.Max(currentQuality, 80f);
                }

                if (sliderFillImage != null) sliderFillImage.color = perfectColor;
            }
            else
            {
                // ZONE: TOO HOT (Burn above 60)
                currentQuality += burnRate * Time.deltaTime;
                if (sliderFillImage != null) sliderFillImage.color = hotColor;
            }

            // Clamp the final score between 0 and 100
            currentQuality = Mathf.Clamp(currentQuality, 0f, 100f);
        }

        void EndMinigame()
        {
            isReady = false;
            hasStarted = false;

            string resultHeader = "";
            Color displayColor = Color.white;

            // Using your new 70-80 logic!
            if (currentQuality < 70f)
            {
                resultHeader = "UNDERCOOKED D:";
                displayColor = coldColor;
                BO_Manager.instance.SetBakingResult(BO_Manager.BakingResult.Undercooked);
            }

            else if (currentQuality >= 70f && currentQuality <= 80f)
            {
                resultHeader = "PERFECT SCORE!";
                displayColor = perfectColor;
                BO_Manager.instance.SetBakingResult(BO_Manager.BakingResult.Perfect);

                // Award the bonus (Restore 1 Full Heart / 2 HP)
                if (BO_Manager.instance != null)
                {
                    // Use a new Heal function (we'll add this to Manager below)
                    BO_Manager.instance.Heal(2);
                }
            }

            else // 81-100
            {
                resultHeader = "BURNT >:(";
                displayColor = hotColor;
                BO_Manager.instance.SetBakingResult(BO_Manager.BakingResult.Burnt);
            }

            // Display it on screen!
            if (resultAnnouncementText != null)
            {
                resultAnnouncementText.text = resultHeader;
                resultAnnouncementText.color = displayColor;
                resultAnnouncementText.gameObject.SetActive(true);
            }

            Debug.Log($"FINAL RESULT: {resultHeader} | Score: {currentQuality:F1}");

            Invoke("ProceedToBoss", 3f);
            BO_StageController.Instance.ShowScreen(BO_StageController.Instance.BakingResultScreen);
        }

        void UpdateTimerUI()
        {
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        }
    }
}