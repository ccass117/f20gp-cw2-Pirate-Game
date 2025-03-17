using UnityEngine;
using System.Collections;

public class SirenTrigger : MonoBehaviour
{
    private CapsuleCollider sirenTrigger;
    private AudioSource sirenSong;
    private Coroutine volumeCoroutine;
    private GameObject playerShip;
    private Transform playerTransform;

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
        sirenTrigger = GetComponent<CapsuleCollider>();
        sirenTrigger.isTrigger = true;
        sirenTrigger.radius = triggerRadius;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered the Siren's area.");
            StartVolumeChange(targetVolume);

            ShipController controller = other.GetComponent<ShipController>();
            if (controller != null)
            {
                controller.sirenInfluenceActive = true;
                controller.sirenTarget = transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
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
