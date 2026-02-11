using UnityEngine;
using System.Collections;

namespace BreakOut
{
    public class BO_BossPaw : MonoBehaviour
    {
        [Header("Damage Settings")]
        public float baseDamage = 10f;
        public float damageMultiplier = 2.0f;

        [Header("Visual Feedback")]
        public Color flashColor = new Color(1f, 0f, 0f, 0.5f);
        public float flashDuration = 0.1f;

        [Header("Retraction Mechanic")]
        public float retractDistance = 2.0f;
        public float retractSpeed = 5f;
        public float cooldownTime = 3.0f;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Vector3 startingPosition; // Now stores World Position
        private bool isRetracted = false;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;

            // We Initialize this here just in case, 
            // but the Entrance script will overwrite it with the CORRECT position later.
            startingPosition = transform.position;
        }

        // --- NEW METHOD called by Entrance Script ---
        public void UpdateHomePosition()
        {
            // Capture where we are NOW (after the slide) as the new "Home"
            startingPosition = transform.position;
            Debug.Log($"[BossPaw] New Home Position set: {startingPosition}");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isRetracted && collision.gameObject.CompareTag("BreakOutBall"))
            {
                BO_BossHead.Instance.TakeDamage(baseDamage * damageMultiplier);
                StartCoroutine(RetractSequence());
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isRetracted && other.CompareTag("BO_Projectile"))
            {
                StartCoroutine(RetractSequence());
            }
        }

        IEnumerator RetractSequence()
        {
            isRetracted = true;

            if (BO_Paddle.Instance != null) BO_Paddle.Instance.RestorePaddle();
            if (BO_BossHead.Instance != null) BO_BossHead.Instance.GetComponent<BO_BossAttack>().StunBoss(3f);

            if (spriteRenderer != null) spriteRenderer.color = flashColor;

            // --- Slide UP (Using World Position) ---
            // We use the UPDATED startingPosition as the anchor
            Vector3 targetUp = startingPosition + new Vector3(0, retractDistance, 0);
            yield return StartCoroutine(MovePaw(targetUp));

            if (spriteRenderer != null) spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(cooldownTime);

            // --- Slide DOWN (Back to the UPDATED Home) ---
            yield return StartCoroutine(MovePaw(startingPosition));

            isRetracted = false;
        }

        // Helper: Changed from localPosition to position (World) to match Entrance script
        IEnumerator MovePaw(Vector3 destination)
        {
            // Use Vector3.Distance check on World Position
            while (Vector3.Distance(transform.position, destination) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destination,
                    retractSpeed * Time.deltaTime
                );
                yield return null;
            }
            transform.position = destination;
        }

        public void FlashPaw()
        {
            StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (spriteRenderer == null) yield break;
            Color tempColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = tempColor;
        }
    }
}