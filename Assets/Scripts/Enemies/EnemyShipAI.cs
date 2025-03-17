using UnityEngine;
using UnityEngine.AI;

public class EnemyShipAI : MonoBehaviour
{
    public Transform player;  // Assign the player's Transform in the Inspector
    public float approachDistance = 20f;  // Distance to start pulling alongside
    public float alongsideDistance = 10f; // Desired distance from the player when alongside
    public float alongsideOffset = 5f;    // Offset to the side of the player
    public float rotationSpeed = 1f; // Adjust for smooth rotation towards target

    private NavMeshAgent agent;
    private enum State { Approaching, Aligning, Alongside }
    private State currentState = State.Approaching;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            Debug.LogError("Player Transform not assigned to EnemyShipAI!");
            enabled = false; // Disable the script if no player is assigned.
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Approaching:
                agent.SetDestination(player.position);
                agent.stoppingDistance = approachDistance; // Set stopping distance for approach

                if (distanceToPlayer <= approachDistance)
                {
                    currentState = State.Aligning;
                }
                break;

            case State.Aligning:
                // Calculate a point to the side of the player.
                Vector3 alongsidePoint = CalculateAlongsidePoint();
                agent.SetDestination(alongsidePoint);
                agent.stoppingDistance = alongsideDistance;  // Adjust for final alongside position

                // Check if we are facing the player and are close enough.
                if (distanceToPlayer <= approachDistance && IsFacingPlayer())
                {
                    currentState = State.Alongside;
                }

                 //Smoothly Rotate towards the target point
                SmoothRotation(alongsidePoint);
                break;
            case State.Alongside:
                //Maintain alongside position and heading.
                Vector3 targetPosition = CalculateAlongsidePoint();
                agent.SetDestination(targetPosition);
                SmoothRotation(targetPosition);

                 // Optional: Add some slight "bobbing" or forward movement here
                 // to make the ship feel more alive while alongside.

                // Re-engage if the player moves away.
                if (distanceToPlayer > approachDistance * 1.5f) // Give some leeway
                {
                    currentState = State.Approaching;
                }
                break;
        }
    }
    private Vector3 CalculateAlongsidePoint()
    {
         // Determine which side to approach (left or right).  A simple way is to use the dot product.
        Vector3 toPlayer = player.position - transform.position;
        float dotProduct = Vector3.Dot(transform.right, toPlayer.normalized);
        Vector3 offsetDirection = (dotProduct > 0) ? player.right : -player.right;

        // Calculate a point offset to the side of the player.
        return player.position + offsetDirection * alongsideOffset;
    }
    private bool IsFacingPlayer()
    {
        // Check if the enemy is roughly facing the same direction as the player.
        float angleDifference = Vector3.Angle(transform.forward, player.forward);
        return angleDifference < 45f;  // Consider it "facing" within a 45-degree cone. Adjust as needed.
    }

    private void SmoothRotation(Vector3 targetPosition)
    {
        // Calculate direction to the target
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Prevent rotation on the Y-axis, keeping the ship upright
        direction.y = 0;

        if (direction != Vector3.zero)
        {
             // Create a target rotation based on the direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly rotate towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
        //Helper function, draws a sphere around the target position.
     private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(CalculateAlongsidePoint(), 2f); // Visualize the target point
        }
    }

}