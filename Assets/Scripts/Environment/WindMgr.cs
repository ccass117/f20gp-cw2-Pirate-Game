using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WindMgr : MonoBehaviour
{
    public static WindMgr Instance { get; private set; }

    public float directionChangeInterval = 20f;
    public float transitionDuration = 10f;
    private float timeSinceLastChange;
    private float transitionTime;
    private float targetWindDir;
    public float targetWindAngle;
    private GameObject player;

    public Vector3 windDir = Vector3.forward;
    public float windStrength = 1f; //Added wind strength!

    [Header("Debug")]
    [SerializeField] private Vector3 windEffect;

    void Awake() //I changed this to awake from start, so it starts on scene start not script instance run
    {


        SceneManager.sceneLoaded += OnSceneLoaded;

        player = GameObject.FindWithTag("Player");
        ShipController shipController = player.GetComponent<ShipController>();
        Cannons cannons = player.GetComponent<Cannons>();
        Health health = player.GetComponent<Health>();

        BuffController.registerBuff("Calm Winds", "Make winds affect the player less", delegate () { shipController.windResistance -= 0.5f; }, delegate () { shipController.windResistance += 0.5f; });
        BuffController.registerBuff("Rocket Boost", "Allows you to rocket forward every 15 seconds, giving a burst of speed", delegate () { RocketBoost.ActivateRocketBoost(); }, delegate () { RocketBoost.DeactivateRocketBoost(); });
        BuffController.registerBuff("Gaon Cannon", "Fires a high damage laser from the front of your ship every 20 seconds", delegate { GaonCannon.ActivateLaserBuff(); }, delegate { GaonCannon.DeactivateLaserBuff(); });
        BuffController.registerBuff("PEDs", "Reduces time taken to raise the anchor", delegate { shipController.anchorRaiseTime -= 0.75f; }, delegate () { shipController.anchorRaiseTime += 0.75f; });
        BuffController.registerBuff("Suspicious Needle", "Greatly reduces time taken to raise the anchor", delegate { shipController.anchorRaiseTime -= 1.25f; }, delegate () { shipController.anchorRaiseTime += 1.25f; });
        BuffController.registerBuff("Noise Cancelling Earbuds", "Reduces the effect of Sirens' pull", delegate { shipController.sirenTurnStrength -= 0.4f; }, delegate () { shipController.sirenTurnStrength += 0.4f; });
        BuffController.registerBuff("Sobered up", "Negate the effect of Sirens' pull", delegate { shipController.sirenTurnStrength = 0; }, delegate () { shipController.sirenTurnStrength += 1.2f; });
        BuffController.registerBuff("Tube of Superglue", "You can't just glue on another cannon and expect it to work", delegate { cannons.cannonsPerSide += 1; cannons.InitializeCannons(); }, delegate () { cannons.cannonsPerSide -= 1; cannons.InitializeCannons(); });
        BuffController.registerBuff("TF2 Engineer", "Add an additional cannon", delegate { cannons.cannonsPerSide += 1; cannons.InitializeCannons(); }, delegate () { cannons.cannonsPerSide -= 1; cannons.InitializeCannons(); });
        BuffController.registerBuff("Reinforced Hull", "Increases maximum HP by 10", delegate { health.maxHealth += 10f; health.currentHealth += 10f; BuffController.deactivateBuff("Reinforced Hull"); }, delegate () { });
        BuffController.registerBuff("Quick Repair", "Heal 10HP", delegate { health.currentHealth += 10f; if (health.currentHealth > health.maxHealth) { health.currentHealth = health.maxHealth; } BuffController.deactivateBuff("Quick Repair"); }, delegate () { });
        BuffController.registerBuff("Theseus's Prodigy", "Fully repair your ship... is it even the same one anymore?", delegate { health.currentHealth = health.maxHealth; BuffController.deactivateBuff("Theseus's Prodigy"); }, delegate () { });
        BuffController.registerBuff("Kilogram of feathers", "Reduces cannon reload speed", delegate { cannons.cooldownTime -= 1f; }, delegate () { cannons.cooldownTime += 1f; });
        BuffController.registerBuff("Exponential Stupidity", "Locks your max hp at 1, Gain 1.5x more cannons per area", delegate { health.currentHealth = 1; health.maxHealth = 1; cannons.cannonsPerSide = Mathf.CeilToInt(cannons.cannonsPerSide * 1.5f); cannons.InitializeCannons(); }, delegate () { });



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
            targetWindDir = Random.Range(0.0f, 360.0f);
            targetWindAngle = targetWindDir;
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
            //change wind direction transition time
            float newAngle = Mathf.LerpAngle(getWindAngle(), targetWindDir, t);
            updateDir(newAngle);
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //default strenght levels
        HashSet<string> windLevel1 = new() { "level_1", "level_2", "level_3", "level_4", "level_9", "level_10", "level_11", "level_12" };
        //first 2 stormy levels
        HashSet<string> windLevel2 = new() { "level_5", "level_6", };
        //level 7 (fnuuy strong wind go brrrrrrrrrrrrrr)
        HashSet<string> windLevel3 = new() { "level_7" };
        //level 8 boss
        HashSet<string> windLevel4 = new() { "level_8" };

        if (windLevel1.Contains(scene.name)) { windStrength = 1f; }        
        else if (windLevel2.Contains(scene.name)) { windStrength = 2f; }
        else if (windLevel3.Contains(scene.name)) { windStrength = 2.75f; }
        else if (windLevel4.Contains(scene.name)) { windStrength = 4f; }
    }
}
