using UnityEngine;

namespace TicTacToe
{
    // Small serializable mapping entry used in the inspector:
    // maps (owner, stage) -> Sprite.
    [System.Serializable]
    public class SeedSpriteEntry
    {
        // Which player the sprite belongs to (or None)
        public ButtonOwner owner;

        // Which growth stage this sprite represents
        public GrowthStage stage;

        // The sprite to use for that owner + stage
        public Sprite sprite;
    }

    // Singleton sprite lookup for all TTB field sprites.
    // Configure the `entries` array in the inspector to provide
    // sprites for all owner/stage combinations you need.
    public class TTBSpriteCatalog : MonoBehaviour
    {
        // Simple singleton instance (non-persistent across scenes).
        public static TTBSpriteCatalog Instance;

        // Inspector-configured mapping entries.
        [SerializeField]
        private SeedSpriteEntry[] entries;

        private void Awake()
        {
            // Basic singleton assignment. If multiple exist, destroy extras.
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        // Lookup: returns the sprite for the given owner+stage or null and logs a warning.
        // Used by TTBFieldButton.SetTile and SetStage to update visuals.
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