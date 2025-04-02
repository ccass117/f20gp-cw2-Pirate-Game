using UnityEngine;
using DG.Tweening;

public class ScreenWaterMovement : MonoBehaviour
{
    [SerializeField] private GameObject screenWater0;
    [SerializeField] private GameObject screenWater1;
    [SerializeField] private GameObject screenWater2;

    private float moveDistance = 2f;
    private float moveTime = 1.5f;

    private void Start()
    {
        // right > left
        screenWater0.transform.DOMoveX(screenWater0.transform.position.x + moveDistance, moveTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // left > right
        screenWater1.transform.DOMoveX(screenWater1.transform.position.x - moveDistance, moveTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // right > left
        screenWater2.transform.DOMoveX(screenWater2.transform.position.x + moveDistance, moveTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
