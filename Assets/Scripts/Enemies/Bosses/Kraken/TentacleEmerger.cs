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

    private Animator anim;
    private float timeAboveWater = 0f;
    private bool retreatTriggered = false;
    private bool isAttacking = false;
    private bool hasRisen = false;  

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
    }

    public void OnRetreatComplete()
    {
        Destroy(gameObject);
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
}