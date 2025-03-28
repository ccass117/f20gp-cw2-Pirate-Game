using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Reflection;

public class PowerUpSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("ToggleGroup that contains the buff selection toggles.")]
    public ToggleGroup buffToggleGroup;
    [Tooltip("Array of exactly three Toggles for selecting buffs.")]
    public Toggle[] buffToggles;
    [Tooltip("Accept button to confirm the selection.")]
    public Button acceptButton;

    private List<BuffData> availableBuffs = new List<BuffData>();

    private class BuffData
    {
        public string name;
        public string description;
        public BuffData(string name, string description)
        {
            this.name = name;
            this.description = description;
        }
    }

    void Start()
    {
        RegisterDummyBuffs();

        Type buffControllerType = typeof(BuffController);
        FieldInfo buffStoreField = buffControllerType.GetField("buffStore", BindingFlags.NonPublic | BindingFlags.Static);
        if (buffStoreField == null)
        {
            Debug.LogError("Could not find buffStore field in BuffController.");
            return;
        }
        var buffStore = buffStoreField.GetValue(null) as Dictionary<string, Buff>;
        if (buffStore == null)
        {
            Debug.LogError("buffStore is null.");
            return;
        }

        foreach (var kvp in buffStore)
        {
            string buffName = kvp.Key;
            if (!BuffController.isActive(buffName))
            {
                FieldInfo descriptionField = kvp.Value.GetType().GetField("description", BindingFlags.Public | BindingFlags.Instance);
                string description = descriptionField != null ? descriptionField.GetValue(kvp.Value) as string : "";
                availableBuffs.Add(new BuffData(buffName, description));
            }
        }
        
        List<BuffData> selectedBuffs = new List<BuffData>();
        int countToSelect = Mathf.Min(3, availableBuffs.Count);
        System.Random rnd = new System.Random();
        while (selectedBuffs.Count < countToSelect)
        {
            int index = rnd.Next(availableBuffs.Count);
            BuffData candidate = availableBuffs[index];
            if (!selectedBuffs.Contains(candidate))
                selectedBuffs.Add(candidate);
        }
        
        if (buffToggles == null || buffToggles.Length < 3)
        {
            Debug.LogError("Please assign exactly 3 toggles in the Inspector.");
            return;
        }
        
        for (int i = 0; i < 3; i++)
        {
            string displayText = "";
            if (i < selectedBuffs.Count)
                displayText = selectedBuffs[i].name + "\n" + selectedBuffs[i].description;
            else
                displayText = "None";
            
            Text toggleLabel = buffToggles[i].GetComponentInChildren<Text>();
            if (toggleLabel != null)
                toggleLabel.text = displayText;
        }
        
        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(() => OnAccept(selectedBuffs));
        }
        else
        {
            Debug.LogWarning("Accept button not assigned.");
        }
    }

    void RegisterDummyBuffs()
    {
        BuffController.registerBuff(
            "Buff A", 
            "Increases damage by 20% for 30 seconds.",
            () => { Debug.Log("Buff A activated."); },
            () => { Debug.Log("Buff A deactivated."); }
        );

        BuffController.registerBuff(
            "Buff B", 
            "Grants 50 shield points for 30 seconds.",
            () => { Debug.Log("Buff B activated."); },
            () => { Debug.Log("Buff B deactivated."); }
        );

        BuffController.registerBuff(
            "Buff C", 
            "Boosts speed by 50% for 15 seconds.",
            () => { Debug.Log("Buff C activated."); },
            () => { Debug.Log("Buff C deactivated."); }
        );
    }

    void OnAccept(List<BuffData> selectedBuffs)
    {
        Toggle selectedToggle = null;
        foreach (Toggle toggle in buffToggleGroup.GetComponentsInChildren<Toggle>())
        {
            if (toggle.isOn)
            {
                selectedToggle = toggle;
                break;
            }
        }
        
        if (selectedToggle != null)
        {
            int index = Array.IndexOf(buffToggles, selectedToggle);
            if (index >= 0 && index < selectedBuffs.Count)
            {
                string buffName = selectedBuffs[index].name;
                Debug.Log("PowerUpSceneManager: Selected Buff - " + buffName);
                BuffController.activateBuff(buffName);
                
                int nextLevel = PlayerPrefs.GetInt("NextLevel", 1);
                if (nextLevel > 0 && nextLevel <= 12)
                {
                    string nextSceneName = "level_" + nextLevel;
                    Debug.Log("Loading next level: " + nextSceneName);
                    SceneManager.LoadScene(nextSceneName);
                }
                else
                {
                    Debug.LogWarning("Next level number is invalid.");
                }
            }
            else
            {
                Debug.LogWarning("Selected toggle index is out of range of available buffs.");
            }
        }
        else
        {
            Debug.Log("No buff selected.");
        }
    }
}