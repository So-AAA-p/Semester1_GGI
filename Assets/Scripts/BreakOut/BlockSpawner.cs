using UnityEngine;

namespace BreakOut
{
    public class BlockSpawner : MonoBehaviour
    {
        [Header("Brick Prefabs")]
        public GameObject normalBlock;
        public GameObject sugarBlock;
        public GameObject eggBlock;
        public GameObject saltBlock;

        [Header("Spawn Chances (0–1)")] // wahrscheinlichkeiten zu blöcken hinzugefügt, damit gameplay nicht zu schwer und chaotisch wird -> zb 5 sugar blöcke -> 10 bälle im game
        [Range(0f, 1f)] public float sugarChance = 0.08f;
        [Range(0f, 1f)] public float eggChance = 0.12f;
        [Range(0f, 1f)] public float saltChance = 0.1f;

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

                    GameObject brick = PickBrick();
                    Instantiate(brick, spawnPos, Quaternion.identity, transform);
                }
            }
        }

        GameObject PickBrick()
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
    }
}

