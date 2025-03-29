using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }
    public int Gold { get; private set; } = 0;
    private const string GoldKey = "Gold";

    void Awake()
    {
        // Ensure a single instance that persists across scenes.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGold();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Adds gold and saves the new total.
    /// </summary>
    public void AddGold(int amount)
    {
        Gold += amount;
        Debug.Log("Gold added: " + amount + ". Total gold: " + Gold);
        SaveGold();
    }

    /// <summary>
    /// Attempts to spend gold. Returns true if successful.
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            SaveGold();
            return true;
        }
        return false;
    }

    void SaveGold()
    {
        PlayerPrefs.SetInt(GoldKey, Gold);
        PlayerPrefs.Save();
    }

    void LoadGold()
    {
        Gold = PlayerPrefs.GetInt(GoldKey, 0);
        Debug.Log("Gold loaded: " + Gold);
    }
}