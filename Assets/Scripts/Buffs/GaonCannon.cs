using UnityEngine;
using System.Collections;

public class GaonCannon : MonoBehaviour
{
    [Header("Sphere Settings")]
    public GameObject spherePrefab;
    public float sphereSpeed = 5f;
    public float orbitRadius = 2f;
    public float convergeTime = 1.5f;

    [Header("Capsule Settings")]
    public GameObject capsulePrefab;
    public float capsuleSpeed = 10f;
    public float capsuleDuration = 0.5f;
    public float retractSpeed = 3f;

    [Header("Cooldown")]
    public float cooldown = 20f;
    
    private bool canShoot = true;
    private GameObject leftSphere;
    private GameObject rightSphere;
    private GameObject activeCapsule;

    void Update()
    {
        if (canShoot && Input.GetMouseButtonDown(2))
        {
            StartCoroutine(ChargeAndFire());
        }
    }

    IEnumerator ChargeAndFire()
    {
        canShoot = false;
        
        SpawnOrbitingSpheres();
        
        yield return StartCoroutine(MoveSpheresToFront());
        
        float timeout = 5f;
        float timer = 0f;
        while ((leftSphere != null || rightSphere != null) && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        CleanupSpheres();
        yield return StartCoroutine(FireCapsule());
        
        yield return new WaitForSeconds(cooldown);
        canShoot = true;
    }

    void SpawnOrbitingSpheres()
    {
        Vector3 leftPos = transform.position - transform.right * orbitRadius;
        Vector3 rightPos = transform.position + transform.right * orbitRadius;

        leftSphere = Instantiate(spherePrefab, leftPos, Quaternion.identity, transform);
        rightSphere = Instantiate(spherePrefab, rightPos, Quaternion.identity, transform);
        
        leftSphere.AddComponent<SphereCollisionHandler>().Init(this);
        rightSphere.AddComponent<SphereCollisionHandler>().Init(this);
    }

    IEnumerator MoveSpheresToFront()
    {
        float t = 0f;
        Vector3 leftStart = leftSphere.transform.position;
        Vector3 rightStart = rightSphere.transform.position;
        Vector3 frontPosition = transform.position + transform.forward * 2f;

        while (t < 1f && leftSphere != null && rightSphere != null)
        {
            t += Time.deltaTime / convergeTime;
            
            leftSphere.transform.position = Vector3.Lerp(leftStart, frontPosition, t) + 
                                         transform.up * Mathf.Sin(t * Mathf.PI) * orbitRadius;
            rightSphere.transform.position = Vector3.Lerp(rightStart, frontPosition, t) + 
                                          transform.up * Mathf.Sin(t * Mathf.PI) * orbitRadius;
            
            yield return null;
        }
    }

    public void OnSpheresCollided()
    {
        if (leftSphere != null && rightSphere != null)
        {
            CleanupSpheres();
            StartCoroutine(FireCapsule());
        }
    }

    IEnumerator FireCapsule()
    {
        Vector3 spawnPos = transform.position + transform.forward * 2f;
        activeCapsule = Instantiate(capsulePrefab, spawnPos, transform.rotation);
        
        float timer = 0f;
        Vector3 startScale = activeCapsule.transform.localScale;
        Vector3 targetScale = new Vector3(startScale.x, startScale.y, startScale.z * 3f);

        while (timer < capsuleDuration)
        {
            activeCapsule.transform.localScale = Vector3.Lerp(startScale, targetScale, timer/capsuleDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        while (timer < 1f)
        {
            activeCapsule.transform.localScale = Vector3.Lerp(targetScale, startScale, timer);
            timer += Time.deltaTime * retractSpeed;
            yield return null;
        }

        Destroy(activeCapsule);
    }

    void CleanupSpheres()
    {
        if (leftSphere != null) Destroy(leftSphere);
        if (rightSphere != null) Destroy(rightSphere);
    }

    class SphereCollisionHandler : MonoBehaviour
    {
        private GaonCannon cannon;

        public void Init(GaonCannon cannon)
        {
            this.cannon = cannon;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("EnergySphere"))
            {
                cannon.OnSpheresCollided();
            }
        }
    }
}