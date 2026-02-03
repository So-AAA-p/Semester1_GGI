using System.Collections;
using UnityEngine;

namespace BreakOut
{
    public class BO_MilkBlock : BO_Block
    {
        public float dripDelay = 0.15f;

        protected override void OnBreak()
        {
            Debug.Log($"[Milk] Milk block broken at grid {gridPosition}");

            // Start the spill BEFORE we lose reference to where we are
            // Pass the position into the coroutine so it's "remembered"
            StartCoroutine(SpillMilkCoroutine(gridPosition));

            // Remove from dictionary so the ball doesn't hit it while it's "dying"
            BO_BlockSpawner.Instance.RemoveBlock(gridPosition);

            // Hide the visuals/collider immediately so it feels broken
            GetComponent<Collider2D>().enabled = false;
            GetComponent<Renderer>().enabled = false;
        }

        IEnumerator SpillMilkCoroutine(Vector2Int startPos)
        {
            // Use the passed-in startPos instead of the member variable
            foreach (BO_Block block in BO_BlockSpawner.Instance.GetBlocksBelow(startPos))
            {
                block.ApplyMilk();
                yield return new WaitForSeconds(dripDelay);
            }

            Destroy(gameObject);
        }
    }
}