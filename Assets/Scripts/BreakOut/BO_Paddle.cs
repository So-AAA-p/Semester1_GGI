using UnityEngine;

namespace BreakOut
{
    public class BO_Paddle : MonoBehaviour
    {
        public BO_Controls controls = new BO_Controls();
        public static BO_Paddle Instance;

        public enum Direction { Right, Left }

        private SpriteRenderer spriteRenderer;
        private float defaultSpeed;

        [Header("Paddle Colors")]
        public Color normalColor = Color.white;
        public Color reversedColor = new Color(1f, 0.4f, 0.4f);

        private void Awake() { Instance = this; }

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultSpeed = controls.paddleSpeed;
            UpdatePaddleColor();
        }

        // --- NEW SIZE LOGIC ---
        public void ShrinkPaddle()
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Max(controls.minScale, currentScale.x - controls.shrinkStep);
            transform.localScale = currentScale;
        }

        public void RestorePaddle()
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Min(controls.maxScale, currentScale.x + controls.restoreStep);
            transform.localScale = currentScale;
        }

        // --- EXISTING MOVEMENT ---
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

        void UpdatePaddleColor()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.color = BO_Manager.instance.controlsReversed ? reversedColor : normalColor;
        }

        public void ApplyBakingModifiers()
        {
            if (BO_Manager.instance == null) return;

            if (BO_Manager.instance.lastBakingResult == BO_Manager.BakingResult.Undercooked)
                controls.paddleSpeed = defaultSpeed * 0.7f;
            else if (BO_Manager.instance.lastBakingResult == BO_Manager.BakingResult.Perfect)
                controls.paddleSpeed = defaultSpeed * 1.2f;
            else
                controls.paddleSpeed = defaultSpeed;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("BO_Blueberry"))
            {
                // 1. Tell Leaf/Stage logic
                if (BO_LeafManager.Instance != null && !BO_LeafManager.Instance.isPhase3Active)
                {
                    BO_BlueberryManager.Instance.CollectBerry();
                }

                // 2. FILL THE JAM METER (Updated to use PowerUpController)
                // Note: We use 'instance' (lowercase) to match the PowerUp script
                if (BO_PowerUpController.instance != null)
                {
                    BO_PowerUpController.instance.AddBerry();
                }

                Destroy(collision.gameObject);
            }
        }
    }
}