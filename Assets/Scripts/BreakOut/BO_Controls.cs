using UnityEngine;

namespace BreakOut
{
    [System.Serializable]
    public class BO_Controls
    {
        public KeyCode LeftKey = KeyCode.LeftArrow;
        public KeyCode RightKey = KeyCode.RightArrow;
        public float paddleSpeed;
        public float minX = -7.5f;
        public float maxX = 7.5f;

        [Header("Size Settings")]
        public float minScale = 0.5f;
        public float maxScale = 1.5f;
        public float shrinkStep = 0.2f;   // How much it loses per hit
        public float restoreStep = 0.25f; // How much it gains per paw hit
    }
}