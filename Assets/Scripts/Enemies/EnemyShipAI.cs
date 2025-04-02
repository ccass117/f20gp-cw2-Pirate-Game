using UnityEngine;
using UnityEngine.AI;
using Unity.AI;

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
    public float farSpeedMultiplier = 0.1f; // Speed multiplier when far from player
    public float farDistanceThreshold = 30f; // Distance to consider "far"

    [Header("Aggro Settings")]
    private bool isAggroed;
    public float aggroRange = 20f;
    public LayerMask lineOfSightMask;

    [Header("Idle Behavior Settings")]
    public float patrolRadius = 10f;
    public float idleAngularSpeed = 3f;
    public float minObstacleDistance = 15f; // Minimum distance from obstacles when patrolling
    public float patrolPointVariance = 25f; // Randomness in patrol points

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

    [Header("Obstacle Avoidance")]
    public float avoidanceForce = 5f;
    public float lookAheadDistance = 50f;
    public float sideCastAngle = 45f;
    public LayerMask obstacleMask;
    private Vector3 currentAvoidance = Vector3.zero;
    public float emergencyBrakeDistance = 30f; 
    public float emergencySpeedMultiplier = 0.3f; 
    public float directionalAvoidWeight = 2f;
    public float islandAvoidanceRadius = 15f;

    [Header("Advanced Navigation")]
    [Tooltip("How aggressively to maintain pursuit while avoiding")]
    public float pursuitAggression = 0.7f;
    [Tooltip("Minimum distance between path updates")]
    public float minPathUpdateDistance = 10f;
    [SerializeField] private Vector3 smoothedTargetPosition;

    [Header("Advanced Avoidance")]
    public float emergencyTurnBoost = 2f;
    public float recoveryPathRadius = 35f;
    public float obstacleRecheckInterval = 0.5f;
    private float lastObstacleRecheckTime;

    [Header("Collision Recovery")]
    [Tooltip("When pushed off course, how aggressively to try to return")]
    public float recoveryAggression = 2f;
    [Tooltip("Speed reduction during recovery maneuvers")]
    public float recoverySpeedMultiplier = 0.6f;
    [Tooltip("How much offset from path before considering it a derailment")]
    public float derailmentThreshold = 8f;
    [SerializeField] private Vector3 lastValidPosition;
    [SerializeField] private float derailmentTimer;

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 1f;
    public float stuckThreshold = 0.5f; // Max speed to consider stuck
    public float escapeForceMultiplier = 3f;
    private float lastStuckCheckTime;
    private Vector3 lastStuckPosition;
    private int stuckCounter;

    private Rigidbody rb;
    private NavMeshAgent navAgent;
    private Vector3 targetPosition;
    private float timer;
    private Vector3 idleCenterPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();
        currentRiggingSpeed = speed;

        if(navAgent != null)
        {
            navAgent.radius = 10f;
            navAgent.height = 5f;
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            navAgent.avoidancePriority = 50;
            navAgent.autoRepath = true;
            navAgent.stoppingDistance = 10f;
            navAgent.updatePosition = false;
            navAgent.updateRotation = true;
        }
        else
        {
            Debug.LogError("NavMeshAgent component missing!", this);
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
        if(navAgent != null && navAgent.enabled)
        {
            navAgent.Warp(transform.position);
            navAgent.transform.rotation = transform.rotation;
        }

        if(Time.time - lastStuckCheckTime > stuckCheckInterval)
        {
            CheckIfStuck();
        }

        if (playerShip == null)
            return;

        // Calculate wind direction
        wind = WindMgr.Instance.windDir * WindMgr.Instance.windStrength;

        // Restore aggro detection logic
        UpdateAggroStatus();
        
        // Compute target position based on state
        if (isAggroed)
            acquireTarget();
        else
            Idle();

        smoothedTargetPosition = Vector3.Lerp(smoothedTargetPosition, targetPosition, 5f * Time.deltaTime);
        targetPosition = smoothedTargetPosition;

        // Add collision avoidance
        CheckFrontalCollision();

        // Restore navigation updates
        if (!(useNavMesh && navAgent != null && isAggroed))
            Rudder();

        adjustSails();
        currentRudderAngle = Mathf.MoveTowards(currentRudderAngle, targetRudderAngle, rudderSpeed * Time.deltaTime);

        // Update NavMesh navigation
        if (useNavMesh && navAgent != null && isAggroed)
            Navigate();

        CheckCourseDeviation();
        
        currentAvoidance = CalculateAvoidanceForce();
        Vector3 avoidanceOffset = currentAvoidance * Time.deltaTime;
        targetPosition = Vector3.Lerp(targetPosition, 
                                    targetPosition + avoidanceOffset, 
                                    Mathf.Clamp01(currentAvoidance.magnitude/avoidanceForce));
    }

    void UpdateAggroStatus()
    {
        isAggroed = true;
    }

    void CheckIfStuck()
    {
        lastStuckCheckTime = Time.time;
        
        if(rb.linearVelocity.magnitude < stuckThreshold && 
        Vector3.Distance(transform.position, lastStuckPosition) < 2f)
        {
            stuckCounter++;
            if(stuckCounter > 2)
            {
                ExecuteEscapeManeuver();
            }
        }
        else
        {
            stuckCounter = 0;
        }
        
        lastStuckPosition = transform.position;
    }

    void ExecuteEscapeManeuver()
    {
        // Find safest escape direction
        Vector3[] testDirs = {
            -transform.forward,
            transform.right,
            -transform.right,
            transform.forward + transform.right,
            transform.forward - transform.right
        };

        foreach(Vector3 dir in testDirs)
        {
            Vector3 testPos = transform.position + dir * 25f;
            NavMeshHit hit;
            if(NavMesh.SamplePosition(testPos, out hit, 30f, NavMesh.AllAreas) &&
            !Physics.CheckSphere(hit.position, islandAvoidanceRadius, obstacleMask))
            {
                targetPosition = hit.position;
                currentAvoidance += dir.normalized * avoidanceForce * escapeForceMultiplier;
                derailmentTimer = 3f;
                break;
            }
        }
    }

    Vector3 CalculateAvoidanceForce()
    {
        Vector3 avoidance = Vector3.zero;
        float[] scanAngles = { 0f, 30f, -30f, 60f, -60f }; // Wider angle coverage
        float closestObstacleDistance = Mathf.Infinity;
        Vector3 primaryAvoidance = Vector3.zero;

        // Primary obstacle detection
        foreach(float angle in scanAngles)
        {
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            if(Physics.SphereCast(transform.position, 15f, dir, out RaycastHit hit, lookAheadDistance, obstacleMask))
            {
                float weight = 1 - Mathf.Clamp01(hit.distance / lookAheadDistance);
                Vector3 avoidDir = (-dir + hit.normal).normalized;
                
                // Prioritize frontal obstacles
                if(angle == 0f)
                {
                    closestObstacleDistance = hit.distance;
                    primaryAvoidance = avoidDir * weight * avoidanceForce * 3f; // Stronger frontal response
                }
                else
                {
                    avoidance += avoidDir * weight * avoidanceForce * directionalAvoidWeight;
                }
            }
        }

        // Apply emergency avoidance
        if(closestObstacleDistance < emergencyBrakeDistance)
        {
            float emergencyWeight = 1 - (closestObstacleDistance / emergencyBrakeDistance);
            avoidance += primaryAvoidance * emergencyWeight;
            avoidance += -transform.forward * avoidanceForce * emergencyWeight;
        }

        // Add periodic obstacle recheck
        if(Time.time - lastObstacleRecheckTime > obstacleRecheckInterval)
        {
            lastObstacleRecheckTime = Time.time;
            RevalidateCurrentPath();
        }

        return Vector3.ClampMagnitude(avoidance, avoidanceForce * 2f);
    }

    void RevalidateCurrentPath()
    {
        if(navAgent != null && navAgent.hasPath)
        {
            foreach(Vector3 corner in navAgent.path.corners)
            {
                if(Physics.CheckSphere(corner, minObstacleDistance * 1.5f, obstacleMask))
                {
                    navAgent.ResetPath();
                    acquireTarget();
                    break;
                }
            }
        }
    }

    float GetPathSafety(Vector3 direction)
    {
        float score = 0f;
        for(int i = 0; i < 5; i++)
        {
            Vector3 testDir = Quaternion.Euler(0, Random.Range(-30f, 30f), 0) * direction;
            if(!Physics.SphereCast(transform.position, 10f, testDir, out _, lookAheadDistance, obstacleMask))
            {
                score += 1f;
            }
        }
        return score;
    }

    void CheckFrontalCollision()
    {
        Vector3[] rayOffsets = {
            Vector3.zero,
            Vector3.up * 2f,
            Vector3.down * 2f
        };

        bool hadCollision = false;

        foreach(Vector3 offset in rayOffsets)
        {
            if(Physics.SphereCast(transform.position + offset, 10f, transform.forward, 
                out RaycastHit obstacleHit, 70f, lineOfSightMask))
            {
                hadCollision = true;
                Vector3 avoidDir = Vector3.Cross(obstacleHit.point - transform.position, Vector3.up).normalized;
                targetPosition += avoidDir * avoidanceRadius * (1 - obstacleHit.distance/70f);
                break;
            }
        }
        
        if(!hadCollision)
        {
            lastValidPosition = transform.position;
        }
    }

    void FixedUpdate()
    {
        if(anchored) return;

        float turnFactor = Mathf.Clamp01(Mathf.Abs(currentRudderAngle)/maxRudderAngle);
        float targetSpeed = Mathf.Lerp(maxSpeed, speed/2, turnFactor);
        
        float windEffect = Vector3.Dot(transform.forward, wind.normalized);
        targetSpeed *= (1 + windEffect * 0.5f);

        float emergencySpeedModifier = 1f;
        if(Physics.SphereCast(transform.position, 15f, transform.forward, 
            out RaycastHit frontHit, emergencyBrakeDistance, obstacleMask))
        {
            float brakePower = Mathf.Clamp01(1 - (frontHit.distance / emergencyBrakeDistance));
            emergencySpeedModifier = Mathf.Lerp(1f, emergencySpeedMultiplier, brakePower);
        }

        targetSpeed *= emergencySpeedModifier;

        if(derailmentTimer > 0)
        {
            targetSpeed *= recoverySpeedMultiplier;
            derailmentTimer -= Time.fixedDeltaTime;
        }

        if(derailmentTimer > 0 && rb.angularVelocity.magnitude < 0.1f)
        {
            rb.AddTorque(transform.up * currentRudderAngle * 0.5f, ForceMode.Acceleration);
        }
        // Adjust speed based on distance to player
        if (playerShip != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerShip.position);
            if (distanceToPlayer > farDistanceThreshold)
            {
                targetSpeed *= farSpeedMultiplier;
            }
        }

        // Smooth speed changes
        currentRiggingSpeed = Mathf.MoveTowards(currentRiggingSpeed, targetSpeed, 0.5f * Time.fixedDeltaTime);
        
        rb.MovePosition(rb.position + transform.forward * currentRiggingSpeed * Time.fixedDeltaTime);
        
        if(navAgent != null && navAgent.enabled)
        {
            navAgent.nextPosition = transform.position;
            navAgent.velocity = rb.linearVelocity;
        }
    }
    void CheckCourseDeviation()
    {
        bool isMovingStraight = 
            Mathf.Abs(currentRudderAngle) < 5f && 
            rb.angularVelocity.magnitude < 1f &&
            Vector3.Distance(transform.position, targetPosition) > 10f;

        // Calculate how far we've strayed from our intended path
        float pathDeviation = Vector3.Distance(transform.position, lastValidPosition);
        if(pathDeviation > derailmentThreshold || isMovingStraight)
        {
            derailmentTimer = 3f;
            lastValidPosition = transform.position;
            
            // Create new target perpendicular to current velocity
            Vector3 recoveryDir = Quaternion.Euler(0, 90f, 0) * rb.linearVelocity.normalized;
            targetPosition = transform.position + recoveryDir * 25f;
            
            // Validate on NavMesh
            NavMeshHit hit;
            if(NavMesh.SamplePosition(targetPosition, out hit, 30f, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
            }
        }
        
        // Update valid position if we're on course
        if(Vector3.Distance(transform.position, targetPosition) < positionTolerance * 2f)
        {
            lastValidPosition = transform.position;
        }
    }

    void acquireTarget()
    {
        // Get base player position with intelligent velocity prediction
        Vector3 predictedPlayerPos = playerShip.position;
        Rigidbody playerRb = playerShip.GetComponent<Rigidbody>();
        
        if(playerRb != null) 
        {
            // Only predict velocity if player is moving away significantly
            Vector3 toPlayer = playerShip.position - transform.position;
            float distanceFactor = Mathf.Clamp01(Vector3.Dot(playerRb.linearVelocity.normalized, toPlayer.normalized));
            predictedPlayerPos += playerRb.linearVelocity * Mathf.Clamp01(distanceFactor) * 1.5f;
        }

        // Calculate dynamic flanking position
        float flankSide = Vector3.Dot(transform.position - predictedPlayerPos, playerShip.right) > 0 ? 1 : -1;
        Vector3 flankOffset = (playerShip.right * flankSide * targetDistance) + 
                            (-playerShip.forward * Mathf.Lerp(5f, 15f, currentAvoidance.magnitude/avoidanceForce));
        Vector3 idealPosition = predictedPlayerPos + flankOffset;

        // Blend target with avoidance direction
        Vector3 avoidanceInfluence = currentAvoidance.normalized * avoidanceRadius;
        Vector3 finalTarget = Vector3.Lerp(idealPosition, 
                                        idealPosition + avoidanceInfluence, 
                                        Mathf.Clamp01(currentAvoidance.magnitude / (avoidanceForce * 0.7f)));

        // Adaptive NavMesh sampling
        NavMeshHit hit;
        float sampleRadius = 25f;
        bool foundValid = false;
        
        // Progressive sampling with path validation
        for(int i = 0; i < 3; i++)
        {
            if(NavMesh.SamplePosition(finalTarget, out hit, sampleRadius, NavMesh.AllAreas))
            {
                if(IsPathSafe(hit.position) && 
                !Physics.CheckSphere(hit.position, islandAvoidanceRadius * 0.8f, obstacleMask))
                {
                    targetPosition = hit.position;
                    foundValid = true;
                    break;
                }
            }
            sampleRadius += 20f; // Aggressive radius expansion
        }

        // Fallback strategies
        if(!foundValid)
        {
            // Try direct path first
            if(!Physics.Linecast(transform.position, finalTarget, obstacleMask))
            {
                targetPosition = finalTarget;
            }
            else
            {
                // Fallback to player-relative escape position
                Vector3 escapeDir = (transform.position - playerShip.position).normalized;
                targetPosition = transform.position + escapeDir * islandAvoidanceRadius * 2f;
            }
        }

        // Final obstacle validation
        if(Physics.CheckSphere(targetPosition, islandAvoidanceRadius, obstacleMask))
        {
            // Systematic escape direction testing
            Vector3[] escapeVectors = {
                transform.right * islandAvoidanceRadius * 2f,
                -transform.right * islandAvoidanceRadius * 2f,
                transform.forward * islandAvoidanceRadius * 1.5f,
                -transform.forward * islandAvoidanceRadius * 1.5f
            };

            foreach(Vector3 dir in escapeVectors)
            {
                Vector3 testPos = transform.position + dir;
                if(NavMesh.SamplePosition(testPos, out hit, islandAvoidanceRadius * 3f, NavMesh.AllAreas) &&
                !Physics.CheckSphere(hit.position, islandAvoidanceRadius, obstacleMask))
                {
                    targetPosition = hit.position;
                    break;
                }
            }
        }

        Debug.DrawLine(transform.position, targetPosition, Color.green, 2f);
    }

    void Idle()
    {
        // Generate new patrol point only when close to current target
        if(Vector3.Distance(transform.position, targetPosition) < holdCourseThreshold)
        {
            targetPosition = GetSafePatrolPoint();
        }
    }

    Vector3 GetSafePatrolPoint()
    {
        Vector3 bestPoint = idleCenterPosition;
        float bestScore = float.MinValue;
        
        // Try multiple random points to find safest one
        for(int i = 0; i < 5; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-patrolRadius, patrolRadius),
                0,
                Random.Range(-patrolRadius, patrolRadius)
            );
            
            Vector3 testPoint = idleCenterPosition + randomOffset;
            
            // Check obstacle proximity
            if(Physics.CheckSphere(testPoint, minObstacleDistance, obstacleMask))
                continue;

            // Get navmesh valid point
            NavMeshHit hit;
            if(NavMesh.SamplePosition(testPoint, out hit, patrolRadius * 2, NavMesh.AllAreas))
            {
                // Calculate score based on distance from obstacles and center
                float obstacleDistance = GetObstacleDistance(hit.position);
                float centerDistance = Vector3.Distance(hit.position, idleCenterPosition);
                float score = obstacleDistance + (centerDistance * 0.5f);
                
                if(score > bestScore)
                {
                    bestScore = score;
                    bestPoint = hit.position;
                }
            }
        }
        
        return bestPoint;
    }

    float GetObstacleDistance(Vector3 position)
    {
        if(Physics.SphereCast(position, 1f, Vector3.forward, out RaycastHit hit, minObstacleDistance, obstacleMask))
        {
            return hit.distance;
        }
        return minObstacleDistance;
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
        
        if(distance < holdCourseThreshold * 2f)
        {
            // Add reverse capability when stuck
            if(distance < 3f && currentAvoidance.magnitude > avoidanceForce * 0.7f)
            {
                targetRudderAngle = 180f; // Hard turn to escape
                return;
            }
            targetRudderAngle = 0;
            return;
        }

        Vector3 targetDir = toTarget.normalized;
        float angleDiff = Vector3.SignedAngle(transform.forward, targetDir, Vector3.up);
        
        // Dynamic turn rate based on obstacle proximity
        float obstacleWeight = Mathf.Clamp01(currentAvoidance.magnitude / avoidanceForce);
        float turnBoost = maxTurnBoost * (1 + obstacleWeight);
        
        targetRudderAngle = Mathf.Clamp(angleDiff * 1.5f, -maxRudderAngle, maxRudderAngle);
        
        // Apply boosted turn rate when avoiding
        float effectiveTurnRate = maxTurnRate * (1 + obstacleWeight) + turnBoost;
        Quaternion desiredRotation = Quaternion.LookRotation(targetDir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, 
                                                    effectiveTurnRate * Time.fixedDeltaTime);
    }

    // Use the NavMeshAgent to update destination and extract the next corner for steering.
    void Navigate()
    {
        if(navAgent == null || !navAgent.enabled) return;

        if(navAgent == null)
        {
            Debug.LogError("NavAgent is null!");
            return;
        }

        try 
        {
            if(!navAgent.isOnNavMesh)
            {
                navAgent.Warp(transform.position);
                return;
            } 

            if(!isAggroed)
            {
                // Verify path safety
                if(navAgent.hasPath)
                {
                    foreach(Vector3 corner in navAgent.path.corners)
                    {
                        if(Physics.CheckSphere(corner, minObstacleDistance, obstacleMask))
                        {
                            navAgent.ResetPath();
                            targetPosition = GetSafePatrolPoint();
                            break;
                        }
                    }
                }
                
                // Reduce update frequency when patrolling
                if(Time.time - timer > updateTimer * 2f)
                {
                    NavMesh.SamplePosition(targetPosition, out NavMeshHit sampleHit, patrolRadius * 2, NavMesh.AllAreas);
                    navAgent.SetDestination(sampleHit.position);
                    timer = Time.time;
                }
            } else {
                // Maintain minimum speed for maneuverability
                currentRiggingSpeed = Mathf.Max(currentRiggingSpeed, speed * 0.8f);

                // Calculate path with dynamic repathing
                if(Time.time - timer > updateTimer || navAgent.remainingDistance < 5f)
                {
                    Vector3 finalTarget = Vector3.Lerp(targetPosition, 
                                                    playerShip.position, 
                                                    Mathf.Clamp01(currentAvoidance.magnitude/avoidanceForce));
                    
                    NavMesh.SamplePosition(finalTarget, out NavMeshHit sampleHit, 25f, NavMesh.AllAreas);
                    navAgent.SetDestination(sampleHit.position);
                    timer = Time.time;
                }

                // Force path refresh when obstructed
                if(navAgent.pathStatus == NavMeshPathStatus.PathPartial)
                {
                    navAgent.ResetPath();
                    acquireTarget();
                }           
            }
        } catch(System.Exception e) {
            Debug.LogError($"Nav error: {e.Message}", this);
        }
    }

    void Rudder()
    {
        Vector3 toTarget = targetPosition - transform.position;
        if (toTarget.magnitude > positionTolerance)
        {
            // Calculate base direction
            Vector3 pursuitDir = (playerShip.position - transform.position).normalized;
            Vector3 desiredDir = Vector3.Lerp(toTarget.normalized, pursuitDir, pursuitAggression);

            // Boost avoidance influence during recovery
            float avoidanceWeight = Mathf.Clamp01(currentAvoidance.magnitude / avoidanceForce);
            if(derailmentTimer > 0) avoidanceWeight = Mathf.Clamp(avoidanceWeight * 1.5f, 0.5f, 1f);
            desiredDir = Vector3.Lerp(desiredDir, currentAvoidance.normalized, avoidanceWeight);

            // Calculate angle with emergency turn boost
            float angleToTarget = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);
            float turnMultiplier = derailmentTimer > 0 ? emergencyTurnBoost : 1f;
            
            targetRudderAngle = Mathf.Clamp(angleToTarget * turnMultiplier, -maxRudderAngle, maxRudderAngle);

            // Prevent straight-line lock
            if(Mathf.Abs(targetRudderAngle) < 5f && derailmentTimer > 0)
            {
                targetRudderAngle = rb.angularVelocity.y < 0 ? -45f : 45f;
            }
        }
    }

    bool IsPathSafe(Vector3 targetPos)
    {
        if(navAgent != null && navAgent.isOnNavMesh)
        {
            NavMeshPath path = new NavMeshPath();
            if(navAgent.CalculatePath(targetPos, path))
            {
                // Check entire path corridor
                for(int i = 0; i < path.corners.Length - 1; i++)
                {
                    Vector3 segmentStart = path.corners[i];
                    Vector3 segmentEnd = path.corners[i+1];
                    if(Physics.SphereCast(segmentStart, navAgent.radius * 1.2f, 
                    (segmentEnd - segmentStart).normalized, out _, 
                    Vector3.Distance(segmentStart, segmentEnd), obstacleMask))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
    }

    void adjustSails()
    {
        if(anchored) return;
        
        float windAlignment = Vector3.Dot(transform.forward, wind.normalized);
        float targetMultiplier = Mathf.Lerp(0.5f, 1.5f, (windAlignment + 1)/2);
        
        // Maintain minimum speed during avoidance
        float obstacleFactor = Mathf.Clamp01(currentAvoidance.magnitude / avoidanceForce);
        float minSpeed = Mathf.Lerp(speed, speed * 1.5f, obstacleFactor);

        // Lower minSpeed when far from player
        if (playerShip != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerShip.position);
            if (distanceToPlayer > farDistanceThreshold)
            {
                minSpeed = 0.2f; // Adjust this value as needed
            }
        }
        
        float smoothedMultiplier = Mathf.Lerp(1f, targetMultiplier, Time.deltaTime * 2f);
        currentRiggingSpeed = Mathf.Clamp(
            currentRiggingSpeed * smoothedMultiplier,
            minSpeed, 
            maxSpeed
        );
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if(collision.impulse.magnitude > 5f)
        {
            derailmentTimer = 3f;
            
            // Calculate escape direction blending collision normal and current velocity
            Vector3 collisionNormal = collision.contacts[0].normal;
            Vector3 velocityDir = rb.linearVelocity.normalized;
            Vector3 escapeDir = Vector3.Lerp(-collisionNormal, velocityDir, 0.3f).normalized;

            // Find safest escape route
            Vector3[] testDirections = {
                escapeDir,
                Quaternion.Euler(0, 45f, 0) * escapeDir,
                Quaternion.Euler(0, -45f, 0) * escapeDir
            };

            foreach(Vector3 dir in testDirections)
            {
                Vector3 testPos = transform.position + dir * recoveryPathRadius;
                NavMeshHit hit;
                if(NavMesh.SamplePosition(testPos, out hit, recoveryPathRadius, NavMesh.AllAreas) &&
                !Physics.CheckSphere(hit.position, islandAvoidanceRadius, obstacleMask))
                {
                    targetPosition = hit.position;
                    break;
                }
            }

            // Force aggressive turning
            currentAvoidance += escapeDir * avoidanceForce * 3f;
            CancelInvoke("DelayedCourseUpdate");
            Invoke("DelayedCourseUpdate", 0.5f);

            if(collision.gameObject.CompareTag("Island"))
            {
                // Push away from island center
                Vector3 islandCenter = collision.transform.position;
                Vector3 islandEscapeDirection = (transform.position - islandCenter).normalized; // Renamed variable
                islandEscapeDirection.y = 0;
                currentAvoidance += islandEscapeDirection * avoidanceForce * 4f;
                targetPosition = transform.position + islandEscapeDirection * 50f;
            }
        }
    }

    void DelayedCourseUpdate()
    {
        if(isAggroed)
        {
            acquireTarget(); // Re-acquire target after recovery
        }
    }

    void OnDrawGizmos()
    {
        // Always draw target position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 1f);

        // Only draw NavMesh info if agent exists
        if(navAgent != null)
        {
            // Draw path if available
            if(navAgent.hasPath)
            {
                Gizmos.color = Color.yellow;
                for(int i = 0; i < navAgent.path.corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(navAgent.path.corners[i], navAgent.path.corners[i + 1]);
                    Gizmos.DrawSphere(navAgent.path.corners[i], 0.5f);
                }
            }

            // Draw agent's believed position
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(navAgent.nextPosition, Vector3.one * 3f);
        }

        // Draw avoidance rays
        Gizmos.color = Color.blue;
        Vector3[] rayOffsets = { Vector3.zero, Vector3.up * 2f, Vector3.down * 2f };
        foreach(Vector3 offset in rayOffsets)
        {
            Gizmos.DrawRay(transform.position + offset, transform.forward * 70f);
        }
    }
}