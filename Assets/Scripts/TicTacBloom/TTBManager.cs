
using UnityEngine;
using TMPro;																						//muss teilswiese manuell hinzugefügt werden
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // für Listen

namespace TicTacToe
{
    public class TTBManager : MonoBehaviour
    {
        int currentPlayer = 0;

        public TextMeshProUGUI InfoText;
        public GameObject WinScreen;
        public TextMeshProUGUI WinnerText; 
        public Button ResetButton; 
        public Button BTMenu;
        public GameObject Visuals;

        bool gameOver = false;

        public int gridWidth = 4;
        public int gridHeight = 4;

        private TTBFieldButton[,] grid;
        public TTBFieldButton[] fieldbuttons;

        int player1Score = 0;
        int player2Score = 0;
        [SerializeField] int pointsToWin = 3;
        [SerializeField] GameObject[] player1ScoreIcons; // size 3
        [SerializeField] GameObject[] player2ScoreIcons; // size 3

        [SerializeField] float sproutToSeedlingDelay = 0.6f;
        [SerializeField] float seedlingToPlantDelay = 0.3f;

        readonly Vector2Int[] directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
        };

        int startingPlayer = 0; // 0 = Player1, 1 = Player2

        //private void OnDestroy()                                                                        // wird immer ausgeführt, wenn Script durchgelaufen ist
        //{
        //    ResetButton.onClick.RemoveListener(RestartGame);
        //}

        void Start()
        {
            player1Score = 0;
            player2Score = 0;
            UpdateScoreUI();

            currentPlayer = startingPlayer;
            UpdateInfoText();

            grid = new TTBFieldButton[gridWidth, gridHeight];

            for (int i = 0; i < fieldbuttons.Length; i++)
            {
                int x = i % gridWidth;
                int y = i / gridWidth;

                TTBFieldButton fb = fieldbuttons[i];
                fb.x = x;
                fb.y = y;

                grid[x, y] = fb;
                fb.SetManager(this);
            }

            WinScreen.SetActive(false);
            WinnerText.enabled = false;
            ResetButton.gameObject.SetActive(false);
        }

        void UpdateScoreUI()
        {
            // Player 1
            for (int i = 0; i < player1ScoreIcons.Length; i++)
            {
                player1ScoreIcons[i].SetActive(i < player1Score);
            }

            // Player 2
            for (int i = 0; i < player2ScoreIcons.Length; i++)
            {
                player2ScoreIcons[i].SetActive(i < player2Score);
            }
        }

        void ApplySproutGrowth()
        {
            bool[,] shouldSprout = new bool[gridWidth, gridHeight];

            // Phase 1: detect
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    TTBFieldButton center = grid[x, y];

                    if (center.owner == ButtonOwner.None || center.stage != GrowthStage.Seed)
                        continue;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0)
                                continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight)
                                continue;

                            TTBFieldButton neighbor = grid[nx, ny];

                            if (
                                neighbor.owner == center.owner &&
                                (neighbor.stage == GrowthStage.Seed || neighbor.stage == GrowthStage.Sprout)
                                )

                            {
                                shouldSprout[x, y] = true;
                                shouldSprout[nx, ny] = true;
                            }
                        }
                    }
                }
            }

            // Phase 2: apply
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (shouldSprout[x, y])
                    {
                        TTBFieldButton fb = grid[x, y];
                        if (fb.stage == GrowthStage.Seed)
                            fb.AdvanceGrowth(); // Seed → Sprout
                    }
                }
            }
        }

        //void Update()
        //{
        //    InfoText.text = currentPlayer == 1 ? ("Current Player: 2") : ("Current Player: 1");
        //}

        void UpdateInfoText() // nichtmehr jeden frame aufgerufen
        {
            InfoText.text = currentPlayer == 1
                ? "Current Player: 2"
                : "Current Player: 1";
        }

        public void OnButtonClickedMan(TTBFieldButton fieldButton)
        {
            if (fieldButton.owner != ButtonOwner.None)
                return;

            ButtonOwner owner =
                currentPlayer == 0 ? ButtonOwner.Player1 : ButtonOwner.Player2;

            // 1. Place seed
            fieldButton.SetTile(owner, GrowthStage.Seed);

            // 2. Fully resolve sprout growth
            ApplySproutGrowth();

            // (Later)
            // ApplySeedlingGrowth();
            // ApplyPlantGrowthAndWin();

            // 3. NOW it's safe to check for winner
            CheckForWinner(fieldButton);

            currentPlayer = currentPlayer == 1 ? 0 : 1;
            UpdateInfoText();
        }

        int CountInLine(TTBFieldButton start, Vector2Int dir, ButtonOwner owner, GrowthStage stage)
        {
            int count = 1; // include start

            // forward
            count += CountDirection(start, dir, owner, stage);

            // backward
            count += CountDirection(start, -dir, owner, stage);

            return count;
        }

        int CountDirection(TTBFieldButton start, Vector2Int dir, ButtonOwner owner, GrowthStage stage)
        {
            int count = 0;
            int x = start.x + dir.x;
            int y = start.y + dir.y;

            while (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                TTBFieldButton fb = grid[x, y];

                if (fb.owner != owner || fb.stage != stage)
                    break;

                count++;
                x += dir.x;
                y += dir.y;
            }

            return count;
        }

        //void SetUpWinningConditions()                                                                   // void, weil nichts zurück gegeben wird
        //{
        //    Reihe1V = new int[] { 0, 1, 2 };                                                            // Array, keine Liste
        //    Reihe1H = new int[] { 1, 2, 3 };                                                            // Unterschied Liste/Array - Array: zu Arrays kann man keine weiteren Element hinzufügen; Liste: kann man weiter ergänzen
        // ...    


        void DisableButtons()
        {
            foreach (TTBFieldButton fieldButton in fieldbuttons)
            {
                fieldButton.DisableButton();
            }
        }

        void CheckForWinner(TTBFieldButton lastPlaced)
        {
            if (gameOver)
                return;

            if (lastPlaced.stage != GrowthStage.Sprout)
                return;

            foreach (var dir in directions)
            {
                int count = CountInLine(
                    lastPlaced,
                    dir,
                    lastPlaced.owner,
                    GrowthStage.Sprout
                );

                if (count >= 3)
                {
                    List<TTBFieldButton> line = new List<TTBFieldButton>();
                    line.Add(lastPlaced);

                    CollectDirection(lastPlaced, dir, line);
                    CollectDirection(lastPlaced, -dir, line);

                    StartCoroutine(ResolveWinGrowth(line));
                    return;
                }
            }

            if (IsBoardFull())
            {
                HandleDraw();
                return;
            }

            //  foreach (FieldButton fb in fieldbuttons)
            //  {
            //      var text = fb.GetComponentInChildren<TextMeshProUGUI>();
            //      text.color = new Color32(124, 171, 86, 255);
            //  }
        }

        IEnumerator ResolveWinGrowth(List<TTBFieldButton> line)
        {
            foreach (var fb in line)
                fb.AdvanceGrowth(); // Sprout → Seedling

            yield return new WaitForSeconds(sproutToSeedlingDelay);

            foreach (var fb in line)
                fb.AdvanceGrowth(); // Seedling → Plant

            yield return new WaitForSeconds(seedlingToPlantDelay);

            ScorePoint(line[0].owner);
        }

        IEnumerator NextRoundRoutine(ButtonOwner roundWinner)
        {
            // Show round result UI here
            // (activate your GameObjects here)
            // example:
            // roundWinPanel.SetActive(true);

            yield return new WaitForSeconds(1.5f);

            // Hide round UI again
            // roundWinPanel.SetActive(false);

            ResetBoard();

            gameOver = false;
            currentPlayer = startingPlayer;
            UpdateInfoText();
        }

        void EndMatch(ButtonOwner winner)
        {
            InfoText.enabled = false;
            WinScreen.SetActive(true);
            WinnerText.enabled = true;
            Visuals.SetActive(false); // shhhhh
            WinnerText.text = winner + " wins the match!";

            ResetButton.gameObject.SetActive(true);
            DisableButtons();
            gameOver = true;
        }

        void ResetBoard()
        {
            foreach (TTBFieldButton fb in fieldbuttons)
            {
                fb.ResetTile(); // you likely already have this
                fb.EnableButton();
            }
        }

        void CollectDirection(TTBFieldButton start, Vector2Int dir, List<TTBFieldButton> line)
        {
            int x = start.x + dir.x;
            int y = start.y + dir.y;

            while (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                TTBFieldButton fb = grid[x, y];

                if (fb.owner != start.owner || fb.stage != GrowthStage.Sprout)
                    break;

                line.Add(fb);
                x += dir.x;
                y += dir.y;
            }
        }

        void ScorePoint(ButtonOwner winner)
        {
            if (winner == ButtonOwner.Player1)
                player1Score++;
            else
                player2Score++;

            UpdateScoreUI();
            // 🔁 swap starting player for next round
            startingPlayer = startingPlayer == 0 ? 1 : 0;

            // 🏆 check match win
            if (player1Score >= pointsToWin || player2Score >= pointsToWin)
            {
                EndMatch(winner);
                return;
            }

            // 🎯 otherwise: start next round
            StartCoroutine(NextRoundRoutine(winner));
        }

        bool IsBoardFull()
        {
            foreach (TTBFieldButton fb in fieldbuttons)                                                // fb als abkürzung für fieldbutton
            {
                if (fb.owner == ButtonOwner.None)
                {
                    return false; 
                }
            }
            return true;
        }

        void HandleDraw()
        {
            gameOver = true;

            WinScreen.SetActive(true);
            WinnerText.enabled = true;
            Visuals.SetActive(false);
            WinnerText.text = "Draw! :3";

            StartCoroutine(DrawNextRoundRoutine());
        }
        IEnumerator DrawNextRoundRoutine()
        {
            yield return new WaitForSeconds(1.5f);

            WinScreen.SetActive(false);
            WinnerText.enabled = false;
            Visuals.SetActive(true);

            ResetBoard();

            // 🔁 starting player still swaps on draw (optional, but recommended)
            startingPlayer = startingPlayer == 0 ? 1 : 0;

            currentPlayer = startingPlayer;
            gameOver = false;
            UpdateInfoText();
        }
    }
}