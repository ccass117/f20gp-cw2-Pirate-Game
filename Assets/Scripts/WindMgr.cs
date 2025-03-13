using UnityEngine;

public class WindMgr : MonoBehaviour
{
    public float directionChangeInterval = 20f;
    public float transitionDuration = 10f;
    private float timeSinceLastChange;
    private float transitionTime;
    private float targetWindDir;

    public float windDir;

    void Start()
    {
        //init variables, start time at 5f to ensure not large gap before first wind change
        timeSinceLastChange = 5f;
        transitionTime = 0f;
        //generate initial wind direction
        windDir = UnityEngine.Random.Range(0.0f, 360.0f);
        targetWindDir = windDir;

        //Insert section to have wind-change time change depending on floor (stormy = frequent more eratic changes)

    }

    void Update()
    {
        //set time since last change
        timeSinceLastChange += Time.deltaTime;

        //Time to change wind!
        if (timeSinceLastChange >= directionChangeInterval)
        {
            //Set new target direction
            targetWindDir = UnityEngine.Random.Range(0.0f, 360.0f);
            transitionTime = 0f;
            timeSinceLastChange = 0f;
            Debug.Log("Changing wind direction to: " + targetWindDir);
        }

        //Change wind direction
        if (transitionTime < transitionDuration)
        {
            transitionTime += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTime / transitionDuration);
            //ease in and out
            t = Mathf.SmoothStep(0f, 1f, t);
            //chaneg wind direction transition time
            windDir = Mathf.LerpAngle(windDir, targetWindDir, t);
        }
    }
}