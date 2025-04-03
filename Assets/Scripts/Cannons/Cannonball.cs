using UnityEngine;
using System.Collections;

public class Cannonball : MonoBehaviour
{
    //effective shooting range should be gravity mult * speed
    public float gravityMultiplier = 1f;
    public GameObject splashPrefab;
    public AudioClip hitSound;
    private Rigidbody rb;
    private AudioSource audioSource;
    private Coroutine splash;
    public GameObject firingShip;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (transform.position.y < 0f && splash == null)
        {
            splash = StartCoroutine(splishsplash());
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * (gravityMultiplier), ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (firingShip != null && (collision.transform.IsChildOf(firingShip.transform) || collision.gameObject == firingShip))
        {
            return;
        }
        //so you can ignore collisisons below water, otherwise splash
        if (transform.position.y >= 0)
        {
          PlaySound(hitSound);
          Destroy(gameObject);

        }
    }

    private IEnumerator splishsplash()
    {
        Instantiate(splashPrefab, new Vector3(transform.position.x, 0.5f, transform.position.z), Quaternion.identity);
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