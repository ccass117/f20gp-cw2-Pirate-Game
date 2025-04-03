using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Head Rising Settings")]
    [Tooltip("Fixed water level (target y) for new heads.")]
    public float waterLevel = -20f;
    [Tooltip("Maximum random offset (in units) to add/subtract from the water level.")]
    public float spawnYOffsetRange = 1f;
    [Tooltip("Rising animation duration (in seconds).")]
    public float riseDuration = 1.5f;

    private Health bossHealth;
    private float spawnTimer = 0f;
    private bool spawnOnLeft = true;
    private bool phase2Triggered = false;

    void Start()
    {
        bossHealth = GetComponent<Health>();
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

    // Instead of averaging children, return a fixed water level plus a small random offset.
    float GetTargetY()
    {
        float targetY = waterLevel + Random.Range(-spawnYOffsetRange, spawnYOffsetRange);
        return targetY;
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

        float targetY = GetTargetY();

        // Left side spawn
        if (leftMost != null)
        {
            Vector3 spawnPos = leftMost.localPosition + new Vector3(-headOffset.x, 0f, headOffset.z);
            spawnPos.y = targetY;
            GameObject newHead = Instantiate(headPrefab, transform);
            Vector3 initialSpawnPos = spawnPos;
            initialSpawnPos.y = -60f; // Start lower so it rises up
            newHead.transform.localPosition = initialSpawnPos;
            StartCoroutine(AnimateHead(newHead, targetY, riseDuration));
        }

        // Right side spawn
        if (rightMost != null)
        {
            Vector3 spawnPos = rightMost.localPosition + new Vector3(headOffset.x, 0f, headOffset.z);
            spawnPos.y = targetY;
            GameObject newHead = Instantiate(headPrefab, transform);
            Vector3 initialSpawnPos = spawnPos;
            initialSpawnPos.y = -60f;
            newHead.transform.localPosition = initialSpawnPos;
            StartCoroutine(AnimateHead(newHead, targetY, riseDuration));
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

        float targetY = GetTargetY();

        if (spawnOnLeft && leftMost != null)
        {
            Vector3 spawnPos = leftMost.localPosition + new Vector3(-headOffset.x, 0f, headOffset.z);
            spawnPos.y = targetY;
            GameObject newHead = Instantiate(headPrefab, transform);
            Vector3 initialSpawnPos = spawnPos;
            initialSpawnPos.y = -60f;
            newHead.transform.localPosition = initialSpawnPos;
            StartCoroutine(AnimateHead(newHead, targetY, riseDuration));
        }
        else if (!spawnOnLeft && rightMost != null)
        {
            Vector3 spawnPos = rightMost.localPosition + new Vector3(headOffset.x, 0f, headOffset.z);
            spawnPos.y = targetY;
            GameObject newHead = Instantiate(headPrefab, transform);
            Vector3 initialSpawnPos = spawnPos;
            initialSpawnPos.y = -60f;
            newHead.transform.localPosition = initialSpawnPos;
            StartCoroutine(AnimateHead(newHead, targetY, riseDuration));
        }

        spawnOnLeft = !spawnOnLeft;
    }

    private IEnumerator AnimateHead(GameObject head, float targetY, float duration)
    {
        // Temporarily disable physics interference.
        Rigidbody rb = head.GetComponent<Rigidbody>();
        bool wasKinematic = false;
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }

        float elapsed = 0f;
        Vector3 startPos = head.transform.localPosition;
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newY = Mathf.Lerp(startPos.y, targetY, elapsed / duration);
            head.transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
            yield return null;
        }
        head.transform.localPosition = endPos;

        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }
    }
}