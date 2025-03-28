using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    public GameObject meteorPrefab; // Reference to the meteor prefab
    GameObject player; // Reference to the player
    public float minSpawnDelay = 2f; // Minimum time between meteor spawns
    public float maxSpawnDelay = 5f; // Maximum time between meteor spawns
    public int clusterCount = 1; //number of meteors spawning at once
    AudioSource MeteorFall;


    private void Start()
    {
        player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player object has the 'Player' tag.");
            return;
            
        }
        MeteorFall = GetComponent<AudioSource>();
        if (MeteorFall == null)
        {
            Debug.LogError("No meteor fall audiosource found");
        }
        InvokeRepeating("SpawnMeteor", 0f, Random.Range(minSpawnDelay, maxSpawnDelay));
    }

    private void SpawnMeteor()
    {
        MeteorFall.Play();
        for (int i = 0; i < clusterCount; i++)
        {
            // Calculate a random spawn position based on the player's position
            Vector3 spawnPosition = new Vector3(
                player.transform.position.x + Random.Range(-7.5f, 7.5f),
                player.transform.position.y + 0f,
                player.transform.position.z + Random.Range(-7.5f, 7.5f)
            );

            // Instantiate the meteor prefab at the random spawn position
            GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
        }
    }
}