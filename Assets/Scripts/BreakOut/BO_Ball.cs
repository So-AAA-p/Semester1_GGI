using UnityEngine;

namespace BreakOut
{
    public class BO_Ball : MonoBehaviour
    {
        public static BO_Ball Instance;

        private Rigidbody2D rb;

        [Header("Movement Settings")]
        public float constantSpeed = 10f;
        public float minYComponent = 0.3f;

        [Header("Jam Mode")]
        public bool isJamCoated = false;
        public Color jamColor = new Color(0.5f, 0f, 0.5f); // Purple
        private Color normalBallColor;
        private SpriteRenderer sr;

        // Sticky behavior
        private bool isAttached = true;
        private Transform paddleTransform;
        private Vector2 paddleOffset;
        private Vector2 lastVelocity;

        void Awake()
        {
            Instance = this;
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            normalBallColor = sr.color;

            // If ball already moving, don't stick
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                isAttached = false;
                return;
            }

            // Find paddle
            BO_Paddle paddle = FindObjectOfType<BO_Paddle>();
            if (paddle != null)
            {
                paddleTransform = paddle.transform;
                isAttached = true;
                Vector3 startPos = paddleTransform.position + Vector3.up * 0.8f;
                transform.position = startPos;
                paddleOffset = transform.position - paddleTransform.position;
            }
            else
            {
                Launch();
            }
        }

        void Update()
        {
            if (isAttached)
            {
                if (paddleTransform != null)
                {
                    transform.position = paddleTransform.position + (Vector3)paddleOffset;
                }

                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Return))
                {
                    Launch();
                }
            }
        }

        void Launch()
        {
            isAttached = false;
            float side = Random.Range(0, 2) == 0 ? 1 : -1;
            float angle = Random.Range(minYComponent, 1f);
            angle *= Random.value < 0.5f ? -1 : 1;

            Vector2 direction = new Vector2(side, angle).normalized;
            rb.linearVelocity = direction * constantSpeed;
        }

        public void SetVelocity(Vector2 velocity)
        {
            rb.linearVelocity = velocity;
            isAttached = false;
        }

        public void EnableJamCoating()
        {
            isJamCoated = true;
            if (sr != null) sr.color = jamColor;
            Debug.Log("<color=magenta>BALL COATED IN JAM!</color>");
        }

        public void DisableJamCoating()
        {
            isJamCoated = false;
            if (sr != null) sr.color = normalBallColor;

            // --- FIX: UPDATED REFERENCE ---
            // Tell the PowerUpController (instead of JamController) the mode is done
            if (BO_PowerUpController.instance != null)
            {
                BO_PowerUpController.instance.JamHitComplete();
            }
        }

        void FixedUpdate()
        {
            if (rb != null && rb.linearVelocity != Vector2.zero)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * constantSpeed;
            }

            if (!isAttached)
            {
                lastVelocity = rb.linearVelocity;
            }
        }

        public void ApplyBakingModifiers()
        {
            Vector3 normalScale = Vector3.one;
            if (BO_Manager.instance.lastBakingResult == BO_Manager.BakingResult.Burnt)
            {
                transform.localScale = normalScale * 0.7f;
                GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.2f, 0.2f);
            }
            else
            {
                transform.localScale = normalScale;
                GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // Boss Interaction
            if (collision.gameObject.CompareTag("BO_BossHead") || collision.gameObject.CompareTag("BO_BossPaw"))
            {
                if (isJamCoated)
                {
                    BO_BossHead.Instance.TakeDamage(5f);
                    DisableJamCoating();
                }
                else
                {
                    BO_BossHead.Instance.TakeDamage(1f);
                }
            }

            // Bounce Logic
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

            BO_BossHead boss = collision.gameObject.GetComponent<BO_BossHead>();
            if (boss != null)
            {
                boss.TakeDamage(3);
            }

            rb.linearVelocity = dir * constantSpeed;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (isJamCoated && other.CompareTag("BO_MoldShot"))
            {
                Destroy(other.gameObject);
                Debug.Log("Mold Shot Pierced by Jam Ball!");
            }
        }

        public Vector2 GetVelocity() { return rb.linearVelocity; }

        public void StopBall()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true;
            }
        }
    }
}