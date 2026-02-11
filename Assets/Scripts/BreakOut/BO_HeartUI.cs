using UnityEngine;

namespace BreakOut
{
    public class BO_HeartUI : MonoBehaviour
    {
        public Sprite fullHeart, halfHeart, emptyHeart;
        private SpriteRenderer sr; // Changed from Image

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        public void SetHeartState(int value)
        {
            // Safety check for race conditions
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;

            // value: 2 = Full, 1 = Half, 0 = Empty
            if (value >= 2) sr.sprite = fullHeart;
            else if (value == 1) sr.sprite = halfHeart;
            else sr.sprite = emptyHeart;
        }
    }
}