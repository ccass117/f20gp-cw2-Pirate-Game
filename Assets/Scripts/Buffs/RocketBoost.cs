using UnityEngine;
using System.Collections;

public class RocketBoost : MonoBehaviour
{
    [Tooltip("Distance to move forward during boost.")]
    public float boostDistance = 20f;
    [Tooltip("Speed factor for the boost. Higher values result in a faster burst.")]
    public float boostSpeed = 30f;
    [Tooltip("Cooldown time between boosts (seconds).")]
    public float cooldown = 15f;
    [Tooltip("Visual effect prefab to spawn at the ship's rear when boosting.")]
    public GameObject boostEffectPrefab;

    private bool canBoost = true;

    void Update()
    {
        if (canBoost && Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(PerformBoost());
        }
    }

    IEnumerator PerformBoost()
    {
        canBoost = false;

        if (boostEffectPrefab != null)
        {
            Transform boostOrigin = transform.Find("BoostOrigin");
            Vector3 effectPos = boostOrigin != null ? boostOrigin.position : transform.position;
            Quaternion effectRot = boostOrigin != null ? boostOrigin.rotation : transform.rotation;
            Instantiate(boostEffectPrefab, effectPos, effectRot, transform);
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + transform.forward * boostDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * boostSpeed / boostDistance;
            float easeT = 1 - Mathf.Pow(1 - t, 2);
            transform.position = Vector3.Lerp(startPos, targetPos, easeT);
            yield return null;
        }

        yield return new WaitForSeconds(cooldown);
        canBoost = true;
    }

    public void Initialize()
    {

    }


    public static void ActivateRocketBoost()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("ActivateRocketBoost: No GameObject tagged 'Player' found. Delaying activation.");
            CoroutineHelper.Instance.StartCoroutine(WaitForPlayerAndActivate());
            return;
        }
        AddRocketBoostComponent(player);
    }

    public static void DeactivateRocketBoost()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            RocketBoost rb = player.GetComponent<RocketBoost>();
            if (rb != null)
            {
                Destroy(rb);
                Debug.Log("Rocket Boost deactivated on " + player.name);
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
        AddRocketBoostComponent(player);
    }

    private static void AddRocketBoostComponent(GameObject player)
    {
        RocketBoost rb = player.GetComponent<RocketBoost>();
        if (rb == null)
        {
            rb = player.AddComponent<RocketBoost>();
            rb.Initialize();
        }
        Debug.Log("Rocket Boost activated on " + player.name);
    }
}
