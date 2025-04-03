using UnityEngine;

//applies upgrades from the gold shop to the player ship
public class UpgradeApplier : MonoBehaviour
{
    public int healthIncreasePerTier = 5;
    
    public float speedIncreasePerTier = 0.5f;
    
    public float reloadTimeDecreasePerTier = 0.1f;
    public float minReloadTime = 0.1f;
    
    public float windResistanceDecreasePerTier = 0.1f;
    public float minWindResistance = 0.2f;
    
    public float turnSpeedIncreasePerTier = 1f;
    
    //every instance works the same here, just grab the attribute from the player script and do whatever maths to increase the values by the ones from the gold shop purchases
    //called on scene load, and is attached to wind because that's the object that persists through scenes    
    void Start()
    {
        int healthTiers = PlayerPrefs.GetInt("HealthUpgradeTiers", 0);
        Health healthComp = GetComponent<Health>();
        if (healthComp != null)
        {
            healthComp.maxHealth += healthTiers * healthIncreasePerTier;
        }
        
        int speedTiers = PlayerPrefs.GetInt("SpeedUpgradeTiers", 0);
        ShipController shipController = GetComponent<ShipController>();
        if (shipController != null)
        {
            shipController.speed += speedTiers * speedIncreasePerTier;
            shipController.maxSpeed += speedTiers * speedIncreasePerTier;
        }
        
        int reloadTiers = PlayerPrefs.GetInt("ReloadUpgradeTiers", 0);
        Cannons cannons = GetComponent<Cannons>();
        if (cannons != null)
        {
            float newCooldown = cannons.cooldownTime - (reloadTiers * reloadTimeDecreasePerTier);
            cannons.cooldownTime = Mathf.Max(newCooldown, minReloadTime);
        }
        
        int windTiers = PlayerPrefs.GetInt("WindUpgradeTiers", 0);
        if (shipController != null)
        {
            float newWindResistance = Mathf.Max(1f - windTiers * windResistanceDecreasePerTier, minWindResistance);
            shipController.windResistance = newWindResistance;
        }
        
        int turnSpeedTiers = PlayerPrefs.GetInt("TurnSpeedUpgradeTiers", 0);
        if (shipController != null)
        {
            shipController.maxTurnRate += turnSpeedTiers * turnSpeedIncreasePerTier;
        }
        
        int extraCannonFlag = PlayerPrefs.GetInt("ExtraCannonPurchased", 0);
        if (cannons != null && extraCannonFlag == 1)
        {
            cannons.cannonsPerSide += 1;
            cannons.InitializeCannons();
        }
    }
}