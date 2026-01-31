using UnityEngine;

namespace BreakOut
{
    public class SugarBlock : BreakOutBlock
    {
        [Header("Sugar Settings")]
        public GameObject ballPrefab;
        public float sizeMultiplier = 0.7f;
        public float angleOffset = 20f;

        protected override void OnBreak()
        {
            SplitBall();
            base.OnBreak();
        }

        void SplitBall()
        {
            BreakOutBall originalBall = FindObjectOfType<BreakOutBall>();
            if (originalBall == null) return;

            // Respect size cap
            if (originalBall.transform.localScale.x <=
                BreakOutManager.instance.minBallScale)
                return;

            Vector2 originalVelocity = originalBall.GetVelocity();

            // 🔹 Shrink original ball ONLY
            originalBall.transform.localScale *= sizeMultiplier;

            // 🔹 Spawn second ball
            GameObject newBallObj = Instantiate(
                ballPrefab,
                originalBall.transform.position,
                Quaternion.identity
            );

            newBallObj.transform.localScale = originalBall.transform.localScale;

            BreakOutBall newBall = newBallObj.GetComponent<BreakOutBall>();

            // 🔹 Only the NEW ball gets angle offset
            Vector2 newVelocity =
                Quaternion.Euler(0, 0, angleOffset) * originalVelocity; // 🧍the helly

            newBall.SetVelocity(newVelocity);

            BreakOutManager.instance.RegisterBall(newBall);
        }
    }
}

