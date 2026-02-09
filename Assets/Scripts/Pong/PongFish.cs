using UnityEngine;

public class UnderwaterFish : MonoBehaviour
{
    [Header("Horizontal Movement")]
    public float speed = 3f;
    private int direction = 1;

    [Header("Vertical Bobbing")]
    public float amplitude = 0.5f;  // How high/low it bobs
    public float frequency = 2f;    // How fast it bobs

    private float startY;
    private float randomOffset;     // Prevents all fish from bobbing in unison

    void Start()
    {
        startY = transform.position.y;
        randomOffset = Random.Range(0f, 2f * Mathf.PI); // Random start point in the sine wave

        // Auto-destruct after 10 seconds to save memory
        Destroy(gameObject, 10f);

        // Determine direction based on spawn side
        direction = (transform.position.x > 0) ? -1 : 1;

        if (direction == -1)
            GetComponent<SpriteRenderer>().flipX = true;
    }

    void Update()
    {
        // 1. Horizontal: Constant velocity
        float posX = transform.position.x + (direction * speed * Time.deltaTime);

        // 2. Vertical: Sine Wave Logic
        // y = A * sin(B * t + C) + offset
        float posY = startY + Mathf.Sin(Time.time * frequency + randomOffset) * amplitude;

        transform.position = new Vector3(posX, posY, transform.position.z);
    }
}