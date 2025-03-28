using UnityEngine;

public class WaterMovementWithWind : MonoBehaviour
{
    private const float Boundary = 10f; 
    public float windInfluence = 0.5f; 

    void Update()
    {
        Vector3 windDir = WindMgr.Instance.windDir;
        float windStrength = WindMgr.Instance.windStrength;

        Vector3 movement = windDir * windStrength * windInfluence * Time.deltaTime;
        transform.position += movement;

        Vector3 pos = transform.position;

        if (pos.x >= Boundary) pos.x -= Boundary;
        else if (pos.x <= -Boundary) pos.x += Boundary;

        if (pos.z >= Boundary) pos.z -= Boundary;
        else if (pos.z <= -Boundary) pos.z += Boundary;

        transform.position = pos;
    }
}