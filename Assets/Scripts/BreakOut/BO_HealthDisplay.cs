using UnityEngine;
using System.Collections.Generic;

namespace BreakOut
{
    public class BO_HealthDisplay : MonoBehaviour
    {
        public static BO_HealthDisplay Instance;

        // Drag your 5 Heart UI GameObjects here in order (Left to Right)
        public BO_HeartUI[] hearts;

        private void Awake() => Instance = this;

        // This is the ONLY method this script needs. 
        // The Manager calls this and passes the health number.
        public void UpdateDisplay(int health)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                // Each heart 'i' represents health points (i*2) and (i*2 + 1)
                // If health is 5: 
                // Heart 0 gets '2' (Full)
                // Heart 1 gets '2' (Full)
                // Heart 2 gets '1' (Half)
                // Heart 3 gets '0' (Empty)

                int heartValue = 0;
                int healthForThisHeart = health - (i * 2);

                if (healthForThisHeart >= 2) heartValue = 2;      // Full
                else if (healthForThisHeart == 1) heartValue = 1; // Half
                else heartValue = 0;                             // Empty

                hearts[i].SetHeartState(heartValue);
            }
        }
    }
}