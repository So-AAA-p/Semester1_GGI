using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BreakOut
{
    public class BO_LeafManager : MonoBehaviour
    {
        public static BO_LeafManager Instance;

        [Header("Setup")]
        public BO_Stage3Leaf[] leafBlocks; // Drag your 4 Stage 3 leaves here

        [Header("Timing")]
        public float minSpawnTime = 3f;
        public float maxSpawnTime = 6f;

        public bool isPhase3Active = false;

        private void Awake() { Instance = this; }

        public void StartPhase3()
        {
            Debug.Log("[LeafManager] StartPhase3 was CALLED!"); // 1. Check if function is reached

            if (isPhase3Active)
            {
                Debug.LogWarning("[LeafManager] StartPhase3 blocked: isPhase3Active is already TRUE.");
                return;
            }

            isPhase3Active = true;
            Debug.Log("[LeafManager] Boolean flipped to true. Now starting Coroutine..."); // 2. Check if logic passes

            StartCoroutine(LeafSpawnRoutine());
        }

        IEnumerator LeafSpawnRoutine()
        {
            Debug.Log("[LeafManager] Routine Started!");

            while (isPhase3Active)
            {
                float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
                yield return new WaitForSeconds(waitTime);

                List<BO_Stage3Leaf> availableLeaves = new List<BO_Stage3Leaf>();

                foreach (BO_Stage3Leaf leaf in leafBlocks)
                {
                    if (leaf == null)
                    {
                        Debug.LogError("[LeafManager] A slot in the array is EMPTY! Check the Inspector.");
                        continue;
                    }

                    if (!leaf.isActive)
                    {
                        availableLeaves.Add(leaf);
                    }
                }

                if (availableLeaves.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableLeaves.Count);
                    availableLeaves[randomIndex].ActivateLeaf();
                    Debug.Log($"[LeafManager] Successfully spawned: {availableLeaves[randomIndex].gameObject.name}");
                }
                else
                {
                    Debug.LogWarning("[LeafManager] No available leaves found! Are they all already active?");
                }
            }
        }
    }
}