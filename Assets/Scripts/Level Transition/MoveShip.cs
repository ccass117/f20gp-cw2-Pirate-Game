using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class MoveShip : MonoBehaviour
{
    [SerializeField] MapPoints mapPoints; // points on map ship will move to
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private AudioSource mapMarkerSound; // pencil stroke sound for map markers

    private static Dictionary<int, bool> markedPoints = new Dictionary<int, bool>(); // store visited points
    public static int lvls = -1; // to show map without doing anything before level_1

    private void Start()
    {
        // show map to player without doing anything before first level
        if (lvls < 0)
        {
            lvls = 0;
            StartCoroutine(LoadLevel1());
        }
        else
        {
            PositionShip();
            PreviouslyMarked();
            StartCoroutine(DelayMark());
        }
    }

    private IEnumerator LoadLevel1()
    {
        yield return new WaitForSeconds(3f);
        levelLoader.LoadLevel("level_1");
    }

    // get properties of the current recorded map point (level just completed)
    // move ship to start at last map position 
    private void PositionShip()
    {
        Transform currentPoint = mapPoints.GetPoint(lvls);
        transform.position = currentPoint.position;
        Debug.Log($"ship positioned at: {transform.position}");
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

    // small delay makes the transition feel better
    private IEnumerator DelayMark()
    {
        yield return new WaitForSeconds(1f);
        MarkMap(mapPoints.GetPoint(lvls));
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

            // load win screen if the level just beaten is level_12 (final)
            if (lvls == 11)
            {
                levelLoader.LoadLevel("Win");
            }
            else
            {
                MoveToNext();
            }
        }
    }

    // helper to find sprites
    private (SpriteRenderer x1, SpriteRenderer x2) FindSprite(Transform thisPoint)
    {
        SpriteRenderer x1 = thisPoint.Find("x_1").GetComponent<SpriteRenderer>();
        SpriteRenderer x2 = thisPoint.Find("x_2").GetComponent<SpriteRenderer>();

        return (x1, x2);
    }

    // move ship to the next map point
    void MoveToNext()
    {
        StartCoroutine(ShipTransition());
    }

    private IEnumerator ShipTransition()
    {
        yield return new WaitForSeconds(1.5f);

        Transform nextPoint = mapPoints.GetPoint(lvls + 1);
        Vector3 direction = nextPoint.position - transform.position;

        // maths derived / adapted from: https://www.reddit.com/r/Unity2D/comments/xz5u0m/smoothly_rotating_a_object_towards_a_vector/?rdt=51309
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Sequence shipMove = DOTween.Sequence();
        // angle sprite towards next point on map and move it there
        shipMove.Append(transform.DORotate(new Vector3(0, 0, angle), 1.5f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine));
        shipMove.Join(transform.DOMove(nextPoint.position, 4f).SetEase(Ease.InOutSine));

        if (direction.x < 0) // backwards
        {
            Sequence flipSprite = DOTween.Sequence();
            flipSprite.Append(transform.DOScaleY(-10, 0.5f).SetEase(Ease.InOutSine)); // flip on Y to correct sprite

            shipMove.Join(transform.DORotate(new Vector3(0, 0, 180), 1.5f).SetEase(Ease.InOutSine).SetDelay(2.5f)); // turn sprite around
        }
        else
        {
            shipMove.Join(transform.DORotate(Vector3.zero, 1.5f).SetEase(Ease.InOutSine).SetDelay(2.5f)); // default rotation
        }
        shipMove.OnComplete(OnPoint);
    }

    // when ship gets to the next point, increment lvls and prepare to load next level
    void OnPoint()
    {
        lvls++;
        StartCoroutine(LoadNextLevel());
    }

    private IEnumerator LoadNextLevel()
    {
        // short delay before loading because it feels nicer to have it
        yield return new WaitForSeconds(1.5f);

        // if lvls = 1 -> "level_" + (1 + 1) = "level_2"
        // if lvls = 2 -> "level_" + (2 + 1) = "level_3" etc.
        string next = "level_" + (lvls + 1);
        Debug.Log($"loading: {next}");
        levelLoader.LoadLevel(next); // change to ("LevelChange") to test map movement
    }
}
