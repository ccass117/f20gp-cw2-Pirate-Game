using System.Collections.Generic;
using UnityEngine;

public class MapPoints : MonoBehaviour
{
    [SerializeField] public List<Transform> points = new List<Transform>();

    private void Start()
    {
        // to handle any number of points on the map
        foreach (Transform point in transform)
        {
            points.Add(point);
        }
    }
    public Transform GetPoint(int i)
    {
        if (i < points.Count)
        {
            return points[i];
        }
        // if going out of bounds then return last point to prevent error
        return points[points.Count - 1];
    }


    /*
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
    public Transform GetThisPoint(int i)
    {
        if (i >= 0 && i < points.Count)
        {
            return points[i];
        }
        return null;
    }
    */
}
