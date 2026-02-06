using UnityEngine;

namespace BreakOut
{
    public class BO_Ball : MonoBehaviour
    {
        public static BO_Ball Instance; 

        private Rigidbody2D rb;


        [Header("Movement Settings")]
        //public float speedIncreasePerSecond = 2f;
        //public float maxSpeed = 20f;
        public float constantSpeed = 10f;
        public float minYComponent = 0.3f;

        // New variables for the "Sticky" behavior
        private bool isAttached = true;
        private Transform paddleTransform;
        private Vector2 paddleOffset;

        private Vector2 lastVelocity;
        
        void Awake()
        {
            Instance = this;
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
            BO_Paddle paddle = FindObjectOfType<BO_Paddle>();

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
                //IncreaseSpeedOverTime();
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
            rb.linearVelocity = direction * constantSpeed;
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
            // If the ball is moving, force it to ALWAYS stay at 'constantSpeed'
            if (rb != null && rb.linearVelocity != Vector2.zero)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * constantSpeed;
            }

            if (!isAttached) // Only track physics velocity if we are actually flying
            {
                lastVelocity = rb.linearVelocity;
            }
        }

            /*
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
            */

        public void ApplyBakingModifiers() // selber Name wie in BO_Paddle -> Encapsulation (Methoden haben den selben Namen, aber leben in verschiedene Klassen)
        {
            // Default scale (usually 1, 1, 1 or whatever your prefab is)
            Vector3 normalScale = Vector3.one;

            if (BO_Manager.instance.lastBakingResult == BO_Manager.BakingResult.Burnt)
            {
                // Shriveled ball!
                transform.localScale = normalScale * 0.7f;

                // Optional: Make it look charcoal grey
                GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.2f, 0.2f);

                Debug.Log("[Ball] Ball shriveled: It's a charcoal briquette now!");
            }
            else
            {
                transform.localScale = normalScale;
                GetComponent<SpriteRenderer>().color = Color.white;
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

            // --- BOSS HIT LOGIC START ---
            // Check if the object we hit has the BossManager script
            BO_BossHead boss = collision.gameObject.GetComponent<BO_BossHead>();

            if (boss != null)
            {
                // We hit the boss!
                boss.TakeDamage(3);
            }
            // --- BOSS HIT LOGIC END ---

            rb.linearVelocity = dir * constantSpeed;
        }

        public Vector2 GetVelocity()
        {
            return rb.linearVelocity;
        }

        public void StopBall()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Stops movement
                rb.isKinematic = true;      // Prevents physics from moving it again
            }
        }
    }
}