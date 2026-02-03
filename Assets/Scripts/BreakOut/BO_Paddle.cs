using UnityEngine;

namespace BreakOut
{
    public class BO_Paddle : MonoBehaviour
    {
        public BO_Controls controls = new BO_Controls();

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
                BO_Manager.instance.controlsReversed
                ? reversedColor
                : normalColor;
        }

        void Update()
        {
            bool reversed = BO_Manager.instance.controlsReversed;

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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("BO_Blueberry"))
            {
                // 1. Tell the manager we caught one!
                BO_BlueberryManager.Instance.CollectBerry();

                // 2. Play a sound or effect here if you want!

                // 3. Destroy the falling berry object
                Destroy(collision.gameObject);
            }
        }
    }
}
