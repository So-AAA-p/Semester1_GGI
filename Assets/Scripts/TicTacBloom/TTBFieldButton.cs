using UnityEngine;
using UnityEngine.UI;

//extra scipt für die buttons, weil wir nicht wissen mit wie vielen buttons wir arbeiten, deswegen so einfacher und übersichtlicher bzw so einfacher auf andere Anzhal buttons anzupassen!
namespace TicTacToe
{
    public class TTBFieldButton : MonoBehaviour
    {
        private TTBManager TTBManager;

        public int FieldValue = -1;                                                             // -1 als beispiel, kann auch anderer Wert sein
                                                                                                // -1 wird oft benutzt, um nichts bzw eine leere Variable zu kennzeichnen
        public int x;
        public int y;

        public ButtonOwner owner;
        public GrowthStage stage;


        [SerializeField] private Image image;
        //[SerializeField] private Animator animator;

        private void Awake()
        {
            if (image == null)
            {
                Debug.LogError("IMAGE IS NULL on " + gameObject.name);
                return;
            }

            //Vinc: Bild am Anfang deaktivieren, damit keine weiße Fläche angezeigt wird🕺
            image.enabled = false;

            //if (animator == null)
            //{
            //    animator = GetComponentInChildren<Animator>();
            //}
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            // TTBManager = FindFirstObjectByType<TicTacBloomMan>(); // das in Start nicht ideal -> langsam, wenn es oft aufgerufen wird + kann kaputt gehen, wenn es mehrere Manager gibt - für jetzt aber noch fine
            // nicht mehr gebraucht, weil unten SetManager Funktion 
        }

        public void SetManager(TTBManager newManager)
        {
            TTBManager = newManager;
        }


        public void OnButtonClicked()
        {
            if (owner == ButtonOwner.None)            // wenn das Feld schon belegt ist, dann nichts machen
            {
                TTBManager.OnButtonClickedMan(this);
            }
        }


        public void SetTile(ButtonOwner newOwner, GrowthStage newStage)
        {
            Debug.Log($"SetTile called on {gameObject.name}");

            owner = newOwner;
            stage = newStage;

            if (image == null)
            {
                Debug.LogError("IMAGE IS NULL on " + gameObject.name);
                return;
            }

            var sprite = TTBSpriteCatalog.Instance.GetSprite(owner, stage);

            if (sprite == null)
            {
                Debug.LogError($"SPRITE NULL for {owner} {stage}");
                return;
            }
                Debug.Log("IMAGE IS " + sprite.name);
            
            image.sprite = sprite;

            //Vinc: Bild aktivieren, damit Sprite angezeigt wird :3
            image.enabled = true;
        }

        public void DisableButton()
        {
            GetComponent<UnityEngine.UI.Button>().interactable = false;                             // GetComp ding, sucht solange in Children, bis er ein element mit dem darauffolgendem Typ findet -> hiet TextMeshPro 
                                                                                                    // nimmt man, damit man nicht alle Elemente separat zum Script hinzufügen muss
        }

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
        }

        void SetStage(GrowthStage newStage)
        {
            stage = newStage;

            var sprite = TTBSpriteCatalog.Instance.GetSprite(owner, stage);
            image.sprite = sprite;
            image.enabled = true;

            // animator.Play(stage.ToString());
        }

        public void ResetTile()
        {
            owner = ButtonOwner.None;
            stage = GrowthStage.None;

            image.sprite = null;
            image.enabled = false;

            GetComponent<Button>().interactable = true;
        }

        public void EnableButton()
        {
            GetComponent<Button>().interactable = true;
        }
    }
}
  