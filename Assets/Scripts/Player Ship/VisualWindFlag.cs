using UnityEngine;

public class VisualWindFlag : MonoBehaviour
{
    public Vector3 windDirection;
    public float flapAmplitude = 3f; // max degrees of wiggle
    public float flapSpeed = 5f;      // how fast it flaps

    void Update()
    {
        windDirection = WindMgr.Instance.windDir;

        if (windDirection != Vector3.zero)
        {
            // Get base angle from wind
            float baseAngle = Mathf.Atan2(windDirection.x, windDirection.z) * Mathf.Rad2Deg;

            // Apply sine wave flutter
            float flutter = Mathf.Sin(Time.time * flapSpeed) * flapAmplitude;
            float finalAngle = baseAngle + flutter;

            // Convert to direction vector
            float rad = finalAngle * Mathf.Deg2Rad;
            Vector3 flutterDirection = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)).normalized;

            // Rotate flag smoothly
            Quaternion targetRotation = Quaternion.LookRotation(flutterDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}