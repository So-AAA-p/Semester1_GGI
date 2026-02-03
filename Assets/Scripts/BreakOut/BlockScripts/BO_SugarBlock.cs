using UnityEngine;

namespace BreakOut
{
    public class BO_SugarBlock : BO_Block
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
            BO_Ball originalBall = FindObjectOfType<BO_Ball>();
            if (originalBall == null) return;

            // Respect size cap
            if (originalBall.transform.localScale.x <=
                BO_Manager.instance.minBallScale)
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

            BO_Ball newBall = newBallObj.GetComponent<BO_Ball>();

            // 🔹 Only the NEW ball gets angle offset
            Vector2 newVelocity =
                Quaternion.Euler(0, 0, angleOffset) * originalVelocity; // 🧍the helly

            newBall.SetVelocity(newVelocity);

            BO_Manager.instance.RegisterBall(newBall);
        }
    }
}

