using UnityEngine;

public class Cannons : MonoBehaviour
{
    [Header("Cannon Positioning (Relative to Ship)")]
    public float shipLength = 1f;
    public float yOffset = 0f;
    public float xOffset = 0f;
    public float zOffset = 0f;

    [Header("Cannon Statistics")]
    public int cannonsPerSide = 1;
    public float cooldownTime = 3f;
    public float shotSpeed = 10f;
    public float shotDamageBase = 1f;
    public float shotDamageMult = 1f;
    public float shotGravityMult = 1f;
    public float shotSizeMult = 1f;
    public float maxAngleDifference = 0f;

    [Header("Prefabs")]
    public GameObject cannonPrefab;
    public GameObject cannonballPrefab;

    private GameObject leftCannons;
    private GameObject rightCannons;

    private float leftCooldown = 0f;
    private float rightCooldown = 0f;

    private Rigidbody shipRigidbody;

    void Start()
    {
        shipRigidbody = GetComponent<Rigidbody>();
        InitializeCannons();
    }

    void Update()
    {
        if (leftCooldown > 0)
        {
            leftCooldown -= Time.deltaTime;
        }

        if (rightCooldown > 0)
        {
            rightCooldown -= Time.deltaTime;
        }
    }

    // SETTERS: USE THESE IF POWERUPS CHANGE THESE STATS TO REGENERATE CANNONS WITH THE NEW STATS!!!

    public void setShipLength(float newLength)
    {
        shipLength = newLength;
        InitializeCannons();
    }

    public void setYOffset(float newYOffset)
    {
        yOffset = newYOffset;
        InitializeCannons();
    }

    public void setXOffset(float newXOffset)
    {
        xOffset = newXOffset;
        InitializeCannons();
    }

    public void setZOffset(float newZOffset)
    {
        zOffset = newZOffset;
        InitializeCannons();
    }

    public void setCannonsPerSide(int newCannonsPerSide)
    {
        cannonsPerSide = newCannonsPerSide;
        InitializeCannons();
    }

    public void setMaxAngleDifference(float newMaxAngleDifference)
    {
        maxAngleDifference = newMaxAngleDifference;
        InitializeCannons();
    }


    public void InitializeCannons()
    {
        if (leftCannons == null)
        {
            leftCannons = new GameObject("leftCannons");
            leftCannons.transform.SetParent(transform);
            rightCannons = new GameObject("rightCannons");
            rightCannons.transform.SetParent(transform);
        }

        // Destroy all existing cannons
        foreach (Transform child in leftCannons.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in rightCannons.transform)
        {
            Destroy(child.gameObject);
        }

        if (cannonsPerSide < 1)
        {
            return;
        }

        float cannonSpacing = cannonsPerSide > 1 ? shipLength / (cannonsPerSide - 1) : 0; // set cannon spacing to 0 if there is only 1 cannon per side
        float angleDeviation = cannonsPerSide > 1 ? 2 * maxAngleDifference / (cannonsPerSide - 1) : 0;

        for (int i = 0; i < cannonsPerSide; i++)
        {
            float zPosition = cannonsPerSide > 1 ? -shipLength / 2 + i * cannonSpacing : 0; // cannons are spaced out evenly along the ship's length if there are multiple per side
            float angle = cannonsPerSide > 1 ? -maxAngleDifference + i * angleDeviation : 0; // cannons are angled evenly between -maxAngleDifference and maxAngleDifference if there are multiple per side

            // Right side cannons
            Vector3 rightLocalPosition = new Vector3(xOffset, yOffset, zPosition);
            Vector3 rightWorldPosition = transform.TransformPoint(rightLocalPosition);
            Instantiate(cannonPrefab, rightWorldPosition, transform.rotation * Quaternion.Euler(0, 90 - angle, 0), rightCannons.transform);

            // Left side cannons
            Vector3 leftLocalPosition = new Vector3(-xOffset, yOffset, zPosition);
            Vector3 leftWorldPosition = transform.TransformPoint(leftLocalPosition);
            Instantiate(cannonPrefab, leftWorldPosition, transform.rotation * Quaternion.Euler(0, -90 + angle, 0), leftCannons.transform);
        }
    }

    public void FireLeft()
    {
        if (leftCooldown <= 0)
        {
            Fire(leftCannons);
            leftCooldown = cooldownTime;
        }
    }

    public void FireRight()
    {
        if (rightCooldown <= 0)
        {
            Fire(rightCannons);
            rightCooldown = cooldownTime;
        }
    }

    void Fire(GameObject cannons)
    {
        foreach (Transform cannon in cannons.transform)
        {
            // Instantiate a cannonball
            GameObject cannonball = Instantiate(cannonballPrefab, cannon.position, cannon.rotation);

            // Adjust the size of the cannonball
            cannonball.transform.localScale *= shotSizeMult;

            // Apply velocity to the cannonball
            Rigidbody rb = cannonball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (shipRigidbody != null)
                {
                    rb.linearVelocity = shipRigidbody.linearVelocity + (cannon.forward * shotSpeed);
                }
                else
                {
                    rb.linearVelocity = cannon.forward * shotSpeed;
                }
                rb.useGravity = true;
                rb.mass *= shotGravityMult;
            }

            // Set the damage of the cannonball
            Cannonball cannonballScript = cannonball.GetComponent<Cannonball>();
            if (cannonballScript != null)
            {
                cannonballScript.damage = shotDamageBase * shotDamageMult;
            }
        }
    }



}