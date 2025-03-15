using UnityEngine;

public class WindMgr : MonoBehaviour
{
    public static WindMgr Instance { get; private set; }

    public float directionChangeInterval = 20f;
    public float transitionDuration = 10f;
    private float timeSinceLastChange;
    private float transitionTime;
    private float targetWindDir;
    private float targetWindAngle;

    public Vector3 windDir = Vector3.forward;
    public float windStrength = 1f; //Added wind strength!

    [Header("Debug")]
    [SerializeField] private Vector3 windEffect;

    void Awake() //I changed this to awake from start, so it starts on scene start not script instance run
    {

        //makes sure that there is only gonna be one instance of wind for when we start changing scenes later
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        //init variables, start time at 5f to ensure not large gap before first wind change
        timeSinceLastChange = 5f;
        transitionTime = 0f;
        //generate initial wind direction (now generates a target wind direction and THEN calls updateDir() to move wind to that direction)
        targetWindDir = Random.Range(0f, 360f);
        updateDir();

        //Insert section to have wind-change time change depending on floor (stormy = frequent more eratic changes) - You can probably just do this in the updateDir() I added, if you have it just read the current scene name

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
            float windDir = Mathf.LerpAngle(getWindAngle(), targetWindDir, t);
            updateDir();
        }
    }

    //convert wind angle to a vector
    private void updateDir(float angle = -1)
    {
        if (angle == -1)
        {
            angle = targetWindAngle;
        }
        float angleRad = angle * Mathf.Deg2Rad;
        windDir = new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad)).normalized;
    }

    //getter for current wind angle
    private float getWindAngle()
    {
        return Mathf.Atan2(windDir.z, windDir.x) * Mathf.Rad2Deg;
    }
}