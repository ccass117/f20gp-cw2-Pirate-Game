using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static int Gold { get; private set; } = 0;
    private const string GoldKey = "Gold";

    void Awake()
    {
        LoadGold();
    }

    /// <summary>
    /// Adds gold and saves the new total.
    /// </summary>
    public static void AddGold(int amount)
    {
        Gold += amount;
        Debug.Log("Gold added: " + amount + ". Total gold: " + Gold);
        SaveGold();
    }

    /// <summary>
    /// Attempts to spend gold. Returns true if successful.
    /// </summary>
    public static bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            SaveGold();
            return true;
        }
        return false;
    }

    private static void SaveGold()
    {
        PlayerPrefs.SetInt(GoldKey, Gold);
        PlayerPrefs.Save();
    }

    private static void LoadGold()
    {
        Gold = PlayerPrefs.GetInt(GoldKey, 0);
        Debug.Log("Gold loaded: " + Gold);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Equals))
        {
            AddGold(100);
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            AddGold(-Gold);
        }
    }
}
