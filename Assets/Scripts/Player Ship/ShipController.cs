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
    public float maxRudderAngle = 90f;
    public float rudderSpeed = 45f;
    public float maxTurnRate = 15f;
    public float turnDamping = 0.9f;
    public float anchorRaiseTime = 3f;
    public float maxTurnBoost = 2f;
    public float sirenTurnStrength = 1.2f;

    [Header("Read Only")]
    [SerializeField] public float targetRudderAngle;
    [SerializeField] public float currentRiggingSpeed;
    [SerializeField] private float currentRudderAngle = 0f;
    public bool anchored = false;
    public bool isRaisingAnchor = false;
    [SerializeField] private Vector3 wind = Vector3.zero;
    [SerializeField] private float anchorTurnMomentum = 0f;

    [SerializeField] private AudioSource anchorRaiseSFX;
    [SerializeField] private AudioSource anchorDropSFX;

    public bool sirenInfluenceActive = false;
    public Transform sirenTarget = null;

    private Rigidbody rb;
    private Vector3 currentVelocity;
    private Cannons cannons;
    
    [Header("Wind Resistance")]
    public float windResistance = 1f;

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
    }

    private void FixedUpdate()
    {
        applyForces();
    }

    void playerMovement()
    {
        if (!anchored)
        {
            if (Input.GetKey(KeyCode.W))
                currentRiggingSpeed = Mathf.Min(maxSpeed, currentRiggingSpeed + riggingSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.S))
                currentRiggingSpeed = Mathf.Max(speed, currentRiggingSpeed - riggingSpeed * Time.deltaTime);
        }

        targetRudderAngle = 0;
        if (Input.GetKey(KeyCode.A)) targetRudderAngle = maxRudderAngle;
        if (Input.GetKey(KeyCode.D)) targetRudderAngle = -maxRudderAngle;

        if (sirenInfluenceActive && sirenTarget != null)
        {
            Vector3 toSiren = sirenTarget.position - transform.position;
            toSiren.y = 0;
            float angleToSiren = Vector3.SignedAngle(transform.forward, toSiren.normalized, Vector3.up);
            float influence = Mathf.Clamp(angleToSiren / maxRudderAngle, -1f, 1f);
            float sirenRudder = influence * maxRudderAngle * sirenTurnStrength;
            targetRudderAngle += sirenRudder;
            targetRudderAngle = Mathf.Clamp(targetRudderAngle, -maxRudderAngle, maxRudderAngle);
        }

        currentRudderAngle = Mathf.MoveTowards(
            currentRudderAngle,
            targetRudderAngle,
            rudderSpeed * Time.deltaTime
        );

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!anchored && !isRaisingAnchor)
            {
                anchored = true;
                anchorDropSFX.Play();
                float turnRate = maxTurnRate * (currentRudderAngle / maxRudderAngle);
                anchorTurnMomentum = turnRate;
                Debug.Log("Anchor Dropped");
            }
            else if (anchored && !isRaisingAnchor)
            {
                StartCoroutine(RaiseAnchor());
            }
        }
    }

    void playerWeapons()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            cannons.FireLeft();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            cannons.FireRight();
        }
    }

    void windEffect()
    {
        wind = WindMgr.Instance.windDir * WindMgr.Instance.windStrength * windResistance;
    }

    void applyForces()
    {
        if (anchored)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, anchorForce * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
            float turnBoost = maxTurnBoost * (currentRiggingSpeed / maxSpeed);
            if (Mathf.Abs(anchorTurnMomentum) > 0.01f)
            {
                float boostedTurnMomentum = anchorTurnMomentum * turnBoost;
                Quaternion targetRot = rb.rotation * Quaternion.Euler(0, boostedTurnMomentum * Time.fixedDeltaTime, 0);
                rb.MoveRotation(targetRot);
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
        isRaisingAnchor = true;
        anchorRaiseSFX.Play();
        yield return new WaitForSeconds(anchorRaiseTime);
        anchorRaiseSFX.Stop();
        anchored = false;
        isRaisingAnchor = false;
        float elapsed = 0f;
        float restoreDuration = 0.5f;
        float targetRiggingSpeed = currentRiggingSpeed;
        while (elapsed < restoreDuration)
        {
            elapsed += Time.deltaTime;
            currentRiggingSpeed = Mathf.Lerp(0f, targetRiggingSpeed, elapsed / restoreDuration);
            yield return null;
        }
    }
}
