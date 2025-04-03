using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//spawns hydra heads when low health, alternating left and right sides
public class HydraHeadGenerator : MonoBehaviour
{
    [Header("Head Prefab Settings")]
    public GameObject headPrefab;
    public Vector3 headOffset = new Vector3(2f, 0f, 0f);

    [Header("Spawn Timing")]
    public float spawnInterval = 1f;

    [Header("Head Rising Settings")]
    public float waterLevel = -20f;
    public float spawnYOffsetRange = 1f;
    public float riseDuration = 1.5f;

    private Health bossHealth;
    private float spawnTimer = 0f;
    private bool spawnOnLeft = true;
    private bool phase2 = false;

    void Start()
    {
        bossHealth = GetComponent<Health>();
    }

    void Update()
    {
        if (bossHealth == null)
            return;

        //phase 2: spawn 2 more heads
        float healthPercentage = bossHealth.GetCurrentHealth() / bossHealth.maxHealth;
        if (healthPercentage < 0.5f && !phase2)
        {
            SpawnHeads();
            phase2 = true;
        }

        //phase 3? kinda, spawn a new head every second, good luck
        if (healthPercentage < 0.1f)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                GoodLuck();
            }
        }
    }

    //getter for y pos
    //adds a little variation to head spawn height, just makes it look a bit better
    float GetTargetY()
    {
        float targetY = waterLevel + Random.Range(-spawnYOffsetRange, spawnYOffsetRange);
        return targetY;
    }

    void SpawnHeads()
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


        //alternate spawn side between left and rightmost heads
        if (leftMost != null)
        {
            Vector3 spawnPos = leftMost.localPosition + new Vector3(-headOffset.x, 0f, headOffset.z);
            spawnPos.y = targetY;
            GameObject newHead = Instantiate(headPrefab, transform);
            Vector3 initialSpawnPos = spawnPos;
            //spawn way under the map so it rises up
            initialSpawnPos.y = -60f;
            newHead.transform.localPosition = initialSpawnPos;
            StartCoroutine(InstantiateHead(newHead, targetY, riseDuration));
        }

        if (rightMost != null)
        {
            Vector3 spawnPos = rightMost.localPosition + new Vector3(headOffset.x, 0f, headOffset.z);
            spawnPos.y = targetY;
            GameObject newHead = Instantiate(headPrefab, transform);
            Vector3 initialSpawnPos = spawnPos;
            initialSpawnPos.y = -60f;
            newHead.transform.localPosition = initialSpawnPos;
            StartCoroutine(InstantiateHead(newHead, targetY, riseDuration));
        }
    }

    //spawns heads forever till the boss dies
    void GoodLuck()
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
            StartCoroutine(InstantiateHead(newHead, targetY, riseDuration));
        }
        else if (!spawnOnLeft && rightMost != null)
        {
            Vector3 spawnPos = rightMost.localPosition + new Vector3(headOffset.x, 0f, headOffset.z);
            spawnPos.y = targetY;
            GameObject newHead = Instantiate(headPrefab, transform);
            Vector3 initialSpawnPos = spawnPos;
            initialSpawnPos.y = -60f;
            newHead.transform.localPosition = initialSpawnPos;
            StartCoroutine(InstantiateHead(newHead, targetY, riseDuration));
        }

        spawnOnLeft = !spawnOnLeft;
    }

    //head spawner
    private IEnumerator InstantiateHead(GameObject head, float targetY, float duration)
    {
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