using UnityEngine;
using System.Collections;
using System;

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
    public float anchorRaiseTime = 3f;
    public float maxTurnBoost = 2f;


    public float sirenTurnStrength = 0.75f;

    [Header("Read Only")]
    [SerializeField] private float targetRudderAngle;
    [SerializeField] private float currentRiggingSpeed;
    [SerializeField] private float currentRudderAngle = 0f;
    [SerializeField] private bool anchored = false;
    [SerializeField] private bool isRaisingAnchor = false;
    [SerializeField] private Vector3 wind = Vector3.zero;
    [SerializeField] private float anchorTurnMomentum = 0f;

    [SerializeField] private AudioSource anchorRaiseSFX;
    [SerializeField] private AudioSource anchorDropSFX;

    public bool sirenInfluenceActive = false;
    public Transform sirenTarget = null;


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

        //Siren influence override
        if (sirenInfluenceActive && sirenTarget != null)
        {
            Vector3 toSiren = sirenTarget.position - transform.position;
            toSiren.y = 0;

            float angleToSiren = Vector3.SignedAngle(transform.forward, toSiren.normalized, Vector3.up);

            // Apply a portion of this angle as a rudder influence
            float influence = Mathf.Clamp(angleToSiren / maxRudderAngle, -1f, 1f);
            float sirenRudder = influence * maxRudderAngle * sirenTurnStrength;

            // Blend in the Siren influence with player input
            targetRudderAngle += sirenRudder;
            targetRudderAngle = Mathf.Clamp(targetRudderAngle, -maxRudderAngle, maxRudderAngle);
        }

        currentRudderAngle = Mathf.MoveTowards(
            currentRudderAngle,
            targetRudderAngle,
            rudderSpeed * Time.deltaTime
        );

        // Anchor Control (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!anchored && !isRaisingAnchor)
            {
                //drop anchor
                anchored = true;
                //maintain turnrate
                anchorDropSFX.Play();
                float turnRate = maxTurnRate * (currentRudderAngle / maxRudderAngle);
                anchorTurnMomentum = turnRate;
                Debug.Log("Anchor Dropped");
            }
            else if (anchored && !isRaisingAnchor)
            {
                // Start raising anchor
                StartCoroutine(RaiseAnchor());
            }
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
            //stop forward movement
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, anchorForce * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);

            // Add extra turn boost for more spin when anchored
            float turnBoost = maxTurnBoost * (currentRiggingSpeed / maxSpeed);
            if (Mathf.Abs(anchorTurnMomentum) > 0.01f)
            {
                //add extra turn boost to the current momentum
                float boostedTurnMomentum = anchorTurnMomentum * turnBoost;

                //apply the boosted turn momentum to the ship's rotation
                Quaternion targetRot = rb.rotation * Quaternion.Euler(0, boostedTurnMomentum * Time.fixedDeltaTime, 0);
                rb.MoveRotation(targetRot);

                //gradually reduce the spin (momentum) of the turn
                anchorTurnMomentum = Mathf.Lerp(anchorTurnMomentum, 0f, 1f * Time.fixedDeltaTime);
            }
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

    private IEnumerator RaiseAnchor()
    {
        //Begin raising anchor
        isRaisingAnchor = true;
        Debug.Log("Raising anchor...");
        anchorRaiseSFX.Play();

        yield return new WaitForSeconds(anchorRaiseTime);

        anchorRaiseSFX.Stop();
        anchored = false;
        isRaisingAnchor = false;
        float elapsed = 0f;
        float restoreDuration = 0.5f;

        Debug.Log("Anchor Raised");
        //build up to min speed over 0.5 seconds
        float targetRiggingSpeed = currentRiggingSpeed;
        while (elapsed < restoreDuration)
        {
            elapsed += Time.deltaTime;
            currentRiggingSpeed = Mathf.Lerp(0f, targetRiggingSpeed, elapsed / restoreDuration);
            yield return null;
        }      
    }
}