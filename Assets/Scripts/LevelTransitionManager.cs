using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LevelTransitionManager : MonoBehaviour
{
    [Tooltip("Time (in seconds) between enemy checks.")]
    public float checkInterval = 1f;

    public bool levelCompleted = false;

    private GameObject levelloader;

    void Start()
    {
        levelloader = GameObject.Find("LevelLoader");

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(CheckEnemies());
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //only work on level_1 to level_12
        if (Regex.IsMatch(scene.name, @"^level_(1[0-2]|[1-9])$"))
        {
            levelloader = GameObject.Find("LevelLoader");
            levelCompleted = false;  // Reset level completion when a new scene loads
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

            if (allInactive)
            {
                levelCompleted = true;
                TransitionToPowerUpScene();
            }
        }
    }

    void TransitionToPowerUpScene()
    {
        Debug.Log("All enemies defeated! Transitioning to PowerUpScene for next level");
        LevelLoader loaderScript = levelloader.GetComponent<LevelLoader>();
        loaderScript.LoadLevel("powerup");
    }
}