using UnityEngine;

public class meteorRock : MonoBehaviour
{
    bool splashed = false;
    public GameObject splashPrefab;
    // Update is called once per frame

    private void Start()
    {
    }
    private void Update()
    {
        if (transform.position.y <= 0 && !splashed)
        {
            Instantiate(splashPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z), Quaternion.identity);
            splashed = !splashed;
        }
        
        if (transform.position.y <= -50)
        {

            Destroy(transform.parent.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // Check if the other object has the "fireball shadow" tag
        if (other.CompareTag("fireball shadow"))
        {
            // Destroy the object with the "fireball shadow" tag
            Destroy(other.gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the other object is tagged as "player"
        if (collision.gameObject.CompareTag("Player"))
        {

            // Example action: Destroy the object
            Destroy(transform.parent.gameObject);
        }
    }
}
