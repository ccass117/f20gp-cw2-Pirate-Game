using UnityEngine;

public class ShipController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float maxSpeed = 5f;
    public float riggingSpeed = 2f;
    public float anchorForce = 3f;
    public float rudderReturnSpeed = 60f;
    public float maxRudderAngle = 90f;
    public float rudderSpeed = 45f;
    public float maxTurnRate = 15f;
    public float turnDamping = 0.9f;

    [Header("Read Only")]
    [SerializeField] private float targetRudderAngle;
    [SerializeField] private float currentRiggingSpeed;
    [SerializeField] private float currentRudderAngle = 0f;
    [SerializeField] private bool anchored = false;
    [SerializeField] private Vector3 wind = Vector3.zero;

    private Rigidbody rb;
    private Vector3 currentVelocity;
    private Cannons cannons;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cannons = GetComponent<Cannons>();
        currentRiggingSpeed = speed;
        currentVelocity = Vector3.zero;
    }

    void Update()
    {
        playerMovement();
        windEffect();
        playerWeapons();

        // TEMPORARY DEV CHEAT: PRESS SPACE TO RE-INITIALISE CANNONS (regenerate cannons with current stats, useful for editing cannons via inspector in play mode)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            cannons.InitializeCannons();
        }
    }

    private void FixedUpdate()
    {
        applyForces();
    }

    void playerMovement()
    {
        //Sail Control (W/S)
        if (!anchored)
        {
            if (Input.GetKey(KeyCode.W))
                currentRiggingSpeed = Mathf.Min(maxSpeed, currentRiggingSpeed + riggingSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.S))
                currentRiggingSpeed = Mathf.Max(speed, currentRiggingSpeed - riggingSpeed * Time.deltaTime);
        }

        //Rudder Control (A/D)
        targetRudderAngle = 0;
        if (Input.GetKey(KeyCode.A)) targetRudderAngle = maxRudderAngle;
        if (Input.GetKey(KeyCode.D)) targetRudderAngle = -maxRudderAngle;

        currentRudderAngle = Mathf.MoveTowards(
            currentRudderAngle,
            targetRudderAngle,
            rudderSpeed * Time.deltaTime
        );

        //Anchor Control (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            anchored = !anchored;
            Debug.Log("Anchor " + (anchored ? "Dropped" : "Raised"));
        }
    }

    void playerWeapons()
    {
        // Cannon firing
        if (Input.GetKeyDown(KeyCode.Q))
        {
            cannons.FireLeft();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            cannons.FireRight();
        }
    }   


    void windEffect() //integrates this script with WindMgr
    {
        wind = WindMgr.Instance.windDir * WindMgr.Instance.windStrength;
    }

    void applyForces()
    {
        if (anchored)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, anchorForce * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
        }
        else
        {
            float forwardSpeed = currentRiggingSpeed + Vector3.Dot(transform.forward, wind);
            currentVelocity = transform.forward * forwardSpeed;

            float turnRate = maxTurnRate * (currentRudderAngle / maxRudderAngle);
            Quaternion targetRot = rb.rotation * Quaternion.Euler(0, turnRate * Time.fixedDeltaTime, 0);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, turnDamping));

            rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
        }
    }
}