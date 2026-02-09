using UnityEngine;
using System.Collections;

namespace Pong
{
    public class PongPaddle : MonoBehaviour
    {
        public PongControls controls;

        public enum Direction
        {
            Positive,                                                           // hoch oder nach rechts
            Negative,                                                           // runter oder nach links
            None
        }


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

        public bool ModifySize(float delta)
        {
            Vector3 scale = transform.localScale;
            float oldY = scale.y;

            scale.y += delta;
            scale.y = Mathf.Clamp(scale.y, minHeight, maxHeight);

            transform.localScale = scale;

            // return true if size actually changed
            return !Mathf.Approximately(oldY, scale.y);
        }

        void UnlockSize()
        {
            sizeLocked = false;
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
            sr.color = originalColor; // your #E0B46E color
        }


        void Update()
        {
            float input = 0;
            if (Input.GetKey(controls.PositiveKey)) input = 1;
            if (Input.GetKey(controls.NegativeKey)) input = -1;

            // Check level from the Manager
            bool isLevel1 = PongManager.instance.currentLevel == PongManager.LevelType.Classic;

            if (isLevel1 && paddleRb != null)
            {
                // ONLY Level 1 gets this
                ApplyIceMovement(input);
            }
            else
            {
                // LEVEL 2, 3, etc. get snappy movement
                if (paddleRb != null)
                {
                    paddleRb.linearVelocity = Vector2.zero;
                }

                // Use your original Move method for precision
                Move(input > 0 ? Direction.Positive : (input < 0 ? Direction.Negative : Direction.None));
            }
        }

        void Move(Direction direction)
        {
            float moveAmount = controls.paddleSpeed * Time.deltaTime;
            moveAmount *= direction == Direction.Positive ? 1 : -1;
            // int directionMultiplier = direction == Direction.Up ? 1 : -1;
            // moveDistance *= directionMultiplier;

            if (direction == Direction.None)
            {
                // If we aren't pressing anything, don't move at all!
                return;
            }

            if (moveHorizontally)
            {
                float newX = transform.position.x + moveAmount;

                newX = Mathf.Clamp(newX, controls.minX, controls.maxX);

                transform.position = new Vector3(
                    newX,
                    transform.position.y,
                    transform.position.z
                );
            }
            else
            {
                float newY = transform.position.y + moveAmount;

                newY = Mathf.Clamp(newY, controls.minY, controls.maxY);

                transform.position = new Vector3(
                    transform.position.x,
                    newY,
                    transform.position.z
                );
            }
            //Debug.Log(direction.ToString() + "gedrückt");
        }

        void ApplyIceMovement(float input)
        {
            // We only need to check the axis here, not the level anymore 
            // because the level check happened in Update()
            if (moveHorizontally)
            {
                paddleRb.AddForce(Vector2.right * input * controls.paddleSpeed);
            }
            else
            {
                paddleRb.AddForce(Vector2.up * input * controls.paddleSpeed);
            }
        }
    }
}
