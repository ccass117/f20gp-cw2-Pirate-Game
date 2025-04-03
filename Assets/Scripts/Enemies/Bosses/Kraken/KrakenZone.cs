using UnityEngine;

//kraken, tentacles spawn as children
public class KrakenZone : MonoBehaviour
{
    [Header("Tentacle Spawning")]
    public GameObject tentaclePrefab;
    public float zoneRadius = 8f;       
    public float spawnInterval = 3f;    
    public float spawnDepth = -5f;      
    [Header("Zone Tracking")]
    public float trackingSpeed = 1f;    
    private Transform player;
    private float spawnTimer;

    void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;

        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (player == null)
            return;
        //follow player
        transform.position = Vector3.Lerp(transform.position, player.position, trackingSpeed * Time.deltaTime);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnTentacle();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnTentacle()
    {
        //squids only have 8 tentacles (i love spreading misinformation)
        if (TentacleEmerger.ActiveTentacles.Count >= 8)
            return;

        Vector2 randomPoint = Random.insideUnitCircle * zoneRadius;
        Vector3 spawnPosition = new Vector3(transform.position.x + randomPoint.x, spawnDepth, transform.position.z + randomPoint.y);

        //make sure the tentacle is facing the player
        Vector3 directionToPlayer = (player.position - spawnPosition).normalized;
        Vector3 horizontalDir = new Vector3(directionToPlayer.x, 0f, directionToPlayer.z).normalized;
        Quaternion spawnRotation = Quaternion.LookRotation(horizontalDir);

        GameObject tentacleInstance = Instantiate(tentaclePrefab, spawnPosition, spawnRotation, transform);
    }
}