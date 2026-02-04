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
            if (leaf == null) return;

            // 1. CHECK FIRST: Is this leaf actually tracked?
            if (!leafBerries.ContainsKey(leaf))
            {
                Debug.LogWarning($"Leaf {leaf.name} not in dictionary. Destroying.");
                leaf.ForceDestroy();
                return;
            }

            // 2. NOW it is safe to access leafBerries[leaf]
            CollectBerry();
            leafBerries[leaf]--;

            // 3. Handle Bonus Logic
            if (leafBerries[leaf] > 0)
            {
                float roll = Random.value;
                if (roll < dropChance)
                {
                    SpawnFallingBerry(leaf.transform.position);
                    leafBerries[leaf]--;
                }
                else if (roll < dropChance + moveChance)
                {
                    // Pass the leaf to redistribute
                    RedistributeFromLeaf(leaf);
                }
            }

            // 4. Update or Cleanup
            // We check the dictionary again because Redistribute might have changed the count
            if (leafBerries.ContainsKey(leaf))
            {
                if (leafBerries[leaf] <= 0)
                {
                    allLeaves.Remove(leaf);
                    leafBerries.Remove(leaf);
                    leaf.SetBerryCount(0); // This triggers the final break
                }
                else
                {
                    leaf.SetBerryCount(leafBerries[leaf]);
                }
            }
        }

        void RedistributeFromLeaf(BO_LeafBlock source)
        {
            // Safety check for the redistribute method specifically
            if (source == null || !leafBerries.ContainsKey(source)) return;

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
            // If we've already won, stop counting
            if (collected == -999) return;

            collected++;
            UpdateUI();

            if (collected >= requiredToClear)
            {
                collected = -999; // Lock this method
                Debug.Log("Blueberry Goal Reached!");
                BO_Manager.instance.OnStageCleared();
            }
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


        public void ClearRemainingLeaves()
        {
            // Find all objects with the BO_LeafBlock script and destroy them
            BO_LeafBlock[] remaining = FindObjectsOfType<BO_LeafBlock>();
            foreach (var leaf in remaining)
            {
                Destroy(leaf.gameObject);
            }
            allLeaves.Clear();
            leafBerries.Clear();
        }
    }
}