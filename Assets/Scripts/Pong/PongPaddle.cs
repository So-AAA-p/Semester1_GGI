using UnityEngine;
using System.Collections;

namespace Pong
{
    public class PongPaddle : MonoBehaviour
    {
        public PongControls controls;

        // We don't need the Direction enum anymore!

        public bool moveHorizontally = false;
        private bool sizeLocked;

        [Header("Size Modifiers")]
        public float sizeStep = 0.3f;
        public float minHeight = 1.0f;
        public float maxHeight = 4.0f;

        private SpriteRenderer sr;
        private Color originalColor;
        private Rigidbody2D paddleRb;

        public static readonly Color DarkGreen = new Color(0.2f, 0.5f, 0.2f);
        public static readonly Color DarkRed = new Color(0.5f, 0.2f, 0.2f);

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            originalColor = sr.color;
            paddleRb = GetComponent<Rigidbody2D>();
        }

        // --- PHYSICS MOVEMENT LOOP ---
        void FixedUpdate()
        {
            float input = 0;
            if (Input.GetKey(controls.PositiveKey)) input = 1;
            if (Input.GetKey(controls.NegativeKey)) input = -1;

            // Check if we are in Level 1 (Ice Mode)
            bool isLevel1 = false;
            if (PongManager.instance != null) // Safety check
            {
                isLevel1 = (PongManager.instance.currentLevel == PongManager.LevelType.Classic);
            }

            if (isLevel1)
            {
                // TYPE A: Ice / Sliding Movement (AddForce)
                Vector2 forceDirection = moveHorizontally ? Vector2.right : Vector2.up;
                paddleRb.AddForce(forceDirection * input * controls.paddleSpeed);
            }
            else
            {
                // TYPE B: Snappy / Exact Movement (Velocity)
                // This stops instantly when you let go of the key.
                Vector2 velocityVector = Vector2.zero;

                if (moveHorizontally)
                    velocityVector = new Vector2(input * controls.paddleSpeed, 0);
                else
                    velocityVector = new Vector2(0, input * controls.paddleSpeed);

                paddleRb.linearVelocity = velocityVector;
            }
        }

        // --- SIZE & VISUALS ---

        public bool ModifySize(float delta)
        {
            Vector3 scale = transform.localScale;
            float oldY = scale.y;

            scale.y += delta;
            scale.y = Mathf.Clamp(scale.y, minHeight, maxHeight);

            transform.localScale = scale;

            // Note: Since we use Physics now, the BoxCollider2D automatically 
            // resizes with the transform. No extra code needed!
            return !Mathf.Approximately(oldY, scale.y);
        }

        public void Flash(Color flashColor, float duration)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine(flashColor, duration));
        }

        IEnumerator FlashRoutine(Color flashColor, float duration)
        {
            sr.color = flashColor;
            yield return new WaitForSeconds(duration);
            sr.color = originalColor;
        }
    }
}