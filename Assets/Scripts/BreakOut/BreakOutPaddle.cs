using UnityEngine;

namespace BreakOut
{
    public class BreakOutPaddle : MonoBehaviour
    {
        public BreakOutControls controls = new BreakOutControls();

        public enum Direction
        {
            Right,
            Left
        }

        void Update()
        {
            if (Input.GetKey(controls.LeftKey))
            {
                Move(Direction.Left);
            }

            if (Input.GetKey(controls.RightKey))
            {
                Move(Direction.Right);
            }
        }

        void Move(Direction direction)
        {
            float moveDistance = controls.paddleSpeed * (3 * Time.deltaTime);
            moveDistance *= direction == Direction.Right ? 1 : -1;

            Vector3 moveVector = new Vector3(moveDistance, 0, 0);

            float newX = transform.position.x + moveDistance;

            if (newX > controls.maxX)
            {
                transform.position = new Vector3(
                    controls.maxX,
                    transform.position.y,
                    transform.position.z
                );
            }
            else if (newX < controls.minX)
            {
                transform.position = new Vector3(
                    controls.minX,
                    transform.position.y,
                    transform.position.z
                );
            }
            else
            {
                transform.Translate(moveVector);
            }
        }
    }
}
