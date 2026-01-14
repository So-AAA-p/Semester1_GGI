using UnityEngine;

namespace Pong
{
    public class BallTeleporter : MonoBehaviour
    {
        public Transform exitPoint;                                     // exit point is where the ball ends up!
        public Vector2 exitDirection = Vector2.right;
    }
}

