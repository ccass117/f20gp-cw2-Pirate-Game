using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelTransitionManager : MonoBehaviour
{
    [Tooltip("Time (in seconds) between enemy checks.")]
    public float checkInterval = 1f;
    
    private bool levelCompleted = false;

    private GameObject levelloader;

    void Start()
    {
        levelloader = GameObject.Find("LevelLoader");
        StartCoroutine(CheckEnemies());
    }

    IEnumerator CheckEnemies()
    {
        while (!levelCompleted)
        {
            yield return new WaitForSeconds(checkInterval);

            GameObject[] enemyShips = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
            // Combine both arrays into one list
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
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentLevel = 0;
        if (int.TryParse(currentSceneName.Replace("level_", ""), out currentLevel))
        {
            int nextLevel = currentLevel + 1;
            if (nextLevel <= 12)
            {
                // Store next level number for the power-up scene.
                PlayerPrefs.SetInt("NextLevel", nextLevel);
                PlayerPrefs.Save();
                Debug.Log("All enemies defeated! Transitioning to PowerUpScene for next level: " + nextLevel);
                LevelLoader loaderScript = levelloader.GetComponent<LevelLoader>();
                loaderScript.LoadLevel("powerup");
                levelCompleted = !levelCompleted;
            }
            else
            {
                Debug.Log("Last level reached! No further levels to load.");
            }
        }
        else
        {
            Debug.LogError("Unable to parse current level from scene name: " + currentSceneName);
        }
    }
}