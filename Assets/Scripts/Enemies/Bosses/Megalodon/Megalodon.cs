using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Megalodon : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;

    [Header("Circling Behavior")]
    public float circleRadius = 15f;
    public float circlingSpeed = 1f;
    [SerializeField] private float movementSmoothTime = 0.3f;
    [SerializeField] private float maxRotationSpeed = 270f;
    [SerializeField] private float minMovementForRotation = 0.1f;

    [Header("Attack Behavior")]
    public float attackInterval = 10f;
    public float attackDuration = 3f;
    public float normalSpeed = 4f;
    public float attackSpeed = 10f;
    public float predictionTime = 1f;
    public float baseOvershootMultiplier = 1.2f;

    [Header("Phase 2 Behavior")]
    public float phase2Threshold = 0.5f;
    public float phase2AttackInterval = 6f;
    public float phase2AttackSpeed = 15f;
    public int phase2AttackChainCount = 2;
    public float chainAttackDelay = 1f;
    public float phase2OvershootMultiplier = 1.5f;

    [Header("Orbit Variation")]
    public float radiusVariation = 3f;
    public float radiusChangeInterval = 2f;
    public float angleVariation = 30f;
    public float patternChangeInterval = 4f;

    [Header("Lateral Movement")]
    public float lateralAmplitude = 2f;
    public float lateralFrequency = 0.5f;

    [Header("Dynamic Speed")]
    public float speedVariation = 2f;
    public float minFollowDistance = 8f;
    public float maxFollowDistance = 20f;

    [Header("Bounce/Recovery Settings")]
    public float bounceDistance = 10f;
    public float recoveryDuration = 2f;

    [Header("Model Settings")]
    public Vector3 rotationOffset = new Vector3(0, 90, 0);

    [Header("Vertical Motion Settings")]
    public float bobbingAmplitude = 0.5f;
    public float bobbingFrequency = 1f;
    public float attackLift = 2f;
    public float verticalSmoothSpeed = 5f;

    [Header("Components & Settings")]
    [SerializeField] private Collider mainBodyCollider;
    [SerializeField] private Collider attackHitbox;
    [SerializeField] private float positionSyncThreshold = 0.1f;
    [SerializeField] private Transform bodyChild;
    [SerializeField] private Health bodyHealth;

    //runtime variables
    private NavMeshAgent agent;
    private float attackTimer;
    private float attackTimeRemaining;
    public bool isAttacking { get; private set; }
    private bool phase2Active;
    private int remainingChainAttacks;
    private float originalAttackInterval;
    private float originalAttackSpeed;
    private Vector3 lastFramePosition;
    private bool isRecovering = false;
    private Vector3 recoveryTarget;
    private float originalGroundY;
    private Vector3 currentVelocity;
    private float currentOrbitAngle;
    private Vector3 smoothedMovementDirection;
    private float currentRadius;
    private float baseCirclingSpeed;
    private float patternTimer;
    private float radiusTimer;
    private Vector3 orbitOffset;
    private float lateralOffset;
    private Vector3 attackTargetPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        if (!mainBodyCollider) mainBodyCollider = GetComponentInChildren<Collider>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        //setup initial values
        attackHitbox.enabled = false;
        originalAttackInterval = attackInterval;
        originalAttackSpeed = attackSpeed;
        attackTimer = attackInterval;
        agent.speed = normalSpeed;
        originalGroundY = transform.position.y;
        lastFramePosition = transform.position;

        currentRadius = circleRadius;
        baseCirclingSpeed = circlingSpeed;
        //generate initial movement pattern
        NewPattern();
    }

    void Update()
    {
        //for when the megalodon has hit the player so that it backs off
        if (isRecovering)
        {
            Recover();
            return;
        }

        //gotta do this separately here than in Health.cs because megalodon body and head are separate children, and only the body takes damage
        if (bodyHealth != null && bodyHealth.GetCurrentHealth() <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!phase2Active && (bodyHealth.GetCurrentHealth() / bodyHealth.maxHealth) <= phase2Threshold)
        {
            EnterPhase2();
        }

        VerticalMovement();
        SyncBodyPosition();

        if (!isAttacking)
        {
            Circle();
            UpdatePattern();
            attackTimer -= Time.deltaTime;
            
            if (attackTimer <= 0f)
            {
                StartAttack();
            }
        }
        else
        {
            AttackBehaviour();
        }
    }

    //get a new pattern for the megalodon to follow, variation is cool!
    void UpdatePattern()
    {
        patternTimer -= Time.deltaTime;
        radiusTimer -= Time.deltaTime;

        if (patternTimer <= 0)
        {
            NewPattern();
            patternTimer = patternChangeInterval + Random.Range(-1f, 1f);
        }

        //randomly change orbit radius
        if (radiusTimer <= 0)
        {
            currentRadius = Mathf.Clamp(
                circleRadius + Random.Range(-radiusVariation, radiusVariation),
                minFollowDistance,
                maxFollowDistance
            );
            radiusTimer = radiusChangeInterval;
        }
    }

    //makes movement patterns a little less predictable and fun
    void NewPattern()
    {
        //randomise orbit radius and speed
        orbitOffset = new Vector3(
            Random.Range(-angleVariation, angleVariation),
            0,
            Random.Range(-angleVariation, angleVariation)
        );

        lateralFrequency = Random.Range(0.3f, 0.8f);
        lateralAmplitude = Random.Range(1f, 3f);

        if (Random.value > 0.8f)
        {
            circlingSpeed = baseCirclingSpeed * Random.Range(0.8f, 1.2f) * (Random.value > 0.5f ? 1 : -1);
        }
    }

    //circle the player like a shark
    void Circle()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        agent.speed = Mathf.Lerp(
            normalSpeed + speedVariation,
            normalSpeed - speedVariation,
            Mathf.InverseLerp(minFollowDistance, maxFollowDistance, distanceToPlayer)
        );

        //calculate the new orbit angle based on circling speed and time, current values on the prefab should not be changed, they are quite specifically set so that the angle is correct when orbiting
        Vector3 baseOrbitPos = player.position + 
            (transform.position - player.position).normalized * currentRadius;

        lateralOffset = Mathf.Sin(Time.time * lateralFrequency) * lateralAmplitude;
        Vector3 lateralDirection = Vector3.Cross(Vector3.up, (player.position - transform.position).normalized);
        Vector3 variedOrbitPos = baseOrbitPos + lateralDirection * lateralOffset + orbitOffset;

        //smoooooooooth movement
        Vector3 newPos = Vector3.SmoothDamp(
            transform.position,
            variedOrbitPos,
            ref currentVelocity,
            movementSmoothTime,
            agent.speed
        );

        //actual movement direction, since we are using SmoothDamp and there is outside interference from paths, actual movement direction is not the same as calculated direction
        //so we need to calculate the actual movement direction and use that for rotation
        Vector3 actualMovement = (newPos - transform.position).normalized;
        if (actualMovement.sqrMagnitude > 0.001f)
        {
            smoothedMovementDirection = Vector3.Slerp(
                smoothedMovementDirection,
                actualMovement,
                Time.deltaTime * 15f
            );
        }

        //rotate to face movement direction
        if (currentVelocity.magnitude > minMovementForRotation)
        {
            Quaternion targetRotation = Quaternion.LookRotation(smoothedMovementDirection) * 
                                       Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * Mathf.Clamp(currentVelocity.magnitude * 2f, 5f, 15f)
            );
        }

        newPos.y = transform.position.y;
        transform.position = newPos;
    }

    void AttackBehaviour()
    {
        //attack cooldown
        attackTimeRemaining -= Time.deltaTime;
        
        //work out where the megalodon should be moving to when it attacks
        Vector3 newPos = Vector3.MoveTowards(
            transform.position, 
            attackTargetPosition, 
            attackSpeed * Time.deltaTime
        );
        
        newPos.y = transform.position.y;
        transform.position = newPos;

        //rotate to face the attack position
        Vector3 moveDir = (attackTargetPosition - transform.position).normalized;
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir) * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                maxRotationSpeed * Time.deltaTime
            );
        }

        if (Vector3.Distance(transform.position, attackTargetPosition) < 1f || attackTimeRemaining <= 0f)
        {
            EndAttack();
        }
    }

    //makes the megalodon bob up and down, looks pretty!
    void VerticalMovement()
    {
        float targetY = originalGroundY + (isAttacking ? attackLift : bobbingAmplitude * Mathf.Sin(Time.time * bobbingFrequency));
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * verticalSmoothSpeed);
        transform.position = pos;
    }

    //since megalodon has two separate children for head and body, and rigidbody is on both, we need to sync the position of the body with the head to make sure it doesn't look weird when moving
    //ideally, we shouldn't have to do this but it lets things move together nicely
    void SyncBodyPosition()
    {
        if (Vector3.Distance(transform.position, lastFramePosition) > positionSyncThreshold && bodyChild != null)
        {
            bodyChild.localPosition = Vector3.zero;
            lastFramePosition = transform.position;
        }
    }

    //getter for player velocity
    Vector3 GetPlayerVelocity()
    {
        if (player == null) return Vector3.zero;
        
        if (player.TryGetComponent<Rigidbody>(out Rigidbody rb))
            return rb.linearVelocity;
        
        return Vector3.zero;
    }

    void StartAttack()
    {
        //attack hitbox is enabled separately so the head doesn't do damage when it's not supposed to
        isAttacking = true;
        attackTimeRemaining = attackDuration;
        agent.speed = phase2Active ? phase2AttackSpeed : attackSpeed;
        attackHitbox.enabled = true;
        
        //predict player pos and charge
        Vector3 playerVelocity = GetPlayerVelocity();
        Vector3 predictedPosition = player.position + playerVelocity * predictionTime;
        Vector3 attackDirection = (predictedPosition - transform.position).normalized;
        attackTargetPosition = predictedPosition + attackDirection * 
            (phase2Active ? phase2OvershootMultiplier : baseOvershootMultiplier);

        Quaternion immediateRotation = Quaternion.LookRotation(attackDirection) * Quaternion.Euler(rotationOffset);
        transform.rotation = immediateRotation;
    }

    //after an attack, the megalodon will back off a bit and then return to circling the player
    void EndAttack()
    {
        if (phase2Active && remainingChainAttacks > 0)
        {
            remainingChainAttacks--;
            attackTimer = chainAttackDelay;
            StartAttack();
        }
        else
        {
            attackHitbox.enabled = false;
            agent.speed = normalSpeed;
            remainingChainAttacks = phase2AttackChainCount;
            isAttacking = false;
            
            StartRecovery();
            
            attackTimer = phase2Active ? phase2AttackInterval : originalAttackInterval;
        }
    }

    //attacks become faster and chain together
    void EnterPhase2()
    {
        phase2Active = true;
        attackInterval = phase2AttackInterval;
        attackSpeed = phase2AttackSpeed;
        remainingChainAttacks = phase2AttackChainCount;
        circleRadius *= 0.8f;
        minFollowDistance *= 0.7f;
        if (!isAttacking) StartAttack();
    }

    //recovery is when the megalodon backs off after an attack, and it will move back to the original position
    void Recover()
    {
        transform.position = Vector3.Lerp(transform.position, recoveryTarget, Time.deltaTime / recoveryDuration);
        Quaternion desiredRot = Quaternion.LookRotation((transform.position - player.position).normalized) * Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * 10f);
        if (Vector3.Distance(transform.position, recoveryTarget) < 0.5f)
        {
            isRecovering = false;
            attackTimer = originalAttackInterval;
        }
    }

    //pass off collisions to Health.cs
    void OnCollisionEnter(Collision collision)
    {
        if (bodyHealth != null)
            bodyHealth.OnCollisionEnter(collision);
    }

    void StartRecovery()
    {
        if (isRecovering) return;
        isRecovering = true;
        
        Vector3 awayDirection = (transform.position - player.position).normalized;
        recoveryTarget = transform.position + awayDirection * bounceDistance;
        
        Quaternion desiredRot = Quaternion.LookRotation(awayDirection) * Quaternion.Euler(rotationOffset);
        transform.rotation = desiredRot;
        
        StartCoroutine(RecoveryBounce());
    }

    //this is the coroutine that lets the shark "bounce" off after attacking
    IEnumerator RecoveryBounce()
    {
        float timer = 0;
        Vector3 startPos = transform.position;
        
        Vector3 bounceEndPos = recoveryTarget + Vector3.up * 2f; 
        
        while (timer < recoveryDuration)
        {
            timer += Time.deltaTime;
            Vector3 arcPos = Vector3.Lerp(startPos, bounceEndPos, timer/recoveryDuration);
            arcPos.y += Mathf.Sin(timer/recoveryDuration * Mathf.PI) * 3f;
            transform.position = arcPos;
            yield return null;
        }
        
        NewPattern();
        Vector3 toPlayer = transform.position - player.position;
        currentOrbitAngle = Mathf.Atan2(toPlayer.z, toPlayer.x);
        
        isRecovering = false;
        attackTimer = phase2Active ? phase2AttackInterval : originalAttackInterval;
    }
}