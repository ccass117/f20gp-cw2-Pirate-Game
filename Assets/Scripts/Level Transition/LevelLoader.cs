using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// general loader to include the crossfade animation for any scene transitions
// from this tutorial: https://www.youtube.com/watch?v=CE9VOZivb3I
public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Animator transition;

    public void LoadLevel(string lvl)
    {
        StartCoroutine(LoadAnim(lvl));
    }

    IEnumerator LoadAnim(string lvl)
    {
        transition.SetTrigger("StartFade");

        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(lvl);
    }
}

