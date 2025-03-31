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

        BuffController.registerBuff(
            "Calm Winds", 
            "Make winds affect the player less", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    ShipController sc = p.GetComponent<ShipController>();
                    if(sc != null) {
                        sc.windResistance -= 0.5f; 
                        Debug.Log("Calm Winds activated: windResistance now " + sc.windResistance);
                    } else {
                        Debug.LogWarning("Calm Winds activate: ShipController not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Calm Winds activate: Player not found!");
                }
            }, 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    ShipController sc = p.GetComponent<ShipController>();
                    if(sc != null) {
                        sc.windResistance += 0.5f; 
                        Debug.Log("Calm Winds deactivated: windResistance now " + sc.windResistance);
                    } else {
                        Debug.LogWarning("Calm Winds deactivate: ShipController not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Calm Winds deactivate: Player not found!");
                }
            }
        );

        BuffController.registerBuff(
            "Rocket Boost", 
            "Allows you to rocket forward every 15 seconds, giving a burst of speed", 
            delegate () { 
                RocketBoost.ActivateRocketBoost(); 
                Debug.Log("Rocket Boost activated");
            }, 
            delegate () { 
                RocketBoost.DeactivateRocketBoost(); 
                Debug.Log("Rocket Boost deactivated");
            }
        );

        BuffController.registerBuff(
            "Gaon Cannon", 
            "Fires a high damage laser from the front of your ship every 20 seconds", 
            delegate () { 
                GaonCannon.ActivateLaserBuff(); 
                Debug.Log("Gaon Cannon activated");
            }, 
            delegate () { 
                GaonCannon.DeactivateLaserBuff(); 
                Debug.Log("Gaon Cannon deactivated");
            }
        );

        BuffController.registerBuff(
            "PEDs", 
            "Reduces time taken to raise the anchor", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    ShipController sc = p.GetComponent<ShipController>();
                    if(sc != null) {
                        sc.anchorRaiseTime -= 0.75f; 
                        Debug.Log("PEDs activated: anchorRaiseTime now " + sc.anchorRaiseTime);
                    } else {
                        Debug.LogWarning("PEDs activate: ShipController not found on Player!");
                    }
                } else {
                    Debug.LogWarning("PEDs activate: Player not found!");
                }
            }, 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    ShipController sc = p.GetComponent<ShipController>();
                    if(sc != null) {
                        sc.anchorRaiseTime += 0.75f; 
                        Debug.Log("PEDs deactivated: anchorRaiseTime now " + sc.anchorRaiseTime);
                    } else {
                        Debug.LogWarning("PEDs deactivate: ShipController not found on Player!");
                    }
                } else {
                    Debug.LogWarning("PEDs deactivate: Player not found!");
                }
            }
        );

        BuffController.registerBuff(
            "Suspicious Needle", 
            "Greatly reduces time taken to raise the anchor", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    ShipController sc = p.GetComponent<ShipController>();
                    if(sc != null) {
                        sc.anchorRaiseTime -= 1.25f; 
                        Debug.Log("Suspicious Needle activated: anchorRaiseTime now " + sc.anchorRaiseTime);
                    } else {
                        Debug.LogWarning("Suspicious Needle activate: ShipController not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Suspicious Needle activate: Player not found!");
                }
            }, 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    ShipController sc = p.GetComponent<ShipController>();
                    if(sc != null) {
                        sc.anchorRaiseTime += 1.25f; 
                        Debug.Log("Suspicious Needle deactivated: anchorRaiseTime now " + sc.anchorRaiseTime);
                    } else {
                        Debug.LogWarning("Suspicious Needle deactivate: ShipController not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Suspicious Needle deactivate: Player not found!");
                }
            }
        );

        BuffController.registerBuff(
            "Noise Cancelling Earbuds", 
            "Reduces the effect of Sirens' pull", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    ShipController sc = p.GetComponent<ShipController>();
                    if(sc != null) {
                        sc.sirenTurnStrength -= 0.4f; 
                        Debug.Log("Noise Cancelling Earbuds activated: sirenTurnStrength now " + sc.sirenTurnStrength);
                    } else {
                        Debug.LogWarning("Earbuds activate: ShipController not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Earbuds activate: Player not found!");
                }
            }, 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    ShipController sc = p.GetComponent<ShipController>();
                    if(sc != null) {
                        sc.sirenTurnStrength += 0.4f; 
                        Debug.Log("Noise Cancelling Earbuds deactivated: sirenTurnStrength now " + sc.sirenTurnStrength);
                    } else {
                        Debug.LogWarning("Earbuds deactivate: ShipController not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Earbuds deactivate: Player not found!");
                }
            }
        );

        BuffController.registerBuff(
            "Sobered up", 
            "Negate the effect of Sirens' pull", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    ShipController sc = p.GetComponent<ShipController>();
                    if(sc != null) {
                        sc.sirenTurnStrength = 0; 
                        Debug.Log("Sobered up activated: sirenTurnStrength set to 0");
                    } else {
                        Debug.LogWarning("Sobered up activate: ShipController not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Sobered up activate: Player not found!");
                }
            }, 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    ShipController sc = p.GetComponent<ShipController>();
                    if(sc != null) {
                        sc.sirenTurnStrength += 1.2f; 
                        Debug.Log("Sobered up deactivated: sirenTurnStrength now " + sc.sirenTurnStrength);
                    } else {
                        Debug.LogWarning("Sobered up deactivate: ShipController not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Sobered up deactivate: Player not found!");
                }
            }
        );

        BuffController.registerBuff(
            "Tube of Superglue", 
            "You can't just glue on another cannon and expect it to work", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    Cannons c = p.GetComponent<Cannons>();
                    if(c != null) {
                        c.cannonsPerSide += 1; 
                        c.InitializeCannons(); 
                        Debug.Log("Tube of Superglue activated: cannonsPerSide now " + c.cannonsPerSide);
                    } else {
                        Debug.LogWarning("Tube of Superglue activate: Cannons not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Tube of Superglue activate: Player not found!");
                }
            }, 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    Cannons c = p.GetComponent<Cannons>();
                    if(c != null) {
                        c.cannonsPerSide -= 1; 
                        c.InitializeCannons(); 
                        Debug.Log("Tube of Superglue deactivated: cannonsPerSide now " + c.cannonsPerSide);
                    } else {
                        Debug.LogWarning("Tube of Superglue deactivate: Cannons not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Tube of Superglue deactivate: Player not found!");
                }
            }
        );

        BuffController.registerBuff(
            "TF2 Engineer", 
            "Add an additional cannon", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    Cannons c = p.GetComponent<Cannons>();
                    if(c != null) {
                        c.cannonsPerSide += 1; 
                        c.InitializeCannons(); 
                        Debug.Log("TF2 Engineer activated: cannonsPerSide now " + c.cannonsPerSide);
                    } else {
                        Debug.LogWarning("TF2 Engineer activate: Cannons not found on Player!");
                    }
                } else {
                    Debug.LogWarning("TF2 Engineer activate: Player not found!");
                }
            }, 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    Cannons c = p.GetComponent<Cannons>();
                    if(c != null) {
                        c.cannonsPerSide -= 1; 
                        c.InitializeCannons(); 
                        Debug.Log("TF2 Engineer deactivated: cannonsPerSide now " + c.cannonsPerSide);
                    } else {
                        Debug.LogWarning("TF2 Engineer deactivate: Cannons not found on Player!");
                    }
                } else {
                    Debug.LogWarning("TF2 Engineer deactivate: Player not found!");
                }
            }
        );

        BuffController.registerBuff(
            "Reinforced Hull", 
            "Increases maximum HP by 10", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    Health h = p.GetComponent<Health>();
                    if(h != null) {
                        h.maxHealth += 10f; 
                        h.currentHealth += 10f; 
                        Debug.Log("Reinforced Hull activated: maxHealth now " + h.maxHealth);
                        BuffController.deactivateBuff("Reinforced Hull"); 
                    } else {
                        Debug.LogWarning("Reinforced Hull activate: Health not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Reinforced Hull activate: Player not found!");
                }
            }, 
            delegate () { }
        );

        BuffController.registerBuff(
            "Quick Repair", 
            "Heal 10HP", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    Health h = p.GetComponent<Health>(); 
                    if(h != null) {
                        h.currentHealth += 10f; 
                        if (h.currentHealth > h.maxHealth) { 
                            h.currentHealth = h.maxHealth; 
                        } 
                        Debug.Log("Quick Repair activated: currentHealth now " + h.currentHealth);
                        BuffController.deactivateBuff("Quick Repair"); 
                    } else {
                        Debug.LogWarning("Quick Repair activate: Health not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Quick Repair activate: Player not found!");
                }
            }, 
            delegate () { }
        );

        BuffController.registerBuff(
            "Theseus's Prodigy", 
            "Fully repair your ship... is it even the same one anymore?", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    Health h = p.GetComponent<Health>(); 
                    if(h != null) {
                        h.currentHealth = h.maxHealth; 
                        Debug.Log("Theseus's Prodigy activated: currentHealth set to maxHealth " + h.maxHealth);
                        BuffController.deactivateBuff("Theseus's Prodigy"); 
                    } else {
                        Debug.LogWarning("Theseus's Prodigy activate: Health not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Theseus's Prodigy activate: Player not found!");
                }
            }, 
            delegate () { }
        );

        BuffController.registerBuff(
            "Kilogram of feathers", 
            "Reduces cannon reload speed", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    Cannons c = p.GetComponent<Cannons>(); 
                    if(c != null) {
                        c.cooldownTime -= 1f; 
                        Debug.Log("Kilogram of feathers activated: cooldownTime now " + c.cooldownTime);
                    } else {
                        Debug.LogWarning("Kilogram of feathers activate: Cannons not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Kilogram of feathers activate: Player not found!");
                }
            }, 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    Cannons c = p.GetComponent<Cannons>(); 
                    if(c != null) {
                        c.cooldownTime += 1f; 
                        Debug.Log("Kilogram of feathers deactivated: cooldownTime now " + c.cooldownTime);
                    } else {
                        Debug.LogWarning("Kilogram of feathers deactivate: Cannons not found on Player!");
                    }
                } else {
                    Debug.LogWarning("Kilogram of feathers deactivate: Player not found!");
                }
            }
        );

        BuffController.registerBuff(
            "Exponential Stupidity", 
            "Locks your max hp at 1, Gain 1.5x more cannons per area", 
            delegate () { 
                GameObject p = GameObject.FindWithTag("Player"); 
                if(p != null) { 
                    Health h = p.GetComponent<Health>(); 
                    Cannons c = p.GetComponent<Cannons>(); 
                    if(h != null && c != null) {
                        h.currentHealth = 1; 
                        h.maxHealth = 1; 
                        c.cannonsPerSide = Mathf.CeilToInt(c.cannonsPerSide * 1.5f); 
                        c.InitializeCannons(); 
                        Debug.Log("Exponential Stupidity activated: maxHealth set to 1, cannonsPerSide now " + c.cannonsPerSide);
                    } else {
                        Debug.LogWarning("Exponential Stupidity activate: Missing Health or Cannons on Player!");
                    }
                } else {
                    Debug.LogWarning("Exponential Stupidity activate: Player not found!");
                }
            }, 
            delegate () { }
        );

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
