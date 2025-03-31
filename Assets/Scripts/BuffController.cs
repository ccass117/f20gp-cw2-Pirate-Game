using UnityEngine;
using System;
using System.Collections.Generic;

public class Buff
{
    public string name;
    public string description;
    public Action activateCallback;
    public Action deactivateCallback;

    public Buff(string name, string description, Action activateCallback, Action deactivateCallback)
    {
        this.name = name;
        this.description = description;
        this.activateCallback = activateCallback;
        this.deactivateCallback = deactivateCallback;
    }
}

public class BuffController : MonoBehaviour
{

    private static Dictionary<string, Buff> buffStore = new Dictionary<string, Buff>();
    private static Dictionary<string, bool> activeBuff = new Dictionary<string, bool>();

    public static void registerBuff(string name, string description, Action activateCallback, Action deactivateCallback)
    {
        // update the store with the current handlers
        buffStore.Add(name, new Buff(name, description, activateCallback, deactivateCallback));

        if (activeBuff.ContainsKey(name))
        {
            // activate buff straight away, this will be on load
            if (activeBuff[name])
            {
                activateBuff(name);
            }
        }
        else
        {
            activeBuff.Add(name, false);
        }
    }

    static public void activateBuff(string name)
    {
        //Debug.Log($"activated, {name}");
        Buff buff = buffStore[name];
        buff.activateCallback();
        activeBuff[name] = true;
    }

    static public void deactivateBuff(string name)
    {
        //Debug.Log($"deactivated, {name}");
        Buff buff = buffStore[name];
        buff.deactivateCallback();
        activeBuff[name] = false;
    }

    static public bool isActive(string name)
    {
        return activeBuff[name];
    }

    static public List<Buff> getBuffsForShop(int amt)
    {
        List<string> keys = new List<string>(buffStore.Keys);
        List<string> activeBuffs = new List<string>();
        for (int i = 0; i > keys.Count; i++)
        {
            if (!isActive(keys[i]))
            {
                activeBuffs.Add(keys[i]);
            }
        }

        if (activeBuffs.Count < amt)
        {
            throw new Exception("not enough buffs");
        }

        List<Buff> chosenBuffs = new List<Buff>();
        HashSet<int> selectedIndices = new HashSet<int>();

        while (chosenBuffs.Count < amt)
        {
            int index = UnityEngine.Random.Range(0, activeBuffs.Count);
            if (selectedIndices.Add(index))
            {
                chosenBuffs.Add(buffStore[activeBuffs[index]]);
            }
        }
        return chosenBuffs;
    }
}
