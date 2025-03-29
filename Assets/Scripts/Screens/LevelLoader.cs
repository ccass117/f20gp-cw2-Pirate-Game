using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// general loader to include the crossfade animation for any scene transitions
public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Animator transition;
    [SerializeField] private float time = 1f;

    public void LoadLevel(string lvl)
    {
        StartCoroutine(LoadAnim(lvl));
    }

    IEnumerator LoadAnim(string lvl)
    {
        transition.SetTrigger("StartFade");

        yield return new WaitForSeconds(time);

        SceneManager.LoadScene(lvl);
    }
}

