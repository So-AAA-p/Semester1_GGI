using UnityEngine;
using System.Collections;

namespace BreakOut
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlourCloudEffect : MonoBehaviour
    {
        [Header("Fade Settings")]
        public float holdTime = 1.0f; // How long it stays fully white/opaque
        public float fadeDuration = 2.5f; // How long it takes to disappear completely

        private SpriteRenderer spriteRenderer;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Start the fading process as soon as this object spawns
            StartCoroutine(FadeOutRoutine());
        }

        IEnumerator FadeOutRoutine()
        {
            // 1. Wait while the cloud is thick
            yield return new WaitForSeconds(holdTime);

            float timer = 0f;
            Color startColor = spriteRenderer.color;

            // 2. Loop to fade out over time
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;

                // Calculate how far along we are (0 to 1)
                float progress = timer / fadeDuration;

                // Math.Lerp moves smoothly from A to B
                // We keep the Red, Green, Blue the same, but change Alpha (A)
                float newAlpha = Mathf.Lerp(startColor.a, 0f, progress);

                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

                yield return null; // Wait for the next frame
            }

            // 3. Ensure it is fully invisible and destroy it
            Destroy(gameObject);
        }
    }
}
