using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MoveShip : MonoBehaviour
{
    public MapPoints mapPoints;
    [SerializeField] private LevelLoader levelLoader;

    private static int lvls = 0; // Static variable for Editor mode

    private void Start()
    {
        Debug.Log($"lvls at start: {lvls}");

        // note current position so ship doesn't start at the first one every time
        transform.position = mapPoints.GetNextPoint(lvls);
        Debug.Log($"ship positioned at: {transform.position}");

        MoveToNext();
    }

    void MoveToNext()
    {
        StartCoroutine(ShipDelayIn());

        IEnumerator ShipDelayIn()
        {
            yield return new WaitForSeconds(2);

            Vector3 nextPoint = mapPoints.GetNextPoint(lvls + 1);

            transform.DOMove(nextPoint, 3)
                .SetEase(Ease.InOutSine)
                .OnComplete(OnPoint);
        }
    }

    void OnPoint()
    {
        lvls++;
        Debug.Log($"after increment: {lvls}");

        StartCoroutine(ShipDelayOut());

        IEnumerator ShipDelayOut()
        {
            // short delay before loading because it feels nicer to have it
            yield return new WaitForSeconds(1.5f);
            LoadNext();
        }
    }

    void LoadNext()
    {
        // if lvls = 1 -> "level_" + (1 + 1) = "level_2"
        // if lvls = 2 -> "level_" + (2 + 1) = "level_3" etc.
        string next = "level_" + (lvls + 1);
        Debug.Log($"loading: {next}");
        levelLoader.LoadLevel(next);
    }
}

