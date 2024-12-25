using UnityEngine;

public class RandomPotionSpawner : MonoBehaviour
{
    public GameObject potionPrefab; 
    public int numberOfPotions = 10; 
    public Vector3 spawnAreaSize = new Vector3(20, 0, 20); 
    public Vector3 spawnAreaCenter = Vector3.zero; 
    public float groundY = 0f;
    void Start()
    {
        SpawnPotions();
    }

    void SpawnPotions()
    {
        for (int i = 0; i < numberOfPotions; i++)
        {
            Vector3 randomPosition = GetRandomPosition();
            Instantiate(potionPrefab, randomPosition, Quaternion.identity);
        }
    }

    Vector3 GetRandomPosition()
    {
        float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
        return new Vector3(randomX + spawnAreaCenter.x, groundY, randomZ + spawnAreaCenter.z);
    }
}
