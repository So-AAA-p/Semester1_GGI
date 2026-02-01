using UnityEngine;

namespace BreakOut
{
    public class BreakOutBlock : MonoBehaviour
    {
        public int hitsRequired = 1;
        protected int currentHits;

        protected bool hasMilk = false;

        [Header("Milk Overlay")]
        public GameObject milkOverlayPrefab;
        private GameObject activeMilkOverlay;

        [HideInInspector]
        public Vector2Int gridPosition;

        public void SetGridPosition(Vector2Int pos)
        {
            gridPosition = pos;
            Debug.Log($"[Grid] {name} assigned grid {gridPosition}");

        }

        protected virtual void Start()
        {
            currentHits = hitsRequired;
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (hasMilk)
            {
                RemoveMilk();
                return;
            }

            currentHits--;

            if (currentHits <= 0)
            {
                OnBreak();
            }
        }

        public void ApplyMilk()
        {
            if (hasMilk) return;

            hasMilk = true;

            if (milkOverlayPrefab != null)
            {
                activeMilkOverlay = Instantiate(
                    milkOverlayPrefab,
                    transform.position,
                    Quaternion.identity,
                    transform
                );
            }
        }

        protected virtual void RemoveMilk()
        {
            hasMilk = false;
            DestroyMilkVisual();
        }

        protected void DestroyMilkVisual()
        {
            if (activeMilkOverlay != null)
                Destroy(activeMilkOverlay);
        }

        protected virtual void OnBreak()
        {
            BlockSpawner.Instance.OnBlockDestroyed(gridPosition);
            Destroy(gameObject);
        }
    }
}
