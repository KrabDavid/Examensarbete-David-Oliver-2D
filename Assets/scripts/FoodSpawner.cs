using System.Collections;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject[] foodPrefabs;
    public float spawnInterval = 1.5f;
    public float xSpawnRange = 8f;
    public float ySpawnPosition = 6f;

    private Coroutine spawnCoroutine;

    private void OnEnable()
    {
        // Start spawning when script is enabled
        spawnCoroutine = StartCoroutine(SpawnFoodRoutine());
    }

    private void OnDisable()
    {
        // Stop spawning when script is disabled
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    private IEnumerator SpawnFoodRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (foodPrefabs != null && foodPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, foodPrefabs.Length);
                Vector3 spawnPos = new Vector3(Random.Range(-xSpawnRange, xSpawnRange), ySpawnPosition, 0f);
                Instantiate(foodPrefabs[randomIndex], spawnPos, Quaternion.identity);
            }
        }
    }
}