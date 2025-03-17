using UnityEngine;
using System.Collections;

public class Cannonball : MonoBehaviour
{
    public float damage = 1f;
    public AudioClip splashSound;
    public AudioClip hitSound;
    private Rigidbody rb;
    private AudioSource audioSource;
    private Coroutine splashCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        // play splash noise and destroy ball if it hits the water.
        if (transform.position.y < 0f && splashCoroutine == null)
        {
            splashCoroutine = StartCoroutine(splashAndDestroy());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        PlaySound(hitSound);
        // update this with hit and damage logic once implemented
    }

    private IEnumerator splashAndDestroy()
    {
        PlaySound(splashSound);
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}