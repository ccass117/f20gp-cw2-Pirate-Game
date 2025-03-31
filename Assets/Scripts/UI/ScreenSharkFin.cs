using UnityEngine;
using DG.Tweening;

public class SharkFin : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 arcPos;

    public float duration = 3.5f; // move time

    void Start()
    {
        startPos = transform.position;
        endPos = startPos + new Vector3(84, 0, 0);
        arcPos = new Vector3((startPos.x + endPos.x) / 2,startPos.y + 16f, 100);

        MoveFin();
    }

    void MoveFin()
    {
        // DOTween sequence to move shark fin in an upwards arc and then again but reversed
        Sequence sharkFin = DOTween.Sequence();
        sharkFin.Append(transform.DOScaleX(12, 0f));
        sharkFin.AppendInterval(10f);
        sharkFin.Append(transform.DOMove(arcPos, duration / 2).SetEase(Ease.OutQuad));
        sharkFin.Append(transform.DOMove(endPos, duration / 2).SetEase(Ease.InQuad));
        sharkFin.AppendInterval(5f);

        sharkFin.Append(transform.DOScaleX(-12, 0f));
        sharkFin.Append(transform.DOMove(arcPos, duration / 2).SetEase(Ease.OutQuad));
        sharkFin.Append(transform.DOMove(startPos, duration / 2).SetEase(Ease.InQuad));
        sharkFin.AppendInterval(5f);

        sharkFin.SetLoops(-1);
    }
}