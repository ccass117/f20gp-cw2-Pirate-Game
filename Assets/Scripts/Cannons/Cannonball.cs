using UnityEngine;
using System.Collections;

public class Cannonball : MonoBehaviour
{
    public float gravityMultiplier = 1f; // gravityMultiplier is used to increase the gravity of the cannonball, usually set by the firing ship - artificial gravity is used rather than  game gravity
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
        // splash if the ball hits the water.
        if (transform.position.y < 0f && splashCoroutine == null)
        {
            splash = StartCoroutine(splishsplash());
        }
    }

    void FixedUpdate()
    {
        // add artificial gravity to the cannonball each frame
        rb.AddForce(Physics.gravity * (gravityMultiplier), ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Ignore collisions with the firing ship (and its children).
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

    // play splash sound effect and then delete the object (it will be hidden under the water so its fine to stay there for a bit)
    private IEnumerator splashAndDestroy()
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

/*
 * 
 * ...........
 * ...................__
 * 
 * ............./��/'...'/���`��
 * 
 * ........../'/.../..../......./��\
 * 
 * ........('(...�...�.... �~/'...')
 * 
 * .........\.................'...../
 * 
 * ..........''...\.......... _.��
 * 
 * 
 * ............\..............(
 * 
 * BROFIST ...........
 */