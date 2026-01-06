using UnityEngine;

namespace BreakOut
{
[System.Serializable]
public class BreakOutControls
{
    public KeyCode LeftKey = KeyCode.LeftArrow;
    public KeyCode RightKey = KeyCode.RightArrow;
    public float paddleSpeed;
    public float minX = -7.5f;
    public float maxX = 7.5f;
}
}

