using UnityEngine;
using System.Collections;

namespace BreakOut
{
    public class BO_PowerUpController : MonoBehaviour
    {
        [Header("Berries (Shots) Settings")]
        public GameObject projectilePrefab;
        public Transform firePoint;
        public int maxAmmo = 5;
        public int currentAmmo;
        public float reloadTime = 15f;
        private bool isReloading = false;

        [Header("Shield Logic")]
        public GameObject shieldVisual;
        public int maxShieldHealth = 3;
        public float baseShieldRegen = 10f;
        private int currentShieldHealth;
        public float shieldRegenTimer = 0f;

        void Start()
        {
            currentAmmo = maxAmmo;
            currentShieldHealth = maxShieldHealth;

            // --- FIX: Initial Visual Check ---
            // This ensures the shield is OFF at the start of the game
            UpdateShieldVisuals();
        }

        void Update()
        {
            // --- BERRY SHOTS UNLOCK CHECK ---
            // (Wait for your "Phase 1 idea" to set this to true in Manager)
            if (BO_Manager.instance.isJamUnlocked && Input.GetKeyDown(KeyCode.E))
            {
                AttemptToShoot();
            }

            // --- SHIELD UNLOCK CHECK ---
            if (BO_Manager.instance.isShieldUnlocked)
            {
                HandleShieldRegen();

                // We also need to make sure the visual hasn't been 
                // accidentally left off once it IS unlocked
                if (currentShieldHealth > 0 && shieldVisual != null && !shieldVisual.activeSelf)
                {
                    shieldVisual.SetActive(true);
                }
            }
            else
            {
                // FORCE OFF if not unlocked yet
                if (shieldVisual != null && shieldVisual.activeSelf)
                {
                    shieldVisual.SetActive(false);
                }
            }
        }

        void HandleShieldRegen()
        {
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

            // --- THE CRITICAL FIX ---
            // Only allow the shield to be active if it's UNLOCKED in the manager
            bool shouldBeVisible = BO_Manager.instance.isShieldUnlocked && currentShieldHealth > 0;

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
            // Can't take a hit if we don't have the shield yet!
            if (!BO_Manager.instance.isShieldUnlocked || currentShieldHealth <= 0) return;

            currentShieldHealth--;
            UpdateShieldVisuals();
        }

        void AttemptToShoot()
        {
            if (isReloading) return;
            if (currentAmmo > 0) Shoot();
            else StartCoroutine(ReloadRoutine());
        }

        void Shoot()
        {
            currentAmmo--;
            Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + new Vector3(0, 0.5f, 0);
            Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            if (currentAmmo <= 0) StartCoroutine(ReloadRoutine());
        }

        IEnumerator ReloadRoutine()
        {
            if (isReloading) yield break;
            isReloading = true;
            yield return new WaitForSeconds(reloadTime);
            currentAmmo = maxAmmo;
            isReloading = false;
        }
    }
}