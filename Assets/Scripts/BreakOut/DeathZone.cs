using UnityEngine;

namespace BreakOut
{
    public class BreakoutDeathZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            BreakOutBall ball = other.GetComponent<BreakOutBall>();
            if (ball == null) return;

            BreakOutManager.instance.RemoveBall(ball);
        }
    }
}
