
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
        private bool isBusy = false;

        public int gridWidth = 4;
        public int gridHeight = 4;

        private TTBFieldButton[,] grid;
        public TTBFieldButton[] fieldbuttons;
        //private TTBFieldButton lastRainTarget; YAYA

        int player1Score = 0;
        int player2Score = 0;
        [SerializeField] int pointsToWin = 3;
        [SerializeField] GameObject[] player1ScoreIcons; // size 3
        [SerializeField] GameObject[] player2ScoreIcons; // size 3

        [SerializeField] float sproutToSeedlingDelay = 0.6f;
        [SerializeField] float seedlingToPlantDelay = 0.3f;

        [Header("Weather Settings")]
        public int turnsUntilDrought = 6; // Increased from 4
        public int turnsUntilRain = 5;    // Increased from 3
        private int droughtCounter = 0;
        private int rainCounter = 0;

        [Header("Weather UI")]
        public TextMeshProUGUI heatwaveText;
        public TextMeshProUGUI bountyText;
        public Color warningColorRed = Color.red;
        public Color warningColorBlue = new Color(0.2f, 0.5f, 1f); // Nice Blue

        private List<TTBFieldButton> protectedTiles = new List<TTBFieldButton>();


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
            UpdateWeatherUI();

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

        public void AdvanceWeather(TTBFieldButton justPlacedButton)
        {
            // Point 11: Only advances if a valid move was made (called from OnButtonClickedMan)
            if (gameOver) return;

            droughtCounter++;
            rainCounter++;

            UpdateWeatherUI();

            // Start the weather sequence (Point 3: Delay for clarity)
            StartCoroutine(HandleWeatherSequence(justPlacedButton));
        }

        IEnumerator HandleWeatherSequence(TTBFieldButton justPlacedButton)
        {
            // Wait for the initial move to breathe
            yield return new WaitForSeconds(0.5f);

            // --- SUN SHOWER ---
            if (rainCounter >= turnsUntilRain)
            {
                rainCounter = 0;
                yield return StartCoroutine(TriggerSunShowerRoutine());
            }

            // --- HEATWAVE ---
            if (droughtCounter >= turnsUntilDrought)
            {
                droughtCounter = 0;
                yield return StartCoroutine(TriggerDroughtRoutine(justPlacedButton));
            }

            UpdateWeatherUI();

            // FINALLY: Unlock the board for the next player!
            isBusy = false;
        }

        IEnumerator TriggerSunShowerRoutine()
        {
            List<TTBFieldButton> safeFields = new List<TTBFieldButton>();

            foreach (var fb in fieldbuttons)
            {
                if (fb.owner == ButtonOwner.None)
                {
                    // Check if placing for P1 OR P2 would cause a win
                    bool p1Win = WouldCauseWin(fb, ButtonOwner.Player1);
                    bool p2Win = WouldCauseWin(fb, ButtonOwner.Player2);

                    // Only add to our list if it's safe for everyone
                    if (!p1Win && !p2Win)
                    {
                        safeFields.Add(fb);
                    }
                }
            }

            // If there are no "safe" spots left, the Sun Shower just skips this turn 
            // (Better to have no rain than an unfair win!)
            if (safeFields.Count > 0)
            {
                TTBFieldButton target = safeFields[Random.Range(0, safeFields.Count)];

                yield return StartCoroutine(FlashButton(target, warningColorBlue));

                ButtonOwner randomOwner = Random.value > 0.5f ? ButtonOwner.Player1 : ButtonOwner.Player2;
                target.SetTile(randomOwner, GrowthStage.Seed);

                // Even though we checked, we still call Sprout growth to be safe
                ApplySproutGrowth();
                // Since it's a "safe" seed, CheckForWinner usually won't trigger, but keep it for consistency
                CheckForWinner(target);
            }
            else
            {
                Debug.Log("Sun Shower skipped: No safe spots to grow without causing a win!");
            }
        }

        IEnumerator TriggerDroughtRoutine(TTBFieldButton justPlaced)
        {
            List<TTBFieldButton> occupiedFields = new List<TTBFieldButton>();
            foreach (var fb in fieldbuttons)
            {
                // Point 5: Only occupied
                // Point 7: Not part of a winning combo
                // Point 8: Not the button the player JUST clicked
                if (fb.owner != ButtonOwner.None &&
                    !protectedTiles.Contains(fb) &&
                    fb != justPlaced)
                {
                    occupiedFields.Add(fb);
                }
            }

            if (occupiedFields.Count > 0)
            {
                TTBFieldButton target = occupiedFields[Random.Range(0, occupiedFields.Count)];

                // Point 4: Flash Red
                yield return StartCoroutine(FlashButton(target, warningColorRed));

                target.ResetTile();
            }
        }

        IEnumerator FlashButton(TTBFieldButton btn, Color flashColor)
        {
            Image bg = btn.GetComponent<Image>();
            Color oldColor = bg.color;

            for (int i = 0; i < 3; i++) // Flash 3 times
            {
                bg.color = flashColor;
                yield return new WaitForSeconds(0.2f);
                bg.color = oldColor;
                yield return new WaitForSeconds(0.2f);
            }
        }

        void UpdateWeatherUI()
        {
            int heatwaveRemaining = turnsUntilDrought - droughtCounter;
            int bountyRemaining = turnsUntilRain - rainCounter;

            heatwaveText.text = heatwaveRemaining.ToString();
            // Point 1: Turn Red at 1
            heatwaveText.color = (heatwaveRemaining <= 1) ? warningColorRed : Color.white;

            bountyText.text = bountyRemaining.ToString();
            // Point 2: Turn Blue at 1
            bountyText.color = (bountyRemaining <= 1) ? warningColorBlue : Color.white;
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
            // Point 11: Added isBusy check here!
            if (gameOver || isBusy || fieldButton.owner != ButtonOwner.None)
                return;

            isBusy = true; // LOCK the board

            // 1. Place seed
            ButtonOwner owner = currentPlayer == 0 ? ButtonOwner.Player1 : ButtonOwner.Player2;
            fieldButton.SetTile(owner, GrowthStage.Seed);

            // 2. Growth logic
            ApplySproutGrowth();

            // 3. Check for winner
            CheckForWinner(fieldButton);

            // 4. Advance weather
            AdvanceWeather(fieldButton);

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
            protectedTiles.AddRange(line); // Protect them from drought during the animation

            // 1. Advance to Seedling
            foreach (var fb in line)
                fb.AdvanceGrowth();

            yield return new WaitForSeconds(sproutToSeedlingDelay);

            // 2. Advance to Plant
            foreach (var fb in line)
                fb.AdvanceGrowth();

            yield return new WaitForSeconds(seedlingToPlantDelay);

            // --- EASTER EGG CHECK ---
            // If the professor managed to get 4 in a row!
            if (line.Count >= 4)
            {
                Debug.Log("Easter Egg Triggered: 4 in a row!");
                yield return StartCoroutine(SpinWinningPlants(line));
            }

            // 3. Finally score the point
            ScorePoint(line[0].owner);
        }

        // The Rotation Logic
        IEnumerator SpinWinningPlants(List<TTBFieldButton> line)
        {
            float duration = 0.8f; // How long the spin takes
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Calculate the rotation (0 to 360 degrees)
                float currentRotation = Mathf.Lerp(0, 360, elapsed / duration);

                foreach (var fb in line)
                {
                    // We rotate the rectTransform of the plant image
                    fb.GetPlantImage().rectTransform.localEulerAngles = new Vector3(0, 0, currentRotation);
                }

                yield return null; // Wait for next frame
            }

            // Ensure they are perfectly snapped back to 0 at the end
            foreach (var fb in line)
            {
                fb.GetPlantImage().rectTransform.localEulerAngles = Vector3.zero;
            }

            yield return new WaitForSeconds(0.2f); // Tiny pause for dramatic effect
        }

        IEnumerator NextRoundRoutine(ButtonOwner roundWinner)
        {
            yield return new WaitForSeconds(1.5f);

            ResetBoard();

            gameOver = false;
            isBusy = false; // Ensure it's unlocked for the new round!
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

        bool WouldCauseWin(TTBFieldButton target, ButtonOwner potentialOwner)
        {
            // 1. Temporarily "fake" the state of this button
            // We check for Sprout stage because that's what your CheckForWinner looks for
            target.owner = potentialOwner;
            target.stage = GrowthStage.Sprout;

            bool winDetected = false;

            // 2. Run the same line-check logic you use for regular wins
            foreach (var dir in directions)
            {
                if (CountInLine(target, dir, potentialOwner, GrowthStage.Sprout) >= 3)
                {
                    winDetected = true;
                    break;
                }
            }

            // 3. IMPORTANT: Reset the button back to empty
            target.owner = ButtonOwner.None;
            target.stage = GrowthStage.None;

            return winDetected;
        }

        void ScorePoint(ButtonOwner winner)
        {
            if (winner == ButtonOwner.Player1)
                player1Score++;
            else
                player2Score++;

            droughtCounter = 0;
            rainCounter = 0;
            protectedTiles.Clear(); // Clear the "locked" winning tiles for the next round
            UpdateWeatherUI();
            UpdateScoreUI();
            //  swap starting player for next round
            startingPlayer = startingPlayer == 0 ? 1 : 0;

            //  check match win
            if (player1Score >= pointsToWin || player2Score >= pointsToWin)
            {
                EndMatch(winner);
                return;
            }

            //  otherwise: start next round
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

            //  starting player still swaps on draw (optional, but recommended)
            startingPlayer = startingPlayer == 0 ? 1 : 0;

            currentPlayer = startingPlayer;
            gameOver = false;
            UpdateInfoText();
        }
    }
}