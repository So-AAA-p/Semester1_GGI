using UnityEngine;

namespace Pong
{
    public class PongPaddle : MonoBehaviour
    {
        public PongControls controls;

        public enum Direction
        {
            Positive,                                                           // hoch oder nach rechts
            Negative                                                            // runter oder nach links
        }

        public bool moveHorizontally = false;



        void Update()
        {
            if(Input.GetKey(controls.PositiveKey))
            {
                Debug.Log("Nach unten gedrückt");
                Move(Direction.Positive);
            }
            if (Input.GetKey(controls.NegativeKey))
            {
                Debug.Log("Nach oben gedrückt");
                Move(Direction.Negative);
            }
        }

        void Move(Direction direction)
        {
            float moveAmount = controls.paddleSpeed * Time.deltaTime;
            moveAmount *= direction == Direction.Positive ? 1 : -1;
            // int directionMultiplier = direction == Direction.Up ? 1 : -1;
            // moveDistance *= directionMultiplier;

            if (moveHorizontally)
            {
                float newX = transform.position.x + moveAmount;

                newX = Mathf.Clamp(newX, controls.minX, controls.maxX);

                transform.position = new Vector3(
                    newX,
                    transform.position.y,
                    transform.position.z
                );
            }
            else
            {
                float newY = transform.position.y + moveAmount;

                newY = Mathf.Clamp(newY, controls.minY, controls.maxY);

                transform.position = new Vector3(
                    transform.position.x,
                    newY,
                    transform.position.z
                );
            }
            Debug.Log(direction.ToString() + "gedrückt");
        }
    }
}
