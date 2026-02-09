using UnityEngine;

namespace BreakOut
{
    public class BO_Stage3Leaf : MonoBehaviour
    {
        // REMOVED: public static BO_Stage3Leaf Instance; (We don't want this for multiple objects!)

        public GameObject berryPrefab;
        private SpriteRenderer sr;
        private BoxCollider2D col;

        // We make this public so the Manager can "see" it
        public bool isActive { get; private set; } = false;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<BoxCollider2D>();
        }

        void Start()
        {
            SetActiveState(false);
        }

        public void ActivateLeaf()
        {
            if (isActive) return;
            SetActiveState(true);
        }

        public void SetActiveState(bool state)
        {
            isActive = state;
            if (sr != null) sr.enabled = state;
            if (col != null) col.enabled = state;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Log EVERY collision to see if the ball is actually being detected
            //Debug.Log($"{gameObject.name} collided with: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

            if (!isActive) return;

            if (collision.gameObject.CompareTag("BreakOutBall"))
            {
                if (berryPrefab != null)
                {
                    Instantiate(berryPrefab, transform.position, Quaternion.identity);
                }

                // IMPORTANT: We must call this to set isActive = false
                SetActiveState(false);
                //Debug.Log($"<color=green>{gameObject.name} IS NOW AVAILABLE FOR RESPAWN</color>");
            }
        }
    }
}