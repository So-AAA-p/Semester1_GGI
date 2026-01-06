using UnityEngine;
// using UnityEngine.SceneManagement;     

namespace BreakOut
{
    public class BreakOutBall : MonoBehaviour
    {
        private Rigidbody2D rb;

        public float speedIncreasePerSecond = 2f;
        public float maxSpeed = 20f;

        private float currentSpeed = 5f;
        public float minYComponent = 0.3f; // prevents flat horizontal movement

        private Vector2 lastVelocity;
        public float ballstartvelocity;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            float side = Random.Range(0, 2) == 0 ? 1 : -1;

            // ensure non-flat launch angle
            float angle = Random.Range(minYComponent, 1f);
            angle *= Random.value < 0.5f ? -1 : 1;


            Vector2 direction = new Vector2(side, angle).normalized;
            rb.linearVelocity = direction * currentSpeed;
        }


        // Update is called once per frame
        void Update()
        {
            IncreaseSpeedOverTime();
        }

        void FixedUpdate()
        {
            lastVelocity = rb.linearVelocity;
        }

        void IncreaseSpeedOverTime()
        {
            currentSpeed += speedIncreasePerSecond * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);                       

            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;       
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            int layer = collision.gameObject.layer;

            Vector2 incomingVelocity = lastVelocity;
            Vector2 normal = collision.contacts[0].normal;

            // Always reflect
            Vector2 reflected = Vector2.Reflect(incomingVelocity, normal);
            Vector2 dir = reflected.normalized;

            // ONLY enforce angle when hitting paddles
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

    }
}
