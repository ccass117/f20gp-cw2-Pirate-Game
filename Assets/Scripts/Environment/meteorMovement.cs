using UnityEngine;

public class meteorMovement : MonoBehaviour
{
    public GameObject ball;  // Reference to the ball
    public GameObject shadow; // Reference to the shadow
    public float forceMagnitude = 10f;  // The magnitude of the applied force

    private void Start()
    {
        // Ensure the ball and shadow are referenced if not set via inspector
        if (ball == null) ball = transform.Find("meteor").gameObject;
        if (shadow == null) shadow = transform.Find("shadow").gameObject;

        // Apply random directional force to the meteor
        ApplyRandomForce();
    }

    private void ApplyRandomForce()
    {
        // Random direction vector
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        // Apply force to the ball (and shadow if needed)
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.AddForce(randomDirection * forceMagnitude, ForceMode.Impulse);
        }

        // Optionally, you can apply force to the shadow as well (if it has a Rigidbody)
        Rigidbody shadowRb = shadow.GetComponent<Rigidbody>();
        if (shadowRb != null)
        {
            shadowRb.AddForce(randomDirection * forceMagnitude, ForceMode.Impulse);
        }
    }
}
