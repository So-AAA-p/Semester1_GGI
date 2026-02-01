using UnityEngine;

namespace BreakOut
{
    public class FlourBlock : BreakOutBlock
    {
        [Header("Flour Effect")]
        public GameObject flourOverlayPrefab;
        public float radius = 2.5f;

        protected override void OnBreak()
        {
            SpawnFlourCloud();
            base.OnBreak();
        }

        void SpawnFlourCloud()
        {
            GameObject cloud = Instantiate(
                flourOverlayPrefab,
                transform.position,
                Quaternion.identity
            );

            // scale based on radius
            float diameter = radius * 2f;
            cloud.transform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }
}

