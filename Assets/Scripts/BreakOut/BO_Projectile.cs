using UnityEngine;

namespace BreakOut
{
    public class BO_Projectile : MonoBehaviour
    {
        [Header("Projectile Stats")]
        public float speed = 15f;    // Fast enough to feel like a bullet
        public float damage = 2f;    // Small chip damage (5 shots = 10 dmg)
        public float lifetime = 2f;  // Destroy if it misses

        void Start()
        {
            // Auto-destroy after 2 seconds to keep the game clean
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            // Move straight UP
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // --- HIT THE BOSS HEAD ---
            if (collision.CompareTag("BO_BossHead"))
            {
                if (BO_BossHead.Instance != null)
                {
                    // 1. Damage the boss
                    BO_BossHead.Instance.TakeDamage(damage);
                    // 2. Trigger Head flash (assuming it happens inside TakeDamage)
                    Debug.Log("Headshot!");
                }
                Destroy(gameObject);
                return;
            }

            // --- HIT THE PAWS ---
            if (collision.CompareTag("BO_BossPaw"))
            {
                var pawScript = collision.GetComponent<BO_BossPaw>();
                if (pawScript != null) pawScript.FlashPaw();

                if (BO_BossHead.Instance != null)
                {
                    // Tell the head: Take damage, but DON'T flash (false)
                    BO_BossHead.Instance.TakeDamage(damage, false);
                }

                Destroy(gameObject);
                return;
            }

            // --- WALLS/BLOCKS ---
            string layerName = LayerMask.LayerToName(collision.gameObject.layer);
            if (layerName == "BreakOutWalls" || layerName == "BreakOutBlock")
            {
                Destroy(gameObject);
            }
        }
    }
}