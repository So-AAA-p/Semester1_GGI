using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BreakOut
{
    public class BO_BossHead : MonoBehaviour
    {
        // --- NEW: Singleton to allow Paws to find the Head easily ---
        public static BO_BossHead Instance;

        public enum Phase { PhaseOne, PhaseTwo, PhaseThree }
        public Phase currentPhase = Phase.PhaseOne;

        [Header("Stats")]
        public float maxHealth = 100f;
        private float currentHealth;

        [Header("UI & Visuals")]
        public GameObject bossVisualParent;
        public Slider healthSlider;
        public float healthChangeSpeed = 5f;
        public SpriteRenderer bossSprite;
        public Color flashColor = new Color(1f, 0f, 0f, 0.5f);
        public float flashDuration = 0.1f;

        private Color originalColor;
        private Coroutine flashRoutine;

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

        void Die()
        {
            Debug.Log("[Boss] The Chihuahua has been tuckered out! (Defeated)");
            if (bossVisualParent != null) bossVisualParent.SetActive(false);
            if (healthSlider != null) healthSlider.gameObject.SetActive(false);

            if (BO_StageController.Instance != null)
            {
                BO_StageController.Instance.ShowScreen(BO_StageController.Instance.Stage3Win);
                BO_Manager.instance.SetState(BO_Manager.GameState.Transition);
                BO_Manager.instance.ClearAllBalls();
            }
        }

        void UpdatePhase()
        {
            float healthPercent = currentHealth / maxHealth;
            if (healthPercent < 0.33f) currentPhase = Phase.PhaseThree;
            else if (healthPercent < 0.66f) currentPhase = Phase.PhaseTwo;
            else currentPhase = Phase.PhaseOne;
            Debug.Log($"[Boss] Current Phase: {currentPhase}");
        }
    }
}