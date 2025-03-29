using UnityEngine;

public class MapPoints : MonoBehaviour
{
    public Transform[] points;

    public Vector3 GetNextPoint(int i)
    {
        // returns correctly if index is within bounds
        if (i < points.Length)
        {
            return points[i].position;
        }
        // if going out of bounds then return last point to prevent error
        return points[points.Length - 1].position; 
    }
}
