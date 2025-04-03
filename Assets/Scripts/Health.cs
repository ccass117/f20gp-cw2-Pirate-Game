using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.UI;

//global health script, this is designed to be attached to any object where combat and damage is concerned, and is configurable for player, enemy, and projectile objects
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
    private GameObject levelLoader;
    void Awake()
    {
        levelLoader = GameObject.Find("LevelLoader");
        //if attached to player, use PlayerData.currentHealth from PlayerData.cs, otherwise use maxHealth
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

            //buff modifiers, see BuffController.cs
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

        //apply damage resistance
        float finalDamage = amount * damageResistance;
        currentHealth -= finalDamage;
        lastDamageTime = Time.time;

        //if player, update PlayerData.currentHealth
        if (gameObject.CompareTag("Player") && Time.timeSinceLevelLoad >= 1)
        {
            PlayerData.currentHealth = currentHealth;
            if (hitSfx != null)
            {
                hitSfx.pitch = Random.Range(0.5f, 1.0f);
                hitSfx.Play();
            }
        }


        //if enemy, play sound if within 15 units of player
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
                        float volume = Mathf.Clamp01(1.0f - (distanceToPlayer / 15f));

                        hitSfx.volume = volume;
                        hitSfx.pitch = Random.Range(0.5f, 1.0f);
                        hitSfx.Play();
                    }
                }
            }
        }

        if (currentHealth <= 0)
        {
            Die(); // :(
        }
    }

    //heal, this is only ever used for the player in practice
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (gameObject.CompareTag("Player"))
        {
            PlayerData.currentHealth = currentHealth;
        }
    }

    //health drops to zero, and the object sinks below the water, and then is disabled after a couple seconds
    private void Die()
    {
        GoldManager.AddGold(goldAmount);
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            //turn on gravity so it falls
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
            StartCoroutine(Fall(rb));
        }
        if (gameObject.CompareTag("Player"))
        {
            LevelLoader loaderScript = levelLoader.GetComponent<LevelLoader>();
            PlayerData.currentHealth = 0;
            loaderScript.LoadLevel("lose");
        }
        else
        {
            StartCoroutine(Disable(2f));
        }
    }

    private IEnumerator Fall(Rigidbody rb)
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        float fallTime = 3f;
        float elapsedTime = 0f;
        Vector3 startPosition = rb.transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y - 8f, startPosition.z);

        while (elapsedTime < fallTime)
        {
            rb.transform.position = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / fallTime));
            rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, Quaternion.Euler(-90, 0, 0), Time.deltaTime * 2f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Disable(float delay)
    {
         yield return new WaitForSeconds(delay);
         gameObject.SetActive(false);
    }

    //getter
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    //the main way damage works, when two objects collide they exchange velocities and resistances, and then we call this to apply damage correclty to each other based on combined velocity and resistance
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
        //if one of the colliding objects is a projectile, the handshake becomes one way, the projectile does damage to the other object, and the other object does not do damage to the projectile
        //if both are projectiles, they do damage to each other
        if (isProjectile || (otherHealth != null && otherHealth.isProjectile))
        {
            if (isProjectile && (otherHealth == null || !otherHealth.isProjectile))
            {
                return;
            }
            if (!isProjectile && otherHealth != null && otherHealth.isProjectile)
            {
                //if it's a projectile, damageMultiplier is used as a flat damage number
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

    //the exact same as on collider enter, but has a few use cases for things like fire breath on the hydra
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
