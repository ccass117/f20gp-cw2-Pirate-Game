using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TentacleEmerger : MonoBehaviour
{
    public static List<TentacleEmerger> ActiveTentacles = new List<TentacleEmerger>();

    [Header("Position Settings")]
    public float spawnDepth = -5f;   
    public float targetHeight = 0f;   

    [Header("Timing Settings")]
    public float activeTime = 7f;    
    public float retreatDuration = 1f;  

    [Header("Attack Settings")]
    public float attackRange = 8f;    

    [Header("Tornado Attack")]
    public GameObject tornadoPrefab;
    public float tornadoDelay = 1f;
    public float tornadoSpawnOffset = 1f;
    public float tornadoChance = 0.15f;

    
    [Header("Attack Timer")]
    public float minAttackInterval = 5f;
    public float maxAttackInterval = 10f;

    private Animator anim;
    private float attackTimer;
    private float timeAboveWater = 0f;
    private bool retreatTriggered = false;
    private bool isAttacking = false;
    private bool hasRisen = false;  
    private bool isTornadoSpawnPending = false;

    public Transform krakenZone;
    private Transform player;
    private Vector3 initialPosition;

    void Awake()
    {
        ActiveTentacles.Add(this);
    }

    void Start()
    {
        Vector3 pos = transform.position;
        pos.y = spawnDepth;
        transform.position = pos;
        initialPosition = new Vector3(transform.position.x, 0f, transform.position.z);

        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            player = p.transform;
            Debug.Log(gameObject.name + ": Player found - " + player.name);
        }

        ResetAttackTimer();
    }

    void Update()
    {
        if (!hasRisen)
        {
            return;
        }

        if (krakenZone != null)
        {
            //check if the tentacle is outside the kraken zone as it moves, if so, retreat so tentacles keep spawning near the player
            Vector3 tentacleXZ = initialPosition;
            Vector3 zonePosXZ = new Vector3(krakenZone.position.x, 0f, krakenZone.position.z);
            float distanceFromZone = Vector3.Distance(tentacleXZ, zonePosXZ);
            float zoneRadius = krakenZone.GetComponent<KrakenZone>().zoneRadius;
            if (distanceFromZone > zoneRadius)
            {
                if (!retreatTriggered)
                {
                    anim.SetTrigger("RetreatTrigger");
                    retreatTriggered = true;
                }
            }
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        bool inIdle = stateInfo.IsName("Idle");

        //timer for how long the tentacle has been above water
        if (inIdle && !isAttacking && !retreatTriggered)
        {
            timeAboveWater += Time.deltaTime;
        }

        //tentacle attack timer, just picks a random attack from the animator
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange && !isAttacking && !retreatTriggered)
        {
            int attackIndex = Random.Range(0, 5);
            anim.SetInteger("AttackIndex", attackIndex);
            anim.SetTrigger("AttackTrigger");
            isAttacking = true;
        }

        if (timeAboveWater >= activeTime && !retreatTriggered && inIdle)
        {
            anim.SetTrigger("RetreatTrigger");
            retreatTriggered = true;
        }
    }

    void LateUpdate()
    {
        Vector3 currentPos = transform.position;
        transform.position = new Vector3(initialPosition.x, currentPos.y, initialPosition.z);
    }

    //called by animator event
    public void OnRiseComplete()
    {
        hasRisen = true;
    }

    //animator event 2: electric boogaloo
    public void OnAttackComplete()
    {
        isAttacking = false;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (timeAboveWater >= activeTime && stateInfo.IsName("Idle") && !retreatTriggered)
        {
            anim.SetTrigger("RetreatTrigger");
            retreatTriggered = true;
        }

        //if in phase 2, 50% chance to spawn a tornado
        Health parentHealth = transform.root.GetComponent<Health>();
        if (!isTornadoSpawnPending && 
            parentHealth != null && 
            (parentHealth.currentHealth / parentHealth.maxHealth) <= 0.4f &&
            Random.value <= tornadoChance && 
            tornadoPrefab != null)
        {
            StartCoroutine(SpawnTornado());
        }
    }

    //animator event the third
    public void OnRetreatComplete()
    {
        Destroy(gameObject);
    }

    void ResetAttackTimer()
    {
        attackTimer = Random.Range(minAttackInterval, maxAttackInterval);
    }

    //tentacles take damage for this boss; parent has no colldier since its just a spawner, but Health.cs should still be attached to parent
    void OnCollisionEnter(Collision collision)
    {
        Health parentHealth = transform.root.GetComponent<Health>();
        if (parentHealth != null)
        {
            float damageAmount = 5f;
            parentHealth.TakeDamage(damageAmount, gameObject);
        }
    }

    void OnDestroy()
    {
        ActiveTentacles.Remove(this);
    }

    //phase 2 tornado spawn
    private IEnumerator SpawnTornado()
    {
        //don't know why tbh, but multiple tornados can spawn at once if this isn't here
        isTornadoSpawnPending = true; 
        
        yield return new WaitForSeconds(tornadoDelay);
        
        Vector3 spawnPos = transform.position + transform.forward * tornadoSpawnOffset;
        Quaternion spawnRot = transform.rotation;
        
        //make sure tornado is facing and moving the right direction
        GameObject tornadoInstance = Instantiate(tornadoPrefab, spawnPos, spawnRot);
        Tornado tornadoScript = tornadoInstance.GetComponent<Tornado>();
        if (tornadoScript != null)
        {
            tornadoScript.forwardDirection = transform.forward;
        }
        Destroy(tornadoInstance, 5f);

        isTornadoSpawnPending = false;
    }
}