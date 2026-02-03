using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BreakOut
{
    public class BO_BlueberryManager : MonoBehaviour
    {
        public static BO_BlueberryManager Instance;

        [Header("UI References")]
        public TextMeshProUGUI counterText;

        [Header("Blueberry Settings")]
        public int totalBlueberries = 20;
        public int maxPerLeaf = 3;
        public int requiredToClear = 15;

        [Header("Animation Settings")]
        public GameObject berryVisualPrefab;
        public float travelDuration = 0.5f;

        [Header("Physics Settings")]
        public GameObject fallingBerryPrefab;

        [Header("Probabilities (0.0 to 1.0)")]
        [Range(0f, 1f)] public float dropChance = 0.3f; // 30%
        [Range(0f, 1f)] public float moveChance = 0.5f; // 50%

        private int collected = 0;
        private Dictionary<BO_LeafBlock, int> leafBerries = new();
        private List<BO_LeafBlock> allLeaves = new();

        private void Awake() => Instance = this;

        private void UpdateUI()
        {
            if (counterText != null)
            {
                counterText.text = $"Berries: {collected} / {requiredToClear}";
            }
        }

        public void RegisterLeaf(BO_LeafBlock leaf)
        {
            if (leafBerries.ContainsKey(leaf)) return;
            leafBerries.Add(leaf, 0);
            allLeaves.Add(leaf);
        }

        public void InitializeBlueberries()
        {
            int capacity = allLeaves.Count * maxPerLeaf;
            int toSpawn = Mathf.Min(totalBlueberries, capacity);
            int remaining = toSpawn;

            while (remaining > 0)
            {
                BO_LeafBlock leaf = allLeaves[Random.Range(0, allLeaves.Count)];
                if (leafBerries[leaf] < maxPerLeaf)
                {
                    leafBerries[leaf]++;
                    remaining--;
                }
            }

            collected = 0; // Reset count for the new stage
            UpdateUI();
            UpdateLeafVisuals();
        }

        public void OnLeafHit(BO_LeafBlock leaf)
        {
            if (leaf == null || !leafBerries.ContainsKey(leaf)) return;

            // 1. Collect the "Primary" hit berry
            // (This one is guaranteed because the ball touched the leaf)
            collected++;
            leafBerries[leaf]--;

            // Update the visual immediately so the player sees the berry disappear
            leaf.SetBerryCount(leafBerries[leaf]);

            // 2. THE NEW GATE: Only allow bonus drops/moves if there are berries LEFT
            if (leafBerries[leaf] > 0)
            {
                float roll = Random.value;

                // Roll for a Bonus Drop (30% chance)
                if (roll < dropChance)
                {
                    SpawnFallingBerry(leaf.transform.position);
                    leafBerries[leaf]--; // Subtract the bonus berry from the leaf
                }
                // Roll for a Move (50% chance)
                // Using "else if" means a berry can either drop OR move, not both at once
                else if (roll < dropChance + moveChance)
                {
                    RedistributeFromLeaf(leaf);
                }

                // Final visual update after potential bonus actions
                leaf.SetBerryCount(leafBerries[leaf]);
            }

            // 3. Cleanup: If the leaf is now 0, remove it from the tracking lists
            if (leafBerries[leaf] <= 0)
            {
                allLeaves.Remove(leaf);
                leafBerries.Remove(leaf);
            }

            UpdateUI();
            CheckWinCondition();
        }

        void RedistributeFromLeaf(BO_LeafBlock source)
        {
            if (source == null) return;

            int toProcess = leafBerries[source];
            Vector3 startPos = source.transform.position;

            for (int i = 0; i < toProcess; i++)
            {
                float roll = Random.value;

                if (roll < dropChance)
                {
                    SpawnFallingBerry(startPos);
                    leafBerries[source]--; // Subtract from source
                }
                else if (roll < dropChance + moveChance)
                {
                    BO_LeafBlock target = GetRandomLeafWithSpace();
                    if (target != null && target != source)
                    {
                        leafBerries[target]++;
                        leafBerries[source]--; // Subtract from source
                        StartCoroutine(AnimateBerryFly(startPos, target));
                    }
                }
                // If it fails both rolls, we do NOTHING. 
                // The berry stays on the 'source' leaf.
            }
        }

        public void CollectBerry()
        {
            collected++;
            UpdateUI();

            if (collected >= requiredToClear)
            {
                BO_Manager.instance.OnStageCleared();
            }
            CheckWinCondition();
        }

        void SpawnFallingBerry(Vector3 position)
        {
            Instantiate(fallingBerryPrefab, position, Quaternion.identity);
        }

        // The Manager-based animation
        private IEnumerator AnimateBerryFly(Vector3 startPos, BO_LeafBlock targetLeaf)
        {
            // Create a temp visual berry
            GameObject flyer = Instantiate(berryVisualPrefab, startPos, Quaternion.identity);

            float elapsed = 0;
            Vector3 endPos;

            while (elapsed < travelDuration)
            {
                elapsed += Time.deltaTime;
                float percent = elapsed / travelDuration;

                // Safety: if target leaf is destroyed while berry is mid-air, 
                // it just flies to where the leaf WAS.
                endPos = targetLeaf != null ? targetLeaf.transform.position : flyer.transform.position;

                // Simple arc movement
                Vector3 currentPos = Vector3.Lerp(startPos, endPos, percent);
                currentPos.y += Mathf.Sin(percent * Mathf.PI) * 1.2f;

                flyer.transform.position = currentPos;
                yield return null;
            }

            Destroy(flyer);

            // Tell the target leaf to update its color/sprite once the berry "lands"
            if (targetLeaf != null)
            {
                targetLeaf.SetBerryCount(leafBerries[targetLeaf]);
            }
        }

        BO_LeafBlock GetRandomLeafWithSpace()
        {
            allLeaves.RemoveAll(l => l == null);
            List<BO_LeafBlock> candidates = new();

            foreach (var leaf in allLeaves)
            {
                if (leafBerries.ContainsKey(leaf) && leafBerries[leaf] < maxPerLeaf)
                {
                    candidates.Add(leaf);
                }
            }
            return candidates.Count == 0 ? null : candidates[Random.Range(0, candidates.Count)];
        }

        void UpdateLeafVisuals()
        {
            foreach (var pair in leafBerries)
            {
                if (pair.Key != null)
                    pair.Key.SetBerryCount(pair.Value);
            }
        }

        public void CheckWinCondition()
        {
            if (collected >= requiredToClear)
            {
                Debug.Log("Blueberry Goal Reached!");
                // Force the stage manager to transition
                BO_Manager.instance.OnStageCleared();
            }
        }
    }
}