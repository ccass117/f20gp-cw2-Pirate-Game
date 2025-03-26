using UnityEngine;

public class SplashEffect : MonoBehaviour
// Splashes just delete themselves after a few seconds.
{
    void Start()
    {
        Destroy(gameObject, 4.9f);
    }
}