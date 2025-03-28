using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelTransitionManager : MonoBehaviour
{
    [Tooltip("Time (in seconds) between enemy checks.")]
    public float checkInterval = 1f;
    
    private bool levelCompleted = false;

    void Start()
    {
        StartCoroutine(CheckEnemies());
    }

    IEnumerator CheckEnemies()
    {
        while (!levelCompleted)
        {
            yield return new WaitForSeconds(checkInterval);
            
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
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
                TransitionToNextLevel();
            }
        }
    }

    void TransitionToNextLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentLevel = 0;
        if (int.TryParse(currentSceneName.Replace("level_", ""), out currentLevel))
        {
            int nextLevel = currentLevel + 1;
            if (nextLevel <= 12)
            {
                string nextSceneName = "level_" + nextLevel;
                Debug.Log("All enemies defeated! Transitioning to " + nextSceneName);
                SceneManager.LoadScene(nextSceneName);
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
