using UnityEngine;
using System.Collections;

public class Cannons : MonoBehaviour
{
    [Header("Cannon Positioning (Relative to Ship)")]
    public float shipLength = 1f;
    public float yOffset = 0f;
    public float xOffset = 0f;
    public float zOffset = 0f;
    public float cannonScale = 1f;

    [Header("Cannon Statistics")]
    public int cannonsPerSide = 1;
    public float cooldownTime = 3f;
    public float timeBetweenShots = 0.1f;
    public float shotSpeed = 10f;
    public float shotGravityMult = 1f;
    public float shotSizeMult = 1f;
    public float maxAngleDifference = 0f;
    public int ballsFiredPerCannon = 1;
    
    [Header("Detection Settings")]
    [Tooltip("Distance from cannon within which the target must be detected")]
    public float cannonDetectionRange = 50f;
    [Tooltip("Layers to check when raycasting for targets")]
    public LayerMask detectionLayers;
    [Tooltip("Assign the target ship (if left unassigned and this object is not a Player, the first object tagged 'Player' will be used)")]
    public Transform target;
    [Tooltip("Set true if this cannons component is on an enemy ship")]
    public bool isEnemy = true;
    public int ballsFiredPerCannon = 1;

    [Header("Prefabs")]
    public GameObject cannonPrefab;
    public GameObject cannonballPrefab;

    private GameObject leftCannons;
    private GameObject rightCannons;

    private float leftCooldown = 0f;
    private float rightCooldown = 0f;

    private Rigidbody shipRb;

    void Start()
    {
        //If this object is not a player, it is an enemy, so the cannon should automatically target the player with shots
        if (!gameObject.CompareTag("Player") && target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }

        shipRb = GetComponent<Rigidbody>();
        InitializeCannons();
    }

    void Update()
    {
        if (leftCooldown > 0)
            leftCooldown -= Time.deltaTime;
        if (rightCooldown > 0)
            rightCooldown -= Time.deltaTime;

        if (target != null)
        {
            foreach (Transform cannon in leftCannons.transform)
            {
                if (targetAcquired(cannon))
                {
                    FireLeft();
                    break;
                }
            }

            foreach (Transform cannon in rightCannons.transform)
            {
                if (targetAcquired(cannon))
                {
                    FireRight();
                    break;
                }
            }
        }
    }

    bool targetAcquired(Transform cannon)
    {
        RaycastHit hit;
        Debug.DrawRay(cannon.position, cannon.forward * cannonDetectionRange, Color.red, 0.5f);
        if (Physics.Raycast(cannon.position, cannon.forward, out hit, cannonDetectionRange, detectionLayers))
        {
            if (hit.transform == target || hit.transform.IsChildOf(target))
            {
                return true;
            }
        }
        return false;
    }

    // SETTERS: (Use these if powerups change cannon stats, etc.)
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

    public void setCannonScale(float newCannonScale)
    {
        cannonScale = newCannonScale;
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

        foreach (Transform child in leftCannons.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in rightCannons.transform)
        {
            Destroy(child.gameObject);
        }

        if (cannonsPerSide < 1)
            return;

        float cannonSpacing = cannonsPerSide > 1 ? shipLength / (cannonsPerSide - 1) : 0;
        float angleDeviation = cannonsPerSide > 1 ? 2 * maxAngleDifference / (cannonsPerSide - 1) : 0;

        for (int i = cannonsPerSide - 1; i >= 0; i--)
        {
            float zPosition = cannonsPerSide > 1 ? -shipLength / 2 + i * cannonSpacing : 0;
            float angle = cannonsPerSide > 1 ? -maxAngleDifference + i * angleDeviation : 0;

            Vector3 rightLocalPosition = new Vector3(xOffset, yOffset, zPosition);
            Vector3 rightWorldPosition = transform.TransformPoint(rightLocalPosition);
            GameObject rightCannon = Instantiate(cannonPrefab, rightWorldPosition, transform.rotation * Quaternion.Euler(0, 90 - angle, 0), rightCannons.transform);
            rightCannon.transform.localScale *= cannonScale;

            Vector3 leftLocalPosition = new Vector3(-xOffset, yOffset, zPosition);
            Vector3 leftWorldPosition = transform.TransformPoint(leftLocalPosition);
            GameObject leftCannon = Instantiate(cannonPrefab, leftWorldPosition, transform.rotation * Quaternion.Euler(0, -90 + angle, 0), leftCannons.transform);
            leftCannon.transform.localScale *= cannonScale;
        }
    }

    public void FireLeft()
    {
        if (leftCooldown <= 0)
        {
            StartCoroutine(Fire(leftCannons, ballsFiredPerCannon));
            leftCooldown = cooldownTime;
        }
    }

    public void FireRight()
    {
        if (rightCooldown <= 0)
        {
            StartCoroutine(Fire(rightCannons, ballsFiredPerCannon));
            rightCooldown = cooldownTime;
        }
    }

    private IEnumerator Fire(GameObject cannons, int timesToFire)
    {
        for (int i = 0; i < timesToFire; i++)
        {
            foreach (Transform cannon in cannons.transform)
        {
            GameObject cannonball = Instantiate(cannonballPrefab, cannon.position, cannon.rotation);

            Cannonball cannonballScript = cannonball.GetComponent<Cannonball>();
            if (cannonballScript != null)
            {
                cannonballScript.firingShip = this.gameObject;
                cannonballScript.gravityMultiplier = shotGravityMult;
            }

            cannonball.transform.localScale *= shotSizeMult;

            Rigidbody rb = cannonball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (shipRb != null)
                {
                    rb.linearVelocity = shipRb.linearVelocity + (cannon.forward * shotSpeed);
                }
                else
                {
                    rb.linearVelocity = cannon.forward * shotSpeed;
                }
            }

            //Disable collisions between cannonball and this ship
            Collider cannonballCollider = cannonball.GetComponent<Collider>();
            if (cannonballCollider != null)
            {
                Collider[] shipColliders = GetComponentsInChildren<Collider>();
                foreach (Collider col in shipColliders)
                {
                    Physics.IgnoreCollision(cannonballCollider, col);
                }
            }

            yield return new WaitForSeconds(timeBetweenShots);
        }
    }
}