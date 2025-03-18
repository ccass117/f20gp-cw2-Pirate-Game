using UnityEngine;
using UnityEngine.AI;

public class EnemyShipAI : MonoBehaviour
{
    public Transform player;
    public float approachDistance = 20f;
    public float alongsideDistance = 10f;
    public float rotationSpeed = 1f;
    public float baseSpeed = 5f;
    public float minSpeed = 2f;
    public float windEffect = 0.5f;
    public bool enableRamming = false;
    public float rammingDistance = 5f;
    public float rammingSpeedMultiplier = 1.5f;

    [Header ("Ship Stats")]
    public float maxHealth = 50f;
    public float health;

    private NavMeshAgent agent;
    private enum State { Approaching, Aligning, Alongside, Ramming } //Finite state machine for movement behaviours, add combat states to this later
    private State currentState = State.Approaching;
    private float originalSpeed;

void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = maxHealth;
        agent.speed = baseSpeed;
        originalSpeed = baseSpeed;
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            player = playerGameObject.transform;
        }
        else
        {
            Debug.LogError("L + plundered + marooned (you need to assign the player tag)");
            enabled = false;
        }
    }

    void Update()
    {
        float windSpeedMod = windSpeedModify();
        //Briefly ignore wind effects if ramming (maybe? We'll need to see how this feels in gameplay)
        if (currentState != State.Ramming)
        {
            agent.speed = Mathf.Clamp(baseSpeed * windSpeedMod, minSpeed, baseSpeed * (1 + windEffect));
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Approaching:
                agent.SetDestination(player.position);
                agent.stoppingDistance = approachDistance;

                if (enableRamming && distanceToPlayer <= rammingDistance)
                {
                    currentState = State.Ramming;
                    originalSpeed = agent.speed;
                    agent.speed *= rammingSpeedMultiplier;
                }
                else if (distanceToPlayer <= approachDistance)
                {
                    currentState = State.Aligning;
                }
                break;

            case State.Aligning:
                Vector3 alongsidePoint = pullUpOnDaOpps();
                agent.SetDestination(alongsidePoint);
                agent.stoppingDistance = alongsideDistance;

                if (enableRamming && distanceToPlayer <= rammingDistance)
                {
                    currentState = State.Ramming;
                    originalSpeed = agent.speed;
                    agent.speed *= rammingSpeedMultiplier;
                }
                else if (distanceToPlayer <= approachDistance && facingPlayer())
                {
                    currentState = State.Alongside;
                }
                 smoothRotation(alongsidePoint);

                break;

            case State.Alongside:
                Vector3 targetPosition = pullUpOnDaOpps();
                agent.SetDestination(targetPosition);
                smoothRotation(targetPosition);

                if (enableRamming && distanceToPlayer <= rammingDistance)
                {
                    currentState = State.Ramming;
                    originalSpeed = agent.speed;
                    agent.speed *= rammingSpeedMultiplier;
                }
                else if (distanceToPlayer > approachDistance * 1.5f)
                {
                    currentState = State.Approaching;
                }
                break;

            case State.Ramming:
                agent.SetDestination(player.position);
                agent.stoppingDistance = 0f;
                smoothRotation(player.position);

                if (!enableRamming || distanceToPlayer > rammingDistance * 2f)
                {
                    currentState = State.Approaching;
                    agent.speed = originalSpeed;
                }
                break;
        }
    }

    private Vector3 pullUpOnDaOpps()
    {
        Vector3 toPlayer = player.position - transform.position;
        float dotProduct = Vector3.Dot(transform.right, toPlayer.normalized);
        Vector3 offsetDirection = (dotProduct > 0) ? player.right : -player.right;
        return player.position + offsetDirection;
    }

    private bool facingPlayer()
    {
        float angleDifference = Vector3.Angle(transform.forward, player.forward);
        return angleDifference < 45f;
    }

  private void smoothRotation(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private float windSpeedModify()
    {
        Vector3 windDirection = WindMgr.Instance.windDir;
        float dotProduct = Vector3.Dot(transform.forward, windDirection);
        float speedModifier = 1 + (dotProduct * windEffect);
        return speedModifier;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        Debug.Log("Enemy took " + damage + " damage. Health = " + health);

        if (health <= 0)
        {
            //Play an animation or somethin idk
            Destroy(gameObject); //For now
        }
    }
}