using UnityEngine;
using System;
using System.Collections.Generic;

class Buff
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

    public void registerBuff(string name, string description, Action activateCallback, Action deactivateCallback)
    {
        buffStore.Add(name, new Buff(name, description, activateCallback, deactivateCallback));
        activeBuff.Add(name, false);
    }

    public void activateBuff(string name)
    {
        Buff buff = buffStore[name];
        buff.activateCallback();
        activeBuff[name] = true;
    }

    public void deactivateBuff(string name)
    {
        Buff buff = buffStore[name];
        buff.deactivateCallback();

        activeBuff[name] = false;
    }

}
