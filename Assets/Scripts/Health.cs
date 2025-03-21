using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the object")]
    public float maxHealth = 20f;
    
    [SerializeField, Tooltip("Current health of the object")]
    private float currentHealth;

    [Header("Damage Multipliers")]
    [Tooltip("Damage resistance multiplier e.g. 1 = regular damage taken, 0.5 = 50%, 0 = doesn't take damage")]
    [Range(0f, 1f)]
    public float damageResistance = 1f;
    
    [Tooltip("Generic multiplier applied to damage when colliding with anything. Usage is to make an object do more damage, increase this value")]
    public float damageMultiplier = 1f;

    [Header("Projectile Settings")]
    [Tooltip("If true, this object is considered a projectile (like a cannonball) and will ignore velocity based damage")]
    public bool isProjectile = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        float finalDamage = amount * damageResistance;
        currentHealth -= finalDamage;
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
        gameObject.SetActive(false);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void DamageHandshake(Health other, float mySpeed, float otherSpeed)
    {
        float damageToMe = (mySpeed + otherSpeed) * other.damageMultiplier;
        TakeDamage(damageToMe);
    }

    void OnCollisionEnter(Collision collision)
    {
        Rigidbody rbSelf = GetComponent<Rigidbody>();
        float mySpeed = (rbSelf != null) ? rbSelf.linearVelocity.magnitude : 0f;
        float otherSpeed = (collision.rigidbody != null) ? collision.rigidbody.linearVelocity.magnitude : 0f;
        Health otherHealth = collision.gameObject.GetComponent<Health>();

        if (isProjectile || (otherHealth != null && otherHealth.isProjectile))
        {
            float projectileMultiplier = isProjectile ? damageMultiplier : (otherHealth != null ? otherHealth.damageMultiplier : 1f);
            TakeDamage(projectileMultiplier);
            if (otherHealth != null)
            {
                otherHealth.TakeDamage(projectileMultiplier);
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
                TakeDamage(damage);
            }
        }
    }
}