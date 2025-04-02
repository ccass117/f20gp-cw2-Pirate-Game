using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using TMPro;

public class PowerUpSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("ToggleGroup that contains the buff selection toggles.")]
    public ToggleGroup buffToggleGroup;
    [Tooltip("Array of exactly three Toggles for selecting buffs.")]
    public Toggle[] buffToggles;
    [Tooltip("Accept button to confirm the selection.")]
    public Button acceptButton;

    private GameObject levelloader;

    void Start()
    {
        levelloader = GameObject.Find("LevelLoader");

        List<Buff> selectedBuffs = BuffController.getBuffsForShop(3);

        if (buffToggles == null || buffToggles.Length < 3)
        {
            Debug.LogError("Please assign exactly 3 toggles in the Inspector.");
            return;
        }

        // Update the toggle UI.
        // Assuming each toggle prefab has two child TMP_Text components:
        // texts[0] for the buff name and texts[1] for the buff description.
        for (int i = 0; i < 3; i++)
        {
            if (i < selectedBuffs.Count)
            {
                TMP_Text[] texts = buffToggles[i].GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 2)
                {
                    texts[0].text = selectedBuffs[i].name;
                    texts[1].text = selectedBuffs[i].description;
                }
                else if (texts.Length == 1)
                {
                    // Fallback: combine name and description if only one text component is found.
                    texts[0].text = selectedBuffs[i].name + "\n" + selectedBuffs[i].description;
                }
            }
            else
            {
                TMP_Text[] texts = buffToggles[i].GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 2)
                {
                    texts[0].text = "None";
                    texts[1].text = "";
                }
                else if (texts.Length == 1)
                {
                    texts[0].text = "None";
                }
            }
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

    void OnAccept(List<Buff> selectedBuffs)
    {
        LevelLoader loaderScript = levelloader.GetComponent<LevelLoader>();
        loaderScript.LoadLevel("LevelChange");

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
                BuffController.setActive(buffName);
                loaderScript.LoadLevel("LevelChange");
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
