using UnityEngine;

public class Tornado : MonoBehaviour
{

    [Header("Movement Settings")]
    public Vector3 forwardDirection; 
    public float speed = 5f;
    public float amplitude = 2f;
    public float frequency = 1f;

    private Vector3 startPosition;
    private float elapsedTime = 0f; // Track time since spawn

    [Header("Repulsion Settings")]
    [Tooltip("Radius within which the tornado repels objects.")]
    public float repulsionRadius = 5f;
    [Tooltip("Force applied to repulse objects.")]
    public float repulsionForce = 500f;

    void Start()
    {
        startPosition = transform.position;
        forwardDirection.Normalize();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        
        Vector3 forwardMovement = forwardDirection * speed * elapsedTime;
        Vector3 oscillation = transform.right * Mathf.Sin(elapsedTime * frequency) * amplitude;
        
        transform.position = startPosition + forwardMovement + oscillation;
    }

    void FixedUpdate()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, repulsionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Cannonball"))
            {
                Rigidbody rb = hit.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 pushDirection = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(pushDirection * repulsionForce, ForceMode.Impulse);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, repulsionRadius);
    }
}
