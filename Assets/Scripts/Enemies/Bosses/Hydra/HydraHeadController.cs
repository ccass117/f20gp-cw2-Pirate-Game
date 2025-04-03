using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//controller for a single hydra head, headgenerator just instantiates prefabs with this script attached
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
    public float closeDistanceThreshold = 5f;

    [Header("Attack 1 (Fire Breath) Settings")]
    public GameObject attack1Prefab;
    public Transform mouthTransform;
    public float attack1PrefabScale = 1f;
    public float attack1AnimationLength = 3f;

    //animation speed is set to 0.3 for this so it can be outrun
    [Header("Fire Cone Settings")]
    public float fireConeAngle = 30f;
    public float fireConeRange = 5f;
    public float fireDirectionOffset = 0f;
    public float fireVerticalAngle = -15f;

    [Header("Attack 2 Settings")]
    public GameObject attack2Prefab;
    public float predictionTime = 1f;

    [Header("Tracking Settings")]
    public float trackingSpeed = 90f;

    private Animator anim;
    private Transform player;

    //drop the collider so it actually hits the player; looks fine in 2.5D
    [Header("Attack 2 Collider Movement")]
    public float headDropAmount = 2f;
    public float headDropDuration = 0.5f;
    public float headDropHoldTime = 0.3f;

    void Start()
    {
        anim = GetComponent<Animator>();
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        player = playerGO.transform;

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

        //track the player
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

    //randomly select one of three attacks, probability weighted based on distance
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

        //add up all the weights and select a random attack based on the weights
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

        //
        if (selectedAttack == 1)
        {
            anim.SetInteger("AttackIndex", 1);
            anim.SetTrigger("AttackTrigger");
        }
        else if (selectedAttack == 2)
        {
            anim.SetInteger("AttackIndex", 2);
            anim.SetTrigger("AttackTrigger");
            Vector3 predictedPosition = player.position;
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            Vector3 predictedVelocity = Vector3.zero;
            //predict player pos for fireball
            if (playerRb != null)
            {
                predictedVelocity = Vector3.Project(playerRb.linearVelocity, player.forward);
            }
            else
            {
                predictedVelocity = player.forward * 1.5f;
            }
            predictedPosition += predictedVelocity * predictionTime;
            //account for height difference
            Quaternion spawnRot;
            if (predictedVelocity != Vector3.zero)
                spawnRot = Quaternion.LookRotation(predictedVelocity);
            else
                spawnRot = Quaternion.LookRotation(player.forward);

            Instantiate(attack2Prefab, predictedPosition, spawnRot);
        }
        else
        {
            anim.SetInteger("AttackIndex", 0);
            anim.SetTrigger("AttackTrigger");
            StartCoroutine(MoveHeadColliderDown());
        }
    }

    void ResetAttackTimer()
    {
        attackTimer = Random.Range(minAttackInterval, maxAttackInterval);
    }

    public void FireBreath()
    {       
        //fire breath spawns at mouth transform and moves with head, it has its own colliders and damage, just uses Health.cs like everything else
        Quaternion baseRot = mouthTransform.rotation;
        Quaternion horizontalOffset = Quaternion.Euler(0, fireDirectionOffset, 0);
        Quaternion verticalOffset = Quaternion.Euler(fireVerticalAngle, 0, 0);
        Quaternion finalRot = verticalOffset * horizontalOffset * baseRot;
        
        GameObject instance = Instantiate(attack1Prefab, mouthTransform.position, finalRot, mouthTransform);
        instance.SetActive(true);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localScale = Vector3.one * attack1PrefabScale;
        
        //stops hydra from killing itself
        IgnoreCollisions(instance, transform.root);
        
        
        Destroy(instance, attack1AnimationLength);
    }

    void IgnoreCollisions(GameObject child, Transform parentRoot)
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

    //make sure the bite actually hits the player, fine for 2.5D but definitely not the best way of doing this in any other dimensions lol
    private IEnumerator MoveHeadColliderDown()
    {
        Vector3 originalPos = transform.localPosition;
        Vector3 targetPos = originalPos - new Vector3(0, headDropAmount, 0);

        float elapsed = 0f;
        while (elapsed < headDropDuration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPos, targetPos, elapsed / headDropDuration);
            yield return null;
        }
        transform.localPosition = targetPos;
        yield return new WaitForSeconds(headDropHoldTime);
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