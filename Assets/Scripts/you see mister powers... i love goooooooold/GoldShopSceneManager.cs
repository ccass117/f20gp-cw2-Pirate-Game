using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GoldShopSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text displaying the player's current gold.")]
    public TextMeshProUGUI currentGoldText;
    
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
    public Button extraCannonButton;
    
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
    
    public int extraCannonCost = 5000;
    
    private int additionalHealthTiers = 0;
    private int additionalSpeedTiers = 0;
    private int additionalReloadTiers = 0;
    private int additionalWindTiers = 0;
    private int additionalTurnSpeedTiers = 0;
    private bool wantsExtraCannon = false; 
    
    private int currentHealthTiers = 0;
    private int currentSpeedTiers = 0;
    private int currentReloadTiers = 0;
    private int currentWindTiers = 0;
    private int currentTurnSpeedTiers = 0;
    private bool extraCannonPurchased = false;
    
    void Start()
    {
        currentHealthTiers = PlayerPrefs.GetInt("HealthUpgradeTiers", 0);
        currentSpeedTiers = PlayerPrefs.GetInt("SpeedUpgradeTiers", 0);
        currentReloadTiers = PlayerPrefs.GetInt("ReloadUpgradeTiers", 0);
        currentWindTiers = PlayerPrefs.GetInt("WindUpgradeTiers", 0);
        currentTurnSpeedTiers = PlayerPrefs.GetInt("TurnSpeedUpgradeTiers", 0);
        extraCannonPurchased = PlayerPrefs.GetInt("ExtraCannonPurchased", 0) == 1;
        
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
        
        extraCannonButton.onClick.AddListener(() => {
            if (!extraCannonPurchased)
            {
                wantsExtraCannon = !wantsExtraCannon;
                UpdateUI();
            }
        });
        
        purchaseButton.onClick.AddListener(OnPurchase);
        resetButton.onClick.AddListener(ResetUpgrades);
        
        UpdateUI();
    }
    
    void UpdateUI()
    {
        int currentGold = GoldManager.Instance != null ? GoldManager.Instance.Gold : 0;
        currentGoldText.text = "Gold: " + currentGold;
        
        healthTierText.text = "Health: " + currentHealthTiers + " + " + additionalHealthTiers;
        healthCostText.text = "Cost: " + (additionalHealthTiers * costPerHealthTier);
        
        speedTierText.text = "Speed: " + currentSpeedTiers + " + " + additionalSpeedTiers;
        speedCostText.text = "Cost: " + (additionalSpeedTiers * costPerSpeedTier);
        
        reloadTierText.text = "Reload: " + currentReloadTiers + " + " + additionalReloadTiers;
        reloadCostText.text = "Cost: " + (additionalReloadTiers * costPerReloadTier);
        
        windTierText.text = "Wind Resist: " + currentWindTiers + " + " + additionalWindTiers;
        windCostText.text = "Cost: " + (additionalWindTiers * costPerWindTier);
        
        turnSpeedTierText.text = "Turn Speed: " + currentTurnSpeedTiers + " + " + additionalTurnSpeedTiers;
        turnSpeedCostText.text = "Cost: " + (additionalTurnSpeedTiers * costPerTurnSpeedTier);
        
        if (extraCannonPurchased)
        {
            extraCannonText.text = "Extra Cannon: Purchased";
            extraCannonButton.interactable = false;
        }
        else
        {
            extraCannonText.text = "Extra Cannon: " + (wantsExtraCannon ? "Selected (" + extraCannonCost + " gold)" : "Not Selected (" + extraCannonCost + " gold)");
            extraCannonButton.interactable = true;
        }
    }
    
    void OnPurchase()
    {
        int totalAdditionalCost = (additionalHealthTiers * costPerHealthTier) +
                                  (additionalSpeedTiers * costPerSpeedTier) +
                                  (additionalReloadTiers * costPerReloadTier) +
                                  (additionalWindTiers * costPerWindTier) +
                                  ( (!extraCannonPurchased && wantsExtraCannon) ? extraCannonCost : 0 );
        
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
        
        if (!extraCannonPurchased && wantsExtraCannon)
            extraCannonPurchased = true;
        
        PlayerPrefs.SetInt("HealthUpgradeTiers", currentHealthTiers);
        PlayerPrefs.SetInt("SpeedUpgradeTiers", currentSpeedTiers);
        PlayerPrefs.SetInt("ReloadUpgradeTiers", currentReloadTiers);
        PlayerPrefs.SetInt("WindUpgradeTiers", currentWindTiers);
        PlayerPrefs.SetInt("TurnSpeedUpgradeTiers", currentTurnSpeedTiers);
        PlayerPrefs.SetInt("ExtraCannonPurchased", extraCannonPurchased ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log("Purchased upgrades: Health: " + additionalHealthTiers +
                  ", Speed: " + additionalSpeedTiers +
                  ", Reload: " + additionalReloadTiers +
                  ", Wind Resist: " + additionalWindTiers +
                  ", Turn Speed: " + additionalTurnSpeedTiers +
                  (wantsExtraCannon ? ", Extra Cannon purchased" : "") +
                  ". Total cost: " + totalAdditionalCost);
        
        additionalHealthTiers = 0;
        additionalSpeedTiers = 0;
        additionalReloadTiers = 0;
        additionalWindTiers = 0;
        additionalTurnSpeedTiers = 0;
        wantsExtraCannon = false;
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
        additionalHealthTiers = 0;
        additionalSpeedTiers = 0;
        additionalReloadTiers = 0;
        additionalWindTiers = 0;
        additionalTurnSpeedTiers = 0;
        extraCannonPurchased = false;
        wantsExtraCannon = false;
        
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
}