using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class CloudMovement : MonoBehaviour
{
    private float startPos = -180f; // start point
    private float endPos = 450f; // goal point

    private List<Transform> clouds = new List<Transform>();

    private void Start()
    {
        foreach (Transform cloud in transform)
        {
            clouds.Add(cloud);
            StartCoroutine(MoveCloud(cloud));
        }

        // move each cloud to the goal point then reset at the start point
        IEnumerator MoveCloud(Transform cloud)
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
}
