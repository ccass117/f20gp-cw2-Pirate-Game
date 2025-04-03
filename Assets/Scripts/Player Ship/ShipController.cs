using UnityEngine;
using System.Collections;
using System;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;

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
    public GameObject anchorFlail;

    public bool sirenInfluenceActive = false;
    public Transform sirenTarget = null;

    private Rigidbody rb;
    private Vector3 currentVelocity;
    private Cannons cannons;


    private static bool canDoubleCannons = true;
    private static bool canIncreaseTf2 = true;
    private static bool canIncreaseSuperglue = true;

    private Health health;

    [Header("Wind Resistance")]
    public float windResistance = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cannons = GetComponent<Cannons>();
        currentRiggingSpeed = speed;
        currentVelocity = Vector3.zero;
        health = GetComponent<Health>();

        // ################################################################################
        // #                                                                              #
        // #                                     Buffs                                    #
        // #                                                                              #
        // ################################################################################

        // #########################################
        // #            HP Related Buffs           #
        // #########################################

        if (BuffController.registerBuff("Reinforced Hull", "Increases maximum HP by 10"))
        {
            health.maxHealth += 10f;
            health.currentHealth += 10f;
            BuffController.setInactive("Reinforced Hull");
        }

        if (BuffController.registerBuff("Quick Repair", "Heal 10HP"))
        {
            health.currentHealth += 10f;
            if (health.currentHealth > health.maxHealth)
            {
                health.currentHealth = health.maxHealth;
            }
            Debug.Log("Quick Repair activated: currentHealth now " + health.currentHealth);
            BuffController.setInactive("Quick Repair");
        }

        if (BuffController.registerBuff("Theseus's Prodigy", "Fully repair your ship... is it even the same one anymore?"))
        {
            health.currentHealth = health.maxHealth;
            Debug.Log("Theseus's Prodigy activated: currentHealth set to maxHealth " + health.maxHealth);
            BuffController.setInactive("Theseus's Prodigy");
        }

        // #########################################
        // #                Movement               #
        // #########################################

        if (BuffController.registerBuff("Calm Winds", "Make winds affect the player less")) { windResistance -= 0.5f; }


        if (BuffController.registerBuff("Sobered up", "Negate the effect of Sirens' pull")) { sirenTurnStrength = 0; }

        if (BuffController.registerBuff("Noise Cancelling Earbuds", "Reduces the effect of Sirens' pull")) { sirenTurnStrength -= 0.4f; }

        if (BuffController.registerBuff("Rocket Boost", "Allows you to rocket forward every 15 seconds, giving a burst of speed"))
        {
            RocketBoost.ActivateRocketBoost();
            Debug.Log("Rocket Boost activated");
        }

        if (BuffController.registerBuff("Suspicious Needle", "Greatly reduces time taken to raise the anchor")) { anchorRaiseTime -= 1.25f; }

        if (BuffController.registerBuff("PEDs", "Reduces time taken to raise the anchor")) { anchorRaiseTime -= 0.75f; }

        // #########################################
        // #               Offensive               #
        // #########################################



        if (BuffController.registerBuff("Anchor Flail", "What if we just used the anchor as a mace instead?"))
        {
            Debug.Log("Spawning Anchor Flail");
            Instantiate(anchorFlail, new Vector3(0, 0, 0), Quaternion.identity);  
        }
        
        if (BuffController.registerBuff("Gaon Cannon", "Fires a high damage laser from the front of your ship every 20 seconds"))
        {
            GaonCannon.ActivateLaserBuff();
            Debug.Log("Gaon Cannon activated");
        }

        if (BuffController.registerBuff("Tube of Superglue", "You can't just glue on another cannon and expect it to work"))
        {
            cannons = GetComponent<Cannons>();
            cannons.cannonsPerSide += 1;
            cannons.InitializeCannons();
        }

        if (BuffController.registerBuff("TF2 Engineer", "Add an additional cannon"))
        {
            Debug.Log("running");
            cannons = GetComponent<Cannons>();
            cannons.cannonsPerSide += 1;
            cannons.InitializeCannons();
        }

        if (BuffController.registerBuff("Double Decker Cannons", "Double the amount of cannons"))
        {
            cannons = GetComponent<Cannons>();
            cannons.cannonsPerSide *= 2;
            cannons.InitializeCannons();
        }

        if (BuffController.registerBuff("Black Powder", "Increase the power of the cannons"))
        {
            cannons = GetComponent<Cannons>();
            cannons.shotSpeed = 20f;
            cannons.InitializeCannons();
        }

        if (BuffController.registerBuff("Kilogram of feathers", "Reduces cannon reload speed"))
        {
            cannons = GetComponent<Cannons>();
            cannons.cooldownTime -= 1f;
        }

        if (BuffController.registerBuff("Exponential Stupidity", "Locks your max hp at 1, Gain 1.5x more cannons per area"))
        {
            health = GetComponent<Health>();
            health.currentHealth = 1;
            health.maxHealth = 1;
            GameObject wind = GameObject.Find("Wind");
            WindMgr windmgr = wind.GetComponent<WindMgr>();
            cannons.cannonsPerSide = Mathf.CeilToInt(windmgr.cannonCount * 1.5f); //TODO fix..ed (doesnt work with double)?
            if (BuffController.registerBuff("TF2 Engineer", "Add an additional cannon") && canIncreaseTf2)
            {
                cannons.cannonsPerSide += 1;
                canIncreaseTf2 = false;
            }
            if (BuffController.registerBuff("Tube of Superglue", "You can't just glue on another cannon and expect it to work") && canIncreaseSuperglue)
            {
                cannons.cannonsPerSide += 1;
                canIncreaseSuperglue = false;
            }
            if (BuffController.registerBuff("Double Decker Cannons", "Double the amount of cannons") && canDoubleCannons)
            {
                cannons.cannonsPerSide *= 2;
                canDoubleCannons = false;
            }

            cannons.InitializeCannons();
        }

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
