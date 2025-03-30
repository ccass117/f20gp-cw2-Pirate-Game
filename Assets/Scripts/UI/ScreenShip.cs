using UnityEngine;
using DG.Tweening;

public class FloatingShip : MonoBehaviour
{
    private float bobbingAltitude = 0.5f;
    private float bobbingTime = 2f;
    private float tiltAngle = 2f;
    private float tiltTime = 2.5f;

    private void Start()
    {
        StartBobbing();
        StartTilting();
    }

    void StartBobbing()
    {
        transform.DOMoveY(transform.position.y + bobbingAltitude, bobbingTime)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void StartTilting()
    {
        transform.DORotate(new Vector3(0, 0, tiltAngle), tiltTime)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                transform.DORotate(new Vector3(0, 0, -tiltAngle), tiltTime)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(StartTilting);
            });
    }
}
