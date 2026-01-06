using UnityEngine;

namespace BreakOut
{
    public class BrickGrid : MonoBehaviour
    {
        [Header("References")]
        public GameObject brickPrefab;

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

                    Instantiate(brickPrefab, spawnPos, Quaternion.identity, transform);
                }
            }
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


