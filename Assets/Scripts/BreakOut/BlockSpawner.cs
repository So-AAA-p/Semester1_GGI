using UnityEngine;
using System.Collections.Generic;

namespace BreakOut
{
    public class BlockSpawner : MonoBehaviour
    {
        [Header("Brick Prefabs")]
        public GameObject normalBlock;
        public GameObject sugarBlock;
        public GameObject eggBlock;
        public GameObject saltBlock;
        public GameObject milkBlock;
        public GameObject flourBlock;

        [Header("Spawn Chances (0–1)")] // wahrscheinlichkeiten zu blöcken hinzugefügt, damit gameplay nicht zu schwer und chaotisch wird -> zb 5 sugar blöcke -> 10 bälle im game
        [Range(0f, 1f)] public float sugarChance = 0.08f;
        [Range(0f, 1f)] public float eggChance = 0.12f;
        [Range(0f, 1f)] public float saltChance = 0.1f;
        [Range(0f, 1f)] public float milkChance = 0.08f;
        [Range(0f, 1f)] public float flourChance = 0.1f;

        [Header("Grid Size")]
        public int rows = 5;
        public int columns = 10;

        [Header("Brick Size")]
        public float brickWidth = 1.6f;
        public float brickHeight = 0.8f;

        [Header("Spacing")]
        public float spacing = 0.1f;

        [Header("Vertical Offset")]
        public float topOffset = 4f;

        public static BlockSpawner Instance;

        private Dictionary<Vector2Int, BreakOutBlock> blockGrid = new();


        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            GenerateGrid();
        }

        void GenerateGrid()
        {
            Vector2 startPosition = CalculateStartPosition();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Vector2 spawnPos = new Vector2(
                        startPosition.x + col * (brickWidth + spacing),
                        startPosition.y - row * (brickHeight + spacing)
                    );

                    GameObject prefab = PickBrick(row);
                    GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

                    BreakOutBlock block = instance.GetComponent<BreakOutBlock>();

                    Vector2Int gridPos = new Vector2Int(col, row);

                    block.SetGridPosition(gridPos);
                    blockGrid.Add(gridPos, block);
                    Debug.Log($"[Grid] Set {block.name} at {gridPos}");
                }
            }
        }
        public BreakOutBlock GetBlockAt(Vector2Int pos)
        {
            blockGrid.TryGetValue(pos, out BreakOutBlock block);
            return block;
        }

        public IEnumerable<BreakOutBlock> GetBlocksBelow(Vector2Int milkPos)
        {
            for (int row = milkPos.y + 1; row < rows; row++)
            {
                Vector2Int pos = new Vector2Int(milkPos.x, row);
                Debug.Log($"[Milk] Checking {pos}");

                Debug.Log($"[Milk] Looking for block at grid {pos}");

                if (!blockGrid.TryGetValue(pos, out BreakOutBlock block))
                {
                    Debug.Log($"[Milk]  No block found at {pos}, skipping...");
                    continue;
                }

                Debug.Log($"[Milk]  Found block at {pos}");
                yield return block;
            }
        }

        GameObject PickBrick(int currentRow)
        {
            float roll = Random.value;

            if (roll < sugarChance)
                return sugarBlock;
            roll -= sugarChance;

            if (roll < eggChance)
                return eggBlock;
            roll -= eggChance;

            if (roll < saltChance)
                return saltBlock;
            roll -= saltChance;

            if (currentRow <= 1)
            {
                if (roll < milkChance)
                    return milkBlock;
                roll -= milkChance;
            }

            if (roll < flourChance)
                return flourBlock;
            roll -= flourChance;

            return normalBlock;
        }

        Vector2 CalculateStartPosition()
        {
            float totalWidth =
                columns * brickWidth +
                (columns - 1) * spacing;

            float startX = -totalWidth / 2f + brickWidth / 2f;
            float startY = topOffset;

            return new Vector2(startX, startY);
        }

        public void RemoveBlock(Vector2Int pos)
        {
            blockGrid.Remove(pos);
        }
        public void OnBlockDestroyed(Vector2Int pos)
        {
            if (blockGrid.Remove(pos))
            {
                Debug.Log($"[Blocks] Remaining: {blockGrid.Count}");

                if (blockGrid.Count == 0)
                {
                    Debug.Log("[Stage] All blocks cleared!");
                    BreakOutManager.instance.OnStageCleared();
                }
            }
        }
    }
}

