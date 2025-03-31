using UnityEngine;

public class HydraHeadGenerator : MonoBehaviour
{
    [Header("Head Prefab Settings")]
    [Tooltip("Prefab for the hydra head to spawn.")]
    public GameObject headPrefab;
    [Tooltip("Local offset applied when spawning a new head. Use the X value for horizontal spacing.")]
    public Vector3 headOffset = new Vector3(2f, 0f, 0f);

    [Header("Spawn Timing")]
    [Tooltip("Time interval (in seconds) between new head spawns when health is below 10%.")]
    public float spawnInterval = 1f;

    private Health bossHealth;

    private float spawnTimer = 0f;

    private bool spawnOnLeft = true;

    private bool phase2Triggered = false;

    void Start()
    {
        bossHealth = GetComponent<Health>();
        if (bossHealth == null)
        {
            Debug.LogError("HydraHeadGenerator: No Health component found on the boss parent.");
        }
    }

    void Update()
    {
        if (bossHealth == null)
            return;

        float healthPercentage = bossHealth.GetCurrentHealth() / bossHealth.maxHealth;

        if (healthPercentage < 0.5f && !phase2Triggered)
        {
            SpawnSideHeads();
            phase2Triggered = true;
        }

        if (healthPercentage < 0.1f)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnAlternatingHead();
            }
        }
    }

    void SpawnSideHeads()
    {
        Transform leftMost = null;
        Transform rightMost = null;

        foreach (Transform child in transform)
        {
            if (leftMost == null || child.localPosition.x < leftMost.localPosition.x)
            {
                leftMost = child;
            }
            if (rightMost == null || child.localPosition.x > rightMost.localPosition.x)
            {
                rightMost = child;
            }
        }

        if (leftMost != null)
        {
            Vector3 spawnPos = leftMost.localPosition + new Vector3(-headOffset.x, headOffset.y, headOffset.z);
            GameObject newHead = Instantiate(headPrefab, transform);
            newHead.transform.localPosition = spawnPos;
            Debug.Log("Spawned extra head on the left at " + spawnPos);
        }

        if (rightMost != null)
        {
            Vector3 spawnPos = rightMost.localPosition + new Vector3(headOffset.x, headOffset.y, headOffset.z);
            GameObject newHead = Instantiate(headPrefab, transform);
            newHead.transform.localPosition = spawnPos;
            Debug.Log("Spawned extra head on the right at " + spawnPos);
        }
    }

    void SpawnAlternatingHead()
    {
        Transform leftMost = null;
        Transform rightMost = null;

        foreach (Transform child in transform)
        {
            if (leftMost == null || child.localPosition.x < leftMost.localPosition.x)
            {
                leftMost = child;
            }
            if (rightMost == null || child.localPosition.x > rightMost.localPosition.x)
            {
                rightMost = child;
            }
        }

        if (spawnOnLeft && leftMost != null)
        {
            Vector3 spawnPos = leftMost.localPosition + new Vector3(-headOffset.x, headOffset.y, headOffset.z);
            GameObject newHead = Instantiate(headPrefab, transform);
            newHead.transform.localPosition = spawnPos;
            Debug.Log("Spawned alternating head on the left at " + spawnPos);
        }
        else if (!spawnOnLeft && rightMost != null)
        {
            Vector3 spawnPos = rightMost.localPosition + new Vector3(headOffset.x, headOffset.y, headOffset.z);
            GameObject newHead = Instantiate(headPrefab, transform);
            newHead.transform.localPosition = spawnPos;
            Debug.Log("Spawned alternating head on the right at " + spawnPos);
        }

        spawnOnLeft = !spawnOnLeft;
    }
}