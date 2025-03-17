using UnityEngine;
using System.Collections;

public class SirenTrigger : MonoBehaviour
{
    private SphereCollider sirenTrigger;
    private CapsuleCollider hitbox;
    private AudioSource sirenSong;
    private Coroutine volumeCoroutine;
    private GameObject playerShip;
    private Transform playerTransform;
    private bool playerInsideRadius = false;

    public float triggerRadius = 10f;
    public float volumeChangeDuration = 1f;
    public float targetVolume = 0.25f;
    public float rotationSpeed = 1.0f; // Rotation speed in degrees per second

    private void Start()
    {
        playerShip = GameObject.FindWithTag("Player");
        if (playerShip != null)
        {
            playerTransform = playerShip.transform;
        }
        else
        {
            Debug.LogWarning("Player ship not found. Ensure it has the 'Player' tag assigned.");
        }

        
        sirenSong = GetComponent<AudioSource>();
        hitbox = GetComponent<CapsuleCollider>();
        sirenTrigger = GetComponent<SphereCollider>();
        sirenTrigger.isTrigger = true;
        sirenTrigger.radius = triggerRadius;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other == playerShip.GetComponent<Collider>())
        {
            // Only activate influence if not already in
            if (!playerInsideRadius)
            {
                Debug.Log("Player has entered the Siren's area.");
                StartVolumeChange(targetVolume);

                ShipController controller = other.GetComponent<ShipController>();
                if (controller != null)
                {
                    controller.sirenInfluenceActive = true;
                    controller.sirenTarget = transform;
                }

                playerInsideRadius = true;
            }
        }

        // Cannonball entered Siren's hitbox (Capsule Collider)
        if (other.CompareTag("Cannonball"))
        {
            // Check if this is the hitbox capsule trigger and not the big sphere trigger
            if (hitbox != null && hitbox.bounds.Contains(other.transform.position))
            {
                Debug.Log("Siren was hit by a cannonball!");

                Destroy(other.gameObject); // Remove the cannonball

                StartVolumeChange(0f);
                sirenSong.Stop();

                // Free the player
                if (playerShip != null)
                {
                    releasePlayer(playerShip.GetComponent<Collider>()); ;
                }

                Destroy(gameObject);
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInsideRadius)
        {
            // Ensure the player is truly outside the attraction zone
            if (!sirenTrigger.bounds.Contains(other.transform.position))
            {
                releasePlayer(other);
                playerInsideRadius = false;
            }
        }
    }

    private void releasePlayer(Collider other)
    {
        Debug.Log("Player has left the Siren's area.");
        StartVolumeChange(0f);

        ShipController controller = other.GetComponent<ShipController>();
        if (controller != null)
        {
            controller.sirenInfluenceActive = false;
            controller.sirenTarget = null;
        }
    }


    private void StartVolumeChange(float newVolume)
    {
        // Stop any existing volume change coroutine
        if (volumeCoroutine != null)
        {
            StopCoroutine(volumeCoroutine);
        }
        // Start a new coroutine to change the volume
        volumeCoroutine = StartCoroutine(ChangeVolumeOverTime(newVolume));
    }

    private IEnumerator ChangeVolumeOverTime(float newVolume)
    {
        float startVolume = sirenSong.volume;
        float elapsedTime = 0f;

        while (elapsedTime < volumeChangeDuration)
        {
            elapsedTime += Time.deltaTime;
            sirenSong.volume = Mathf.Lerp(startVolume, newVolume, elapsedTime / volumeChangeDuration);
            yield return null;
        }

        // Ensure the volume is set to the exact target at the end
        sirenSong.volume = newVolume;
    }

}
