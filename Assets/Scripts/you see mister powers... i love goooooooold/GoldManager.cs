using UnityEngine;

//uses playerprefs to save and load gold, so that it saves even if you close the game
public class GoldManager : MonoBehaviour
{
    public static int Gold { get; private set; } = 0;
    private const string GoldKey = "Gold";

    void Awake()
    {
        LoadGold();
    }

    //gets used in Health.cs when something dies
    public static void AddGold(int amount)
    {
        Gold += amount;
        SaveGold();
    }

    //used in the gold shop
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
    }

    //dev cheats, shhhh don't tell anyone
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
