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
    [SerializeField] private float rotationSmoothTime = 0.1f;
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
    private float rotationVelocity;
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

        attackHitbox.enabled = false;
        originalAttackInterval = attackInterval;
        originalAttackSpeed = attackSpeed;
        attackTimer = attackInterval;
        agent.speed = normalSpeed;
        originalGroundY = transform.position.y;
        lastFramePosition = transform.position;

        currentRadius = circleRadius;
        baseCirclingSpeed = circlingSpeed;
        GenerateNewPattern();
    }

    void Update()
    {
        if (isRecovering)
        {
            HandleRecovery();
            return;
        }

        if (bodyHealth != null && bodyHealth.GetCurrentHealth() <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!phase2Active && (bodyHealth.GetCurrentHealth() / bodyHealth.maxHealth) <= phase2Threshold)
        {
            EnterPhase2();
        }

        HandleVerticalMovement();
        SyncBodyPosition();

        if (!isAttacking)
        {
            HandleCirclingBehavior();
            HandlePatternUpdates();
            attackTimer -= Time.deltaTime;
            
            if (attackTimer <= 0f)
            {
                StartAttack();
            }
        }
        else
        {
            HandleAttackBehavior();
        }
    }

    void HandlePatternUpdates()
    {
        patternTimer -= Time.deltaTime;
        radiusTimer -= Time.deltaTime;

        if (patternTimer <= 0)
        {
            GenerateNewPattern();
            patternTimer = patternChangeInterval + Random.Range(-1f, 1f);
        }

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

    void GenerateNewPattern()
    {
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

    void HandleCirclingBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        agent.speed = Mathf.Lerp(
            normalSpeed + speedVariation,
            normalSpeed - speedVariation,
            Mathf.InverseLerp(minFollowDistance, maxFollowDistance, distanceToPlayer)
        );

        Vector3 baseOrbitPos = player.position + 
            (transform.position - player.position).normalized * currentRadius;

        lateralOffset = Mathf.Sin(Time.time * lateralFrequency) * lateralAmplitude;
        Vector3 lateralDirection = Vector3.Cross(Vector3.up, (player.position - transform.position).normalized);
        Vector3 variedOrbitPos = baseOrbitPos + lateralDirection * lateralOffset + orbitOffset;

        Vector3 newPos = Vector3.SmoothDamp(
            transform.position,
            variedOrbitPos,
            ref currentVelocity,
            movementSmoothTime,
            agent.speed
        );

        Vector3 actualMovement = (newPos - transform.position).normalized;
        if (actualMovement.sqrMagnitude > 0.001f)
        {
            smoothedMovementDirection = Vector3.Slerp(
                smoothedMovementDirection,
                actualMovement,
                Time.deltaTime * 15f
            );
        }

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

    void HandleAttackBehavior()
    {
        attackTimeRemaining -= Time.deltaTime;
        
        // Move towards the precalculated attack target
        Vector3 newPos = Vector3.MoveTowards(
            transform.position, 
            attackTargetPosition, 
            attackSpeed * Time.deltaTime
        );
        
        // Maintain vertical position
        newPos.y = transform.position.y;
        transform.position = newPos;

        // Calculate ideal facing direction
        Vector3 moveDir = (attackTargetPosition - transform.position).normalized;
        if (moveDir.sqrMagnitude > 0.001f)
        {
            // Use faster rotation during attack
            Quaternion targetRotation = Quaternion.LookRotation(moveDir) * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                maxRotationSpeed * Time.deltaTime
            );
        }

        // End attack early if we reach target
        if (Vector3.Distance(transform.position, attackTargetPosition) < 1f || attackTimeRemaining <= 0f)
        {
            EndAttack();
        }
    }

    void HandleVerticalMovement()
    {
        float targetY = originalGroundY + (isAttacking ? attackLift : bobbingAmplitude * Mathf.Sin(Time.time * bobbingFrequency));
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * verticalSmoothSpeed);
        transform.position = pos;
    }

    void SyncBodyPosition()
    {
        if (Vector3.Distance(transform.position, lastFramePosition) > positionSyncThreshold && bodyChild != null)
        {
            bodyChild.localPosition = Vector3.zero;
            lastFramePosition = transform.position;
        }
    }

    Vector3 GetPlayerVelocity()
    {
        if (player == null) return Vector3.zero;
        
        // Check for different movement components
        if (player.TryGetComponent<Rigidbody>(out Rigidbody rb))
            return rb.linearVelocity;
        if (player.TryGetComponent<CharacterController>(out CharacterController cc))
            return cc.velocity;
        if (player.TryGetComponent<NavMeshAgent>(out NavMeshAgent nma))
            return nma.velocity;
        
        return Vector3.zero;
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimeRemaining = attackDuration;
        agent.speed = phase2Active ? phase2AttackSpeed : attackSpeed;
        attackHitbox.enabled = true;
        
        // Calculate attack target ONCE at attack start with prediction
        Vector3 playerVelocity = GetPlayerVelocity();
        Vector3 predictedPosition = player.position + playerVelocity * predictionTime;
        Vector3 attackDirection = (predictedPosition - transform.position).normalized;
        attackTargetPosition = predictedPosition + attackDirection * 
            (phase2Active ? phase2OvershootMultiplier : baseOvershootMultiplier);

        // Immediately face the attack direction at start
        Quaternion immediateRotation = Quaternion.LookRotation(attackDirection) * Quaternion.Euler(rotationOffset);
        transform.rotation = immediateRotation;
    }

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
            
            // Immediately start recovery after attack
            StartRecovery();
            
            // Reset attack timer but keep movement active
            attackTimer = phase2Active ? phase2AttackInterval : originalAttackInterval;
        }
    }

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

    void HandleRecovery()
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

    void OnCollisionEnter(Collision collision)
    {
        // Just handle health impacts here
        if (bodyHealth != null)
            bodyHealth.OnCollisionEnter(collision);
    }

    void StartRecovery()
    {
        if (isRecovering) return;
        isRecovering = true;
        
        // Calculate bounce direction based on player position
        Vector3 awayDirection = (transform.position - player.position).normalized;
        recoveryTarget = transform.position + awayDirection * bounceDistance;
        
        // Immediate rotation towards bounce direction
        Quaternion desiredRot = Quaternion.LookRotation(awayDirection) * Quaternion.Euler(rotationOffset);
        transform.rotation = desiredRot;
        
        // Shorten recovery duration for faster return to circling
        StartCoroutine(RecoveryBounce());
    }

    IEnumerator RecoveryBounce()
    {
        float timer = 0;
        Vector3 startPos = transform.position;
        
        // Add vertical lift during bounce
        Vector3 bounceEndPos = recoveryTarget + Vector3.up * 2f; 
        
        while (timer < recoveryDuration)
        {
            timer += Time.deltaTime;
            // Add arc to bounce movement
            Vector3 arcPos = Vector3.Lerp(startPos, bounceEndPos, timer/recoveryDuration);
            arcPos.y += Mathf.Sin(timer/recoveryDuration * Mathf.PI) * 3f; // Height curve
            transform.position = arcPos;
            yield return null;
        }
        
        // Force new pattern and reset orbit
        GenerateNewPattern();
        Vector3 toPlayer = transform.position - player.position;
        currentOrbitAngle = Mathf.Atan2(toPlayer.z, toPlayer.x);
        
        isRecovering = false;
        attackTimer = phase2Active ? phase2AttackInterval : originalAttackInterval;
    }
}