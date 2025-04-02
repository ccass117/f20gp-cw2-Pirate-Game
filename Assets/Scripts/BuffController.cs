using UnityEngine;
using System;
using System.Collections.Generic;

public class Buff
{
    public string name;
    public string description;

    public Buff(string name, string description)
    {
        this.name = name;
        this.description = description;
    }
}

public class BuffController : MonoBehaviour
{
    private static Dictionary<string, Buff> buffStore = new Dictionary<string, Buff>();
    private static Dictionary<string, bool> activeBuff = new Dictionary<string, bool>();

    // Public API to access the currently registered buffs
    public static Dictionary<string, Buff> AvailableBuffs
    {
        get { return buffStore; }
    }

    public static List<(Buff, bool)> getBuffs()
    {
        List<(Buff, bool)> ret = new List<(Buff, bool)>();

        var keys = new List<string>(buffStore.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            string cur = keys[i];
            bool active = isActive(cur);
            ret.Add((buffStore[cur], active));
        }

        return ret;
    }

    public static bool registerBuff(string name, string description)
    {
        // Add the buff to the store

        if (buffStore.ContainsKey(name))
        {
            // Activate buff immediately if it was already set active
            return activeBuff[name];
        }
        else
        {
            buffStore.Add(name, new Buff(name, description));
            activeBuff.Add(name, false);
            return false;
        }
    }

    public static void setActive(string name)
    {
        if (buffStore.ContainsKey(name))
        {
            activeBuff[name] = true;
        }
        else
        {
            Debug.LogWarning("Buff '" + name + "' not found in buffStore.");
        }
    }

    public static void setInactive(string name)
    {
        if (buffStore.ContainsKey(name))
        {
            activeBuff[name] = false;
        }
        else
        {
            Debug.LogWarning("Buff '" + name + "' not found in buffStore.");
        }
    }

    public static bool isActive(string name)
    {
        return activeBuff.ContainsKey(name) && activeBuff[name];
    }

    public static IEnumerable<string> GetActiveBuffNames()
    {
        foreach (var kvp in activeBuff)
        {
            if (kvp.Value)
                yield return kvp.Key;
        }
    }

    public static List<Buff> getBuffsForShop(int amt)
    {
        List<string> keys = new List<string>(buffStore.Keys);
        List<string> inactiveBuffs = new List<string>();
        for (int i = 0; i < keys.Count; i++)
        {
            if (!isActive(keys[i]))
            {
                inactiveBuffs.Add(keys[i]);
            }
        }

        if (inactiveBuffs.Count < amt)
        {
            throw new Exception("Not enough buffs available");
        }

        List<Buff> chosenBuffs = new List<Buff>();
        HashSet<int> selectedIndices = new HashSet<int>();
        while (chosenBuffs.Count < amt)
        {
            int index = UnityEngine.Random.Range(0, inactiveBuffs.Count);
            if (selectedIndices.Add(index))
            {
                chosenBuffs.Add(buffStore[inactiveBuffs[index]]);
            }
        }
        return chosenBuffs;
    }
}
