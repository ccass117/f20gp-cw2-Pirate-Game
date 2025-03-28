using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Megalodon : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;

    [Header("Circling Behavior")]
    public float circleRadius = 15f;
    public float circlingSpeed;

    [Header("Attack Behavior")]
    public float attackInterval = 10f;
    public float attackDuration = 3f;
    public float normalSpeed = 4f;
    public float attackSpeed = 10f;
    public float predictionTime = 1f;
    public float baseOvershootMultiplier = 1.2f;

    [Header("Phase 2 Behavior")]
    public float phase2Threshold = 0.5f;
    public float phase2AttackInterval = 6f;
    public float phase2AttackSpeed = 15f;
    public int phase2AttackChainCount = 2;
    public float chainAttackDelay = 1f;
    public float phase2OvershootMultiplier = 1.5f;

    [Header("Bounce Settings")]
    public float bounceDistance = 10f;          // How far to bounce away from the player
    public float bounceRecoveryTime = 2f;         // Duration to remain in bounce state

    [Header("Model Settings")]
    public Vector3 rotationOffset = new Vector3(0, 90, 0);  // Adjust so the model's front faces movement direction

    private NavMeshAgent agent;
    private float attackTimer;
    private float attackTimeRemaining;
    public bool isAttacking { get; private set; }
    private float orbitAngle;
    private Vector3 attackTargetPosition;
    private bool phase2Active;
    private int remainingChainAttacks;
    private float originalAttackInterval;
    private float originalAttackSpeed;

    [Header("Components & Settings")]
    [SerializeField] private Collider mainBodyCollider;
    [SerializeField] private Collider attackHitbox;
    [SerializeField] private float positionSyncThreshold = 0.1f;
    [SerializeField] private Transform bodyChild;
    [SerializeField] private Health bodyHealth;  // Health component on the body child

    private Vector3 lastFramePosition;

    // Bounce state variables:
    private bool isBouncing = false;
    private Vector3 bounceTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // We'll control rotation manually.
        if (!mainBodyCollider) mainBodyCollider = GetComponentInChildren<Collider>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
            else
                Debug.LogWarning("Megalodon: Player not found.");
        }
        attackHitbox.enabled = false;
        originalAttackInterval = attackInterval;
        originalAttackSpeed = attackSpeed;
        attackTimer = attackInterval;
        agent.speed = normalSpeed;
        circlingSpeed = normalSpeed / circleRadius;

        SyncBodyPosition();
    }

    void Update()
    {
        // If in bounce state, override normal behavior.
        if (isBouncing)
        {
            agent.SetDestination(bounceTarget);
            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                Quaternion bounceRot = Quaternion.LookRotation(agent.velocity.normalized) * Quaternion.Euler(rotationOffset);
                transform.rotation = Quaternion.Slerp(transform.rotation, bounceRot, Time.deltaTime * 10f);
            }
            // During bounce, do nothing else.
            return;
        }

        // Disable boss if health reaches zero.
        if (bodyHealth != null && bodyHealth.GetCurrentHealth() <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        // Phase 2 transition.
        if (!phase2Active && (bodyHealth.GetCurrentHealth() / bodyHealth.maxHealth) <= phase2Threshold)
        {
            EnterPhase2();
        }

        if (Vector3.Distance(transform.position, lastFramePosition) > positionSyncThreshold)
        {
            SyncBodyPosition();
            lastFramePosition = transform.position;
        }

        if (!isAttacking)
        {
            attackTimer -= Time.deltaTime;
            orbitAngle += circlingSpeed * Time.deltaTime;
            Vector3 targetPos = player.position + new Vector3(Mathf.Cos(orbitAngle), 0f, Mathf.Sin(orbitAngle)) * circleRadius;
            agent.SetDestination(targetPos);

            // Face movement direction.
            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized) * Quaternion.Euler(rotationOffset);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            if (attackTimer <= 0f)
            {
                StartAttack();
            }
        }
        else
        {
            attackTimeRemaining -= Time.deltaTime;
            agent.SetDestination(attackTargetPosition);
            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized) * Quaternion.Euler(rotationOffset);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }
            if (attackTimeRemaining <= 0f)
            {
                EndAttack();
            }
        }
    }

    // Keep the body child at the parent's origin.
    private void SyncBodyPosition()
    {
        if (bodyChild != null)
            bodyChild.localPosition = Vector3.zero;
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimeRemaining = attackDuration;
        agent.speed = phase2Active ? phase2AttackSpeed : attackSpeed;
        attackHitbox.enabled = true;

        // Predict target position.
        Vector3 playerVelocity = GetPlayerVelocity();
        Vector3 predictedPosition = player.position + playerVelocity * predictionTime;
        float currentOvershoot = phase2Active ? phase2OvershootMultiplier : baseOvershootMultiplier;
        attackTargetPosition = predictedPosition + (predictedPosition - transform.position).normalized * currentOvershoot;

        // Snap target to NavMesh.
        NavMeshHit hit;
        if (NavMesh.SamplePosition(attackTargetPosition, out hit, 2f, NavMesh.AllAreas))
        {
            attackTargetPosition = hit.position;
        }
    }

    void EndAttack()
    {
        if (phase2Active && remainingChainAttacks > 0)
        {
            remainingChainAttacks--;
            attackTimer = chainAttackDelay;
            agent.speed = phase2AttackSpeed;
            StartAttack();
        }
        else
        {
            attackHitbox.enabled = false;
            attackTimer = phase2Active ? phase2AttackInterval : originalAttackInterval;
            agent.speed = phase2Active ? phase2AttackSpeed : originalAttackSpeed;
            remainingChainAttacks = phase2AttackChainCount;
            Vector3 dir = transform.position - player.position;
            orbitAngle = Mathf.Atan2(dir.z, dir.x);
            isAttacking = false;
        }
    }

    void EnterPhase2()
    {
        phase2Active = true;
        attackInterval = phase2AttackInterval;
        attackSpeed = phase2AttackSpeed;
        remainingChainAttacks = phase2AttackChainCount;
        if (!isAttacking)
        {
            StartAttack();
        }
    }

    Vector3 GetPlayerVelocity()
    {
        if (player.TryGetComponent<Rigidbody>(out Rigidbody rb))
            return rb.linearVelocity;
        if (player.TryGetComponent<CharacterController>(out CharacterController cc))
            return cc.velocity;
        if (player.TryGetComponent<NavMeshAgent>(out NavMeshAgent nma))
            return nma.velocity;
        return Vector3.zero;
    }

    void OnCollisionEnter(Collision collision)
    {
        // When colliding with the player, enter bounce state.
        if (collision.gameObject.CompareTag("Player"))
        {
            BounceAway();
            return;
        }

        if (bodyHealth != null)
            bodyHealth.OnCollisionEnter(collision);
    }

    void BounceAway()
    {
        // Prevent multiple bounces.
        if (isBouncing) return;
        isBouncing = true;

        // Cancel any current attack.
        isAttacking = false;
        attackHitbox.enabled = false;

        // Calculate bounce target away from the player.
        Vector3 awayDirection = (transform.position - player.position).normalized;
        bounceTarget = transform.position + awayDirection * bounceDistance;

        // Immediately rotate to face away.
        transform.rotation = Quaternion.LookRotation(awayDirection) * Quaternion.Euler(rotationOffset);

        // Optionally, set the agent speed for bouncing.
        agent.speed = normalSpeed;

        // Start a cooldown coroutine to remain in bounce state for a fixed duration.
        StartCoroutine(BounceRecovery());
    }

    IEnumerator BounceRecovery()
    {
        // Wait for the bounce recovery period.
        yield return new WaitForSeconds(bounceRecoveryTime);
        isBouncing = false;
        // Reset attack timer so the shark doesn't immediately re-attack.
        attackTimer = originalAttackInterval;
    }
}