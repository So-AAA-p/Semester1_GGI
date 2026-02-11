using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BreakOut
{
    public class BO_PowerUpController : MonoBehaviour
    {
        public static BO_PowerUpController instance;

        [Header("--- BERRY SHOTS (Key: E) ---")]
        public GameObject projectilePrefab;
        public Transform firePoint;
        public GameObject canon;
        public int maxAmmo = 5;
        public int currentAmmo;
        public float reloadTime = 15f;
        private float reloadTimer = 0f;
        private bool isReloading = false;

        [Header("Berry UI")]
        public Slider shotSlider;
        public Image shotFillImage;

        [Header("--- SHIELD (Passive) ---")]
        public GameObject shieldVisual;
        public int maxShieldHealth = 3;
        public float baseShieldRegen = 10f;
        private int currentShieldHealth;
        public float shieldRegenTimer = 0f;

        [Header("--- JAM MODE (Key: Q) ---")]
        public int maxJam = 5;
        public int currentJam = 0;
        public bool isJamModeActive = false;

        [Header("Jam UI")]
        public Slider jamSlider;
        public Image jamFillImage;
        public Color normalJamColor = new Color(0.5f, 0f, 0.5f); // Dark Purple
        public Color jamReadyColor = Color.magenta;             // Bright Pink

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            // Initialize Ammo & Shield
            currentAmmo = maxAmmo;
            currentShieldHealth = maxShieldHealth;

            // Initialize UI ranges
            if (jamSlider != null) { jamSlider.maxValue = maxJam; jamSlider.wholeNumbers = true; }
            if (shotSlider != null) { shotSlider.maxValue = 1f; shotSlider.wholeNumbers = false; }

            // Initial Visual States
            UpdateShieldVisuals();
            if (canon != null) canon.SetActive(false);
        }

        void Update()
        {
            // 1. UPDATE UI & CHECK UNLOCKS
            // We do this every frame to handle the "Unlock Pop-in" effect
            UpdateUI();

            // 2. INPUT HANDLING
            HandleInputs();

            // 3. PASSIVE REGEN LOGIC
            if (BO_Manager.instance.isShieldUnlocked)
            {
                HandleShieldRegen();
            }
        }

        // --- CORE INPUT LOOP ---
        void HandleInputs()
        {
            // SHOOTING (Key: E)
            if (BO_Manager.instance.isBerriesUnlocked && Input.GetKeyDown(KeyCode.E))
            {
                AttemptToShoot();
            }

            // JAM MODE (Key: Q)
            if (BO_Manager.instance.isJamUnlocked && Input.GetKeyDown(KeyCode.Q))
            {
                if (currentJam >= maxJam && !isJamModeActive)
                {
                    ActivateJamMode();
                }
            }
        }

        // --- UI LOGIC ---
        void UpdateUI()
        {
            // A. SHOT UI
            if (shotSlider != null)
            {
                bool berriesUnlocked = BO_Manager.instance.isBerriesUnlocked;

                // Toggle visibility based on unlock
                if (shotSlider.gameObject.activeSelf != berriesUnlocked)
                    shotSlider.gameObject.SetActive(berriesUnlocked);

                if (berriesUnlocked)
                {
                    shotSlider.value = GetCooldownNormalized();

                    if (shotFillImage != null)
                    {
                        Color c = shotFillImage.color;
                        c.a = (shotSlider.value >= 1f) ? 1.0f : 0.4f; // Fade if reloading
                        shotFillImage.color = c;
                    }
                }
            }

            // B. JAM UI
            if (jamSlider != null)
            {
                bool jamUnlocked = BO_Manager.instance.isJamUnlocked;

                // Toggle visibility based on unlock
                if (jamSlider.gameObject.activeSelf != jamUnlocked)
                    jamSlider.gameObject.SetActive(jamUnlocked);

                if (jamUnlocked)
                {
                    jamSlider.value = currentJam;
                    if (jamFillImage != null)
                        jamFillImage.color = (currentJam >= maxJam) ? jamReadyColor : normalJamColor;
                }
            }
        }

        // --- BERRY SHOT LOGIC ---
        void AttemptToShoot()
        {
            if (isReloading) return;

            if (currentAmmo > 0)
            {
                currentAmmo--;
                Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + new Vector3(0, 0.5f, 0);
                Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                if (currentAmmo <= 0) StartCoroutine(ReloadRoutine());
            }
            else
            {
                StartCoroutine(ReloadRoutine());
            }
        }

        IEnumerator ReloadRoutine()
        {
            if (isReloading) yield break;
            isReloading = true;
            reloadTimer = reloadTime;

            while (reloadTimer > 0)
            {
                reloadTimer -= Time.deltaTime;
                yield return null;
            }

            currentAmmo = maxAmmo;
            isReloading = false;
        }

        public float GetCooldownNormalized()
        {
            if (!isReloading && currentAmmo > 0) return 1f;
            if (isReloading) return 1f - (reloadTimer / reloadTime);
            return 0f;
        }

        // --- SHIELD LOGIC ---
        void HandleShieldRegen()
        {
            // Ensure visual is ON if we have health
            if (currentShieldHealth > 0 && shieldVisual != null && !shieldVisual.activeSelf)
                shieldVisual.SetActive(true);

            if (currentShieldHealth >= maxShieldHealth) return;

            float regenMultiplier = (maxShieldHealth - currentShieldHealth) + 1;
            shieldRegenTimer += Time.deltaTime * regenMultiplier;

            if (shieldRegenTimer >= baseShieldRegen)
            {
                currentShieldHealth++;
                shieldRegenTimer = 0f;
                UpdateShieldVisuals();
            }
        }

        void UpdateShieldVisuals()
        {
            if (shieldVisual == null) return;

            bool shouldBeVisible = BO_Manager.instance != null && BO_Manager.instance.isShieldUnlocked && currentShieldHealth > 0;
            shieldVisual.SetActive(shouldBeVisible);

            if (shouldBeVisible)
            {
                SpriteRenderer sr = shieldVisual.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = (float)currentShieldHealth / maxShieldHealth;
                    sr.color = c;
                }
            }
        }

        public void TakeShieldHit()
        {
            if (BO_Manager.instance == null || !BO_Manager.instance.isShieldUnlocked || currentShieldHealth <= 0) return;
            currentShieldHealth--;
            UpdateShieldVisuals();
        }

        // --- JAM LOGIC ---
        public void AddBerry()
        {
            if (currentJam < maxJam)
            {
                currentJam++;
            }
        }

        void ActivateJamMode()
        {
            BO_Ball ball = Object.FindFirstObjectByType<BO_Ball>();
            if (ball != null)
            {
                isJamModeActive = true;
                ball.EnableJamCoating();
                currentJam = 0;
                Debug.Log("JAM MODE ACTIVE!");
            }
        }

        public void JamHitComplete()
        {
            isJamModeActive = false;
            Debug.Log("Jam Attack landed! Mode deactivated.");
        }
    }
}