using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the object")]
    public float maxHealth = 20f;

    [SerializeField, Tooltip("Current health of the object")]
    public float currentHealth;

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

    [Header("Gold Drop")]
    [Tooltip("Amount of gold this enemy drops on death")]
    public int goldAmount = 0;

    [Header("Speed Thresholds")]
    [Tooltip("Minimum speed this object needs to move to deal damage")]
    public float attackerSpeedThreshold = 0f;
    [Tooltip("Minimum speed attackers need to be moving to damage this object")]
    public float defenderSpeedThreshold = 0f;

    [Header("for player and enemy")]
    public AudioSource hitSfx;

    void Awake()
    {
        // If this Health script is attached to the player, use PlayerData to set currentHealth.
        if (gameObject.CompareTag("Player"))
        {
            if (PlayerData.currentHealth >= 0)
            {
                currentHealth = PlayerData.currentHealth;
            }
            else
            {
                currentHealth = maxHealth;
                PlayerData.currentHealth = maxHealth;
            }

            if (BuffController.registerBuff("Strong Hull", "Increase the resistance to damage")) { damageResistance = 0.5f; }

            if (BuffController.registerBuff("Cast Iron Figurehead", "Increase ramming dammage")) { damageMultiplier = 2f; }
        }
        else
        {
            currentHealth = maxHealth;
        }
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

        // If this is the player, update PlayerData.
        if (gameObject.CompareTag("Player") && Time.timeSinceLevelLoad >= 1)
        {
            PlayerData.currentHealth = currentHealth;
            if (hitSfx != null)
            {
                hitSfx.pitch = Random.Range(0.5f, 1.0f);
                hitSfx.Play();
            }
        }


        if (gameObject.CompareTag("Enemy"))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (distanceToPlayer <= 15f)
                {
                    if (hitSfx != null)
                    {
                        // Normalize volume between 0 and 1
                        float volume = Mathf.Clamp01(1.0f - (distanceToPlayer / 15f));

                        hitSfx.volume = volume; // Set the volume
                        hitSfx.pitch = Random.Range(0.5f, 1.0f);
                        hitSfx.Play();
                    }
                }
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // If this is the player, update PlayerData.
        if (gameObject.CompareTag("Player"))
        {
            PlayerData.currentHealth = currentHealth;
        }
    }

    private void Die()
    {
        GoldManager.AddGold(goldAmount);
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
                float damage = (mySpeed + otherSpeed) * 1.5f;
                TakeDamage(damage, collision.gameObject);
            }
        }
    }

    public void OnTriggerEnter(Collider collision)
    {
        Rigidbody rbSelf = GetComponent<Rigidbody>();
        float mySpeed = (rbSelf != null) ? rbSelf.linearVelocity.magnitude : 0f;
        float otherSpeed = (collision.GetComponent<Rigidbody>() != null) ? collision.GetComponent<Rigidbody>().linearVelocity.magnitude : 0f;
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
