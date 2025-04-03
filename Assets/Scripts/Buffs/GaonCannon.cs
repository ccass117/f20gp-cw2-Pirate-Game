using UnityEngine;
using System.Collections;

public class GaonCannon : MonoBehaviour
{
    //expects a prefab reference in resources named exactly GaonCannon
    public GameObject gaonCannon;
    public Vector3 spawnOffset = new Vector3(0, 0, 2f);
    public float cooldown = 20f;

    private bool canShoot = true;

    void Update()
    {
        if (canShoot && Input.GetMouseButtonDown(2))
        {
            StartCoroutine(Laser());
        }
    }


    IEnumerator Laser()
    {
        canShoot = false;

        if (gaonCannon != null)
        {
            GameObject laserInstance = Instantiate(gaonCannon, transform);
            laserInstance.transform.localPosition = spawnOffset;
            laserInstance.transform.localRotation = Quaternion.identity;
        }

        yield return new WaitForSeconds(cooldown);
        canShoot = true;
    }

    //loader for the prefab
    public void Initialise()
    {
        if (gaonCannon == null)
            gaonCannon = Resources.Load<GameObject>("GaonCannon");
    }

    //use this one for registering the buff
    public static void Fire()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            CoroutineHelper.Instance.StartCoroutine(ChargeUp());
            return;
        }
        AddLaser(player);
    }

    public static void Deactivate()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            GaonCannon gc = player.GetComponent<GaonCannon>();
            if (gc != null)
            {
                Destroy(gc);
            }
        }
    }

    private static IEnumerator ChargeUp()
    {
        GameObject player = null;
        while (player == null)
        {
            yield return null;
        }
        AddLaser(player);
    }

    private static void AddLaser(GameObject player)
    {
        GaonCannon gc = player.GetComponent<GaonCannon>();
        if (gc == null)
        {
            gc = player.AddComponent<GaonCannon>();
            gc.Initialise();
        }
    }
}
