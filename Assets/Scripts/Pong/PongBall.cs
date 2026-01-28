using UnityEngine;
// using UnityEngine.SceneManagement;     
using System.Collections;

namespace Pong 
{ 
    public class PongBall : MonoBehaviour
    {
        private Rigidbody2D rb;

        public float speedIncreasePerSecond = 1f;
        public float maxSpeed = 20f;

        private float currentSpeed;
        public float minYComponent = 0.3f;                                          // prevents flat horizontal movement

        private bool isSquished;
        private bool isServing;

        private Vector2 serveDirection;
        private Vector2 lastVelocity;
        private Vector3 baseScale;
        private Vector3 originalScale;

        private BallTeleporter lastTeleporter;

        public enum BallType
        {
            Classic,
            UnderWater,
            PaddleDecrease,
            PaddleIncrease,
            Unstable,
            Switch
        }

        public BallType ballType;

        public enum LaunchMode
        {
            Auto,
            Serve
        }

        [Header("Launch")]
        public LaunchMode launchMode = LaunchMode.Auto;

        [Header("Ball Colors")]
        public Color paddleIncreaseColor = new Color(0.88f, 0.70f, 0.43f); // #E0B46E
        public Color paddleDecreaseColor = new Color(0.88f, 0.70f, 0.43f);
        public Color unstableColor = new Color(0.75f, 0.2f, 0.9f);
        public Color switchColor = new Color(0.61f, 0.21f, 0.05f); // #9C350D

        [Header("LEVEL FEATURES")]
        [Header("Ball Shrinking")]
        public bool canShrink = false;

        public float shrinkFactor = 0.95f;                                          // 5% smaller per paddle hit
        public float minScale = 0.3f;                                               // never go below this

        [Header("Liquid Mode")]
        public bool isLiquid = false;

        public float liquidDrag = 1.2f;                                             // bestimmt, wie 'schwer' sich der Ball anf�hlt
        public float liquidBounceDamping = 0.9f;

        [Header("Teleportation")]
        public bool canTeleport = false;

        [Header("Materials")]
        public Material defaultMaterial;
        public Material underwaterMaterial;

        private SpriteRenderer sr;

        [Header("Squish Settings")]
        public bool canSquish = false;

        public float squishAmount = 0.25f;                                          // how strong the squish is
        public float squishReturnSpeed = 10f;                                        // how fast it returns to normal

        public float maxSquishImpactSpeed = 20f;
        public float maxSquishMultiplier = 1.5f;

        [Header("Wobble Settings")]
        public float maxWobbleStrength = 0.4f;
        public float wobbleFadeSpeed = 2f;

        [Header("Paddle Effect Flash")]
        public float successFlashDuration = 0.15f;
        public float blockedFlashDuration = 0.08f;

        [Header("Unstable Ball")]
        public float unstableAngleVariance = 20f; // degrees, kann bis 30-40 gesetzt werden, wenn man böse sein will tehe (sehr chaotisch)

        [Header("Switch Ball")]
        public float switchDelay = 10f;
        public Color switchBlinkColor = new Color(0.44f, 0.14f, 0.04f); // #6F2409
        public float minBlinkInterval = 0.05f;
        public float maxBlinkInterval = 0.6f;

        private float currentWobble;
        private Material runtimeMaterial;

        

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();

            runtimeMaterial = Instantiate(sr.material);
            sr.material = runtimeMaterial;

            currentSpeed = PongManager.instance.ballstartvelocity;

            originalScale = transform.localScale; // in ursprüngliche Form zurückkehren
            baseScale = originalScale;

            ApplyBallType();

            if (launchMode == LaunchMode.Auto)
            {
                LaunchImmediately();
            }
            else
            {
                isServing = true;
                rb.linearVelocity = Vector2.zero;
            }

            if (ballType == BallType.Switch)
            {
                StartCoroutine(SwitchRoutine());
            }
        }

        void LaunchImmediately()
        {
            float side = Random.value < 0.5f ? 1f : -1f;
            float angle = Random.Range(minYComponent, 1f);
            angle *= Random.value < 0.5f ? -1 : 1;

            Vector2 dir = new Vector2(side, angle).normalized;
            rb.linearVelocity = dir * currentSpeed;
        }

        IEnumerator SwitchRoutine()
        {
            float elapsed = 0f;
            Color baseColor = sr.color;

            while (elapsed < switchDelay)
            {
                float t = elapsed / switchDelay;

                // Blink faster over time
                float blinkInterval = Mathf.Lerp(maxBlinkInterval, minBlinkInterval, t);

                sr.color = switchBlinkColor;
                yield return new WaitForSeconds(blinkInterval * 0.2f);

                sr.color = baseColor;
                yield return new WaitForSeconds(blinkInterval * 0.2f);

                elapsed += blinkInterval;
            }

            sr.color = baseColor;
            SwitchToRandomBall();
        }

        void SwitchToRandomBall()
        {
            BallType[] options =
            {
        BallType.PaddleIncrease,
        BallType.PaddleDecrease,
        BallType.Unstable
    };

            BallType newType = options[Random.Range(0, options.Length)];

            ballType = newType;
            Debug.Log("[SwitchBall] Switching to " + newType);
            ApplyBallType();
        }

        void ApplyBallType()
        {
            // ===== FULL RESET =====
            canShrink = false;
            isLiquid = false;
            canSquish = false;
            canTeleport = true;

            rb.linearDamping = 0f;
            currentWobble = 0f;

            transform.localScale = originalScale;
            baseScale = originalScale;

            //sr.color = Color.white;

            // ======================

            switch (ballType)
            {
                case BallType.Classic:
                    canShrink = true;
                    canTeleport = false;
                    break;

                case BallType.UnderWater:
                    isLiquid = true;
                    canSquish = true;
                    SetLiquidVisual(true);
                    canTeleport = false;
                    break;

                case BallType.PaddleDecrease:
                    break;

                case BallType.PaddleIncrease:
                    break;

                case BallType.Unstable:
                    break;

                case BallType.Switch:
                    break;
            }
            ApplyBallColor();
        }

        void ApplyBallColor()
        {
            switch (ballType)
            {
                case BallType.PaddleIncrease:
                    sr.color = paddleIncreaseColor;
                    break;

                case BallType.PaddleDecrease:
                    sr.color = paddleDecreaseColor;
                    break;

                case BallType.Unstable:
                    sr.color = unstableColor;
                    break;

                case BallType.Switch:
                    sr.color = switchColor;
                    break;
            }
        }

        void Update()
        {
            if (isServing)
                return;

            IncreaseSpeedOverTime();

            currentWobble = Mathf.Lerp(currentWobble, 0f, wobbleFadeSpeed * Time.deltaTime);

            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetFloat("_WobbleStrength", currentWobble);
            }

        }

        void FixedUpdate()
        {
            if (isServing)
                return;

            lastVelocity = rb.linearVelocity;

            if (isLiquid)
            {
                ApplyLiquidDrag();
            }
        }


        void LateUpdate()
        {
            if (!canSquish)
                return;

            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale,
                squishReturnSpeed * Time.deltaTime
            );
        }

        void IncreaseSpeedOverTime()
        {
            currentSpeed += speedIncreasePerSecond * ((3 * Time.deltaTime) / 2 );
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);                       // Math.Min nimmt einfach den kleineren der beiden folgeneden Werte, hier wichtig, damit die Geschwindigkeit das Geschwindigkeitsmaximum nicht �bertrifft

            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;        // .normalized beh�lt die selbe richtung an und �ndert die l�nge bzw magnitude des Vektors zu 1, diese wird dann sp�ter auf die geschwindigkeit (currentspeed) ge�ndert 
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            bool hitPaddle = collision.gameObject.CompareTag("PPaddle");

            if (isServing)
                return;

            Vector2 incomingVelocity = lastVelocity;
            float impactSpeed = incomingVelocity.magnitude;

            Vector2 normal = collision.contacts[0].normal;

            // =====================
            // PADDLE EFFECT SECTION
            // =====================
            if (hitPaddle)
            {
                //Debug.Log("[Ball] Hit paddle");

                if (ballType == BallType.PaddleIncrease || ballType == BallType.PaddleDecrease)
                {
                    PongPaddle paddle = collision.gameObject.GetComponent<PongPaddle>();

                    if (paddle != null)
                    {
                        //Debug.Log("[Ball] Applying paddle effect");
                        ApplyPaddleEffect(paddle);
                    }
                    else
                    {
                        //Debug.LogWarning("[Ball] Paddle has no PongPaddle component!");
                    }
                }
            }

            // =====================
            // PHYSICS / BOUNCE LOGIC
            // =====================
            Vector2 reflected = Vector2.Reflect(incomingVelocity, normal);
            Vector2 dir = reflected.normalized;

            // Liquid damping
            if (isLiquid)
            {
                reflected *= liquidBounceDamping;
                dir = reflected.normalized;
            }

            // Angle enforcement (classic safety net)
            if (hitPaddle && canShrink)
            {
                ShrinkBall();

                if (Mathf.Abs(dir.y) < minYComponent)
                {
                    dir.y = Mathf.Sign(dir.y == 0 ? 1 : dir.y) * minYComponent;
                    dir.x = Mathf.Sign(dir.x) * Mathf.Sqrt(1f - dir.y * dir.y);
                }
            }

            // 🔥 UNSTABLE BALL MODIFIER (ONLY ON PADDLES)
            if (ballType == BallType.Unstable && hitPaddle)
            {
                dir = ApplyUnstableAngle(dir);
            }

            // Apply final velocity
            rb.linearVelocity = dir * currentSpeed;

            // =====================
            // SQUISH / WOBBLE
            // =====================
            if (canSquish)
            {
                float squishStrength = Mathf.InverseLerp(0f, maxSquishImpactSpeed, impactSpeed);
                float scaledSquish = squishAmount * Mathf.Lerp(0.5f, maxSquishMultiplier, squishStrength);

                float wobbleFromImpact = Mathf.InverseLerp(5f, 25f, impactSpeed);
                float targetWobble = wobbleFromImpact * maxWobbleStrength;

                currentWobble = Mathf.Max(currentWobble, targetWobble);

                if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
                    SquishHorizontal(scaledSquish);
                else
                    SquishVertical(scaledSquish);
            }
        }

        Vector2 ApplyUnstableAngle(Vector2 dir)
        {
            // Reduce chaos as speed increases
            float speedFactor = Mathf.InverseLerp(0f, maxSpeed, currentSpeed);
            float angleVariance = Mathf.Lerp(
                unstableAngleVariance,
                unstableAngleVariance * 0.4f, // late-game calmer
                speedFactor
            );

            float randomAngle = Random.Range(-angleVariance, angleVariance);
            return (Quaternion.Euler(0f, 0f, randomAngle) * dir).normalized;
        }

        void ApplyPaddleEffect(PongPaddle paddle)
        {
            bool changed;

            switch (ballType)
            {
                case BallType.PaddleIncrease:
                    changed = paddle.ModifySize(+paddle.sizeStep);
                    paddle.Flash(
                        changed ? Color.green : PongPaddle.DarkGreen,
                        changed ? successFlashDuration : blockedFlashDuration
                    );
                    break;

                case BallType.PaddleDecrease:
                    changed = paddle.ModifySize(-paddle.sizeStep);
                    paddle.Flash(
                        changed ? Color.red : PongPaddle.DarkRed,
                        changed ? successFlashDuration : blockedFlashDuration
                    );
                    break;
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (PongManager.instance.currentLevel != PongManager.LevelType.BallTypes)
                return;

            if (isServing)
                return;

            if (!canTeleport)
                return;

            if (!other.CompareTag("Teleporter"))
                return;

            BallTeleporter teleporter = other.GetComponent<BallTeleporter>();
            if (teleporter == null)
                return;

            // Prevent instant bounce-back
            if (teleporter == lastTeleporter)
                return;

            Teleport(teleporter);
        }



        void Teleport(BallTeleporter teleporter)
        {
            lastTeleporter = teleporter;

            // Preserve current Y
            float preservedY = rb.position.y;

            // Take X from exit point
            float exitX = teleporter.exitPoint.position.x;

            // Build final position
            Vector2 exitPos = new Vector2(exitX, preservedY);

            // Exit direction from teleporter
            Vector2 exitNormal = teleporter.exitDirection.normalized;

            float safeOffset = 0.9f; // nicht weniger! ansonsten krise

            rb.position = exitPos + exitNormal * safeOffset;

            // Preserve direction & speed
            Vector2 preservedDir = rb.linearVelocity.normalized;
            rb.linearVelocity = preservedDir * currentSpeed;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            BallTeleporter teleporter = other.GetComponent<BallTeleporter>();
            if (teleporter != null && teleporter == lastTeleporter)
            {
                lastTeleporter = null;
            }
        }

        void ShrinkBall()
        {
            baseScale *= shrinkFactor;

            if (baseScale.x < minScale)
                return;

            transform.localScale = baseScale;
        }


        void ApplyLiquidDrag()
        {
            rb.linearDamping = liquidDrag;
        }

        public void SetLiquidVisual(bool liquid)
        {
            if (sr == null)
                return;

            sr.material = liquid ? underwaterMaterial : defaultMaterial;
        }

        public void PrepareServe(Vector2 direction)
        {
            if (rb == null)
            {
                //Debug.LogError("Rigidbody2D is NULL on PongBall!", this);
                return;
            }

            // Debug.Log("PrepareServe called with dir: " + direction);
            rb.linearVelocity = Vector2.zero;
            isServing = true;
            serveDirection = direction;

            StartCoroutine(ServeRoutine());
        }

        IEnumerator ServeRoutine()
        {
            yield return new WaitForSeconds(1f);

            float angle = Random.Range(-0.5f, 0.5f);
            Vector2 dir = new Vector2(serveDirection.x, angle).normalized;

            rb.linearVelocity = dir * currentSpeed;
            isServing = false;
        }

        void SquishHorizontal(float amount)
        {
            //isSquished = true;
            transform.localScale = new Vector3(
                originalScale.x - amount,
                originalScale.y + amount,
                originalScale.z
            );
        }

        void SquishVertical(float amount)
        {
            //isSquished = true;
            transform.localScale = new Vector3(
                originalScale.x + amount,
                originalScale.y - amount,
                originalScale.z
            );
        }
    }
}