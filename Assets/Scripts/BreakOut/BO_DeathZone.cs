using UnityEngine;

namespace BreakOut
{
    public class BO_DeathZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            BO_Ball ball = other.GetComponent<BO_Ball>();
            if (ball == null) return;

            BO_Manager.instance.RemoveBall(ball);
        }
    }
}
