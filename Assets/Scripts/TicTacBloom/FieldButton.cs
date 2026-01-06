
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//extra scipt für die buttons, weil wir nicht wissen mit wie vielen buttons wir arbeiten, deswegen so einfacher und übersichtlicher bzw so einfacher auf andere Anzhal buttons anzupassen!
namespace TicTacToe
{
    public class FieldButton : MonoBehaviour
    {
        private TicTacBloomMan TTBManager;

        public int FieldValue = -1;                                                             // -1 als beispiel, kann auch anderer Wert sein
                                                                                                // -1 wird oft benutzt, um nichts bzw eine leere Variable zu kennzeichnen

        public ButtonOwner owner;
        public GrowthStage stage;

        [SerializeField] private Image image;



        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            TTBManager = FindFirstObjectByType<TicTacBloomMan>(); // das in Start nicht ideal -> langsam, wenn es oft aufgerufen wird + kann kaputt gehen, wenn es mehrere Manager gibt - für jetzt aber noch fine

        }

        // Update is called once per frame
        void Update()
        {

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
        }



        public void DisableButton()
        {
            GetComponent<UnityEngine.UI.Button>().interactable = false;                             // GetComp ding, sucht solange in Children, bis er ein element mit dem darauffolgendem Typ findet -> hiet TextMeshPro 
                                                                                                    // nimmt man, damit man nicht alle Elemente separat zum Script hinzufügen muss
        }

    }
}
  
