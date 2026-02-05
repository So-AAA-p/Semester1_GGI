using UnityEngine;

namespace BreakOut
{
    public class BO_ChihuahuaBoss : MonoBehaviour
    {
        public enum Phase { PhaseOne, PhaseTwo, PhaseThree }
        public Phase currentPhase = Phase.PhaseOne;

        public float maxHealth = 100f;
        private float currentHealth;


        private void Start()
        {
            // Initialize health at the start
            currentHealth = maxHealth;
            Debug.Log($"[Boss] Chihuahua spawned with {currentHealth} HP!");
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;

            // Step 1: Console Update
            Debug.Log($"[Boss] Ouch! Took {amount} damage. HP: {currentHealth}/{maxHealth}");

            UpdatePhase();

            if (currentHealth <= 0)
            {
                Debug.Log("[Boss] The Chihuahua has been tuckered out! (Defeated)");
                // Handle death logic later
            }
        }

        void UpdatePhase()
        {
            float healthPercent = currentHealth / maxHealth;

            // Simple logic: Change phase based on HP thresholds
            if (healthPercent < 0.33f) currentPhase = Phase.PhaseThree;
            else if (healthPercent < 0.66f) currentPhase = Phase.PhaseTwo;
            else currentPhase = Phase.PhaseOne;

            // Log phase changes
            Debug.Log($"[Boss] Current Phase: {currentPhase}");
        }
    }
}

