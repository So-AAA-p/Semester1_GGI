using UnityEngine;

namespace Pong
{
    public class PongGoal : MonoBehaviour
    {
        public PongManager.Player player;

        private void OnTriggerEnter2D(Collider2D other)
        {
            PongManager.instance.OnGoalScored(player);

            Destroy(other.gameObject);
        }
    }
}