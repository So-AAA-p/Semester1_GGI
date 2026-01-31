using UnityEngine;

namespace BreakOut
{
    public class BreakOutBlock : MonoBehaviour
    {
        public int hitsRequired = 1;
        protected int currentHits;

        protected virtual void Start()
        {
            currentHits = hitsRequired;
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            currentHits--;

            if (currentHits <= 0)
            {
                OnBreak();
            }
        }

        protected virtual void OnBreak()
        {
            Destroy(gameObject);
        }
    }
}
