using UnityEngine;
using System.Collections;

public class MeteorSpawner : MonoBehaviour
{
    public GameObject meteorPrefab; // Reference to the meteor prefab
    GameObject player; // Reference to the player
    public float minSpawnDelay = 2f; // Minimum time between meteor spawns
    public float maxSpawnDelay = 5f; // Maximum time between meteor spawns
    public int clusterCount = 1; // Number of meteors spawning at once
    AudioSource MeteorFall;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");

        if (BuffController.registerBuff("Big Umbrella", "Lower the chance for a meteor to spawn")) { minSpawnDelay = 4f; }

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

        StartCoroutine(SpawnMeteorsCoroutine());
    }

    private IEnumerator SpawnMeteorsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
            SpawnMeteor();
        }
    }

    private void SpawnMeteor()
    {
        if (MeteorFall != null)
        {
            MeteorFall.Play();
        }

        for (int i = 0; i < clusterCount; i++)
        {
            Vector3 spawnPosition = new Vector3(
                player.transform.position.x + Random.Range(-7.5f, 7.5f),
                player.transform.position.y,
                player.transform.position.z + Random.Range(-7.5f, 7.5f)
            );

            Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
