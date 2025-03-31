using System.Collections.Generic;
using UnityEngine;

public class BuffReapplier : MonoBehaviour
{
    void Start()
    {
        foreach (string buffName in BuffController.GetActiveBuffNames())
        {
            Debug.Log("Reapplying buff: " + buffName);
            BuffController.activateBuff(buffName);
        }
    }
}