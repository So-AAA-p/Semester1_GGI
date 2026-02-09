using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BreakOut
{
    public class BO_LeafBlock : BO_Block
    {
        public SpriteRenderer berryIndicator;

        [Header("Future Sprite Support")]
        public Sprite sprite3Berries;
        public Sprite sprite2Berries;
        public Sprite sprite1Berry;

        private int berryCount;


        private void Start()
        {
            // 1. Register with Manager
            if (BO_BlueberryManager.Instance != null)
                BO_BlueberryManager.Instance.RegisterLeaf(this);

            // 2. Hide indicator immediately until manager assigns berries
            if (berryIndicator != null)
                berryIndicator.enabled = false;
        }

        public void SetBerryCount(int count)
        {
            berryCount = count;
            currentHits = hitsRequired; // RESET hit counter so it needs a fresh hit for the next berry
            UpdateVisuals();
        }

        public void ForceDestroy()
        {
            base.OnBreak();
        }


        private void UpdateVisuals()
        {
            if (berryIndicator == null) return;

            berryIndicator.enabled = berryCount > 0;

            // This is where you will swap sprites later!
            switch (berryCount)
            {
                case 3:
                    // berryIndicator.sprite = sprite3Berries; 
                    berryIndicator.color = Color.red; // Temp placeholder
                    break;
                case 2:
                    // berryIndicator.sprite = sprite2Berries;
                    berryIndicator.color = Color.yellow; // Temp placeholder
                    break;
                case 1:
                    // berryIndicator.sprite = sprite1Berry;
                    berryIndicator.color = Color.white; // Light blue
                    break;
            }
        }

        // The "Flying Berry" Animation
        public IEnumerator AnimateBerryTo(Vector3 targetPos, float duration)
        {
            // Create a temporary "Flying" berry
            GameObject flyer = Instantiate(berryIndicator.gameObject, transform.position, Quaternion.identity);
            flyer.transform.localScale = transform.localScale * 0.5f; // Make it a bit smaller while flying

            float elapsed = 0;
            Vector3 startPos = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percent = elapsed / duration;

                // Move in a slight arc for "juice"
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);
                currentPos.y += Mathf.Sin(percent * Mathf.PI) * 1.5f; // Adds a little hop!

                flyer.transform.position = currentPos;
                yield return null;
            }

            Destroy(flyer);
        }
    }
}