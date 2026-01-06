using UnityEngine;

namespace TicTacToe
{
    [System.Serializable]
    public class SeedSpriteEntry
    {
        public ButtonOwner owner;
        public GrowthStage stage;
        public Sprite sprite;
    }

    public class TTBSpriteCatalog : MonoBehaviour
    {
        public static TTBSpriteCatalog Instance;

        [SerializeField]
        private SeedSpriteEntry[] entries;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public Sprite GetSprite(ButtonOwner owner, GrowthStage stage)
        {
            foreach (var entry in entries)
            {
                if (entry.owner == owner && entry.stage == stage)
                    return entry.sprite;
            }

            Debug.LogWarning($"No sprite found for {owner} + {stage}");
            return null;
        }
    }
}