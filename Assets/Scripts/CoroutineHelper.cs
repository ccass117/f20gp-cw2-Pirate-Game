using UnityEngine;

/*
this class is used to create a singleton instance of a MonoBehaviour that can be used to start coroutines from anywhere, 
it is used in the PowerUpSceneManager and LevelTransitionManager classes and is used to start coroutines from the static methods in those classes, 
since static methods cannot start coroutines directly in Unity
*/

public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper _instance;
    public static CoroutineHelper Instance {
        get {
            if (_instance == null) {
                GameObject go = new GameObject("CoroutineHelper");
                _instance = go.AddComponent<CoroutineHelper>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
}
