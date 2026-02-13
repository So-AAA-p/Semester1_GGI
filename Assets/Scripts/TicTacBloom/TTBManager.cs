                                                                            using UnityEngine;
                                                                            using TMPro; 
                                                                            using UnityEngine.SceneManagement;
                                                                            using UnityEngine.UI;
                                                                            using System.Collections;
                                                                            using System.Collections.Generic;

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

                                                                                    
                                                                                    bool gameOver = false;
                                                                                    private bool isBusy = false; // locks input while coroutines/animations run

                                                                                    // Grid configuration
                                                                                    public int gridWidth = 4;
                                                                                    public int gridHeight = 4;

                                                                                    // Runtime grid structures
                                                                                    private TTBFieldButton[,] grid;       // 2D array for neighbor checks
                                                                                    public TTBFieldButton[] fieldbuttons; // flat list assigned in inspector

                                                                                    // Match scoring
                                                                                    int player1Score = 0;
                                                                                    int player2Score = 0;
                                                                                    [SerializeField] int pointsToWin = 3;
                                                                                    [SerializeField] GameObject[] player1ScoreIcons;
                                                                                    [SerializeField] GameObject[] player2ScoreIcons;

                                                                                    // Timing used during win animations (tunable in inspector)
                                                                                    [SerializeField] float sproutToSeedlingDelay = 0.6f;
                                                                                    [SerializeField] float seedlingToPlantDelay = 0.3f;

                                                                                    [Header("Weather Settings")]
                                                                                    // Weather triggers after N turns (counters are incremented after valid moves)
                                                                                    public int turnsUntilDrought = 6;
                                                                                    public int turnsUntilRain = 5;
                                                                                    private int droughtCounter = 0;
                                                                                    private int rainCounter = 0;

                                                                                    [Header("Weather UI")]
                                                                                    public TextMeshProUGUI heatwaveText;
                                                                                    public TextMeshProUGUI bountyText;
                                                                                    public Color warningColorRed = Color.red;
                                                                                    public Color warningColorBlue = new Color(0.2f, 0.5f, 1f);

                                                                                    [Header("Audio")]
                                                                                    // Audio source and clips used across events and actions
                                                                                    public AudioSource sfxSource;
                                                                                    public AudioClip placementSound;
                                                                                    public AudioClip growthSound;
                                                                                    public AudioClip bountySound;
                                                                                    public AudioClip heatwaveSound;
                                                                                    public AudioClip spinEasterEggSound;
                                                                                    public AudioClip winSound;

                                                                                    [Header("Music")]
                                                                                    public AudioSource musicSource;
                                                                                    public AudioClip backgroundMusic;

                                                                                    // Tiles that are temporarily protected from drought (winning tiles during animation)
                                                                                    private List<TTBFieldButton> protectedTiles = new List<TTBFieldButton>();

                                                                                    // Directions used to check lines (horizontal, vertical and two diagonals)
                                                                                    readonly Vector2Int[] directions =
                                                                                    {
                                                                                        new Vector2Int(1, 0),
                                                                                        new Vector2Int(0, 1),
                                                                                        new Vector2Int(1, 1),
                                                                                        new Vector2Int(1, -1),
                                                                                    };

                                                                                    // Who starts each round; swapped on round end
                                                                                    int startingPlayer = 0; // 0 = Player1, 1 = Player2

                                                                                    // Start: initialize scores, UI, grid array and play background music if assigned.
                                                                                    void Start()
                                                                                    {
                                                                                        player1Score = 0;
                                                                                        player2Score = 0;
                                                                                        UpdateScoreUI();
                                                                                        UpdateWeatherUI();

                                                                                        currentPlayer = startingPlayer;
                                                                                        UpdateInfoText();

                                                                                        // Build 2D grid and assign manager to each button
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

                                                                                        if (musicSource != null && backgroundMusic != null)
                                                                                        {
                                                                                            musicSource.clip = backgroundMusic;
                                                                                            musicSource.loop = true;
                                                                                            musicSource.playOnAwake = true;
                                                                                            musicSource.Play();
                                                                                        }
                                                                                    }

                                                                                    // Convenience wrapper to play one-shot SFX via assigned sfxSource.
                                                                                    public void PlaySFX(AudioClip clip)
                                                                                    {
                                                                                        if (clip != null && sfxSource != null)
                                                                                        {
                                                                                            sfxSource.PlayOneShot(clip);
                                                                                        }
                                                                                    }

                                                                                    // Sync the simple heart/point UI with current match score.
                                                                                    void UpdateScoreUI()
                                                                                    {
                                                                                        for (int i = 0; i < player1ScoreIcons.Length; i++)
                                                                                        {
                                                                                            player1ScoreIcons[i].SetActive(i < player1Score);
                                                                                        }

                                                                                        for (int i = 0; i < player2ScoreIcons.Length; i++)
                                                                                        {
                                                                                            player2ScoreIcons[i].SetActive(i < player2Score);
                                                                                        }
                                                                                    }

                                                                                    // Called after a valid player move to advance weather counters and start weather handling coroutine.
                                                                                    public void AdvanceWeather(TTBFieldButton justPlacedButton)
                                                                                    {
                                                                                        if (gameOver) return;

                                                                                        droughtCounter++;
                                                                                        rainCounter++;

                                                                                        UpdateWeatherUI();

                                                                                        // Run weather sequence asynchronously so animations and delays don't block the main thread.
                                                                                        StartCoroutine(HandleWeatherSequence(justPlacedButton));
                                                                                    }

                                                                                    // Orchestrates weather events in order with a small delay to allow players to see the placed tile.
                                                                                    IEnumerator HandleWeatherSequence(TTBFieldButton justPlacedButton)
                                                                                    {
                                                                                        // Short breathing room for animations
                                                                                        yield return new WaitForSeconds(0.5f);

                                                                                        // Sun shower (rain) may spawn a safe seed
                                                                                        if (rainCounter >= turnsUntilRain)
                                                                                        {
                                                                                            rainCounter = 0;
                                                                                            yield return StartCoroutine(TriggerSunShowerRoutine());
                                                                                        }

                                                                                        // Heatwave/drought may remove a random occupied tile (not protected)
                                                                                        if (droughtCounter >= turnsUntilDrought)
                                                                                        {
                                                                                            droughtCounter = 0;
                                                                                            yield return StartCoroutine(TriggerDroughtRoutine(justPlacedButton));
                                                                                        }

                                                                                        UpdateWeatherUI();

                                                                                        // Unlock the board after weather routines finish
                                                                                        isBusy = false;
                                                                                    }

                                                                                    // Sun shower picks a random "safe" empty field (one that would not immediately cause a win
                                                                                    // for either player) and plants a seed there, followed by sprout propagation.
                                                                                    IEnumerator TriggerSunShowerRoutine()
                                                                                    {
                                                                                        List<TTBFieldButton> safeFields = new List<TTBFieldButton>();

                                                                                        PlaySFX(bountySound);

                                                                                        // Collect empty fields that are safe for both players (won't cause immediate win)
                                                                                        foreach (var fb in fieldbuttons)
                                                                                        {
                                                                                            if (fb.owner == ButtonOwner.None)
                                                                                            {
                                                                                                bool p1Win = WouldCauseWin(fb, ButtonOwner.Player1);
                                                                                                bool p2Win = WouldCauseWin(fb, ButtonOwner.Player2);

                                                                                                if (!p1Win && !p2Win)
                                                                                                {
                                                                                                    safeFields.Add(fb);
                                                                                                }
                                                                                            }
                                                                                        }

                                                                                        if (safeFields.Count > 0)
                                                                                        {
                                                                                            TTBFieldButton target = safeFields[Random.Range(0, safeFields.Count)];

                                                                                            // Flash to give player visual feedback
                                                                                            yield return StartCoroutine(FlashButton(target, warningColorBlue));

                                                                                            // Randomly assign owner for the sun-seeded tile
                                                                                            ButtonOwner randomOwner = Random.value > 0.5f ? ButtonOwner.Player1 : ButtonOwner.Player2;
                                                                                            target.SetTile(randomOwner, GrowthStage.Seed);

                                                                                            // Apply sprout growth rules and check for winner consistency
                                                                                            ApplySproutGrowth();
                                                                                            CheckForWinner(target);
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            Debug.Log("Sun Shower skipped: No safe spots to grow without causing a win!");
                                                                                        }
                                                                                    }

                                                                                    // Drought routine picks a random occupied tile (excluding protected tiles and the tile that was just placed)
                                                                                    // flashes it red and resets it to empty.
                                                                                    IEnumerator TriggerDroughtRoutine(TTBFieldButton justPlaced)
                                                                                    {
                                                                                        List<TTBFieldButton> occupiedFields = new List<TTBFieldButton>();

                                                                                        PlaySFX(heatwaveSound);

                                                                                        foreach (var fb in fieldbuttons)
                                                                                        {
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

                                                                                            // Brief flashing for visual feedback
                                                                                            yield return StartCoroutine(FlashButton(target, warningColorRed));

                                                                                            // Remove the tile
                                                                                            target.ResetTile();
                                                                                        }
                                                                                    }

                                                                                    // Flashes a button's background color a few times.
                                                                                    IEnumerator FlashButton(TTBFieldButton btn, Color flashColor)
                                                                                    {
                                                                                        Image bg = btn.GetComponent<Image>();
                                                                                        Color oldColor = bg.color;

                                                                                        for (int i = 0; i < 3; i++)
                                                                                        {
                                                                                            bg.color = flashColor;
                                                                                            yield return new WaitForSeconds(0.2f);
                                                                                            bg.color = oldColor;
                                                                                            yield return new WaitForSeconds(0.2f);
                                                                                        }
                                                                                    }

                                                                                    // Update the weather UI counters and apply warning colors when close to event triggers.
                                                                                    void UpdateWeatherUI()
                                                                                    {
                                                                                        int heatwaveRemaining = turnsUntilDrought - droughtCounter;
                                                                                        int bountyRemaining = turnsUntilRain - rainCounter;

                                                                                        heatwaveText.text = heatwaveRemaining.ToString();
                                                                                        heatwaveText.color = (heatwaveRemaining <= 1) ? warningColorRed : Color.white;

                                                                                        bountyText.text = bountyRemaining.ToString();
                                                                                        bountyText.color = (bountyRemaining <= 1) ? warningColorBlue : Color.white;
                                                                                    }

                                                                                    // Sprout propagation rules:
                                                                                    // Phase 1: detect which seeds should sprout (based on neighbors)
                                                                                    // Phase 2: advance those seeds to Sprout stage.
                                                                                    void ApplySproutGrowth()
                                                                                    {
                                                                                        bool[,] shouldSprout = new bool[gridWidth, gridHeight];

                                                                                        // Phase 1: detection — if a Seed has a neighbor with same owner that is Seed or Sprout,
                                                                                        // mark both positions to sprout.
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

                                                                                        // Phase 2: apply growth to those marked positions (Seed -> Sprout).
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

                                                                                    // Updates the on-screen info text to indicate the current player.
                                                                                    void UpdateInfoText()
                                                                                    {
                                                                                        InfoText.text = currentPlayer == 1
                                                                                            ? "Current Player: 2"
                                                                                            : "Current Player: 1";
                                                                                    }

                                                                                    // Called by a TTBFieldButton when the user clicks a free tile.
                                                                                    // Enforces isBusy lock and gameOver guard so we don't accept input while animations/weather run.
                                                                                    public void OnButtonClickedMan(TTBFieldButton fieldButton)
                                                                                    {
                                                                                        if (gameOver || isBusy || fieldButton.owner != ButtonOwner.None)
                                                                                            return;

                                                                                        PlaySFX(placementSound);

                                                                                        isBusy = true; // lock input during processing

                                                                                        // 1) Place a seed for the current player.
                                                                                        ButtonOwner owner = currentPlayer == 0 ? ButtonOwner.Player1 : ButtonOwner.Player2;
                                                                                        fieldButton.SetTile(owner, GrowthStage.Seed);

                                                                                        // 2) Apply sprout propagation rules immediately.
                                                                                        ApplySproutGrowth();

                                                                                        // 3) Check if this caused a winning sprout line.
                                                                                        CheckForWinner(fieldButton);

                                                                                        // 4) Advance weather counters and start weather handling.
                                                                                        AdvanceWeather(fieldButton);

                                                                                        // Swap current player and update UI.
                                                                                        currentPlayer = currentPlayer == 1 ? 0 : 1;
                                                                                        UpdateInfoText();
                                                                                    }

                                                                                    // Counts contiguous tiles of a particular owner+stage along a line (both directions).
                                                                                    int CountInLine(TTBFieldButton start, Vector2Int dir, ButtonOwner owner, GrowthStage stage)
                                                                                    {
                                                                                        int count = 1; // include start

                                                                                        // count forward and backward along direction
                                                                                        count += CountDirection(start, dir, owner, stage);
                                                                                        count += CountDirection(start, -dir, owner, stage);

                                                                                        return count;
                                                                                    }

                                                                                    // Helper: count along one direction until a mismatch or board edge.
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

                                                                                    // Disable all buttons UI interaction (used on match end).
                                                                                    void DisableButtons()
                                                                                    {
                                                                                        foreach (TTBFieldButton fieldButton in fieldbuttons)
                                                                                        {
                                                                                            fieldButton.DisableButton();
                                                                                        }
                                                                                    }

                                                                                    // Check whether the lastPlaced sprout caused a winning line.
                                                                                    // Only evaluates when the last placed tile is in Sprout stage.
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
                                                                                                // Collect full line tiles and resolve win growth sequence.
                                                                                                List<TTBFieldButton> line = new List<TTBFieldButton>();
                                                                                                line.Add(lastPlaced);

                                                                                                CollectDirection(lastPlaced, dir, line);
                                                                                                CollectDirection(lastPlaced, -dir, line);

                                                                                                StartCoroutine(ResolveWinGrowth(line));
                                                                                                return;
                                                                                            }
                                                                                        }

                                                                                        // If no winner and board full -> draw.
                                                                                        if (IsBoardFull())
                                                                                        {
                                                                                            HandleDraw();
                                                                                            return;
                                                                                        }
                                                                                    }

                                                                                    // Animates winning tiles through growth stages, optionally spins for 4-in-a-row,
                                                                                    // then awards a point and proceeds to next round or match end.
                                                                                    IEnumerator ResolveWinGrowth(List<TTBFieldButton> line)
                                                                                    {
                                                                                        // Protect these tiles from drought while animating.
                                                                                        protectedTiles.AddRange(line);

                                                                                        // 1) Advance to Seedling
                                                                                        foreach (var fb in line)
                                                                                            fb.AdvanceGrowth();

                                                                                        yield return new WaitForSeconds(sproutToSeedlingDelay);

                                                                                        // 2) Advance to Plant
                                                                                        foreach (var fb in line)
                                                                                            fb.AdvanceGrowth();

                                                                                        yield return new WaitForSeconds(seedlingToPlantDelay);

                                                                                        // 3) Easter-egg: spin if 4+ in a row
                                                                                        if (line.Count >= 4)
                                                                                        {
                                                                                            Debug.Log("Easter Egg Triggered: 4 in a row!");
                                                                                            yield return StartCoroutine(SpinWinningPlants(line));
                                                                                        }

                                                                                        // 4) Award point for the owner of the winning tiles
                                                                                        ScorePoint(line[0].owner);
                                                                                    }

                                                                                    // Spins the plant images on the winning tiles for a short duration for effect.
                                                                                    IEnumerator SpinWinningPlants(List<TTBFieldButton> line)
                                                                                    {
                                                                                        float duration = 0.8f;
                                                                                        float elapsed = 0f;
                                                                                        PlaySFX(spinEasterEggSound);

                                                                                        while (elapsed < duration)
                                                                                        {
                                                                                            elapsed += Time.deltaTime;

                                                                                            float currentRotation = Mathf.Lerp(0, 360, elapsed / duration);

                                                                                            foreach (var fb in line)
                                                                                            {
                                                                                                fb.GetPlantImage().rectTransform.localEulerAngles = new Vector3(0, 0, currentRotation);
                                                                                            }

                                                                                            yield return null;
                                                                                        }

                                                                                        // Reset rotation to avoid drift
                                                                                        foreach (var fb in line)
                                                                                        {
                                                                                            fb.GetPlantImage().rectTransform.localEulerAngles = Vector3.zero;
                                                                                        }

                                                                                        yield return new WaitForSeconds(0.2f);
                                                                                    }

                                                                                    // Routine used to start the next round after a short delay.
                                                                                    IEnumerator NextRoundRoutine(ButtonOwner roundWinner)
                                                                                    {
                                                                                        yield return new WaitForSeconds(1.5f);

                                                                                        ResetBoard();

                                                                                        gameOver = false;
                                                                                        isBusy = false;
                                                                                        currentPlayer = startingPlayer;
                                                                                        UpdateInfoText();
                                                                                    }

                                                                                    // Show win UI and disable board when a player wins the match.
                                                                                    void EndMatch(ButtonOwner winner)
                                                                                    {
                                                                                        InfoText.enabled = false;
                                                                                        WinScreen.SetActive(true);
                                                                                        WinnerText.enabled = true;
                                                                                        WinnerText.text = winner + " wins the match!";

                                                                                        ResetButton.gameObject.SetActive(true);
                                                                                        DisableButtons();
                                                                                        gameOver = true;
                                                                                    }

                                                                                    // Reset every tile to empty and re-enable button interaction.
                                                                                    void ResetBoard()
                                                                                    {
                                                                                        foreach (TTBFieldButton fb in fieldbuttons)
                                                                                        {
                                                                                            fb.ResetTile();
                                                                                            fb.EnableButton();
                                                                                        }
                                                                                    }

                                                                                    // Helper: collect contiguous tiles in one direction for a winning line.
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

                                                                                    // Light-weight simulation to determine whether planting a sprout for potentialOwner
                                                                                    // on target would immediately create a win. Used by SunShower to avoid unfair auto-wins.
                                                                                    bool WouldCauseWin(TTBFieldButton target, ButtonOwner potentialOwner)
                                                                                    {
                                                                                        // Temporarily set the state to sprout for the candidate.
                                                                                        target.owner = potentialOwner;
                                                                                        target.stage = GrowthStage.Sprout;

                                                                                        bool winDetected = false;

                                                                                        // Reuse the same directional count logic used in CheckForWinner.
                                                                                        foreach (var dir in directions)
                                                                                        {
                                                                                            if (CountInLine(target, dir, potentialOwner, GrowthStage.Sprout) >= 3)
                                                                                            {
                                                                                                winDetected = true;
                                                                                                break;
                                                                                            }
                                                                                        }

                                                                                        // Reset the tile back to empty.
                                                                                        target.owner = ButtonOwner.None;
                                                                                        target.stage = GrowthStage.None;

                                                                                        return winDetected;
                                                                                    }

                                                                                    // Award a point to the winner, reset weather counters and either end match or
                                                                                    // start the next round (swap starting player).
                                                                                    void ScorePoint(ButtonOwner winner)
                                                                                    {
                                                                                        if (winner == ButtonOwner.Player1)
                                                                                            player1Score++;
                                                                                        else
                                                                                            player2Score++;

                                                                                        // Reset weather progression on scoring and clear protection list.
                                                                                        droughtCounter = 0;
                                                                                        rainCounter = 0;
                                                                                        protectedTiles.Clear();

                                                                                        UpdateWeatherUI();
                                                                                        UpdateScoreUI();

                                                                                        // Swap starting player for the next round.
                                                                                        startingPlayer = startingPlayer == 0 ? 1 : 0;

                                                                                        // If match has been won, end it. Otherwise begin next round.
                                                                                        if (player1Score >= pointsToWin || player2Score >= pointsToWin)
                                                                                        {
                                                                                            EndMatch(winner);
                                                                                            return;
                                                                                        }

                                                                                        StartCoroutine(NextRoundRoutine(winner));
                                                                                    }

                                                                                    // Utility: is the whole board full (no None owners)?
                                                                                    bool IsBoardFull()
                                                                                    {
                                                                                        foreach (TTBFieldButton fb in fieldbuttons)
                                                                                        {
                                                                                            if (fb.owner == ButtonOwner.None)
                                                                                            {
                                                                                                return false;
                                                                                            }
                                                                                        }
                                                                                        return true;
                                                                                    }

                                                                                    // Handle draw state by showing draw UI and scheduling next round.
                                                                                    void HandleDraw()
                                                                                    {
                                                                                        gameOver = true;

                                                                                        WinScreen.SetActive(true);
                                                                                        WinnerText.enabled = true;
                                                                                        WinnerText.text = "Draw! :3";

                                                                                        StartCoroutine(DrawNextRoundRoutine());
                                                                                    }

                                                                                    // Rolls the board back to an empty state after a draw and swaps starting player.
                                                                                    IEnumerator DrawNextRoundRoutine()
                                                                                    {
                                                                                        yield return new WaitForSeconds(1.5f);

                                                                                        WinScreen.SetActive(false);
                                                                                        WinnerText.enabled = false;

                                                                                        ResetBoard();

                                                                                        startingPlayer = startingPlayer == 0 ? 1 : 0;

                                                                                        currentPlayer = startingPlayer;
                                                                                        gameOver = false;
                                                                                        UpdateInfoText();
                                                                                    }
                                                                                }
                                                                            }