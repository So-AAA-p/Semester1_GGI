using UnityEngine;
using System.Collections;

namespace BreakOut
{
    public class BO_BossAttack : MonoBehaviour
    {
        public GameObject moldShotPrefab;
        public Transform playerPaddle;
        public Transform firePoint; // Optional: The mouth/spawn point

        // --- DIFFICULTY SETTINGS ---

        [Header("Phase 1: Warming Up")]
        public float fireRatePhase1 = 3.0f; // Fire every 3 seconds
        public float wobblePhase1 = 30f;    // Very sloppy
        public float speedPhase1 = 4f;      // Slow speed

        [Header("Phase 2: Getting Serious")]
        public float fireRatePhase2 = 2.0f; // Fire every 2 seconds
        public float wobblePhase2 = 15f;    // Decent aim
        public float speedPhase2 = 8f;      // Medium speed

        [Header("Phase 3: RAGE MODE")]
        public float fireRatePhase3 = 1.0f; // Fire every 1 second (Fast!)
        public float wobblePhase3 = 0f;     // Perfect aim
        public float speedPhase3 = 12f;     // Fast speed


        private float currentWaitTime;

        private void Start()
        {
            StartCoroutine(AttackRoutine());
        }

        IEnumerator AttackRoutine()
        {
            while (true)
            {
                // 1. Determine how long to wait based on the CURRENT phase
                currentWaitTime = fireRatePhase1; // Default

                if (BO_BossHead.Instance != null)
                {
                    switch (BO_BossHead.Instance.currentPhase)
                    {
                        case BO_BossHead.Phase.PhaseOne:
                            currentWaitTime = fireRatePhase1;
                            break;
                        case BO_BossHead.Phase.PhaseTwo:
                            currentWaitTime = fireRatePhase2;
                            break;
                        case BO_BossHead.Phase.PhaseThree:
                            currentWaitTime = fireRatePhase3;
                            break;
                    }
                }

                // 2. Wait for that specific time
                yield return new WaitForSeconds(currentWaitTime);

                // 3. Shoot!
                if (BO_BossHead.Instance != null)
                {
                    ShootAtPlayer();
                }
            }
        }

        void ShootAtPlayer()
        {
            if (playerPaddle == null) return;

            // Setup variables
            float currentMaxWobble = 0f;
            float currentSpeed = 5f;
            string debugPhase = "";

            // Get settings based on Phase
            switch (BO_BossHead.Instance.currentPhase)
            {
                case BO_BossHead.Phase.PhaseOne:
                    currentMaxWobble = wobblePhase1;
                    currentSpeed = speedPhase1;
                    debugPhase = "Phase 1";
                    break;
                case BO_BossHead.Phase.PhaseTwo:
                    currentMaxWobble = wobblePhase2;
                    currentSpeed = speedPhase2;
                    debugPhase = "Phase 2";
                    break;
                case BO_BossHead.Phase.PhaseThree:
                    currentMaxWobble = wobblePhase3;
                    currentSpeed = speedPhase3;
                    debugPhase = "Phase 3";
                    break;
            }

            // Calculate Aim
            Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
            Vector2 direction = (Vector2)playerPaddle.position - (Vector2)spawnPos;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Apply Wobble
            float actualWobble = Random.Range(-currentMaxWobble, currentMaxWobble);
            Quaternion finalRotation = Quaternion.Euler(0, 0, angle - 90f + actualWobble);

            // Spawn & Set Speed
            GameObject shotObj = Instantiate(moldShotPrefab, spawnPos, finalRotation);
            BO_MoldShot moldScript = shotObj.GetComponent<BO_MoldShot>();

            if (moldScript != null)
            {
                moldScript.SetSpeed(currentSpeed);
            }

            // Debug Log to track the chaos
            Debug.Log($"[Boss] {debugPhase} | Rate: {currentWaitTime}s | Speed: {currentSpeed} | Wobble: {currentMaxWobble}");
        }
    }
}