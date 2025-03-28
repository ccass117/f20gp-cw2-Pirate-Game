using UnityEngine;

public class KrakenZone : MonoBehaviour
{
    [Header("Tentacle Spawning")]
    public GameObject tentaclePrefab;
    public float zoneRadius = 8f;       // Slightly smaller spawn radius.
    public float spawnInterval = 3f;    // Time between spawn attempts.
    public float spawnDepth = -5f;      // Y position for tentacle spawn (underwater).

    [Header("Zone Tracking")]
    public float trackingSpeed = 1f;    // How quickly the zone moves toward the player.

    [Header("Optional Shadow")]
    public GameObject shadowPrefab;     // Optional: assign a shadow effect prefab.

    private Transform player;
    private float spawnTimer;
    private GameObject shadowInstance;

    void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;
        else
            Debug.LogWarning("KrakenZone: Player not found!");

        spawnTimer = spawnInterval;

        if (shadowPrefab != null)
        {
            // Instantiate the shadow as a child.
            shadowInstance = Instantiate(shadowPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    void Update()
    {
        if (player == null)
            return;

        // Smoothly track the player's position.
        transform.position = Vector3.Lerp(transform.position, player.position, trackingSpeed * Time.deltaTime);

        if (shadowInstance != null)
        {
            // Place the shadow at water surface (assumed Y = 0) at the same XZ as the zone.
            Vector3 shadowPos = transform.position;
            shadowPos.y = 0f;
            shadowInstance.transform.position = shadowPos;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnTentacle();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnTentacle()
    {
        // Only spawn if fewer than 8 tentacles are active.
        if (TentacleEmerger.ActiveTentacles.Count >= 8)
            return;

        // Choose a random point within a circle of radius zoneRadius around the zone's position.
        Vector2 randomPoint = Random.insideUnitCircle * zoneRadius;
        Vector3 spawnPosition = new Vector3(transform.position.x + randomPoint.x, spawnDepth, transform.position.z + randomPoint.y);

        // Compute horizontal direction from spawn position to the player.
        Vector3 directionToPlayer = (player.position - spawnPosition).normalized;
        Vector3 horizontalDir = new Vector3(directionToPlayer.x, 0f, directionToPlayer.z).normalized;
        Quaternion spawnRotation = Quaternion.LookRotation(horizontalDir);

        // Instantiate the tentacle as a child of this KrakenZone.
        GameObject tentacleInstance = Instantiate(tentaclePrefab, spawnPosition, spawnRotation, transform);
    }
}