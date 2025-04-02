using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cannons : MonoBehaviour
{
    [Header("Cannon Positioning (Relative to Ship)")]
    public float shipLength = 2f;
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

    [Header("Auto-Aim Options")]
    public float autoAimFOV = 0f;
    public float autoAimRange = 5f;
    public float autoAimTurnSpeed = 1f;
    private Transform leftAimTarget = null;
    private Transform rightAimTarget = null;




    [Header("Detection Settings")]
    [Tooltip("Distance from cannon within which the target must be detected")]
    public float cannonDetectionRange = 50f;
    [Tooltip("Layers to check when raycasting for targets")]
    public LayerMask detectionLayers;
    [Tooltip("Assign the target ship. If unassigned and this object is not the Player, it will automatically target the GameObject tagged 'Player'")]
    public Transform target;
    [Tooltip("Set true if this cannons component is on an enemy ship")]
    public bool isEnemy = true;

    [Header("Prefabs")]
    public GameObject cannonPrefab;
    public GameObject cannonballPrefab;

    private GameObject leftCannons;
    private GameObject rightCannons;

    private float leftCooldown = 0f;
    private float rightCooldown = 0f;

    private Rigidbody shipRb;

    private Dictionary<Transform, Quaternion> defaultRotations = new Dictionary<Transform, Quaternion>();  // Stores the default rotation of each cannon to be returned to if there is no auto aim target.

    void Start()
    {
        if (!gameObject.CompareTag("Player") && target == null && isEnemy)
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

        if (autoAimFOV > 0 && autoAimRange > 0)
        {
            leftAimTarget = FindAutoAimTarget(-transform.right);
            AimCannons(leftCannons.transform, leftAimTarget);

            rightAimTarget = FindAutoAimTarget(transform.right);
            AimCannons(rightCannons.transform, rightAimTarget);
        }

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

    Transform FindAutoAimTarget(Vector3 direction)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, autoAimRange, detectionLayers);
        Transform nearestTarget = null;
        float nearestDistance = autoAimRange;

        foreach (Collider hit in hits)
        {
            Vector3 toTarget = hit.transform.position - transform.position;
            float angleToTarget = Vector3.Angle(direction, toTarget);

            if (angleToTarget <= (autoAimFOV/2))
            {
                float distance = toTarget.magnitude;
                if (distance < nearestDistance)
                {
                    nearestTarget = hit.transform;
                    nearestDistance = distance;
                }
            }
        }

        return nearestTarget;
    }

    void AimCannons(Transform cannons, Transform target)
    // Aims cannons for a respective side (left or right) if a target exists for that side, else resets the cannons to the default angle stored in defaultRotations.
    {
        foreach (Transform cannon in cannons)
        {
            Quaternion targetRotation;
            if (target != null)
            {
                Vector3 directionToTarget = (target.position - cannon.position).normalized;
                targetRotation = Quaternion.LookRotation(directionToTarget);
            }
            else
            {
                targetRotation = defaultRotations.ContainsKey(cannon) ? transform.rotation * defaultRotations[cannon] : cannon.rotation;
            }
            cannon.rotation = Quaternion.RotateTowards(cannon.rotation, Quaternion.Euler(0, targetRotation.eulerAngles.y, 0), autoAimTurnSpeed * Time.deltaTime);
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

    // SETTERS for dynamic cannon stat changes.
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

        defaultRotations.Clear();
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

        float effectiveLength = shipLength * 0.5f;
        float cannonSpacing = cannonsPerSide > 1 ? effectiveLength / (cannonsPerSide - 1) : 0;
        float startZ = -effectiveLength * 0.5f;

        // No angle deviation is used now since cannons face directly along the ship's z axis.
        for (int i = cannonsPerSide - 1; i >= 0; i--)
        {
            // Position cannons between -shipLength/2+margin and shipLength/2-margin.
            float zPosition = startZ + i * cannonSpacing;
            
            // Instantiate right cannon with fixed rotation (facing right)
            Vector3 rightLocalPosition = new Vector3(xOffset, yOffset, zPosition);
            Vector3 rightWorldPosition = transform.TransformPoint(rightLocalPosition);
            GameObject rightCannon = Instantiate(cannonPrefab, rightWorldPosition, transform.rotation * Quaternion.Euler(0, 90, 0), rightCannons.transform);
            rightCannon.transform.localScale *= cannonScale;
            defaultRotations[rightCannon.transform] = Quaternion.Inverse(transform.rotation) * rightCannon.transform.rotation;

            // Instantiate left cannon with fixed rotation (facing left)
            Vector3 leftLocalPosition = new Vector3(-xOffset, yOffset, zPosition);
            Vector3 leftWorldPosition = transform.TransformPoint(leftLocalPosition);
            GameObject leftCannon = Instantiate(cannonPrefab, leftWorldPosition, transform.rotation * Quaternion.Euler(0, -90, 0), leftCannons.transform);
            leftCannon.transform.localScale *= cannonScale;
            defaultRotations[leftCannon.transform] = Quaternion.Inverse(transform.rotation) * leftCannon.transform.rotation;
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
}