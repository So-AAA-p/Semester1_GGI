using UnityEngine;
// using UnityEngine.SceneManagement;     

namespace Pong 
{ 
    public class PongBall : MonoBehaviour
    {
        private Rigidbody2D rb;

        public float speedIncreasePerSecond = 1f;
        public float maxSpeed = 20f;

        private float currentSpeed;
        public float minYComponent = 0.3f;                                          // prevents flat horizontal movement

        private Vector2 lastVelocity;

        private bool isSquished;

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

        [Header("Level Features")]
        public bool canShrink = false;

        public float shrinkFactor = 0.95f;                                          // 5% smaller per paddle hit
        public float minScale = 0.3f;                                               // never go below this

        [Header("Liquid Mode")]
        public bool isLiquid = false;

        public float liquidDrag = 1.2f;                                             // bestimmt, wie 'schwer' sich der Ball anf�hlt
        public float liquidBounceDamping = 0.9f;

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

        private float currentWobble;
        private Material runtimeMaterial;


        private Vector3 baseScale;
        private Vector3 originalScale;




        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();

            runtimeMaterial = Instantiate(sr.material);
            sr.material = runtimeMaterial;


            float side = Random.Range(0, 2) == 0 ? 1 : -1;

            // ensure non-flat launch angle
            float angle = Random.Range(minYComponent, 1f);
            angle *= Random.value < 0.5f ? -1 : 1;

            currentSpeed = PongManager.instance.ballstartvelocity;

            Vector2 direction = new Vector2(side, angle).normalized;
            rb.linearVelocity = direction * currentSpeed;

            ApplyBallType();
            Debug.Log($"Spawned ball of type: {ballType}");

            originalScale = transform.localScale;                                   // immer wieder zur ursrünglichen Form zurückkehren
            baseScale = originalScale;
        }

        void ApplyBallType()
        {
            // defaults
            canShrink = false;
            isLiquid = false;
            canSquish = false;

            switch (ballType)
            {
                case BallType.Classic:
                    canShrink = true;
                    break;

                case BallType.UnderWater:
                    isLiquid = true;
                    canSquish = true;
                    SetLiquidVisual(true);
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
        }


        void Update()
        {
            IncreaseSpeedOverTime();

            currentWobble = Mathf.Lerp(currentWobble, 0f, wobbleFadeSpeed * Time.deltaTime);

            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetFloat("_WobbleStrength", currentWobble);
            }

        }

        void FixedUpdate()
        {
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
            currentSpeed += speedIncreasePerSecond * (Time.deltaTime / 2);
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);                       // Math.Min nimmt einfach den kleineren der beiden folgeneden Werte, hier wichtig, damit die Geschwindigkeit das Geschwindigkeitsmaximum nicht �bertrifft

            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;        // .normalized beh�lt die selbe richtung an und �ndert die l�nge bzw magnitude des Vektors zu 1, diese wird dann sp�ter auf die geschwindigkeit (currentspeed) ge�ndert 
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            Vector2 incomingVelocity = lastVelocity;
            float impactSpeed = incomingVelocity.magnitude;

            int layer = collision.gameObject.layer;
            Vector2 normal = collision.contacts[0].normal;


            // Always reflect
            Vector2 reflected = Vector2.Reflect(incomingVelocity, normal);
            Vector2 dir = reflected.normalized;

            if (isLiquid)
            {
                reflected *= liquidBounceDamping;
                dir = reflected.normalized;
            }


            // ONLY enforce angle when hitting paddles
            if (layer == LayerMask.NameToLayer("PongPaddles") && canShrink)
            {
                ShrinkBall();

                if (Mathf.Abs(dir.y) < minYComponent)
                {
                    dir.y = Mathf.Sign(dir.y == 0 ? 1 : dir.y) * minYComponent;
                    dir.x = Mathf.Sign(dir.x) * Mathf.Sqrt(1f - dir.y * dir.y);
                }
            }

            rb.linearVelocity = dir * currentSpeed;

            float squishStrength = Mathf.InverseLerp(0f, maxSquishImpactSpeed, impactSpeed);

            float scaledSquish = squishAmount * Mathf.Lerp(0.5f, maxSquishMultiplier, squishStrength);


            if (canSquish)
            {
                float wobbleFromImpact = Mathf.InverseLerp(5f, 25f, impactSpeed);
                float targetWobble = wobbleFromImpact * maxWobbleStrength;

                currentWobble = Mathf.Max(currentWobble, targetWobble);

                if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
                    SquishHorizontal(scaledSquish);
                else
                    SquishVertical(scaledSquish);
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

        public void SetLiquidVisual(bool isLiquid)
        {
            if (sr == null) return;

            sr.material = isLiquid ? underwaterMaterial : defaultMaterial;
        }

        void SquishHorizontal(float amount)
        {
            isSquished = true;
            transform.localScale = new Vector3(
                originalScale.x - amount,
                originalScale.y + amount,
                originalScale.z
            );
        }

        void SquishVertical(float amount)
        {
            isSquished = true;
            transform.localScale = new Vector3(
                originalScale.x + amount,
                originalScale.y - amount,
                originalScale.z
            );
        }
    }
}