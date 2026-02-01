using UnityEngine;

namespace BreakOut
{
    public class BreakOutPaddle : MonoBehaviour
    {
        public BreakOutControls controls = new BreakOutControls();

        public enum Direction
        {
            Right,
            Left
        }

        private SpriteRenderer spriteRenderer;

        [Header("Paddle Colors")]
        public Color normalColor = Color.white;
        public Color reversedColor = new Color(1f, 0.4f, 0.4f); // light red


        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdatePaddleColor();
        }

        void UpdatePaddleColor()
        {
            if (spriteRenderer == null) return;

            spriteRenderer.color =
                BreakOutManager.instance.controlsReversed
                ? reversedColor
                : normalColor;
        }

        void Update()
        {
            bool reversed = BreakOutManager.instance.controlsReversed;

            UpdatePaddleColor();

            if (Input.GetKey(controls.LeftKey))
            {
                Move(reversed ? Direction.Right : Direction.Left);
            }

            if (Input.GetKey(controls.RightKey))
            {
                Move(reversed ? Direction.Left : Direction.Right);
            }
        }

        void Move(Direction direction)
        {
            float moveDistance = controls.paddleSpeed * (3 * Time.deltaTime);
            moveDistance *= direction == Direction.Right ? 1 : -1;

            Vector3 moveVector = new Vector3(moveDistance, 0, 0);

            float newX = transform.position.x + moveDistance;

            if (newX > controls.maxX)
            {
                transform.position = new Vector3(
                    controls.maxX,
                    transform.position.y,
                    transform.position.z
                );
            }
            else if (newX < controls.minX)
            {
                transform.position = new Vector3(
                    controls.minX,
                    transform.position.y,
                    transform.position.z
                );
            }
            else
            {
                transform.Translate(moveVector);
            }
        }
    }
}
