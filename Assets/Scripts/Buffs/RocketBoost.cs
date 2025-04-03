using UnityEngine;
using System.Collections;

public class RocketBoost : MonoBehaviour
{
    public float boostDistance = 20f;
    public float boostSpeed = 30f;
    public float cooldown = 15f;
    public GameObject boostEffect;

    private bool canBoost = true;

    void Update()
    {
        if (canBoost && Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(Boost());
        }
    }

    IEnumerator Boost()
    {
        canBoost = false;

        if (boostEffect != null)
        {
            Transform boostOrigin = transform.Find("BoostOrigin");
            Vector3 effectPos = boostOrigin != null ? boostOrigin.position : transform.position;
            Quaternion effectRot = boostOrigin != null ? boostOrigin.rotation : transform.rotation;
            Instantiate(boostEffect, effectPos, effectRot, transform);
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + transform.forward * boostDistance;

        float t = 0f;
        //quadratic ease-out (1-(1-t)^2) deceleration
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

    //use this one for registering the buff
    public static void ActivateBoost()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            CoroutineHelper.Instance.StartCoroutine(Attach());
            return;
        }
        AttachBoost(player);
    }

    public static void DeactivateBoost()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            RocketBoost rb = player.GetComponent<RocketBoost>();
            if (rb != null)
            {
                Destroy(rb);
            }
        }
    }

    //don't reassign player var, just attach so that player instance from a previous scene doesn't get stored since this class isn't actually being attached to anything
    private static IEnumerator Attach()
    {
        GameObject player = null;
        while (player == null)
        {
            yield return null;
        }
        AttachBoost(player);
    }

    private static void AttachBoost(GameObject player)
    {
        RocketBoost rb = player.GetComponent<RocketBoost>();
        if (rb == null)
        {
            rb = player.AddComponent<RocketBoost>();
        }
    }
}
