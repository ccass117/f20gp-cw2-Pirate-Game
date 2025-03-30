using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

public class MoveShip : MonoBehaviour
{
    public MapPoints mapPoints; // points on map ship will move to
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private AudioSource mapMarkerSound; // pencil stroke sound for map markers

    // stores the points already visited / marked to be restored on each load
    private static Dictionary<int, bool> markedPoints = new Dictionary<int, bool>();
    private static int lvls = 0;

    private void Start()
    {
        Debug.Log($"lvls at start: {lvls}");

        // get properties of the current recorded map point (level just completed)
        // move ship to start at last map position 
        Transform currentPoint = mapPoints.GetPoint(lvls);
        transform.position = currentPoint.position;
        Debug.Log($"ship positioned at: {transform.position}");

        PreviouslyMarked();

        // small delay makes the transition feel better
        StartCoroutine(SmallDelay());
        IEnumerator SmallDelay()
        {
            yield return new WaitForSeconds(1f);
            MarkMap(currentPoint);
        }
    }
    // checks for previously marked levels against the dictionary
    // adds a 'X' back to the points that previously had one
    void PreviouslyMarked()
    {
        for (int i = 0; i <= lvls; i++)
        {
            if (markedPoints.ContainsKey(i) && markedPoints[i])
            {
                Transform prevPoint = mapPoints.GetPoint(i);
                var sprite = FindSprite(prevPoint);

                // move up sorting order to become visible
                if (sprite.x1 != null && sprite.x2 != null)
                {
                    sprite.x1.sortingOrder = 1;
                    sprite.x2.sortingOrder = 1;
                }
            }
        }
    }

    // places the 'X' mark on current position on map
    void MarkMap(Transform currentPoint)
    {
        StartCoroutine(PlaceCross());
        IEnumerator PlaceCross()
        {
            var sprite = FindSprite(currentPoint);

            sprite.x1.sortingOrder = 1;
            mapMarkerSound.Play();
            yield return new WaitForSeconds(0.5f);

            sprite.x2.sortingOrder = 1;
            mapMarkerSound.Play();
            yield return new WaitForSeconds(0.5f);

            // notes the current position as having been visited / marked
            if (!markedPoints.ContainsKey(lvls))
                markedPoints[lvls] = true;

            MoveToNext();
        }
    }

    // helper to find sprites
    private (SpriteRenderer x1, SpriteRenderer x2) FindSprite(Transform thisPoint)
    {
        SpriteRenderer x1 = thisPoint.Find("x_1")?.GetComponent<SpriteRenderer>();
        SpriteRenderer x2 = thisPoint.Find("x_2")?.GetComponent<SpriteRenderer>();

        return (x1, x2);
    }

    // move ship to the next map point
    void MoveToNext()
    {
        StartCoroutine(ShipDelayIn());
        IEnumerator ShipDelayIn()
        {
            yield return new WaitForSeconds(1.5f);

            Transform nextPoint = mapPoints.GetPoint(lvls + 1);
            Vector3 nextPointPos = nextPoint.position;

            transform.DOMove(nextPointPos, 3)
                .SetEase(Ease.InOutSine)
                .OnComplete(OnPoint);
        }
    }

    // when ship gets to the next point, increment lvls and prepare to load next level
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

    // load next level with the naming convention "level_x" using own level loader
    void LoadNext()
    {
        // if lvls = 1 -> "level_" + (1 + 1) = "level_2"
        // if lvls = 2 -> "level_" + (2 + 1) = "level_3" etc.
        string next = "level_" + (lvls + 1);
        Debug.Log($"loading: {next}");
        levelLoader.LoadLevel(next);
    }
}

