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
        public float retractDistance = 2.0f; // How far up it moves
        public float retractSpeed = 5f;      // Speed of the slide
        public float cooldownTime = 3.0f;    // How long it stays up

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Vector3 startingPosition;
        private bool isRetracted = false;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;

            startingPosition = transform.localPosition;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Only take damage and retract if it's not already "hidden"
            if (!isRetracted && collision.gameObject.CompareTag("BreakOutBall"))
            {
                // 1. Send damage to Head
                BO_BossHead.Instance.TakeDamage(baseDamage * damageMultiplier);

                // 2. Start the Retract and Flash sequence
                StartCoroutine(RetractSequence());
            }
        }

        // This handles the Berry Projectiles (Triggers)
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if it's a projectile and make sure the paw isn't already retracted
            if (!isRetracted && other.CompareTag("BO_Projectile"))
            {
                // 1. We don't need to send damage here because the Projectile 
                // script is already doing that! We just handle the movement.

                // 2. Start the Retract and Flash sequence
                StartCoroutine(RetractSequence());
            }
        }

        IEnumerator RetractSequence()
        {
            isRetracted = true;

            // Visual Flash
            if (spriteRenderer != null) spriteRenderer.color = flashColor;

            // --- Slide UP ---
            Vector3 targetUp = startingPosition + new Vector3(0, retractDistance, 0);
            yield return StartCoroutine(MovePaw(targetUp));

            // Reset color once it's up
            if (spriteRenderer != null) spriteRenderer.color = originalColor;

            // --- Wait in hiding ---
            yield return new WaitForSeconds(cooldownTime);

            // --- Slide DOWN ---
            yield return StartCoroutine(MovePaw(startingPosition));

            isRetracted = false;
        }

        // Helper coroutine to smooth the sliding movement
        IEnumerator MovePaw(Vector3 destination)
        {
            while (Vector3.Distance(transform.localPosition, destination) > 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(
                    transform.localPosition,
                    destination,
                    retractSpeed * Time.deltaTime
                );
                yield return null;
            }
            transform.localPosition = destination;
        }

        public void FlashPaw()
        {
            // If you have a flash routine already, call it here
            // Otherwise, a quick way is:
            StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
        }
    }
}