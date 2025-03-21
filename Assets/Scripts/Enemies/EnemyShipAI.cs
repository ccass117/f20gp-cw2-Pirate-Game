using UnityEngine;
using UnityEngine.AI;

public class EnemyShipAI : MonoBehaviour
{
    [Header("Targeting")]
    public Transform playerShip;
    public float targetDistance = 20f;
    public float side = 1f; //1 = right, -1 = left
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
            navAgent.updatePosition = false;
            navAgent.updateRotation = false;
        }

        idleCenterPosition = transform.position;
    }

    void Update()
    {
        if (playerShip == null)
            return;

        wind = WindMgr.Instance.windDir * WindMgr.Instance.windStrength;

        Vector3 toPlayer = playerShip.position - transform.position;
        if (toPlayer.magnitude <= aggroRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, toPlayer.normalized, out hit, aggroRange, lineOfSightMask))
            {
                if (hit.transform == playerShip || hit.transform.IsChildOf(playerShip))
                    isAggroed = true;
                else
                    isAggroed = false;
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

        if (useNavMesh && navAgent != null && isAggroed)
            Navigate();
        else
            Rudder();

        adjustSails();

        Debug.Log($"Target: {targetPosition} | Dist: {Vector3.Distance(transform.position, targetPosition):F2} | Aggro: {isAggroed}");

        currentRudderAngle = Mathf.MoveTowards(currentRudderAngle, targetRudderAngle, rudderSpeed * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (anchored)
            return;

        Move();
        Steer();

        if (useNavMesh && navAgent != null && isAggroed)
            navAgent.nextPosition = rb.position;
    }

    // Aggro behavior: target player's offset position with prediction.
    void acquireTarget()
    {
        Vector3 target = playerShip.position + playerShip.right * targetDistance * side;
        float distanceToStatic = Vector3.Distance(transform.position, target);
        if (distanceToStatic < targetDistance * 0.75f)
            targetPosition = target;
        else
        {
            Rigidbody playerRb = playerShip.GetComponent<Rigidbody>();
            Vector3 playerVelocity = (playerRb != null) ? playerRb.linearVelocity : Vector3.zero;
            float predictionTime = 2f;
            Vector3 predPosition = playerShip.position + playerVelocity * predictionTime;
            targetPosition = predPosition + playerShip.right * targetDistance * side;
        }
    }

    //circle around spawn point when not aggro'd
    void Idle()
    {
        float angle = Time.time * idleAngularSpeed;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * patrolRadius;
        targetPosition = idleCenterPosition + offset;
    }

    void Move()
    {
        float windEffect = Vector3.Dot(transform.forward, wind);
        float speed = currentRiggingSpeed + windEffect;
        speed = Mathf.Max(0, speed);
        Vector3 velocity = transform.forward * speed;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    void Steer()
    {
        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;
        Vector3 targetDir = toTarget.normalized;
        float angleBuffer = Mathf.Abs(Vector3.SignedAngle(transform.forward, targetDir, Vector3.up));
        float turnRate = maxTurnRate;
        if (angleBuffer > 20f)
            turnRate += maxTurnBoost * (angleBuffer / 90f);
        if (distance > targetDistance)
            turnRate *= 1.5f;
        if (angleBuffer > 60f)
            turnRate *= 1.25f;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, turnRate * Time.fixedDeltaTime));
    }

    void Navigate()
    {
        if (Time.time - timer > updateTimer)
        {
            navAgent.SetDestination(targetPosition);
            timer = Time.time;
        }

        if (navAgent.pathPending || !navAgent.hasPath)
            return;

        Vector3 newPath = navAgent.path.corners.Length > 1 ? navAgent.path.corners[1] : targetPosition;
        Vector3 targetDir = (newPath - transform.position).normalized;
        Vector3 correctedDir = courseCorrect(targetDir); //Account for wind vector in course calculations
        float angleToTarget = Vector3.SignedAngle(transform.forward, correctedDir, Vector3.up);
        targetRudderAngle = Mathf.Clamp(angleToTarget, -maxRudderAngle, maxRudderAngle);
        Debug.DrawRay(transform.position, targetDir * 10, Color.green);
        Debug.DrawRay(transform.position, correctedDir * 10, Color.blue);
    }

    Vector3 courseCorrect(Vector3 targetDir)
    {
        Vector3 windDirection = wind.normalized;
        float windStrength = wind.magnitude;
        Vector3 projectedWind = Vector3.ProjectOnPlane(windDirection, Vector3.up);
        float windEffect = Mathf.Clamp01(windStrength / currentRiggingSpeed);
        return Vector3.Lerp(targetDir, targetDir - projectedWind * windEffect, 0.5f).normalized;
    }

    void Rudder()
    {
        Vector3 toTarget = targetPosition - transform.position;
        float distance = toTarget.magnitude;
        if (distance > positionTolerance)
        {
            Vector3 desiredDirection = toTarget.normalized;
            float angleToTarget = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
            targetRudderAngle = Mathf.Clamp(angleToTarget, -maxRudderAngle, maxRudderAngle);
        }
    }

    void adjustSails()
    {
        if (anchored)
            return;

        float targetDistance = Vector3.Distance(transform.position, targetPosition);
        float angleBuffer = Mathf.Abs(Vector3.SignedAngle(transform.forward, (targetPosition - transform.position).normalized, Vector3.up));
        float alignmentStrength = Mathf.Clamp01(1 - (angleBuffer / 90f));
        float brakingForce = (angleBuffer > 45f) ? 0.5f : 1f;
        float targetSpeed = (targetDistance < positionTolerance)
                                ? speed
                                : Mathf.Lerp(speed, maxSpeed, alignmentStrength * Mathf.Clamp01((targetDistance - positionTolerance) / this.targetDistance));
        targetSpeed *= brakingForce;
        currentRiggingSpeed = Mathf.MoveTowards(currentRiggingSpeed, targetSpeed, riggingSpeed * Time.deltaTime);
        currentRiggingSpeed = Mathf.Clamp(currentRiggingSpeed, speed, maxSpeed);
    }
}