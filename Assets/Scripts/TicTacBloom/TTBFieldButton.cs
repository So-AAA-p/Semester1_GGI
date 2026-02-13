using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe
{
    public class TTBFieldButton : MonoBehaviour
    {
        // Reference to the central manager that controls game flow.
        private TTBManager TTBMan;

        // Arbitrary field value (unused for core logic, kept for extension).
        public int FieldValue = -1;

        // Grid coordinates assigned by the manager in Start()
        public int x;
        public int y;

        // Current owner and growth stage of this tile.
        public ButtonOwner owner;
        public GrowthStage stage;

        [Header("Dirt Tinting")]
        [SerializeField] private Image backgroundImage; // The dirt background used for tinting
        [SerializeField] private Color player1Tint = new Color(0.7f, 1f, 0.7f); // Player1 dirt tint
        [SerializeField] private Color player2Tint = new Color(1f, 0.7f, 0.7f); // Player2 dirt tint
        [SerializeField] private Color neutralColor = Color.white; // Neutral dirt

        // Image used for the plant sprite (set in inspector).
        [SerializeField] private Image image;
        //[SerializeField] private Animator animator;

        private void Awake()
        {
            // Safety checks: image must be assigned for visuals to work.
            if (image == null)
            {
                Debug.LogError("IMAGE IS NULL on " + gameObject.name);
                return;
            }

            // Hide the plant image initially so empty tiles don't show white/empty sprite.
            image.enabled = false;

            // Animator could be used for stage animations (optional).
            // if (animator == null) { animator = GetComponentInChildren<Animator>(); }
        }

        // Start is intentionally empty; manager assigns itself via SetManager.
        void Start()
        {
            // Manager assignment is performed by TTBManager when building the grid.
        }

        // Called by the manager to provide the TTBManager reference.
        public void SetManager(TTBManager newManager)
        {
            TTBMan = newManager;
        }

        // Exposes the plant image for special visual manipulations (e.g. spin).
        public Image GetPlantImage()
        {
            return image;
        }

        // Called by the UI Button onClick (wired in inspector).
        // Sends a click event to the manager only if tile is empty.
        public void OnButtonClicked()
        {
            if (owner == ButtonOwner.None)
            {
                TTBMan.OnButtonClickedMan(this);
            }
        }

        // Set tile ownership + growth stage and update visuals.
        // Uses TTBSpriteCatalog to get the proper sprite.
        public void SetTile(ButtonOwner newOwner, GrowthStage newStage)
        {
            Debug.Log($"SetTile called on {gameObject.name}");

            owner = newOwner;
            stage = newStage;

            // Update background tint based on owner (dirt color).
            UpdateBackgroundTint(owner);

            if (image == null)
            {
                Debug.LogError("IMAGE IS NULL on " + gameObject.name);
                return;
            }

            // Lookup sprite from the catalog singleton.
            var sprite = TTBSpriteCatalog.Instance.GetSprite(owner, stage);

            if (sprite == null)
            {
                Debug.LogError($"SPRITE NULL for {owner} {stage}");
                return;
            }

            Debug.Log("IMAGE IS " + sprite.name);

            // Apply sprite and make it visible.
            image.sprite = sprite;
            image.enabled = true;
        }

        // Change the dirt background tint according to owner.
        public void UpdateBackgroundTint(ButtonOwner currentOwner)
        {
            if (backgroundImage == null) return;

            switch (currentOwner)
            {
                case ButtonOwner.Player1:
                    backgroundImage.color = player1Tint;
                    break;
                case ButtonOwner.Player2:
                    backgroundImage.color = player2Tint;
                    break;
                case ButtonOwner.None:
                    backgroundImage.color = neutralColor;
                    break;
            }
        }

        // Disable the underlying UI Button to prevent clicks.
        public void DisableButton()
        {
            GetComponent<UnityEngine.UI.Button>().interactable = false;
        }

        // Advance this tile one growth stage (None->Seed->Sprout->Seedling->Plant).
        // Plays the shared growth SFX via the manager.
        public void AdvanceGrowth()
        {
            switch (stage)
            {
                case GrowthStage.None:
                    SetStage(GrowthStage.Seed);
                    break;
                case GrowthStage.Seed:
                    SetStage(GrowthStage.Sprout);
                    break;
                case GrowthStage.Sprout:
                    SetStage(GrowthStage.Seedling);
                    break;
                case GrowthStage.Seedling:
                    SetStage(GrowthStage.Plant);
                    break;
                case GrowthStage.Plant:                                                 
                    // Final state – do nothing
                    break;
            }

            // Play growth sound (manager handles audio source).
            TTBMan.PlaySFX(TTBMan.growthSound);
        }

        // Low-level stage setter that updates sprite and visibility.
        void SetStage(GrowthStage newStage)
        {
            stage = newStage;

            var sprite = TTBSpriteCatalog.Instance.GetSprite(owner, stage);
            image.sprite = sprite;
            image.enabled = true;

            // If an animator is used, you could trigger an animation here:
            // animator.Play(stage.ToString());
        }

        // Reset tile to empty initial state (clears owner, stage and visuals).
        public void ResetTile()
        {
            owner = ButtonOwner.None;
            stage = GrowthStage.None;

            image.sprite = null;
            image.enabled = false;

            backgroundImage.color = neutralColor;
            GetComponent<Button>().interactable = true;
        }

        // Re-enable the UI Button for interaction.
        public void EnableButton()
        {
            GetComponent<Button>().interactable = true;
        }
    }
}