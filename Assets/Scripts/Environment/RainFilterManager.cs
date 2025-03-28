using System.Collections;
using UnityEngine;

public class RainFilterManager : MonoBehaviour
{
    private Transform cameraTransform;
    public float fallSpeed = 1.0f;

    private Transform[] rainTiles = new Transform[4];
    public Transform topTile;
    public Transform middleTile;
    public Transform bottomTile;
    private AudioSource stormAudio;
    public float lightningMin = 10f;  //Minimum time between lightning
    public float lightningMax = 20f;  //Maximum time between lightning
    public GameObject lightningTile;  // Reference to the lightning tile object
    private SpriteRenderer lightningRenderer;
    private Color lightningColor;


    void Start()
    {



        // Locate the Player object
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        // Find the Main Camera as a child of the Player
        Transform mainCamera = player.transform.Find("Main Camera");
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found as a child of Player!");
            return;
        }

        // Store reference to camera's transform
        cameraTransform = mainCamera;

        // Get the rain tiles (children of the RainOverlay object)
        int i = 0;
        foreach (Transform child in transform)
        {
            Debug.Log($"iterating through child: {i}");
            rainTiles[i] = child;
            i++;
        }
        topTile = rainTiles[0];
        middleTile = rainTiles[1];
        bottomTile = rainTiles[2];

        stormAudio = GetComponent<AudioSource>();
        lightningRenderer = lightningTile.GetComponent<SpriteRenderer>();
        lightningColor = lightningRenderer.material.color;
        StartCoroutine(Lightning());
    }

    void Update()
    {
        if (cameraTransform != null)
        {
            // Set this object's position relative to the camera
            transform.position = new Vector3(
                cameraTransform.position.x,
                cameraTransform.position.y - 0.4f,
                cameraTransform.position.z
            );
            transform.rotation = Quaternion.Euler(
                transform.rotation.eulerAngles.x,
                cameraTransform.rotation.eulerAngles.y,
                transform.rotation.eulerAngles.z
            );
        }

        // Move the tiles down
        middleTile.localPosition += new Vector3(0, 0, -fallSpeed * Time.deltaTime);
        topTile.localPosition = new Vector3(0, 0, middleTile.localPosition.z + 2);
        bottomTile.localPosition = new Vector3(0, 0, middleTile.localPosition.z - 2);

        // If the bottom tile goes off-screen, reset it
        if (bottomTile.localPosition.z < -3)
        {
            ResetRainTilePosition();
        }
    }

    void ResetRainTilePosition()
    {
        // Reset bottom tile position to above the top tile
        bottomTile.localPosition = new Vector3(bottomTile.localPosition.x, bottomTile.localPosition.y, topTile.localPosition.z + 1);

        // Shift the tiles down
        Transform temp = topTile;
        topTile = middleTile;
        middleTile = bottomTile;
        bottomTile = temp;
    }

    private IEnumerator Lightning()
    {
        while (true)  // Infinite loop
        {
            // Wait for a random time between minInterval and maxInterval
            float waitTime = Random.Range(lightningMin, lightningMax);
            yield return new WaitForSeconds(waitTime);

            // Play the storm audio
            stormAudio.Stop();
            stormAudio.Play();

            StartCoroutine(LightningFade());
        }
    }
    private IEnumerator LightningFade()
    {
        // Quickly fade to full opacity (1)
        float fadeDuration = 0.3f;  // Duration for the quick fade to full
        float startTime = Time.time;

        while (Time.time - startTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 0.75f, (Time.time - startTime) / fadeDuration);
            lightningColor.a = alpha;
            lightningRenderer.color = lightningColor;
            yield return null;
        }

        // Wait before starting the slow fade back to 0
        yield return new WaitForSeconds(0.1f);

        // Slowly fade back to invisible (0)
        float slowFadeDuration = 1.5f;  // Duration for the slow fade back to invisible
        startTime = Time.time;

        while (Time.time - startTime < slowFadeDuration)
        {
            float alpha = Mathf.Lerp(0.75f, 0f, (Time.time - startTime) / slowFadeDuration);
            lightningColor.a = alpha;
            lightningRenderer.color = lightningColor;
            yield return null;
        }

        // Ensure it is completely transparent at the end
        lightningColor.a = 0f;
        lightningRenderer.color = lightningColor;
    }
}
