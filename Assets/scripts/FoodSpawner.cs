using System.Collections;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Item Prefabs")]
    public GameObject[] foodPrefabs; // Assign Burger and Fries prefabs

    [Header("Spawn Position Boundaries")]
    public float spawnY = 6f;       // Height above screen
    public float minX = -7f;        // Left boundary
    public float maxX = 7f;         // Right boundary

    [Header("Timing")]
    public float spawnInterval = 1.2f;
    public bool isSpawning = true;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (foodPrefabs == null || foodPrefabs.Length == 0) continue;

            // Pick a random food item
            GameObject randomPrefab = foodPrefabs[Random.Range(0, foodPrefabs.Length)];

            // Pick a random horizontal position
            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

            // Instantiate
            Instantiate(randomPrefab, spawnPosition, Quaternion.identity);
        }
    }
}