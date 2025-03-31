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
    public float speed = 2f;
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
            // Enable automatic updates so the agent drives movement and rotation
            navAgent.updatePosition = true;
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

        // Apply wind force calculation for use in non-navmesh movement
        wind = WindMgr.Instance.windDir * WindMgr.Instance.windStrength;

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

        if (isAggroed)
            acquireTarget();
        else
            Idle();

        // If using NavMeshAgent and aggroed, let the agent handle movement.
        // Otherwise, use custom rudder steering.
        if (!(useNavMesh && navAgent != null && isAggroed))
            Rudder();

        adjustSails();

        currentRudderAngle = Mathf.MoveTowards(currentRudderAngle, targetRudderAngle, rudderSpeed * Time.deltaTime);

        // If using navmesh and aggroed, update destination periodically.
        if (useNavMesh && navAgent != null && isAggroed)
            Navigate();
    }

    void FixedUpdate()
    {
        if (anchored)
            return;

        // If using NavMeshAgent and aggroed, let it drive the movement automatically.
        // Otherwise, use our custom Move() and Steer() routines.
        if (!(useNavMesh && navAgent != null && isAggroed))
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

        Vector3 directionFromPlayer = transform.position - predictedPlayerPos;
        if (directionFromPlayer == Vector3.zero)
        {
            directionFromPlayer = transform.forward;
        }
        directionFromPlayer.Normalize();

        float desiredDistance = isRammingShip ? targetDistance : Mathf.Max(targetDistance, avoidanceRadius);
        targetPosition = predictedPlayerPos + directionFromPlayer * desiredDistance;
    }

    void Idle()
    {
        float angle = Time.time * idleAngularSpeed;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * patrolRadius;
        targetPosition = idleCenterPosition + offset;
    }

    // Custom movement when not using NavMeshAgent for aggroed behavior.
    void Move()
    {
        float windEffect = Vector3.Dot(transform.forward, wind);
        float effectiveSpeed = currentRiggingSpeed + windEffect;
        effectiveSpeed = Mathf.Max(0, effectiveSpeed);
        Vector3 velocity = transform.forward * effectiveSpeed;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    // Custom steering when not using NavMeshAgent.
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

    // Use NavMeshAgent to navigate toward the target position.
    void Navigate()
    {
        if (Time.time - timer > updateTimer)
        {
            navAgent.SetDestination(targetPosition);
            timer = Time.time;
        }
    }

    Vector3 courseCorrect(Vector3 targetDir)
    {
        Vector3 windDir = wind.normalized;
        float windStrength = wind.magnitude;
        Vector3 projectedWind = Vector3.ProjectOnPlane(windDir, Vector3.up);
        float windEffect = Mathf.Clamp01(windStrength / currentRiggingSpeed);
        return Vector3.Lerp(targetDir, targetDir - projectedWind * windEffect, 0.5f).normalized;
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