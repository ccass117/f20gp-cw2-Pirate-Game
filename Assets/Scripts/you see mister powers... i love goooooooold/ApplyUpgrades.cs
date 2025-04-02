using UnityEngine;

public class UpgradeApplier : MonoBehaviour
{
    [Header("Health Upgrade Settings")]
    public int healthIncreasePerTier = 5;
    
    [Header("Speed Upgrade Settings")]
    public float speedIncreasePerTier = 0.5f;
    
    [Header("Cannon Reload Upgrade Settings")]
    public float reloadTimeDecreasePerTier = 0.1f;
    public float minReloadTime = 0.1f;
    
    [Header("Wind Resistance Upgrade Settings")]
    [Tooltip("Each tier reduces wind effect by reducing the windResistance value.")]
    public float windResistanceDecreasePerTier = 0.1f;
    public float minWindResistance = 0.2f;
    
    [Header("Turn Speed Upgrade Settings")]
    public float turnSpeedIncreasePerTier = 1f;
    
    
    void Start()
    {
        int healthTiers = PlayerPrefs.GetInt("HealthUpgradeTiers", 0);
        Health healthComp = GetComponent<Health>();
        if (healthComp != null)
        {
            healthComp.maxHealth += healthTiers * healthIncreasePerTier;
            healthComp.currentHealth = healthComp.maxHealth;
            Debug.Log("Applied health upgrade: " + healthTiers + " tiers, +" + (healthTiers * healthIncreasePerTier) + " max health.");
        }
        else
        {
            Debug.LogWarning("No Health component found on player.");
        }
        
        int speedTiers = PlayerPrefs.GetInt("SpeedUpgradeTiers", 0);
        ShipController shipController = GetComponent<ShipController>();
        if (shipController != null)
        {
            shipController.speed += speedTiers * speedIncreasePerTier;
            shipController.maxSpeed += speedTiers * speedIncreasePerTier;
            Debug.Log("Applied speed upgrade: " + speedTiers + " tiers, +" + (speedTiers * speedIncreasePerTier) + " speed.");
        }
        else
        {
            Debug.LogWarning("No ShipController component found on player.");
        }
        
        int reloadTiers = PlayerPrefs.GetInt("ReloadUpgradeTiers", 0);
        Cannons cannons = GetComponent<Cannons>();
        if (cannons != null)
        {
            float newCooldown = cannons.cooldownTime - (reloadTiers * reloadTimeDecreasePerTier);
            cannons.cooldownTime = Mathf.Max(newCooldown, minReloadTime);
            Debug.Log("Applied reload upgrade: " + reloadTiers + " tiers, new cannon cooldown: " + cannons.cooldownTime);
        }
        else
        {
            Debug.LogWarning("No Cannons component found on player.");
        }
        
        int windTiers = PlayerPrefs.GetInt("WindUpgradeTiers", 0);
        if (shipController != null)
        {
            float newWindResistance = Mathf.Max(1f - windTiers * windResistanceDecreasePerTier, minWindResistance);
            shipController.windResistance = newWindResistance;
            Debug.Log("Applied wind resistance upgrade: " + windTiers + " tiers, new wind resistance: " + shipController.windResistance);
        }
        else
        {
            Debug.LogWarning("No ShipController component found for wind resistance upgrade on player.");
        }
        
        int turnSpeedTiers = PlayerPrefs.GetInt("TurnSpeedUpgradeTiers", 0);
        if (shipController != null)
        {
            shipController.maxTurnRate += turnSpeedTiers * turnSpeedIncreasePerTier;
            Debug.Log("Applied turn speed upgrade: " + turnSpeedTiers + " tiers, new max turn rate: " + shipController.maxTurnRate);
        }
        else
        {
            Debug.LogWarning("No ShipController component found for turn speed upgrade on player.");
        }
        
        int extraCannonFlag = PlayerPrefs.GetInt("ExtraCannonPurchased", 0);
        if (cannons != null && extraCannonFlag == 1)
        {
            cannons.cannonsPerSide += 1;
            cannons.InitializeCannons();
            Debug.Log("Applied extra cannon upgrade: new cannons per side: " + cannons.cannonsPerSide);
        }
    }
}