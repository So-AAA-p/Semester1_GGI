using UnityEngine;

namespace BreakOut
{
    public class BO_MoldShot : MonoBehaviour
    {
        public float speed = 5f;
        public float lifetime = 5f;

        void Start()
        {
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            // Using transform.up ensures it moves in the direction its "head" is pointing
            // relative to the game world, not just itself.
            transform.position += transform.up * speed * Time.deltaTime;
        }

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 1. HIT THE SHIELD
            if (other.CompareTag("BO_Shield"))
            {
                BO_PowerUpController controller = other.GetComponentInParent<BO_PowerUpController>();
                if (controller != null)
                {
                    controller.TakeShieldHit();
                }
                Destroy(gameObject);
            }
            // 2. HIT THE PADDLE (MISS THE SHIELD)
            else if (other.CompareTag("Player"))
            {
                // Add your player damage logic here later!
                Debug.Log("Ouch! The paddle was hit by mold!");
                Destroy(gameObject);
            }
            // 3. HIT THE BOTTOM BOUNDARY (Optional)
            else if (other.gameObject.layer == LayerMask.NameToLayer("BreakOutWalls"))
            {
                Destroy(gameObject);
            }
        }
    }
}
