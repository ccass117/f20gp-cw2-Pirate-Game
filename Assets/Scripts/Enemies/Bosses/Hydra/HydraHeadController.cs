using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HydraHeadController : MonoBehaviour
{
    [Header("Attack Timing")]
    public float minAttackInterval = 5f; 
    public float maxAttackInterval = 10f; 
    private float attackTimer;

    [Header("Attack Options Toggles")]
    public bool canAttack0 = true;   
    public bool canAttack1 = true;   
    public bool canAttack2 = true;      

    [Header("Distance Settings")]
    [Tooltip("Distance threshold below which Attack 0 is favored.")]
    public float closeDistanceThreshold = 5f;

    [Header("Attack 1 (Fire Breath) Settings")]
    [Tooltip("Prefab to spawn for Attack 1.")]
    public GameObject attack1Prefab;
    [Tooltip("Transform representing the head's mouth.")]
    public Transform mouthTransform;
    [Tooltip("Scale factor for the spawned Attack 1 prefab.")]
    public float attack1PrefabScale = 1f;
    [Tooltip("Length of the Attack 1 animation (in seconds) for auto-destruction of the spawned prefab.")]
    public float attack1AnimationLength = 3f;

    [Header("Fire Cone Settings")]
    [Tooltip("Full cone angle (in degrees) that defines the fire's spread.")]
    public float fireConeAngle = 30f;
    [Tooltip("Distance (in units) that the fire effect covers.")]
    public float fireConeRange = 5f;
    [Tooltip("Additional horizontal rotation offset (in degrees) for fine-tuning the fire's direction.")]
    public float fireDirectionOffset = 0f;
    [Tooltip("Vertical angle offset (in degrees) to tilt the fire cone (negative tilts downward).")]
    public float fireVerticalAngle = -15f;

    [Header("Attack 2 Settings")]
    [Tooltip("Prefab to spawn for Attack 2.")]
    public GameObject attack2Prefab;
    [Tooltip("Time in seconds used for predicting the player's position.")]
    public float predictionTime = 1f;

    [Header("Tracking Settings")]
    [Tooltip("Speed (in degrees per second) at which the hydra head rotates to track the player.")]
    public float trackingSpeed = 90f;

    private Animator anim;
    private Transform player;

    // NEW: Parameters for moving the head collider during Attack 2.
    [Header("Attack 2 Collider Movement")]
    [Tooltip("How far down (in units) the head should drop during Attack 2.")]
    public float headDropAmount = 2f;
    [Tooltip("Duration (in seconds) for the head drop movement.")]
    public float headDropDuration = 0.5f;
    [Tooltip("Optional delay (in seconds) before the head returns to its original position.")]
    public float headDropHoldTime = 0.3f;

    void Start()
    {
        anim = GetComponent<Animator>();
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;
        else
            Debug.LogWarning(gameObject.name + ": Player not found!");

        ResetAttackTimer();
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            Attack();
            ResetAttackTimer();
        }

        if (player != null)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0f;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, trackingSpeed * Time.deltaTime);
            }
        }
    }

    void Attack()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        Dictionary<int, float> weights = new Dictionary<int, float>();
        if (canAttack0)
            weights[0] = (distance < closeDistanceThreshold) ? 0.7f : 0.2f;
        if (canAttack1)
            weights[1] = (distance < closeDistanceThreshold) ? 0.15f : 0.4f;
        if (canAttack2)
            weights[2] = (distance < closeDistanceThreshold) ? 0.15f : 0.4f;

        if (weights.Count == 0)
        {
            return;
        }

        float totalWeight = 0f;
        foreach (float w in weights.Values)
            totalWeight += w;
        float randomValue = Random.Range(0f, totalWeight);
        int selectedAttack = -1;
        foreach (var pair in weights)
        {
            randomValue -= pair.Value;
            if (randomValue <= 0f)
            {
                selectedAttack = pair.Key;
                break;
            }
        }
        if (selectedAttack == -1)
            selectedAttack = 0;

        if (selectedAttack == 1)
        {
            anim.SetInteger("AttackIndex", 1);
            anim.SetTrigger("AttackTrigger");
        }
        else if (selectedAttack == 2)
        {

            if (attack2Prefab == null)
            {
                Debug.LogWarning(gameObject.name + ": Attack 2 prefab not assigned.");
            }
            else
            {

                anim.SetInteger("AttackIndex", 2);
                anim.SetTrigger("AttackTrigger");
                

                StartCoroutine(MoveHeadColliderDown());

                Vector3 predictedPosition = player.position;
                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                Vector3 predictedVelocity = Vector3.zero;
                if (playerRb != null)
                {
                    // Project the player's linear velocity along its forward vector.
                    predictedVelocity = Vector3.Project(playerRb.linearVelocity, player.forward);
                }
                else
                {
                    predictedVelocity = player.forward * 1.5f;
                }
                predictedPosition += predictedVelocity * predictionTime;

                Quaternion spawnRot;
                if (predictedVelocity != Vector3.zero)
                    spawnRot = Quaternion.LookRotation(predictedVelocity);
                else
                    spawnRot = Quaternion.LookRotation(player.forward);

                Instantiate(attack2Prefab, predictedPosition, spawnRot);
            }
        }
        else
        {

            anim.SetInteger("AttackIndex", 0);
            anim.SetTrigger("AttackTrigger");
        }
    }

    void ResetAttackTimer()
    {
        attackTimer = Random.Range(minAttackInterval, maxAttackInterval);
    }

    public void SpawnAttack1Prefab()
    {
        if (attack1Prefab == null || mouthTransform == null)
        {

            return;
        }
        
        Quaternion baseRot = mouthTransform.rotation;
        Quaternion horizontalOffset = Quaternion.Euler(0, fireDirectionOffset, 0);
        Quaternion verticalOffset = Quaternion.Euler(fireVerticalAngle, 0, 0);
        Quaternion finalRot = verticalOffset * horizontalOffset * baseRot;
        
        GameObject instance = Instantiate(attack1Prefab, mouthTransform.position, finalRot, mouthTransform);
        instance.SetActive(true);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localScale = Vector3.one * attack1PrefabScale;
        
        // Ignore collisions with all colliders in the hydra's hierarchy (the main body).
        IgnoreCollisionsWithHierarchy(instance, transform.root);
        
        
        Destroy(instance, attack1AnimationLength);
    }

    void IgnoreCollisionsWithHierarchy(GameObject child, Transform parentRoot)
    {
        Collider[] childColliders = child.GetComponentsInChildren<Collider>();
        Collider[] parentColliders = parentRoot.GetComponentsInChildren<Collider>();

        foreach (Collider childCol in childColliders)
        {
            foreach (Collider parentCol in parentColliders)
            {
                Physics.IgnoreCollision(childCol, parentCol);
            }
        }
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (mouthTransform != null)
        {
            Gizmos.color = Color.red;
            Vector3 origin = mouthTransform.position;
            
            Quaternion baseRot = mouthTransform.rotation;
            Quaternion horizontalOffset = Quaternion.Euler(0, fireDirectionOffset, 0);
            Quaternion verticalOffset = Quaternion.Euler(fireVerticalAngle, 0, 0);
            Quaternion finalRot = verticalOffset * horizontalOffset * baseRot;
            Vector3 finalDir = finalRot * Vector3.forward;
            
            Gizmos.DrawLine(origin, origin + finalDir * fireConeRange);
            
            float halfAngle = fireConeAngle * 0.5f;
            int segments = 20;
            Vector3 prevPoint = origin + (Quaternion.Euler(0, -halfAngle, 0) * finalDir) * fireConeRange;
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
                Quaternion rot = Quaternion.Euler(0, currentAngle, 0);
                Vector3 nextPoint = origin + (rot * finalDir) * fireConeRange;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
    }
    #endif

    // NEW: Coroutine to move the hydra head (collider) downward during Attack 2
    private IEnumerator MoveHeadColliderDown()
    {
        Vector3 originalPos = transform.localPosition;
        Vector3 targetPos = originalPos - new Vector3(0, headDropAmount, 0);

        float elapsed = 0f;
        // Move downward
        while (elapsed < headDropDuration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPos, targetPos, elapsed / headDropDuration);
            yield return null;
        }
        transform.localPosition = targetPos;
        // Optionally, wait a moment before returning.
        yield return new WaitForSeconds(headDropHoldTime);
        // Return to original position.
        elapsed = 0f;
        while (elapsed < headDropDuration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(targetPos, originalPos, elapsed / headDropDuration);
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}