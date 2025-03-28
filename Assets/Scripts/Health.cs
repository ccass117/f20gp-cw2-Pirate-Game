using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the object")]
    public float maxHealth = 20f;
    
    [SerializeField, Tooltip("Current health of the object")]
    private float currentHealth;

    [Header("Damage Multipliers")]
    [Tooltip("Damage resistance multiplier e.g. 1 = full damage taken, 0.5 = 50%, 0 = doesn't take damage")]
    [Range(0f, 1f)]
    public float damageResistance = 1f;
    
    [Tooltip("Generic multiplier applied to damage when colliding with anything. Usage is to make an object do more damage, increase this value")]
    public float damageMultiplier = 1f;

    [Header("Projectile Settings")]
    [Tooltip("If true, this object is considered a projectile (like a cannonball)")]
    public bool isProjectile = false;

    [Header("Damage Cooldown")]
    [Tooltip("Minimum time between taking damage (0 = no cooldown)")]
    public float damageCooldown = 0.5f;
    private float lastDamageTime = -Mathf.Infinity;

    [Header("Speed Thresholds")]
    [Tooltip("Minimum speed this object needs to move to deal damage")]
    public float attackerSpeedThreshold = 0f;
    [Tooltip("Minimum speed attackers need to be moving to damage this object")]
    public float defenderSpeedThreshold = 0f;

    [Tooltip("If true, head region blocks damage but deals damage on contact")]
    [SerializeField] private bool useHeadHitDetection = false;
    [Tooltip("Layer for objects that should collide with this object")]
    public LayerMask collisionLayers;
    [Tooltip("If true, this object will block movement like a wall")]
    public bool isSolidObstacle = true;

    void Awake()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float amount, GameObject damageSource = null)
    {
        if (damageCooldown > 0 && Time.time < lastDamageTime + damageCooldown)
        {
            return;
        }

        float finalDamage = amount * damageResistance;
        currentHealth -= finalDamage;
        lastDamageTime = Time.time;
        
        string sourceName = damageSource != null ? damageSource.name : "unknown source";
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        gameObject.SetActive(false);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void DamageHandshake(Health other, float mySpeed, float otherSpeed)
    {
        float damageToMe = (mySpeed + otherSpeed) * other.damageMultiplier;
        TakeDamage(damageToMe, other.gameObject);
    }

    public void OnCollisionEnter(Collision collision)
    {
        Rigidbody rbSelf = GetComponent<Rigidbody>();
        float mySpeed = (rbSelf != null) ? rbSelf.linearVelocity.magnitude : 0f;
        float otherSpeed = (collision.rigidbody != null) ? collision.rigidbody.linearVelocity.magnitude : 0f;
        Health otherHealth = collision.gameObject.GetComponent<Health>();

        bool attackerValid = mySpeed >= attackerSpeedThreshold;
        bool defenderValid = otherSpeed >= defenderSpeedThreshold;
        
        if (!attackerValid && !defenderValid) return;

        if (isProjectile || (otherHealth != null && otherHealth.isProjectile))
        {
            if (isProjectile && (otherHealth == null || !otherHealth.isProjectile))
            {
                return;
            }
            if (!isProjectile && otherHealth != null && otherHealth.isProjectile)
            {
                float projectileDamage = otherHealth.damageMultiplier;
                TakeDamage(projectileDamage, otherHealth.gameObject);
                return;
            }
            if (isProjectile && otherHealth != null && otherHealth.isProjectile)
            {
                TakeDamage(damageMultiplier, otherHealth.gameObject);
                return;
            }
        }

        else
        {
            if (otherHealth != null)
            {
                DamageHandshake(otherHealth, mySpeed, otherSpeed);
            }
            else
            {
                float damage = (mySpeed + otherSpeed) * 1f;
                TakeDamage(damage, collision.gameObject);
            }
        }
    }
}