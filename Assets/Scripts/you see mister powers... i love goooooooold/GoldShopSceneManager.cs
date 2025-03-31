using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GoldShopSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text displaying the player's current gold.")]
    public TextMeshProUGUI currentGoldText;
    public TextMeshProUGUI totalCost;
    
    [Header("Health Upgrade UI")]
    public TextMeshProUGUI healthTierText;
    public TextMeshProUGUI healthCostText;
    public Button healthPlusButton;
    public Button healthMinusButton;
    
    [Header("Speed Upgrade UI")]
    public TextMeshProUGUI speedTierText;
    public TextMeshProUGUI speedCostText;
    public Button speedPlusButton;
    public Button speedMinusButton;
    
    [Header("Cannon Reload Upgrade UI")]
    public TextMeshProUGUI reloadTierText;
    public TextMeshProUGUI reloadCostText;
    public Button reloadPlusButton;
    public Button reloadMinusButton;
    
    [Header("Wind Resistance Upgrade UI")]
    public TextMeshProUGUI windTierText;
    public TextMeshProUGUI windCostText;
    public Button windPlusButton;
    public Button windMinusButton;
    
    [Header("Turn Speed Upgrade UI")]
    public TextMeshProUGUI turnSpeedTierText;
    public TextMeshProUGUI turnSpeedCostText;
    public Button turnSpeedPlusButton;
    public Button turnSpeedMinusButton;
    
    [Header("Extra Cannon Upgrade UI")]
    [Tooltip("Displays extra cannon info (e.g. cost or 'Purchased').")]
    public TextMeshProUGUI extraCannonText;
    public TextMeshProUGUI extraCannonCostText;
    public Button extraCannonPlusButton;
    public Button extraCannonMinusButton;

    [Header("Purchase & Dev")]
    public Button purchaseButton;
    [Tooltip("Developer button to reset all upgrades to zero.")]
    public Button resetButton;
    
    [Header("Upgrade Settings")]
    public int maxHealthTiers = 10;
    public int costPerHealthTier = 100;
    public int healthIncreasePerTier = 5;
    
    public int maxSpeedTiers = 10;
    public int costPerSpeedTier = 100;
    public float speedIncreasePerTier = 0.5f;
    
    public int maxReloadTiers = 10;
    public int costPerReloadTier = 100;
    public float reloadTimeDecreasePerTier = 0.1f;
    
    public int maxWindTiers = 10;
    public int costPerWindTier = 100;
    public float windResistanceIncreasePerTier = 0.1f;
    
    public int maxTurnSpeedTiers = 10;
    public int costPerTurnSpeedTier = 150;
    public float turnSpeedIncreasePerTier = 1f;

    public int maxExtraCannons = 1;
    public int extraCannonCost = 5000;
    
    private int additionalHealthTiers = 0;
    private int additionalSpeedTiers = 0;
    private int additionalReloadTiers = 0;
    private int additionalWindTiers = 0;
    private int additionalTurnSpeedTiers = 0;
    private int additionalCannon = 0; 
    
    private int currentHealthTiers = 0;
    private int currentSpeedTiers = 0;
    private int currentReloadTiers = 0;
    private int currentWindTiers = 0;
    private int currentTurnSpeedTiers = 0;
    private int currentExtraCannons = 0;
    
    void Start()
    {
        currentHealthTiers = PlayerPrefs.GetInt("HealthUpgradeTiers", 0);
        currentSpeedTiers = PlayerPrefs.GetInt("SpeedUpgradeTiers", 0);
        currentReloadTiers = PlayerPrefs.GetInt("ReloadUpgradeTiers", 0);
        currentWindTiers = PlayerPrefs.GetInt("WindUpgradeTiers", 0);
        currentTurnSpeedTiers = PlayerPrefs.GetInt("TurnSpeedUpgradeTiers", 0);
        currentExtraCannons = PlayerPrefs.GetInt("ExtraCannonPurchased", 0);
        
        healthPlusButton.onClick.AddListener(() => { if(additionalHealthTiers + currentHealthTiers < maxHealthTiers) { additionalHealthTiers++; UpdateUI(); } });
        healthMinusButton.onClick.AddListener(() => { if(additionalHealthTiers > 0) { additionalHealthTiers--; UpdateUI(); } });
        
        speedPlusButton.onClick.AddListener(() => { if(additionalSpeedTiers + currentSpeedTiers < maxSpeedTiers) { additionalSpeedTiers++; UpdateUI(); } });
        speedMinusButton.onClick.AddListener(() => { if(additionalSpeedTiers > 0) { additionalSpeedTiers--; UpdateUI(); } });
        
        reloadPlusButton.onClick.AddListener(() => { if(additionalReloadTiers + currentReloadTiers < maxReloadTiers) { additionalReloadTiers++; UpdateUI(); } });
        reloadMinusButton.onClick.AddListener(() => { if(additionalReloadTiers > 0) { additionalReloadTiers--; UpdateUI(); } });
        
        windPlusButton.onClick.AddListener(() => { if(additionalWindTiers + currentWindTiers < maxWindTiers) { additionalWindTiers++; UpdateUI(); } });
        windMinusButton.onClick.AddListener(() => { if(additionalWindTiers > 0) { additionalWindTiers--; UpdateUI(); } });
        
        turnSpeedPlusButton.onClick.AddListener(() => { if(additionalTurnSpeedTiers + currentTurnSpeedTiers < maxTurnSpeedTiers) { additionalTurnSpeedTiers++; UpdateUI(); } });
        turnSpeedMinusButton.onClick.AddListener(() => { if(additionalTurnSpeedTiers > 0) { additionalTurnSpeedTiers--; UpdateUI(); } });
        
        extraCannonPlusButton.onClick.AddListener(() => { if (additionalCannon + currentExtraCannons < maxExtraCannons) { additionalCannon++; UpdateUI(); } });
        extraCannonMinusButton.onClick.AddListener(() => { if (additionalCannon > 0) { additionalCannon--; UpdateUI(); } });


        purchaseButton.onClick.AddListener(OnPurchase);
        resetButton.onClick.AddListener(ResetUpgrades);
        
        UpdateUI();
    }

    void UpdateUI()
    {
        int currentGold = GoldManager.Instance != null ? GoldManager.Instance.Gold : 0;
        currentGoldText.text = currentGold.ToString();

        healthTierText.text = currentHealthTiers + " + " + additionalHealthTiers;
        if (currentHealthTiers == maxHealthTiers) healthTierText.text = "Max Level";
        int healthCost = CalculateUpgradeCost(currentHealthTiers, additionalHealthTiers, costPerHealthTier);
        healthCostText.text = healthCost.ToString();

        speedTierText.text = currentSpeedTiers + " + " + additionalSpeedTiers;
        if (currentSpeedTiers == maxSpeedTiers) speedTierText.text = "Max Level";
        int speedCost = CalculateUpgradeCost(currentSpeedTiers, additionalSpeedTiers, costPerSpeedTier);
        speedCostText.text = speedCost.ToString();

        reloadTierText.text = currentReloadTiers + " + " + additionalReloadTiers;
        if (currentReloadTiers == maxReloadTiers) reloadTierText.text = "Max Level";
        int reloadCost = CalculateUpgradeCost(currentReloadTiers, additionalReloadTiers, costPerReloadTier);
        reloadCostText.text = reloadCost.ToString();

        windTierText.text = currentWindTiers + " + " + additionalWindTiers;
        if (currentWindTiers == maxWindTiers) windTierText.text = "Max Level";
        int windCost = CalculateUpgradeCost(currentWindTiers, additionalWindTiers, costPerWindTier);
        windCostText.text = windCost.ToString();

        turnSpeedTierText.text = currentTurnSpeedTiers + " + " + additionalTurnSpeedTiers;
        if (currentTurnSpeedTiers == maxTurnSpeedTiers) turnSpeedTierText.text = "Max Level";
        int turnSpeedCost = CalculateUpgradeCost(currentTurnSpeedTiers, additionalTurnSpeedTiers, costPerTurnSpeedTier);
        turnSpeedCostText.text = turnSpeedCost.ToString();

        extraCannonText.text = currentExtraCannons + " + " + additionalCannon;
        if (currentExtraCannons == maxExtraCannons) extraCannonText.text = "Max Level";
        int extraCannonCostTotal = CalculateUpgradeCost(currentExtraCannons, additionalCannon, extraCannonCost);
        extraCannonCostText.text = extraCannonCostTotal.ToString();

        int totalAdditionalCost = healthCost + speedCost + reloadCost + windCost + turnSpeedCost + extraCannonCostTotal;
        totalCost.text = ("Total:" + totalAdditionalCost);

        totalCost.color = totalAdditionalCost > currentGold ? Color.red : Color.white;
    }

    void OnPurchase()
    {
        int totalAdditionalCost = (additionalHealthTiers * costPerHealthTier) +
                                  (additionalSpeedTiers * costPerSpeedTier) +
                                  (additionalReloadTiers * costPerReloadTier) +
                                  (additionalWindTiers * costPerWindTier) +
                                  (additionalTurnSpeedTiers * costPerTurnSpeedTier) +
                                  (additionalCannon * extraCannonCost);

        if (GoldManager.Instance == null)
        {
            Debug.LogWarning("GoldManager instance not found.");
            return;
        }
        
        if (GoldManager.Instance.Gold < totalAdditionalCost)
        {
            Debug.Log("Not enough gold for purchase.");
            return;
        }
        
        bool spent = GoldManager.Instance.SpendGold(totalAdditionalCost);
        if (!spent)
        {
            Debug.Log("Gold deduction failed.");
            return;
        }
        
        currentHealthTiers += additionalHealthTiers;
        currentSpeedTiers += additionalSpeedTiers;
        currentReloadTiers += additionalReloadTiers;
        currentWindTiers += additionalWindTiers;
        currentTurnSpeedTiers += additionalTurnSpeedTiers;
        currentExtraCannons += additionalCannon;
        
        PlayerPrefs.SetInt("HealthUpgradeTiers", currentHealthTiers);
        PlayerPrefs.SetInt("SpeedUpgradeTiers", currentSpeedTiers);
        PlayerPrefs.SetInt("ReloadUpgradeTiers", currentReloadTiers);
        PlayerPrefs.SetInt("WindUpgradeTiers", currentWindTiers);
        PlayerPrefs.SetInt("TurnSpeedUpgradeTiers", currentTurnSpeedTiers);
        PlayerPrefs.SetInt("ExtraCannonPurchased", currentExtraCannons);
        PlayerPrefs.Save();
        
        Debug.Log("Purchased upgrades: Health: " + additionalHealthTiers +
                  ", Speed: " + additionalSpeedTiers +
                  ", Reload: " + additionalReloadTiers +
                  ", Wind Resist: " + additionalWindTiers +
                  ", Turn Speed: " + additionalTurnSpeedTiers +
                  ", Extra Cannon: " + additionalCannon +
                  ". Total cost: " + totalAdditionalCost);
        
        additionalHealthTiers = 0;
        additionalSpeedTiers = 0;
        additionalReloadTiers = 0;
        additionalWindTiers = 0;
        additionalTurnSpeedTiers = 0;
        additionalCannon = 0;
        UpdateUI();
        
        SceneManager.LoadScene("level_1");
    }
    
    void ResetUpgrades()
    {
        currentHealthTiers = 0;
        currentSpeedTiers = 0;
        currentReloadTiers = 0;
        currentWindTiers = 0;
        currentTurnSpeedTiers = 0;
        currentExtraCannons = 0;
        additionalHealthTiers = 0;
        additionalSpeedTiers = 0;
        additionalReloadTiers = 0;
        additionalWindTiers = 0;
        additionalTurnSpeedTiers = 0;
        additionalCannon = 0;
        
        PlayerPrefs.SetInt("HealthUpgradeTiers", 0);
        PlayerPrefs.SetInt("SpeedUpgradeTiers", 0);
        PlayerPrefs.SetInt("ReloadUpgradeTiers", 0);
        PlayerPrefs.SetInt("WindUpgradeTiers", 0);
        PlayerPrefs.SetInt("TurnSpeedUpgradeTiers", 0);
        PlayerPrefs.SetInt("ExtraCannonPurchased", 0);
        PlayerPrefs.Save();
        UpdateUI();
        Debug.Log("All upgrades reset to default.");
    }

    int CalculateUpgradeCost(int currentTier, int additionalTiers, int baseCost)
    {
        int totalCost = 0;
        for (int i = 0; i < additionalTiers; i++)
        {
            totalCost += baseCost + (currentTier + i) * (int)(0.1f * baseCost);
        }
        return totalCost;
    }

}