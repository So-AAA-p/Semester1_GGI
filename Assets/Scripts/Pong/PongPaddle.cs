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
            Negative                                                            // runter oder nach links
        }

        public bool moveHorizontally = false;
        private bool sizeLocked;

        [Header("Size Modifiers")]
        public float sizeStep = 0.3f;
        public float minHeight = 1.0f;
        public float maxHeight = 4.0f;

        private SpriteRenderer sr;
        private Color originalColor;

        public static readonly Color DarkGreen = new Color(0.2f, 0.5f, 0.2f);
        public static readonly Color DarkRed = new Color(0.5f, 0.2f, 0.2f);

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            originalColor = sr.color;
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
            if(Input.GetKey(controls.PositiveKey))
            {
                //Debug.Log("Nach unten gedrückt");
                Move(Direction.Positive);
            }
            if (Input.GetKey(controls.NegativeKey))
            {
                //Debug.Log("Nach oben gedrückt");
                Move(Direction.Negative);
            }
        }

        void Move(Direction direction)
        {
            float moveAmount = controls.paddleSpeed * Time.deltaTime;
            moveAmount *= direction == Direction.Positive ? 1 : -1;
            // int directionMultiplier = direction == Direction.Up ? 1 : -1;
            // moveDistance *= directionMultiplier;

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
    }
}
