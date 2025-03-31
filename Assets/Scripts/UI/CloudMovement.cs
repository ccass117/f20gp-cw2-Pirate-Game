using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class CloudMovement : MonoBehaviour
{
    private float startPos = -180f;
    private float endPos = 180f;

    private List<Transform> clouds = new List<Transform>();

    private void Start()
    {
        foreach (Transform cloud in transform)
        {
            clouds.Add(cloud);
            StartCoroutine(MoveCloud(cloud));
        }
    }

    private IEnumerator MoveCloud(Transform cloud)
    {
        while (true)
        {
            float distance = endPos - cloud.position.x;
            float moveTime = distance / 4f;

            cloud.DOMoveX(endPos, moveTime)
                .SetEase(Ease.Linear);

            yield return new WaitForSeconds(moveTime);

            cloud.position = new Vector3(startPos, cloud.position.y, cloud.position.z);
        }
    }
}
