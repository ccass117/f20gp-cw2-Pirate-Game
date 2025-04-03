using UnityEngine;
using UnityEngine.AI;
using Unity.AI;

//configurable agent, lets us easily create enemy variety!
public class EnemyShipAI : MonoBehaviour
{
    [Header("Targeting")]
    public Transform playerShip;
    public float targetDistance = 20f;
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
    public float farSpeedMultiplier = 0.1f; 
    public float farDistanceThreshold = 30f; 

    [Header("Aggro Settings")]
    private bool isAggroed;
    public float aggroRange = 20f;
    public LayerMask lineOfSightMask;

    [Header("Idle Behavior Settings")]
    public float patrolRadius = 10f;
    public float idleAngularSpeed = 3f;
    public float minObstacleDistance = 15f; 
    public float patrolPointVariance = 25f;

    [Header("Hold Course Settings")]
    public float holdCourseThreshold = 5f;

    [Header("Collision Behavior")]
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
    public float pursuitAggression = 0.7f;
    public float minPathUpdateDistance = 10f;
    [SerializeField] private Vector3 smoothedTargetPosition;

    [Header("Advanced Avoidance")]
    public float emergencyTurnBoost = 2f;
    public float recoveryPathRadius = 35f;
    public float obstacleRecheckInterval = 0.5f;
    private float lastObstacleRecheckTime;

    [Header("Collision Recovery")]
    public float recoveryAggression = 2f;
    public float recoverySpeedMultiplier = 0.6f;
    public float derailmentThreshold = 8f;
    [SerializeField] private Vector3 lastValidPosition;
    [SerializeField] private float derailmentTimer;

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 1f;
    public float stuckThreshold = 0.5f;
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
        //grab components
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
        idleCenterPosition = transform.position;
    }

    void Start()
    {
        //target player ship
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
        //movement and steering
        if(navAgent != null && navAgent.enabled)
        {
            navAgent.Warp(transform.position);
            navAgent.transform.rotation = transform.rotation;
        }

        if(Time.time - lastStuckCheckTime > stuckCheckInterval)
        {
            //ship gets stuck sometimes, because we procedurally generate islands
            CheckIfStuck();
        }

        if (playerShip == null)
            return;

        //grab wind strength
        wind = WindMgr.Instance.windDir * WindMgr.Instance.windStrength;

        UpdateAggroStatus();
        
        if (isAggroed)
            acquireTarget();
        else
            Idle();

        //smooth out target position
        smoothedTargetPosition = Vector3.Lerp(smoothedTargetPosition, targetPosition, 5f * Time.deltaTime);
        targetPosition = smoothedTargetPosition;

        CheckFrontalCollision();

        if (!(useNavMesh && navAgent != null && isAggroed))
            Rudder();

        //we make the enemy ships move using the same rules as the player, they also have to adjust sails and turn with the rudder
        adjustSails();
        currentRudderAngle = Mathf.MoveTowards(currentRudderAngle, targetRudderAngle, rudderSpeed * Time.deltaTime);

        //update navmesh
        if (useNavMesh && navAgent != null && isAggroed)
            Navigate();

        //for if the agent gets knocked off course by collisions or wind or something
        CheckCourseDeviation();
        
        currentAvoidance = CalculateAvoidanceForce();
        Vector3 avoidanceOffset = currentAvoidance * Time.deltaTime;
        targetPosition = Vector3.Lerp(targetPosition, 
                                    targetPosition + avoidanceOffset, 
                                    Mathf.Clamp01(currentAvoidance.magnitude/avoidanceForce));
    }

    //constant aggro, but it will move slower until in range
    void UpdateAggroStatus()
    {
        isAggroed = true;
    }

    void CheckIfStuck()
    {
        lastStuckCheckTime = Time.time;
        
        //checks if the ship is stuck by checking if it has moved in the last second
        if(rb.linearVelocity.magnitude < stuckThreshold && 
        Vector3.Distance(transform.position, lastStuckPosition) < 2f)
        {
            stuckCounter++;
            //if the ship is stuck for 3 seconds, it will try to escape
            if(stuckCounter > 2)
            {
                Escape();
            }
        }
        else
        {
            stuckCounter = 0;
        }
        
        lastStuckPosition = transform.position;
    }

    void Escape()
    {
        //find easiest way out of the situation using navmesh sampling
        Vector3[] testDirs = {
            -transform.forward,
            transform.right,
            -transform.right,
            transform.forward + transform.right,
            transform.forward - transform.right
        };

        //loop through the test directions and find a valid escape route
        foreach(Vector3 dir in testDirs)
        {
            //normalise the direction and check for obstacles when checking for escape routes
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
        //calculate avoidance forces for obstacles in front of the ship
        //using sphere cast to check for obstacles in a cone in front of the ship
        Vector3 avoidance = Vector3.zero;
        float[] scanAngles = { 0f, 30f, -30f, 60f, -60f }; 
        float closestObstacleDistance = Mathf.Infinity;
        Vector3 primaryAvoidance = Vector3.zero;

        foreach(float angle in scanAngles)
        {
            //calculate the direction of the sphere cast based on the angle
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            if(Physics.SphereCast(transform.position, 15f, dir, out RaycastHit hit, lookAheadDistance, obstacleMask))
            {
                float weight = 1 - Mathf.Clamp01(hit.distance / lookAheadDistance);
                Vector3 avoidDir = (-dir + hit.normal).normalized;
                
                if(angle == 0f)
                {
                    //primary avoidance direction is the one directly in front of the ship
                    closestObstacleDistance = hit.distance;
                    primaryAvoidance = avoidDir * weight * avoidanceForce * 3f; 
                }
                else
                {
                    avoidance += avoidDir * weight * avoidanceForce * directionalAvoidWeight;
                }
            }
        }

        //add primary avoidance direction to the total avoidance force
        if(closestObstacleDistance < emergencyBrakeDistance)
        {
            float emergencyWeight = 1 - (closestObstacleDistance / emergencyBrakeDistance);
            avoidance += primaryAvoidance * emergencyWeight;
            avoidance += -transform.forward * avoidanceForce * emergencyWeight;
        }

        if(Time.time - lastObstacleRecheckTime > obstacleRecheckInterval)
        {
            lastObstacleRecheckTime = Time.time;
            //revalidate the calculated path to ensure it's still valid after all the avoidance calculations
            RevalidateCurrentPath();
        }

        return Vector3.ClampMagnitude(avoidance, avoidanceForce * 2f);
    }

    void RevalidateCurrentPath()
    {
        //check if the path is still valid by checking if there are any obstacles in the way
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

    void CheckFrontalCollision()
    {
        //check for obstacles in front of the ship using sphere cast
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
                //calculate the avoidance direction based on the collision normal and the ship's forward direction
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
        //the goal here is to make it so that the enemy ships are under the same movement restrictions as the player, and have to calculate and move positions using the constraints of a real ship
        
        
        if(anchored) return;

        //update the ship's speed and position based on the current rigging speed and wind direction
        float turnFactor = Mathf.Clamp01(Mathf.Abs(currentRudderAngle)/maxRudderAngle);
        float targetSpeed = Mathf.Lerp(maxSpeed, speed/2, turnFactor);
        
        float windEffect = Vector3.Dot(transform.forward, wind.normalized);
        targetSpeed *= (1 + windEffect * 0.5f);

        //calculate the speed modifier (sails/rigging settings) based on the distance to the player ship
        float emergencySpeedModifier = 1f;
        if(Physics.SphereCast(transform.position, 15f, transform.forward, 
            out RaycastHit frontHit, emergencyBrakeDistance, obstacleMask))
        {
            float brakePower = Mathf.Clamp01(1 - (frontHit.distance / emergencyBrakeDistance));
            emergencySpeedModifier = Mathf.Lerp(1f, emergencySpeedMultiplier, brakePower);
        }

        targetSpeed *= emergencySpeedModifier;

        //if the ship is derailing, apply a speed multiplier to recover
        if(derailmentTimer > 0)
        {
            targetSpeed *= recoverySpeedMultiplier;
            derailmentTimer -= Time.fixedDeltaTime;
        }

        if(derailmentTimer > 0 && rb.angularVelocity.magnitude < 0.1f)
        {
            rb.AddTorque(transform.up * currentRudderAngle * 0.5f, ForceMode.Acceleration);
        }

        if (playerShip != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerShip.position);
            if (distanceToPlayer > farDistanceThreshold)
            {
                targetSpeed *= farSpeedMultiplier;
            }
        }

        //smooth the rigging speed to avoid sudden changes
        currentRiggingSpeed = Mathf.MoveTowards(currentRiggingSpeed, targetSpeed, 0.5f * Time.fixedDeltaTime);
        
        rb.MovePosition(rb.position + transform.forward * currentRiggingSpeed * Time.fixedDeltaTime);
        
        //apply the rudder force to the ship's rigidbody
        if(navAgent != null && navAgent.enabled)
        {
            navAgent.nextPosition = transform.position;
            navAgent.velocity = rb.linearVelocity;
        }
    }

    //check if the ship is derailing from its intended course, usually from a collision or extreme winds
    void CheckCourseDeviation()
    {
        //check if the ship is derailing
        bool isMovingStraight = 
            Mathf.Abs(currentRudderAngle) < 5f && 
            rb.angularVelocity.magnitude < 1f &&
            Vector3.Distance(transform.position, targetPosition) > 10f;

        //check last valid pos
        float pathDeviation = Vector3.Distance(transform.position, lastValidPosition);
        if(pathDeviation > derailmentThreshold || isMovingStraight)
        {
            derailmentTimer = 3f;
            lastValidPosition = transform.position;
            
            //calculate a recovery direction based on the ship's current velocity and the last valid pos
            Vector3 recoveryDir = Quaternion.Euler(0, 90f, 0) * rb.linearVelocity.normalized;
            targetPosition = transform.position + recoveryDir * 25f;
            
            NavMeshHit hit;
            if(NavMesh.SamplePosition(targetPosition, out hit, 30f, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
            }
        }
        
        //check if the ship is close to the target position
        if(Vector3.Distance(transform.position, targetPosition) < positionTolerance * 2f)
        {
            lastValidPosition = transform.position;
        }
    }

    void acquireTarget()
    {
        //main target acquisition function, the goal is that the ship should try and pull up alongside the player to line up cannon shots


        //predict player positions
        Vector3 predictedPlayerPos = playerShip.position;
        Rigidbody playerRb = playerShip.GetComponent<Rigidbody>();
        
        if(playerRb != null) 
        {
            Vector3 toPlayer = playerShip.position - transform.position;
            float distanceFactor = Mathf.Clamp01(Vector3.Dot(playerRb.linearVelocity.normalized, toPlayer.normalized));
            predictedPlayerPos += playerRb.linearVelocity * Mathf.Clamp01(distanceFactor) * 1.5f;
        }

        //work out the shortest path to the player ship's flank (either side, whichever one is closer)
        float flankSide = Vector3.Dot(transform.position - predictedPlayerPos, playerShip.right) > 0 ? 1 : -1;
        Vector3 flankOffset = (playerShip.right * flankSide * targetDistance) + 
                            (-playerShip.forward * Mathf.Lerp(5f, 15f, currentAvoidance.magnitude/avoidanceForce));
        Vector3 idealPosition = predictedPlayerPos + flankOffset;

        //calculate the avoidance force based on the ship's current velocity and the ideal position
        Vector3 avoidanceInfluence = currentAvoidance.normalized * avoidanceRadius;
        Vector3 finalTarget = Vector3.Lerp(idealPosition, 
                                        idealPosition + avoidanceInfluence, 
                                        Mathf.Clamp01(currentAvoidance.magnitude / (avoidanceForce * 0.7f)));

        //check if the target position is valid and not obstructed by obstacles
        NavMeshHit hit;
        float sampleRadius = 25f;
        bool foundValid = false;
        
        //try to find a valid target position using navmesh sampling, 3 iterations
        for(int i = 0; i < 3; i++)
        {
            if(NavMesh.SamplePosition(finalTarget, out hit, sampleRadius, NavMesh.AllAreas))
            {
                //check if a generated path is valid and not obstructed by obstacles, if it is, use it
                if(IsPathSafe(hit.position) && 
                !Physics.CheckSphere(hit.position, islandAvoidanceRadius * 0.8f, obstacleMask))
                {
                    targetPosition = hit.position;
                    foundValid = true;
                    break;
                }
            }
            sampleRadius += 20f; //increase radius to search for a path for next iteration
        }

        if(!foundValid)
        {
            //if no valid path is found, use the original target position, and it will update again later
            if(!Physics.Linecast(transform.position, finalTarget, obstacleMask))
            {
                targetPosition = finalTarget;
            }
            else
            {
                Vector3 escapeDir = (transform.position - playerShip.position).normalized;
                targetPosition = transform.position + escapeDir * islandAvoidanceRadius * 2f;
            }
        }

        if(Physics.CheckSphere(targetPosition, islandAvoidanceRadius, obstacleMask))
        {
            //if it gets stuck on an island along the way, try to find a new path around it
            Vector3[] escapeVectors = {
                transform.right * islandAvoidanceRadius * 2f,
                -transform.right * islandAvoidanceRadius * 2f,
                transform.forward * islandAvoidanceRadius * 1.5f,
                -transform.forward * islandAvoidanceRadius * 1.5f
            };

            //loop through the escape vectors and find a valid escape route
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
    }

    void Idle()
    {
        //when far away, move slowly
        if(Vector3.Distance(transform.position, targetPosition) < holdCourseThreshold)
        {
            targetPosition = GetSafePatrolPoint();
        }
    }

    Vector3 GetSafePatrolPoint()
    {
        //when not in aggro, the ship will move slowly, this is overriden by the aggro function, causing the ship to move towards the player, but this will make it so that the ship will not get stuck on islands or obstacles when trying to find a path to the player ship, and move slower so that the player has time to breathe
        Vector3 bestPoint = idleCenterPosition;
        float bestScore = float.MinValue;
        
        //some more navmesh sampling
        for(int i = 0; i < 5; i++)
        {
            //generate a random point within the patrol radius, and check if it is valid, and creates a score for each one to figure out what the best move is
            Vector3 randomOffset = new Vector3(
                Random.Range(-patrolRadius, patrolRadius),
                0,
                Random.Range(-patrolRadius, patrolRadius)
            );
            
            Vector3 testPoint = idleCenterPosition + randomOffset;
            
            if(Physics.CheckSphere(testPoint, minObstacleDistance, obstacleMask))
                continue;

            //check if the point is within the patrol radius and not obstructed by obstacles
            NavMeshHit hit;
            if(NavMesh.SamplePosition(testPoint, out hit, patrolRadius * 2, NavMesh.AllAreas))
            {
                float obstacleDistance = GetObstacleDistance(hit.position);
                float centerDistance = Vector3.Distance(hit.position, idleCenterPosition);
                float score = obstacleDistance + (centerDistance * 0.5f);
                
                //iterate through the points and find the best one
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
        //check for obstacles in the way using sphere cast
        if(Physics.SphereCast(position, 1f, Vector3.forward, out RaycastHit hit, minObstacleDistance, obstacleMask))
        {
            return hit.distance;
        }
        return minObstacleDistance;
    }

    void Move()
    {
        //move using wind force and rigging speed
        float windEffect = Vector3.Dot(transform.forward, wind);
        float effectiveSpeed = currentRiggingSpeed + windEffect;
        effectiveSpeed = Mathf.Max(effectiveSpeed, speed);
        rb.MovePosition(rb.position + transform.forward * effectiveSpeed * Time.fixedDeltaTime);
    }

    void Navigate()
    {
        //main navigation function
        if(navAgent == null || !navAgent.enabled) return;
        try 
        {
            //check if the agent is on the navmesh, and warp it to the current position if it is not
            if(!navAgent.isOnNavMesh)
            {
                navAgent.Warp(transform.position);
                return;
            } 

            if(!isAggroed)
            {
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
                
                if(Time.time - timer > updateTimer * 2f)
                {
                    NavMesh.SamplePosition(targetPosition, out NavMeshHit sampleHit, patrolRadius * 2, NavMesh.AllAreas);
                    navAgent.SetDestination(sampleHit.position);
                    timer = Time.time;
                }
            } else {
                //adjust rigging speed based on distance to player ship
                currentRiggingSpeed = Mathf.Max(currentRiggingSpeed, speed * 0.8f);

                //check distance to player ship and adjust target position accordingly
                if(Time.time - timer > updateTimer || navAgent.remainingDistance < 5f)
                {
                    Vector3 finalTarget = Vector3.Lerp(targetPosition, 
                                                    playerShip.position, 
                                                    Mathf.Clamp01(currentAvoidance.magnitude/avoidanceForce));
                    
                    //check if the target position is valid and not obstructed by obstacles
                    NavMesh.SamplePosition(finalTarget, out NavMeshHit sampleHit, 25f, NavMesh.AllAreas);
                    navAgent.SetDestination(sampleHit.position);
                    timer = Time.time;
                }

                //create a path to the target position, validation is handled in acquireTarget() as above
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
        //calculate the rudder angle based on the target position and the ship's current velocity
        Vector3 toTarget = targetPosition - transform.position;
        if (toTarget.magnitude > positionTolerance)
        {
            //rudder angle is calculated based on the angle between the ship's forward direction and the target position
            Vector3 pursuitDir = (playerShip.position - transform.position).normalized;
            Vector3 desiredDir = Vector3.Lerp(toTarget.normalized, pursuitDir, pursuitAggression);

            //adjust rudder angle here
            float avoidanceWeight = Mathf.Clamp01(currentAvoidance.magnitude / avoidanceForce);
            if(derailmentTimer > 0) avoidanceWeight = Mathf.Clamp(avoidanceWeight * 1.5f, 0.5f, 1f);
            desiredDir = Vector3.Lerp(desiredDir, currentAvoidance.normalized, avoidanceWeight);

            //calculate the angle to the target position and apply turn multiplier based on the ship's current speed
            float angleToTarget = Vector3.SignedAngle(transform.forward, desiredDir, Vector3.up);
            float turnMultiplier = derailmentTimer > 0 ? emergencyTurnBoost : 1f;
            
            targetRudderAngle = Mathf.Clamp(angleToTarget * turnMultiplier, -maxRudderAngle, maxRudderAngle);

            if(Mathf.Abs(targetRudderAngle) < 5f && derailmentTimer > 0)
            {
                targetRudderAngle = rb.angularVelocity.y < 0 ? -45f : 45f;
            }
        }
    }

    bool IsPathSafe(Vector3 targetPos)
    {
        //path validation (again), this is a separate one from the patrol point and revalidation one, since the revalidation one is for being called quickly during the update loop AFTER applying forces, and this one is for checking if the path is valid BEFORE setting it as a target position
        // the other patrol one is for checking if the path is valid when the ship is "idle", which is when it is still moving toward the player, but paths are calculated differently since it is not actively trying to chase the player at full tilt just yet
        if(navAgent != null && navAgent.isOnNavMesh)
        {
            NavMeshPath path = new NavMeshPath();
            if(navAgent.CalculatePath(targetPos, path))
            {
                //check the parts of the path
                for(int i = 0; i < path.corners.Length - 1; i++)
                {
                    //work out if the path is valid
                    Vector3 segmentStart = path.corners[i];
                    Vector3 segmentEnd = path.corners[i+1];
                    //obstacle checking
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
        
        //calculate the rigging speed based on the wind direction and ship's forward direction
        float windAlignment = Vector3.Dot(transform.forward, wind.normalized);
        float targetMultiplier = Mathf.Lerp(0.5f, 1.5f, (windAlignment + 1)/2);
        
        //calculate the speed based on the distance to the player ship and the avoidance force
        float obstacleFactor = Mathf.Clamp01(currentAvoidance.magnitude / avoidanceForce);
        float minSpeed = Mathf.Lerp(speed, speed * 1.5f, obstacleFactor);

        if (playerShip != null)
        {
            //maintain a minimum speed, but slow down if far away from the player ship, speed up if close
            float distanceToPlayer = Vector3.Distance(transform.position, playerShip.position);
            if (distanceToPlayer > farDistanceThreshold)
            {
                minSpeed = 0.2f;
            }
        }
        
        //smooth the rigging speed
        float smoothedMultiplier = Mathf.Lerp(1f, targetMultiplier, Time.deltaTime * 2f);
        currentRiggingSpeed = Mathf.Clamp(
            currentRiggingSpeed * smoothedMultiplier,
            minSpeed, 
            maxSpeed
        );
    }
    
    void OnCollisionEnter(Collision collision)
    {
        //collisions will knock the ship off course, and it will stop being able to navigate properly, so we need to give it a way to recalculate and course correct
        if(collision.impulse.magnitude > 5f)
        {
            derailmentTimer = 3f;
            
            Vector3 collisionNormal = collision.contacts[0].normal;
            Vector3 velocityDir = rb.linearVelocity.normalized;
            Vector3 escapeDir = Vector3.Lerp(-collisionNormal, velocityDir, 0.3f).normalized;

            //escape direction is the opposite of the collision normal, so that the ship can find a way out quickly if the collision is strong enough
            Vector3[] testDirections = {
                escapeDir,
                Quaternion.Euler(0, 45f, 0) * escapeDir,
                Quaternion.Euler(0, -45f, 0) * escapeDir
            };

            //check each direction
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

            //invoke a delayed course update to re-acquire the target
            currentAvoidance += escapeDir * avoidanceForce * 3f;
            CancelInvoke("acquireTarget");
            Invoke("acquireTarget", 0.5f);

            if(collision.gameObject.CompareTag("Island"))
            {
                //push away from island center (breaks our ship rules a little bit, but it feels better in gameplay)
                Vector3 islandCenter = collision.transform.position;
                Vector3 islandEscapeDirection = (transform.position - islandCenter).normalized;
                islandEscapeDirection.y = 0;
                currentAvoidance += islandEscapeDirection * avoidanceForce * 4f;
                targetPosition = transform.position + islandEscapeDirection * 50f;
            }
        }
    }
}