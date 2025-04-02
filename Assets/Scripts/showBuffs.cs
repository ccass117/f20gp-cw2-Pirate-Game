using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class showBuffs : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    TMP_Text t;
    void Start()
    {
        t = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        List<(Buff, bool)> buffs = BuffController.getBuffs();
        string disp = "";
        foreach ((Buff, bool) item in buffs)
        {
            disp += item.Item1.name;
            disp += "          ";
            if (item.Item2)
            {
                disp += "Active";
            }
            disp += "\n";
        }
        t.text = disp;

    }
}
