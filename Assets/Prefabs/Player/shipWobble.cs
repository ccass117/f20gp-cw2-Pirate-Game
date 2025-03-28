using UnityEngine;

public class shipWobble : MonoBehaviour
{

    private Transform selfT;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selfT = GetComponent<Transform>();

    }

    // Update is called once per frame
    void Update()
    {
        selfT.localRotation = Quaternion.Euler(Mathf.Sin(Time.time) * 3, Mathf.Sin(Time.time * 0.379f) * 2, Mathf.Sin(Time.time * 0.654f));
    }
}
