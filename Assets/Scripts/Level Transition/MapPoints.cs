using System.Collections.Generic;
using UnityEngine;

public class MapPoints : MonoBehaviour
{
    [SerializeField] public List<Transform> points = new List<Transform>();

    public Transform GetPoint(int i)
    {
        if (i < points.Count)
        {
            return points[i];
        }
        // if going out of bounds then return last point to prevent error
        return points[points.Count - 1];
    }
}
