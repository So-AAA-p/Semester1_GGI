
using UnityEngine;
using TMPro;																						//muss teilswiese manuell hinzugefügt werden
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TicTacToe
{
    public class TicTacBloomMan : MonoBehaviour
    {
        int currentPlayer = 0;

        public TextMeshProUGUI InfoText;

        public FieldButton[] fieldbuttons;

        // Gewinnkombinationen der Reihen, jeweils für die V-ordere 3 Buttons und H-intere 3 Buttons
        int[] Reihe1V;
        int[] Reihe1H;
        int[] Reihe2V;
        int[] Reihe2H;
        int[] Reihe3V;
        int[] Reihe3H;
        int[] Reihe4V;
        int[] Reihe4H;

        // Gewinnkombinationen der Spalten, jeweils für die O-beren 3 Buttons und U-nteren 3 Buttons
        int[] Spalte1O;
        int[] Spalte1U;
        int[] Spalte2O;
        int[] Spalte2U;
        int[] Spalte3O;
        int[] Spalte3U;
        int[] Spalte4O;
        int[] Spalte4U;

        // Gewinnkombinationen der Diagonalen, rechts oben nach links unten
        int[] Diagonale1;
        int[] Diagonale2;
        int[] Diagonale2Ext; // Diagonale 2 extenden, also selbe Reihen, nur erst die oberen 3 Buttons, dann die unteren 3 Buttons
        int[] Diagonale4;

        // Gewinnkombinationen der Diagonalen, rechts unten nach links oben
        int[] Diagonale5;
        int[] Diagonale6;
        int[] Diagonale6Ext; // Diagonale 6 extenden, also selbe Reihen, nur erst die unteren 3 Buttons, dann die oberen 3 Buttons
        int[] Diagonale8;

        int[][] winningCombinations;

        public TextMeshProUGUI WinnerText;

        public Button ResetButton;

        public Button BTMenu;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ResetButton.onClick.AddListener(RestartGame);                                               // AddListener ist Methode hier; weil Unity weiß, dass RestartGame eine Methode ist, braucht man nicht nochmal Klammern dahinter
                                                                                                        // Jeder Listener nimmt Speicherplatz ein, deswegen Listener auch wieder deabonnieren
            SetUpWinningConditions();

            ResetButton.gameObject.SetActive(false);

            BTMenu.onClick.AddListener(() => SceneManager.LoadScene(0));
        }

        private void OnDestroy()                                                                        // wird immer ausgeführt, wenn Script durchgelaufen ist
        {
            ResetButton.onClick.RemoveListener(RestartGame);
        }


        // Update is called once per frame
        void Update()
        {
            InfoText.text = currentPlayer == 1 ? ("Current Player: 2") : ("Current Player: 1");         // um TextMeshPro in string umzuwandeln, .text nach InfoText
        }



        public void OnButtonClickedMan(FieldButton fieldButton)
        {
            Debug.Log("SLAY");


            ButtonOwner owner =
            currentPlayer == 0 ? ButtonOwner.Player1 : ButtonOwner.Player2;

            fieldButton.SetTile(owner, GrowthStage.Seed);


            CheckForWinner();

            currentPlayer = currentPlayer == 1 ? 0 : 1;

        }

        void SetUpWinningConditions()                                                                   // void, weil nichts zurück gegeben wird
        {
            Reihe1V = new int[] { 0, 1, 2 };                                                            // Array, keine Liste
            Reihe1H = new int[] { 1, 2, 3 };                                                            // Unterschied Liste/Array - Array: zu Arrays kann man keine weiteren Element hinzufügen; Liste: kann man weiter ergänzen
            Reihe2V = new int[] { 4, 5, 6 };
            Reihe2H = new int[] { 5, 6, 7 };
            Reihe3V = new int[] { 8, 9, 10 };
            Reihe3H = new int[] { 9, 10, 11 };
            Reihe4V = new int[] { 12, 13, 14 };
            Reihe4H = new int[] { 13, 14, 15 };

            Spalte1O = new int[] { 0, 4, 8 };
            Spalte1U = new int[] { 4, 8, 12 };
            Spalte2O = new int[] { 1, 5, 9 };
            Spalte2U = new int[] { 5, 9, 13 };
            Spalte3O = new int[] { 2, 6, 10 };
            Spalte3U = new int[] { 6, 10, 14 };
            Spalte4O = new int[] { 3, 7, 11 };
            Spalte4U = new int[] { 7, 11, 15 };

            Diagonale1 = new int[] { 0, 5, 10 };
            Diagonale2 = new int[] { 1, 6, 11 };
            Diagonale2Ext = new int[] { 4, 9, 14 };
            Diagonale4 = new int[] { 5, 10, 15 };

            Diagonale5 = new int[] { 2, 5, 8 };
            Diagonale6 = new int[] { 3, 6, 9 };
            Diagonale6Ext = new int[] { 6, 9, 12 };
            Diagonale8 = new int[] { 7, 10, 13 };


            winningCombinations = new int[][]
            {
                Reihe1V,
                Reihe1H,
                Reihe2V,
                Reihe2H,
                Reihe3V,
                Reihe3H,
                Reihe4V,
                Reihe4H,

                Spalte1O,
                Spalte1U,
                Spalte2O,
                Spalte2U,
                Spalte3O,
                Spalte3U,
                Spalte4O,
                Spalte4U,

                Diagonale1,
                Diagonale2,
                Diagonale2Ext,
                Diagonale4,

                Diagonale5,
                Diagonale6,
                Diagonale6Ext,
                Diagonale8,
            };
        }


        void DisableButtons()
        {
            foreach (FieldButton fieldButton in fieldbuttons)
            {
                fieldButton.DisableButton();
            }
        }


        void CheckForWinner()
        {
            foreach (int[] combo in winningCombinations)
            {
                int a = combo[0]; 
                int b = combo[1]; 
                int c = combo[2];

                var ownerA = fieldbuttons[a].owner;
                var ownerB = fieldbuttons[b].owner;
                var ownerC = fieldbuttons[c].owner;

                if (ownerA != ButtonOwner.None &&
                    ownerA == ownerB &&
                    ownerB == ownerC)
                {
                    if (ownerA != ButtonOwner.None && ownerA == ownerB && ownerB == ownerC)                           // Feld muss belegt sein + alle 3 gleich
                    {
                        ButtonOwner winner = ownerA + 1;

                        InfoText.enabled = false;
                        WinnerText.enabled = true;
                        WinnerText.text = "Player " + winner + " wins!";
                        ResetButton.gameObject.SetActive(true);

                        Debug.Log("WINNER: Player " + winner);

                        foreach (FieldButton fb in fieldbuttons)                                        // Farbe der Texte in den Buttons ändern
                        {
                            var text = fb.GetComponentInChildren<TextMeshProUGUI>();
                            text.color = new Color32(124, 171, 86, 255);                                // bei Verwendun von RGB -> Color32, sonst geht nicht :)
                                                                                                        // in Klammer -> (Rot, Grün, Blau, Alpha) Alpha ist die Deckkraft, bei 100% also 255, ist die Farbe komplett sichtbar
                        }

                        DisableButtons();
                        return;

                    }
                }


                    


                if (IsBoardFull())
                {
                    InfoText.enabled = false;
                    WinnerText.enabled = true;
                    WinnerText.text = "Draw! Better luck next time :3";
                    ResetButton.gameObject.SetActive(true);
                    DisableButtons();

                    foreach (FieldButton fb in fieldbuttons)
                    {
                        var text = fb.GetComponentInChildren<TextMeshProUGUI>();
                        text.color = new Color32(124, 171, 86, 255);
                    }
                }
            }
      

            bool IsBoardFull()
            {
                foreach (FieldButton fb in fieldbuttons)                                                // fb als abkürzung für fieldbutton
                {
                    if (fb.owner == ButtonOwner.None)
                    {
                        return false; 
                    }
                }
                return true;
            }
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

}


