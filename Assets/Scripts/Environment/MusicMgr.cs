using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
public class MusicMgr : MonoBehaviour
{
    public AudioSource world_1_theme;
    public AudioSource world_1_boss;
    public AudioSource world_2_theme;
    public AudioSource world_2_boss;
    public AudioSource world_3_theme;
    public AudioSource world_3_boss;
    private AudioSource currentAudioSource;

    private Dictionary<string, AudioSource> sceneMusicMap = new Dictionary<string, AudioSource>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Initialize dictionary with AudioSources

        sceneMusicMap["level_1"] = world_1_theme;
        sceneMusicMap["level_2"] = world_1_theme;
        sceneMusicMap["level_3"] = world_1_theme;
        sceneMusicMap["level_4"] = world_1_boss;
        sceneMusicMap["level_5"] = world_2_theme;
        sceneMusicMap["level_6"] = world_2_theme;
        sceneMusicMap["level_7"] = world_2_theme;
        sceneMusicMap["level_8"] = world_2_boss;
        sceneMusicMap["level_9"] = world_3_theme;
        sceneMusicMap["level_10"] = world_3_theme;
        sceneMusicMap["level_11"] = world_3_theme;
        sceneMusicMap["level_12"] = world_3_boss;

    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Loaded scene: " + scene.name);
        ChangeMusic(scene.name);
    }

    private void ChangeMusic(string sceneName)
    {
        if (sceneMusicMap.TryGetValue(sceneName, out AudioSource newAudioSource))
        {
            if (currentAudioSource == newAudioSource)
            {
                Debug.Log("Same music continues for scene: " + sceneName);
                return; //No need to restart music
            }

            if (currentAudioSource != null)
            {
                currentAudioSource.Stop();
            }

            currentAudioSource = newAudioSource;
            currentAudioSource.Play();
            Debug.Log("Playing new music for: " + sceneName);
        }
        else
        {
            Debug.LogWarning("No music assigned for scene: " + sceneName);
        }
    }
}