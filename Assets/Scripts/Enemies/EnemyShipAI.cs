using UnityEngine;
using UnityEngine.AI;

public class EnemyShipAI : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Target (player) that the enemy ship will pursue. If left unassigned, the first GameObject tagged 'Player' will be used.")]
    public Transform playerShip;
    [Tooltip("Desired attack distance when ramming.")]
    public float targetDistance = 20f;
    [Tooltip("When avoiding collision, maintain at least this distance from the player.")]
    public float avoidanceRadius = 25f;
    public float positionTolerance = 3f;

    [Header("Navigation")]
    public float updateTimer = 1f;
    public bool useNavMesh = true;

    [Header("Movement")]
    public float speed = 2f;          // Minimum forward speed
    public float maxSpeed = 5f;
    public float riggingSpeed = 2f;
    public float maxTurnRate = 60f;
    public float maxRudderAngle = 90f;
    public float rudderSpeed = 80f;
    public float maxTurnBoost = 30f;

    [Header("Aggro Settings")]
    public float aggroRange = 20f;
    public LayerMask lineOfSightMask;

    [Header("Idle Behavior Settings")]
    public float patrolRadius = 10f;
    public float idleAngularSpeed = 3f;

    [Header("Hold Course Settings")]
    [Tooltip("When within this distance of the target position, hold course rather than turning aggressively.")]
    public float holdCourseThreshold = 5f;

    [Header("Collision Behavior")]
    [Tooltip("If true, the enemy will attempt to ram (attack) the player. If false, it will avoid collisions by keeping at least 'avoidanceRadius' away.")]
    public bool isRammingShip = true;

    [Header("Read Only")]
    [SerializeField] private float targetRudderAngle;
    [SerializeField] private float currentRiggingSpeed;
    [SerializeField] private float currentRudderAngle = 0f;
    [SerializeField] private bool anchored = false;
    [SerializeField] private Vector3 wind = Vector3.zero;
    [SerializeField] private bool isAggroed = false;

    private Rigidbody rb;
    private NavMeshAgent navAgent;
    private Vector3 targetPosition;
    private float timer;
    private Vector3 idleCenterPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentRiggingSpeed = speed;

        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.speed = maxSpeed;
            // Disable automatic position updates so we can force forward-only movement.
            navAgent.updatePosition = false;
            // Let the agent update rotation to help with path calculation.
            navAgent.updateRotation = true;
        }

        idleCenterPosition = transform.position;
    }

    void Start()
    {
        if (playerShip == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerShip = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (playerShip == null)
            return;

        // Calculate wind force for movement.
        wind = WindMgr.Instance.windDir * WindMgr.Instance.windStrength;

        // Determine aggro status based on range and line of sight.
        Vector3 toPlayer = playerShip.position - transform.position;
        if (toPlayer.magnitude <= aggroRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, toPlayer.normalized, out hit, aggroRange, lineOfSightMask))
            {
                isAggroed = (hit.transform == playerShip || hit.transform.IsChildOf(playerShip));
            }
            else
            {
                isAggroed = false;
            }
        }
        else
        {
            isAggroed = false;
        }

        // Compute the target position.
        if (isAggroed)
            acquireTarget();
        else
            Idle();

        // When not using navmesh for aggroed movement, use our custom rudder steering.
        if (!(useNavMesh && navAgent != null && isAggroed))
            Rudder();

        adjustSails();
        currentRudderAngle = Mathf.MoveTowards(currentRudderAngle, targetRudderAngle, rudderSpeed * Time.deltaTime);

        // If using NavMeshAgent, update its destination and extract the next corner for steering.
        if (useNavMesh && navAgent != null && isAggroed)
            Navigate();
    }

    void FixedUpdate()
    {
        if (anchored)
            return;

        // For navmesh-driven behavior, always move along transform.forward.
        if (useNavMesh && navAgent != null && isAggroed)
        {
            float windEffect = Vector3.Dot(transform.forward, wind);
            float effectiveSpeed = currentRiggingSpeed + windEffect;
            effectiveSpeed = Mathf.Max(effectiveSpeed, speed);
            rb.MovePosition(rb.position + transform.forward * effectiveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            Move();
            Steer();
        }
    }

    void acquireTarget()
    {
        Vector3 predictedPlayerPos = playerShip.position;
        Rigidbody playerRb = playerShip.GetComponent<Rigidbody>();
        float predictionTime = 2f;
        if (playerRb != null)
        {
            predictedPlayerPos += playerRb.linearVelocity * predictionTime;
        }

        Vector3 fromPlayerToEnemy = transform.position - predictedPlayerPos;
        if (fromPlayerToEnemy == Vector3.zero)
        {
            fromPlayerToEnemy = transform.forward;
        }
        fromPlayerToEnemy.Normalize();


        float orbitAngle = 30f * Mathf.Sin(Time.time); 
        Vector3 orbitDirection = Quaternion.Euler(0, orbitAngle, 0) * fromPlayerToEnemy;
        
        targetPosition = predictedPlayerPos + orbitDirection * targetDistance;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            targetPosition = hit.position;
        }
    }

    void Idle()
    {
        float angle = Time.time * idleAngularSpeed;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * patrolRadius;
        targetPosition = idleCenterPosition + offset;
    }

    // Custom movement for non-navmesh cases.
    void Move()
    {
        float windEffect = Vector3.Dot(transform.forward, wind);
        float effectiveSpeed = currentRiggingSpeed + windEffect;
        effectiveSpeed = Mathf.Max(effectiveSpeed, speed);
        rb.MovePosition(rb.position + transform.forward * effectiveSpeed * Time.fixedDeltaTime);
    }

    // Custom steering for non-navmesh cases.
    void Steer()
    {
        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;
        if (distance < holdCourseThreshold)
        {
            targetRudderAngle = 0;
            return;
        }
        Vector3 targetDir = toTarget.normalized;
        float angleDiff = Mathf.Abs(Vector3.SignedAngle(transform.forward, targetDir, Vector3.up));
        float aggressiveMultiplier = 3f;
        float effectiveTurnRate = maxTurnRate * aggressiveMultiplier;
        if (angleDiff > 20f)
            effectiveTurnRate += maxTurnBoost * (angleDiff / 90f);
        Quaternion desiredRotation = Quaternion.LookRotation(targetDir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, effectiveTurnRate * Time.fixedDeltaTime);
    }

    // Use the NavMeshAgent to update destination and extract the next corner for steering.
    void Navigate()
    {
        if (Time.time - timer > updateTimer)
        {
            navAgent.SetDestination(targetPosition);
            timer = Time.time;
        }
        // If the agent has a path with corners, use the next corner to guide rotation.
        if (navAgent.path != null && navAgent.path.corners.Length > 1)
        {
            Vector3 nextCorner = navAgent.path.corners[1];
            Vector3 toCorner = nextCorner - transform.position;
            float angleToCorner = Vector3.SignedAngle(transform.forward, toCorner, Vector3.up);
            targetRudderAngle = Mathf.Clamp(angleToCorner, -maxRudderAngle, maxRudderAngle);
        }
    }

    void Rudder()
    {
        Vector3 toTarget = targetPosition - transform.position;
        if (toTarget.magnitude > positionTolerance)
        {
            Vector3 desiredDir = toTarget.normalized;
            float angleToTarget = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);
            targetRudderAngle = Mathf.Clamp(angleToTarget, -maxRudderAngle, maxRudderAngle);
        }
    }

    void adjustSails()
    {
        if (anchored)
            return;

        float dist = Vector3.Distance(transform.position, targetPosition);
        if (isAggroed)
        {
            currentRiggingSpeed = Mathf.MoveTowards(currentRiggingSpeed, maxSpeed, riggingSpeed * Time.deltaTime);
        }
        else
        {
            float angleDiff = Mathf.Abs(Vector3.SignedAngle(transform.forward, (targetPosition - transform.position).normalized, Vector3.up));
            float alignmentStrength = Mathf.Clamp01(1 - (angleDiff / 90f));
            float brakingForce = (angleDiff > 45f) ? 0.5f : 1f;
            float targetSpeed = (dist < positionTolerance) ? speed : Mathf.Lerp(speed, maxSpeed, alignmentStrength * Mathf.Clamp01((dist - positionTolerance) / targetDistance));
            targetSpeed *= brakingForce;
            currentRiggingSpeed = Mathf.MoveTowards(currentRiggingSpeed, targetSpeed, riggingSpeed * Time.deltaTime);
        }
        currentRiggingSpeed = Mathf.Clamp(currentRiggingSpeed, speed, maxSpeed);
    }
}