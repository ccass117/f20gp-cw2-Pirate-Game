using System.Collections.Generic;
using UnityEngine;

public class MapPoints : MonoBehaviour
{
    [SerializeField] public List<Transform> points = new List<Transform>();

    private void Start()
    {
        // to handle any number of points on the map
        foreach (Transform child in transform)
        {
            points.Add(child);
        }
    }
    public Vector3 GetNextPoint(int i)
    {
        // returns correctly if index is within bounds
        if (i < points.Count - 1)
        {
            return points[i].position;
        }
        // if going out of bounds then return last point to prevent error
        return points[points.Count - 1].position; 
    }
}
