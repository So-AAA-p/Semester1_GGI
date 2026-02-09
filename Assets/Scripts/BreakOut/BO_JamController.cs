using UnityEngine;

namespace BreakOut
{
    public class BO_JamController : MonoBehaviour
    {
        public static BO_JamController Instance;

        [Header("Jam Settings")]
        public int maxJam = 5;
        public int currentJam = 0;
        public bool isJamModeActive = false;

        [Header("Visuals")]
        public Color jamReadyColor = Color.magenta; // Color when meter is full

        private void Awake() { Instance = this; }

        void Update()
        {
            // Add the Unlock check here!
            if (BO_Manager.instance.isJamUnlocked)
            {
                if (Input.GetKeyDown(KeyCode.Q) && currentJam >= maxJam && !isJamModeActive)
                {
                    ActivateJamMode();
                }
            }
        }

        public void AddBerry()
        {
            if (currentJam < maxJam)
            {
                currentJam++;
                Debug.Log($"Jam Meter: {currentJam}/{maxJam}");
            }
        }

        void ActivateJamMode()
        {
            // Find the ball dynamically in case it respawned
            BO_Ball ball = Object.FindFirstObjectByType<BO_Ball>();

            if (ball != null)
            {
                isJamModeActive = true;
                ball.EnableJamCoating();
                currentJam = 0; // Reset meter
                Debug.Log("JAM MODE ACTIVE!");
            }
            else
            {
                Debug.LogError("Jam Mode failed: No ball found in scene!");
            }
        }

        // Called by the Ball when it hits the boss
        public void JamHitComplete()
        {
            isJamModeActive = false;
            Debug.Log("Jam Attack landed! Mode deactivated.");
        }
    }
}