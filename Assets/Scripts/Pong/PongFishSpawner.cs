using UnityEngine;

namespace Pong
{
public class PongFishSpawner : MonoBehaviour
{
    // 1. Changed to an array so you can drop multiple fish here!
    public GameObject[] fishPrefabs;

    public float spawnInterval = 5f;
    public float yRange = 4f;
    public float xSpawnDist = 12f;

    void Start()
    {
        InvokeRepeating("SpawnFish", 2f, spawnInterval);
    }

    void SpawnFish()
    {
        if (Pong.PongManager.instance.currentLevel != Pong.PongManager.LevelType.UnderWater)
            return;

        // 2. Safety check: make sure you actually put fish in the list
        if (fishPrefabs.Length == 0) return;

        // 3. The Math: Pick a random index from 0 to the end of the array
        int randomIndex = Random.Range(0, fishPrefabs.Length);
        GameObject selectedFish = fishPrefabs[randomIndex];

        float spawnX = Random.value > 0.5f ? xSpawnDist : -xSpawnDist;
        float spawnY = Random.Range(-yRange, yRange);

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);

        // 4. Spawn the randomly selected fish
        Instantiate(selectedFish, spawnPos, Quaternion.identity);
    }
}
}
