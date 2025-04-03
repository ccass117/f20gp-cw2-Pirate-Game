using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using TMPro;

//buff scene manager
public class PowerUpSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public ToggleGroup buffToggleGroup;
    public Toggle[] buffToggles;
    public Button acceptButton;

    private GameObject levelloader;

    void Start()
    {
        levelloader = GameObject.Find("LevelLoader");
        List<Buff> selectedBuffs = BuffController.getBuffsForShop(3);
        if (buffToggles == null || buffToggles.Length < 3)
        {
            return;
        }

        //randomly select 3 buffs from the list of available buffs, see BuffController.cs
        for (int i = 0; i < 3; i++)
        {
            if (i < selectedBuffs.Count)
            {
                //separate the name and description of the buff
                TMP_Text[] texts = buffToggles[i].GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 2)
                {
                    texts[0].text = selectedBuffs[i].name;
                    texts[1].text = selectedBuffs[i].description;
                }
                else if (texts.Length == 1)
                {
                    texts[0].text = selectedBuffs[i].name + "\n" + selectedBuffs[i].description;
                }
            }
            else
            {
                //this won't happen since we have too many buffs for this to come up
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
    }

    void OnAccept(List<Buff> selectedBuffs)
    {
        LevelLoader loaderScript = levelloader.GetComponent<LevelLoader>();
        //move to the level changer scene
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
                BuffController.setActive(buffName);
                loaderScript.LoadLevel("LevelChange");
            }
        }
        else
        {
            Debug.Log("No buff selected.");
        }
    }
}