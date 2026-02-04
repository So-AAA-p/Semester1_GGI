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

        // Colors 
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
                    if (startPromptText != null) startPromptText.SetActive(false);
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
            bool inZone = (currentHeat >= sweetSpotMin && currentHeat <= sweetSpotMax);

            if (inZone)
            {
                currentQuality += qualityGainRate * Time.deltaTime;
            }

            currentQuality = Mathf.Min(currentQuality, 100f);

            // SAFE VISUAL UPDATE
            // We check specifically if the object has been destroyed to prevent the error
            if (sliderFillImage != null && !sliderFillImage.Equals(null))
            {
                if (inZone)
                    sliderFillImage.color = perfectColor;
                else
                    sliderFillImage.color = (currentHeat > sweetSpotMax) ? hotColor : coldColor;
            }
        }

        void UpdateTimerUI()
        {
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        }

        void EndMinigame()
        {
            isReady = false;
            hasStarted = false;
            Debug.Log($"FINSIHED! Final Quality: {currentQuality}/100");

            // Trigger transition to Stage 3 or Boss here later
        }
    }
}