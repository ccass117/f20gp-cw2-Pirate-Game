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

    [Header("Tornado Attack Settings")]
    public GameObject tornadoPrefab;
    public float tornadoDelay = 1f;
    [Tooltip("Distance in front of the tentacle from which the tornado is spawned.")]
    public float tornadoSpawnOffset = 1f;
    [Tooltip("Chance (0 to 1) that an attack will spawn a tornado when health is low.")]
    public float tornadoChance = 0.15f;

    
    [Header("Attack Timer Settings")]
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
        if (anim == null)
            Debug.LogWarning(gameObject.name + ": Animator component not found!");

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                Debug.Log(gameObject.name + ": Player found - " + player.name);
            }
            else
                Debug.LogWarning(gameObject.name + ": Player not found!");
        }

        ResetAttackTimer();
    }

    void Update()
    {
        if (!hasRisen)
        {
            Debug.Log(gameObject.name + ": Rise not complete yet; waiting.");
            return;
        }

        if (krakenZone != null)
        {
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
                    Debug.Log(gameObject.name + ": Left KrakenZone (distance " + distanceFromZone + "), triggering retreat.");
                }
            }
        }

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        bool inIdle = stateInfo.IsName("Idle");

        if (inIdle && !isAttacking && !retreatTriggered)
        {
            timeAboveWater += Time.deltaTime;
            Debug.Log(gameObject.name + ": Time above water: " + timeAboveWater);
        }

        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange && !isAttacking && !retreatTriggered)
        {
            int attackIndex = Random.Range(0, 5);
            anim.SetInteger("AttackIndex", attackIndex);
            anim.SetTrigger("AttackTrigger");
            isAttacking = true;
            Debug.Log(gameObject.name + ": Attack triggered with AttackIndex " + attackIndex);
        }

        if (timeAboveWater >= activeTime && !retreatTriggered && inIdle)
        {
            anim.SetTrigger("RetreatTrigger");
            retreatTriggered = true;
            Debug.Log(gameObject.name + ": Retreat triggered after " + timeAboveWater + " seconds above water.");
        }
    }

    void LateUpdate()
    {
        Vector3 currentPos = transform.position;
        transform.position = new Vector3(initialPosition.x, currentPos.y, initialPosition.z);
    }

    public void OnRiseComplete()
    {
        hasRisen = true;
    }


    public void OnAttackComplete()
    {
        isAttacking = false;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (timeAboveWater >= activeTime && stateInfo.IsName("Idle") && !retreatTriggered)
        {
            anim.SetTrigger("RetreatTrigger");
            retreatTriggered = true;
        }

        // --- NEW: Check for tornado attack ---
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

    public void OnRetreatComplete()
    {
        Destroy(gameObject);
    }

    void ResetAttackTimer()
    {
        attackTimer = Random.Range(minAttackInterval, maxAttackInterval);
    }

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

    private IEnumerator SpawnTornado()
    {
        isTornadoSpawnPending = true; // Lock spawning
        
        yield return new WaitForSeconds(tornadoDelay);
        
        // Calculate spawn position/rotation
        Vector3 spawnPos = transform.position + transform.forward * tornadoSpawnOffset;
        Quaternion spawnRot = transform.rotation;
        
        // Spawn tornado
        GameObject tornadoInstance = Instantiate(tornadoPrefab, spawnPos, spawnRot);
        Tornado tornadoScript = tornadoInstance.GetComponent<Tornado>();
        if (tornadoScript != null)
        {
            tornadoScript.forwardDirection = transform.forward;
        }
        Destroy(tornadoInstance, 5f);

        isTornadoSpawnPending = false; // Release lock
    }


}