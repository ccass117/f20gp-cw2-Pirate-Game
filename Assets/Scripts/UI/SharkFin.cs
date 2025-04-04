using UnityEngine;
using DG.Tweening;

public class SharkFin : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 endPos;
    private Vector3 arcPos;
    private Sequence sharkFin;

    private float duration = 3.5f;

    void Start()
    {
        startPos = transform.position;
        endPos = startPos + new Vector3(84, 0, 0);
        arcPos = new Vector3((startPos.x + endPos.x) / 2, startPos.y + 16f, 100);

        MoveFin();
    }

    public void MoveFin()
    {
        // prevent dupes
        if (sharkFin != null && sharkFin.IsActive())
        {
            sharkFin.Kill();
        }

        // DOTween sequence to move shark fin in an upwards arc and then again but reversed
        sharkFin = DOTween.Sequence();

        sharkFin.Append(transform.DOScaleX(12, 0f)); // face right
        sharkFin.AppendInterval(10f); // wait before starting shark movement

        // left > right
        sharkFin.Append(transform.DOMove(arcPos, duration / 2).SetEase(Ease.OutQuad));
        sharkFin.Append(transform.DOMove(endPos, duration / 2).SetEase(Ease.InQuad));

        sharkFin.Append(transform.DOScaleX(-12, 0f)); // face left
        sharkFin.AppendInterval(5f); // wait before going back

        // right > left
        sharkFin.Append(transform.DOMove(arcPos, duration / 2).SetEase(Ease.OutQuad));
        sharkFin.Append(transform.DOMove(startPos, duration / 2).SetEase(Ease.InQuad));
        
        // wait additional ~5 seconds before restarting sequence
        sharkFin.AppendInterval(5f);
        sharkFin.SetLoops(-1);
    }

    // stop sequence and move fin back to start position
    public void KillFin()
    {
        if (sharkFin != null && sharkFin.IsActive())
        {
            sharkFin.Kill();
        }
        transform.position = startPos;
    }
}