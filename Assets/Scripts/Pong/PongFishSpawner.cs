using UnityEngine;

public class PongFishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;
    public float spawnInterval = 5f;
    public float yRange = 4f;
    public float xSpawnDist = 12f; // Just off-screen

    void Start()
    {
        // Invokes the 'Spawn' method every 'spawnInterval' seconds
        InvokeRepeating("SpawnFish", 2f, spawnInterval);
    }

    void SpawnFish()
    {
        // Don't spawn fish if we aren't in Level 2!
        if (Pong.PongManager.instance.currentLevel != Pong.PongManager.LevelType.UnderWater)
            return;

        // Choose left or right side randomly
        float spawnX = Random.value > 0.5f ? xSpawnDist : -xSpawnDist;
        float spawnY = Random.Range(-yRange, yRange);

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);
        Instantiate(fishPrefab, spawnPos, Quaternion.identity);
    }
}