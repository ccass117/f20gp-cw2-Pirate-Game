using UnityEngine;
using System.Collections;

public class Cannonball : MonoBehaviour
{
    public AudioClip splashSound;
    public float gravityMultiplier = 1f;
    public AudioClip hitSound;
    private Rigidbody rb;
    private AudioSource audioSource;
    private Coroutine splashCoroutine;
    public GameObject firingShip;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (transform.position.y < 0f && splashCoroutine == null)
        {
            splashCoroutine = StartCoroutine(splashAndDestroy());
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * (gravityMultiplier), ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Ignore collisions with the firing shi (and its children).
        if (firingShip != null && (collision.transform.IsChildOf(firingShip.transform) || collision.gameObject == firingShip))
        {
            return; // Ignore the collision.
        }

        if (transform.position.y >= 0)
        {
          PlaySound(hitSound);
          Destroy(gameObject);

        }
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