using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//basically just counts enemies in a scene, and when it hits zero, transitions to buff selection, and then to the next level using our levelloader class
public class LevelTransitionManager : MonoBehaviour
{
    [Tooltip("Time (in seconds) between enemy checks.")]
    public float checkInterval = 1f;

    public bool levelCompleted = false;

    private GameObject levelloader;
    private string sceneName;

    void Start()
    {
        levelloader = GameObject.Find("LevelLoader");

        //subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(CheckEnemies());
    }

    void OnDestroy()
    {
        //unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //regex to check if the scene name matches level_1 to level_12
        if (Regex.IsMatch(scene.name, @"^level_(1[0-2]|[1-9])$"))
        {
            sceneName = scene.name;
            levelloader = GameObject.Find("LevelLoader");
            levelCompleted = false;
            StartCoroutine(CheckEnemies());
        }
    }

    IEnumerator CheckEnemies()
    {
        while (!levelCompleted)
        {
            yield return new WaitForSeconds(checkInterval);

            GameObject[] enemyShips = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");

            List<GameObject> allEnemies = new List<GameObject>(enemyShips);
            allEnemies.AddRange(bosses);
            GameObject[] enemies = allEnemies.ToArray();

            bool allInactive = true;
            foreach (GameObject enemy in enemies)
            {
                if (enemy.activeInHierarchy)
                {
                    allInactive = false;
                    break;
                }
            }

            if (allInactive || Input.GetKeyDown(KeyCode.Delete))
            {
                levelCompleted = true;
                TransitionToPowerUpScene();
            }
        }
    }

    void TransitionToPowerUpScene()
    {
        LevelLoader loaderScript = levelloader.GetComponent<LevelLoader>();


        if (sceneName == "level_12")
        {
            loaderScript.LoadLevel("LevelChange");
        } else
        {
            loaderScript.LoadLevel("powerup");
        }
        
    }
}