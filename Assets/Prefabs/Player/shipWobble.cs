using UnityEngine;

public class ShipWobble : MonoBehaviour
{
    private Transform selfT;
    private WindMgr windMgr;

    void Start()
    {
        selfT = GetComponent<Transform>();
        GameObject windObject = GameObject.Find("Wind"); // Find the wind GameObject
        if (windObject != null)
        {
            windMgr = windObject.GetComponent<WindMgr>();
        }
    }

    void Update()
    {
        float windStrength = (windMgr != null) ? windMgr.windStrength : 1f; // Default to 1 if windMgr is null
        if (windStrength == 4)
        {
            windStrength = 3f;
        }

        selfT.localRotation = Quaternion.Euler(
            Mathf.Sin(Time.time) * 3 * windStrength,
            Mathf.Sin(Time.time * 0.379f) * 2 * windStrength,
            Mathf.Sin(Time.time * 0.654f) * windStrength
        );
    }
}
