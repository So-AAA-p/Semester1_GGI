using UnityEngine;

namespace BreakOut
{
    public class BO_Paddle : MonoBehaviour
    {
        public BO_Controls controls = new BO_Controls();
        public static BO_Paddle Instance;

        public enum Direction { Right, Left }

        private SpriteRenderer spriteRenderer;
        private float defaultSpeed; // To remember our original speed

        [Header("Paddle Colors")]
        public Color normalColor = Color.white;
        public Color reversedColor = new Color(1f, 0.4f, 0.4f);

        private void Awake() { Instance = this; }

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // SAVE the original speed from your controls object at the start
            defaultSpeed = controls.paddleSpeed;

            UpdatePaddleColor();
        }

        void UpdatePaddleColor()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.color = BO_Manager.instance.controlsReversed ? reversedColor : normalColor;
        }

        void Update()
        {
            bool reversed = BO_Manager.instance.controlsReversed;
            UpdatePaddleColor();

            if (Input.GetKey(controls.LeftKey))
                Move(reversed ? Direction.Right : Direction.Left);

            if (Input.GetKey(controls.RightKey))
                Move(reversed ? Direction.Left : Direction.Right);
        }

        void Move(Direction direction)
        {
            // We use controls.paddleSpeed here, so that's the one we must modify!
            float moveDistance = controls.paddleSpeed * (3 * Time.deltaTime);
            moveDistance *= direction == Direction.Right ? 1 : -1;

            float newX = transform.position.x + moveDistance;

            if (newX > controls.maxX)
                transform.position = new Vector3(controls.maxX, transform.position.y, transform.position.z);
            else if (newX < controls.minX)
                transform.position = new Vector3(controls.minX, transform.position.y, transform.position.z);
            else
                transform.Translate(new Vector3(moveDistance, 0, 0));
        }

        public void ApplyBakingModifiers()
        {
            if (BO_Manager.instance == null) return;

            // We modify controls.paddleSpeed directly so the Move() function sees it
            if (BO_Manager.instance.lastBakingResult == BO_Manager.BakingResult.Undercooked)
            {
                controls.paddleSpeed = defaultSpeed * 0.7f;
                Debug.Log($"[Paddle] Penalty: Undercooked! Speed slowed to {controls.paddleSpeed}");
            }
            else if (BO_Manager.instance.lastBakingResult == BO_Manager.BakingResult.Perfect)
            {
                controls.paddleSpeed = defaultSpeed * 1.2f; // A little reward for being a master baker
                Debug.Log($"[Paddle] Bonus: Perfect! Speed boosted to {controls.paddleSpeed}");
            }
            else
            {
                controls.paddleSpeed = defaultSpeed;
                Debug.Log("[Paddle] Speed is normal.");
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("BO_Blueberry"))
            {
                BO_BlueberryManager.Instance.CollectBerry();
                Destroy(collision.gameObject);
            }
        }
    }
}