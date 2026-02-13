using UnityEngine;

namespace Pong
{
    public class PongGoal : MonoBehaviour
    {
        public PongManager.Player player;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 1. CRITICAL FIX: Only let the "Ball" trigger a goal!
            // Make sure your Ball prefab has the tag "Ball" in the Inspector.
            if (!other.CompareTag("PongBall"))
            {
                // If it's a fish (or anything else), just destroy it silently
                if (other.CompareTag("PongObstacle"))
                {
                    Destroy(other.gameObject);
                }
                return;
            }

            // 2. Prevent scoring if the game is already over
            // This fixes the "Score > 10" issue if a ball hits right after the game ends.
            if (PongManager.instance.currentState == PongManager.GameState.GameDone)
                return;

            PongManager.instance.OnGoalScored(player);

            // Destroy the ball that hit the goal
            Destroy(other.gameObject);
        }
    }
}