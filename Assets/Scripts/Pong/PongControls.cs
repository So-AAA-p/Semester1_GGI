using UnityEngine;

namespace Pong
{ 
    [System.Serializable]                                               // muss man hinzufügen, damit die Klasse im Inspektor angezeigt wird
                                                                        // System wie using UnityEngine oben, aber mega dick und damit man nicht alles inportieren muss und viel Zeug bekommt, dass man nicht braucht, einfach vor Serializable System.
    public class PongControls
    {
        public KeyCode PositiveKey = KeyCode.W;
        public KeyCode NegativeKey = KeyCode.S;

        public float paddleSpeed;
    }
}