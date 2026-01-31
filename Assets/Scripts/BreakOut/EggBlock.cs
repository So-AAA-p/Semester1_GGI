using UnityEngine;

namespace BreakOut
{
    public class EggBlock : BreakOutBlock // -> eggblock erbt von breakoutblock und ist kein monobehaviour
    {
        [Header("Egg Crack Sprites")]
        public Sprite[] crackStages;

        private SpriteRenderer spriteRenderer;

        protected override void Start()
        {
            base.Start();
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateSprite();
        }

        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            base.OnCollisionEnter2D(collision);
            UpdateSprite();
        }

        void UpdateSprite()
        {
            if (crackStages == null || crackStages.Length == 0)
                return;

            int index = Mathf.Clamp(
                crackStages.Length - currentHits,
                0,
                crackStages.Length - 1
            );

            spriteRenderer.sprite = crackStages[index];
        }
    }
}

