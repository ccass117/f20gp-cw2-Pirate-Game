using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class MusicMgr : MonoBehaviour
{
    public AudioSource world_1_theme;
    public AudioSource world_1_boss;
    public AudioSource world_2_theme;
    public AudioSource world_2_boss;
    public AudioSource world_3_theme;
    public AudioSource world_3_boss;
    public AudioSource menuMusic;
    private AudioSource currentAudioSource;
    private ShipController playerShip;

    private Dictionary<string, AudioSource> sceneMusicMap = new Dictionary<string, AudioSource>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Initialize dictionary with AudioSources

        sceneMusicMap["MainMenu"] = menuMusic;
        sceneMusicMap["GoldShop"] = menuMusic;
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
        if (scene.name == "powerup")
        {
            StartCoroutine(FadeAudioPitch(0.5f, 0.5f)); // Lower pitch over 0.5 sec
        }
        else if (scene.name == "LevelChange")
        {
            StartCoroutine(FadeAudioPitch(1.0f, 0.5f)); // Restore pitch over 0.5 sec
        }
        else
        {
            ChangeMusic(scene.name);
            FindPlayer();
        }

        
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

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerShip = playerObject.GetComponent<ShipController>();

            if (playerShip != null)
            {
                StartCoroutine(CheckSirenInfluence());
            }
        }
    }

    private IEnumerator CheckSirenInfluence()
    {
        while (playerShip != null)
        {
            if (playerShip.sirenInfluenceActive)
            {
                StartCoroutine(FadeAudioVolume(0.1f, 0.5f));
            }
            else
            {
                StartCoroutine(FadeAudioVolume(1.0f, 0.5f));
            }
            yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
        }
    }

    private IEnumerator FadeAudioVolume(float targetVolume, float duration)
    {
        float startVolume = currentAudioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            currentAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentAudioSource.volume = targetVolume;
    }
    private IEnumerator FadeAudioPitch(float targetPitch, float duration)
    {
        float startPitch = currentAudioSource.pitch;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            currentAudioSource.pitch = Mathf.Lerp(startPitch, targetPitch, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentAudioSource.pitch = targetPitch;
    }
}