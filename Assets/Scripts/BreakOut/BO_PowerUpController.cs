using UnityEngine;
using System.Collections;

namespace BreakOut
{
    public class BO_PowerUpController : MonoBehaviour
    {
        [Header("Berries (Shots) Settings")]
        public GameObject projectilePrefab; // Drag the 'BerryProjectile' here!
        public Transform firePoint;         // Optional: A specific point on the paddle to fire from

        public int maxAmmo = 5;
        public int currentAmmo;
        public float reloadTime = 15f;

        private bool isReloading = false;

        [Header("Shield Logic")]
        public GameObject shieldVisual; // Drag the 'Shield' child object here
        public int maxShieldHealth = 3;
        public float baseShieldRegen = 10f;
        private int currentShieldHealth;
        public float shieldRegenTimer = 0f;

        void Start()
        {
            currentAmmo = maxAmmo;
            currentShieldHealth = maxShieldHealth;
            UpdateShieldVisuals();
        }

        void Update()
        {
            // PRESS 'E' TO SHOOT
            if (Input.GetKeyDown(KeyCode.E))
            {
                AttemptToShoot();
            }

            HandleShieldRegen();
        }
        void HandleShieldRegen()
        {
            // If shield is full, we don't need to do anything
            if (currentShieldHealth >= maxShieldHealth) return;

            // MATH TIME: The regen speed depends on how much health is missing
            // 3 health left: slow regen. 1 health left: fast regen.
            float regenMultiplier = (maxShieldHealth - currentShieldHealth) + 1;
            shieldRegenTimer += Time.deltaTime * regenMultiplier;

            // If timer hits the base regen threshold (10s), we gain 1 health
            if (shieldRegenTimer >= baseShieldRegen)
            {
                currentShieldHealth++;
                shieldRegenTimer = 0f;
                UpdateShieldVisuals();
                Debug.Log($"Shield Regained! Current: {currentShieldHealth}");
            }
        }

        void UpdateShieldVisuals()
        {
            // If shield health is 0, turn the visual off. Otherwise, turn it on.
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(currentShieldHealth > 0);

                // Optional: Make it more transparent as it gets weaker
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
            if (currentShieldHealth <= 0) return;

            currentShieldHealth--;
            Debug.Log($"Shield hit! Health remaining: {currentShieldHealth}");

            UpdateShieldVisuals();
        }

        void AttemptToShoot()
        {
            if (isReloading)
            {
                Debug.Log("Reloading! Cannot shoot yet.");
                return;
            }

            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                Debug.Log("Click! Out of ammo.");
                // Ensure reload starts if it hasn't already (failsafe)
                StartCoroutine(ReloadRoutine());
            }
        }

        void Shoot()
        {
            currentAmmo--;
            Debug.Log($"Pew! Ammo left: {currentAmmo}");

            // Spawn the bullet
            // If we have a specific firePoint, use it. Otherwise, use paddle center + slight offset
            Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + new Vector3(0, 0.5f, 0);

            Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            // Check if that was the last shot
            if (currentAmmo <= 0)
            {
                StartCoroutine(ReloadRoutine());
            }
        }

        IEnumerator ReloadRoutine()
        {
            if (isReloading) yield break; // Don't start twice

            isReloading = true;
            Debug.Log($"Empty! Reloading for {reloadTime} seconds...");

            yield return new WaitForSeconds(reloadTime);

            currentAmmo = maxAmmo;
            isReloading = false;
            Debug.Log("RELOAD COMPLETE! Ammo full.");
        }
    }
}
