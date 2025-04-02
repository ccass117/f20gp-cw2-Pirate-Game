using UnityEngine;
using System.Collections;

public class GaonCannon : MonoBehaviour
{
    [Tooltip("Laser projectile prefab to spawn when firing.")]
    public GameObject laserPrefab;
    [Tooltip("Local offset from the player's position to spawn the laser (e.g., in front of the ship).")]
    public Vector3 spawnOffset = new Vector3(0, 0, 2f);
    [Tooltip("Cooldown time between laser shots (in seconds).")]
    public float cooldown = 20f;

    private bool canShoot = true;

    void Update()
    {
        // Listen for middle mouse button click.
        if (canShoot && Input.GetMouseButtonDown(2))
        {
            StartCoroutine(ShootLaser());
        }
    }

    IEnumerator ShootLaser()
    {
        canShoot = false;

        if (laserPrefab != null)
        {
            GameObject laserInstance = Instantiate(laserPrefab, transform);
            laserInstance.transform.localPosition = spawnOffset;
            laserInstance.transform.localRotation = Quaternion.identity;
            Debug.Log("Laser fired from local position " + spawnOffset);
        }
        else
        {
            Debug.LogWarning("GaonCannon: No laserPrefab assigned.");
        }

        yield return new WaitForSeconds(cooldown);
        canShoot = true;
    }

    public void Initialize()
    {
        if (laserPrefab == null)
            laserPrefab = Resources.Load<GameObject>("LaserPrefabName");
    }

    public static void ActivateLaserBuff()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("ActivateLaserBuff: No GameObject tagged 'Player' found. Delaying activation.");
            CoroutineHelper.Instance.StartCoroutine(WaitForPlayerAndActivate());
            return;
        }
        AddLaserBuffComponent(player);
    }

    public static void DeactivateLaserBuff()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            GaonCannon gc = player.GetComponent<GaonCannon>();
            if (gc != null)
            {
                Destroy(gc);
                Debug.Log("Laser Buff deactivated on " + player.name);
            }
        }
    }

    private static IEnumerator WaitForPlayerAndActivate()
    {
        GameObject player = null;
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }
        AddLaserBuffComponent(player);
    }

    private static void AddLaserBuffComponent(GameObject player)
    {
        GaonCannon gc = player.GetComponent<GaonCannon>();
        if (gc == null)
        {
            gc = player.AddComponent<GaonCannon>();
            gc.Initialize();
        }
        Debug.Log("Laser Buff activated on " + player.name);
    }
}
