using UnityEngine;

public class meteorRock : MonoBehaviour
{
    bool splashed = false;
    public GameObject splashPrefab;

    private void Update()
    {
        if (transform.position.y <= 0 && !splashed)
        {
            Instantiate(splashPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z), Quaternion.identity);
            splashed = true;
        }

        if (transform.position.y <= -50)
        {
            Destroy(transform.parent.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("fireball shadow"))
        {
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            // Destroy the meteor
            Destroy(transform.parent.gameObject);
        }
    }
}