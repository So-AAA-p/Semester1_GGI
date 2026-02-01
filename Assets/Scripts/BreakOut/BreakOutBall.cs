using UnityEngine;

namespace BreakOut
{
    public class BreakOutBall : MonoBehaviour
    {
        private Rigidbody2D rb;

        [Header("Movement Settings")]
        public float speedIncreasePerSecond = 2f;
        public float maxSpeed = 20f;
        private float currentSpeed = 5f;
        public float minYComponent = 0.3f;

        // New variables for the "Sticky" behavior
        private bool isAttached = true;
        private Transform paddleTransform;
        private Vector2 paddleOffset;

        private Vector2 lastVelocity;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            // 1. Check if the ball already has velocity (e.g. from Sugar Block split)
            // If it's already moving, don't stick it to the paddle!
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                isAttached = false;
                return;
            }

            // 2. Find the paddle automatically
            BreakOutPaddle paddle = FindObjectOfType<BreakOutPaddle>();

            if (paddle != null)
            {
                paddleTransform = paddle.transform;
                isAttached = true;

                // 3. Place ball slightly above the paddle
                // Adjust the '0.8f' if you want it higher/lower
                Vector3 startPos = paddleTransform.position + Vector3.up * 0.8f;
                transform.position = startPos;

                // Calculate the offset so it stays in that relative spot
                paddleOffset = transform.position - paddleTransform.position;
            }
            else
            {
                // Fallback: If no paddle exists, just launch immediately
                Launch();
            }
        }

        void Update()
        {
            if (isAttached)
            {
                // STICKY MODE: Follow the paddle
                if (paddleTransform != null)
                {
                    transform.position = paddleTransform.position + (Vector3)paddleOffset;
                }

                // Wait for Input to Launch
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Return))
                {
                    Launch();
                }
            }
            else
            {
                // NORMAL MODE: Handle speed increase
                IncreaseSpeedOverTime();
            }
        }

        void Launch()
        {
            isAttached = false;

            // This is your original Start() logic, moved here!
            float side = Random.Range(0, 2) == 0 ? 1 : -1;
            float angle = Random.Range(minYComponent, 1f);
            angle *= Random.value < 0.5f ? -1 : 1;

            Vector2 direction = new Vector2(side, angle).normalized;
            rb.linearVelocity = direction * currentSpeed;
        }

        public void SetVelocity(Vector2 velocity)
        {
            rb.linearVelocity = velocity;
            // Important: If we manually set velocity (Sugar Block), stop being sticky!
            isAttached = false;
        }

        // ... (Keep your FixedUpdate, OnCollisionEnter2D, and GetVelocity methods exactly the same) ...

        void FixedUpdate()
        {
            if (!isAttached) // Only track physics velocity if we are actually flying
            {
                lastVelocity = rb.linearVelocity;
            }
        }

        void IncreaseSpeedOverTime()
        {
            currentSpeed += speedIncreasePerSecond * ((3 * Time.deltaTime) / 2);
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            // Only apply velocity normalization if we are moving
            if (!isAttached && rb.linearVelocity.magnitude > 0)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // Paste your existing OnCollisionEnter2D logic here
            // It doesn't need to change!
            int layer = collision.gameObject.layer;

            Vector2 incomingVelocity = lastVelocity;
            Vector2 normal = collision.contacts[0].normal;

            Vector2 reflected = Vector2.Reflect(incomingVelocity, normal);
            Vector2 dir = reflected.normalized;

            if (layer == LayerMask.NameToLayer("BreakOutWalls"))
            {
                if (Mathf.Abs(dir.y) < minYComponent)
                {
                    dir.y = Mathf.Sign(dir.y == 0 ? 1 : dir.y) * minYComponent;
                    dir.x = Mathf.Sign(dir.x) * Mathf.Sqrt(1f - dir.y * dir.y);
                }
            }

            rb.linearVelocity = dir * currentSpeed;
        }

        public Vector2 GetVelocity()
        {
            return rb.linearVelocity;
        }
    }
}