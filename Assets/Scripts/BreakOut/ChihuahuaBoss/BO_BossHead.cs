using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BreakOut
{
    public class BO_BossHead : MonoBehaviour
    {
        // --- NEW: Singleton to allow Paws to find the Head easily ---
        public static BO_BossHead Instance;

        public enum Phase 
        {
            PhaseZero,   // 100-90% (Plain Breakout)
            PhaseOne,    // 90-70%  (Unlock Berries/Shots)
            PhaseTwo,    // 70-40%  (Unlock Shield)
            PhaseThree   // 40-0%   (Unlock Jam Mode + Leaves)
        }
        public Phase currentPhase = Phase.PhaseZero;

        [Header("Stats")]
        public float maxHealth = 100f;
        private float currentHealth;

        [Header("UI & Visuals")]
        public GameObject bossVisualParent;
        public Slider healthSlider;
        public float healthChangeSpeed = 5f;
        public SpriteRenderer bossSprite;
        public Sprite normalSprite;      // Your default idle head
        public Sprite attackSprite;      // Mouth open
        public Sprite deadSprite;        // The "tuckered out" version
        public Color flashColor = new Color(1f, 0f, 0f, 0.5f);
        public float flashDuration = 0.1f;

        private Color originalColor;
        private Coroutine flashRoutine;

        public bool isDead = false;

        private void Awake()
        {
            // Set up the instance
            Instance = this;
        }

        private void Start()
        {
            currentHealth = maxHealth;
            Debug.Log($"[Boss] Chihuahua spawned with {currentHealth} HP!");

            if (bossSprite != null)
                originalColor = bossSprite.color;

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = maxHealth;
            }

            if (healthSlider != null) healthSlider.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (healthSlider != null && healthSlider.value != currentHealth)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, currentHealth, Time.deltaTime * healthChangeSpeed);
            }
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("BreakOutBall"))
            {
                TakeDamage(3f); 
            }
        }

        bool shouldFlash = true;

        public void TakeDamage(float amount, bool shouldFlash = true)
        {
            currentHealth -= amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // Only flash the head if the switch is TRUE
            if (shouldFlash)
            {
                if (flashRoutine != null) StopCoroutine(flashRoutine);
                flashRoutine = StartCoroutine(HitFlashRoutine());
            }

            UpdatePhase();

            if (currentHealth <= 0) Die();
        }

        private IEnumerator HitFlashRoutine()
        {
            if (bossSprite == null) yield break;
            bossSprite.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            bossSprite.color = originalColor;
            flashRoutine = null;
        }

        public void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            //Debug.Log($"[Boss] Mmm, tasty paddle! Healed to {currentHealth} HP.");

            // Optional: Add a green flash later to show healing
        }

        void Die()
        {
            if (isDead) return; // Prevent Die from running twice
            isDead = true;

            Debug.Log("[Boss] The Chihuahua has been tuckered out! (Defeated)");

            // Stop any flashing that might be happening
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            bossSprite.color = originalColor;

            SetBossSprite(deadSprite);

            if (healthSlider != null) healthSlider.gameObject.SetActive(false);

            if (BO_StageController.Instance != null)
            {
                BO_StageController.Instance.ShowScreen(BO_StageController.Instance.Stage3Win);
                BO_Manager.instance.SetState(BO_Manager.GameState.Transition);
                BO_Manager.instance.ClearAllBalls();
            }
        }

        public void SetBossSprite(Sprite newSprite)
        {
            // If the boss is dead, don't let any other script (like Attack) change the sprite back!
            if (isDead && newSprite != deadSprite) return;

            if (bossSprite != null && newSprite != null)
            {
                bossSprite.sprite = newSprite;
            }
        }

        void UpdatePhase()
        {
            float healthPercent = currentHealth / maxHealth;
            Phase previousPhase = currentPhase;

            // --- Thresholds ---
            if (healthPercent < 0.40f) currentPhase = Phase.PhaseThree;
            else if (healthPercent < 0.70f) currentPhase = Phase.PhaseTwo;
            else if (healthPercent < 0.90f) currentPhase = Phase.PhaseOne;
            else currentPhase = Phase.PhaseZero;

            if (previousPhase == currentPhase) return;

            // --- TRIGGER LOGIC ---

            // Enter Phase 1: BERRY SHOTS
            if (previousPhase == Phase.PhaseZero && currentPhase == Phase.PhaseOne)
            {
                Debug.Log("[Boss] Entering Phase 1 - Unlocking Berries");
                // We'll need to create this method in StageController
                BO_StageController.Instance.TriggerPhase1Tutorial();
            }

            // Enter Phase 2: SHIELD
            else if (previousPhase == Phase.PhaseOne && currentPhase == Phase.PhaseTwo)
            {
                Debug.Log("[Boss] Entering Phase 2 - Unlocking Shield");
                BO_StageController.Instance.TriggerPhase2Tutorial();
            }

            // Enter Phase 3: JAM MODE + LEAVES
            else if (previousPhase == Phase.PhaseTwo && currentPhase == Phase.PhaseThree)
            {
                Debug.Log("[Boss] Entering Phase 3 - Unlocking Jam Mode");
                if (BO_LeafManager.Instance != null) BO_LeafManager.Instance.StartPhase3();
                BO_StageController.Instance.TriggerPhase3Tutorial();
            }
        }
    }
}