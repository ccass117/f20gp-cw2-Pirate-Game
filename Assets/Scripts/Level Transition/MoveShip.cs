using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MoveShip : MonoBehaviour
{
    public MapPoints mapPoints; // points on the map
    public LevelLoader loadLevel; // crossfade anim and level loading
    private int lvls; // track number of completed levels

    private void Start()
    {
        // for testing purposes
        if (Application.isEditor)
        {
            lvls = 0;
        }
        else
        {
            // store data across sessions
            lvls = PlayerPrefs.GetInt("LevelsCompleted", 0);
        }

        MoveToNext();
    }

    void MoveToNext()
    {
        StartCoroutine(ShipDelayIn());

        IEnumerator ShipDelayIn()
        {
            yield return new WaitForSeconds(2);

            Vector3 nextPoint = mapPoints.GetNextPoint(lvls + 1);

            // DOTween shortcut to move the ship to the next point on the map in 'time' seconds
            transform.DOMove(nextPoint, 3)
                .SetEase(Ease.InOutSine)
                .OnComplete(OnPoint);
        }
    }
  
    void OnPoint()
    {
        // increments current level and stores it
        lvls++;
        PlayerPrefs.SetInt("LevelsCompleted", lvls);
        PlayerPrefs.Save();

        StartCoroutine(ShipDelayOut());
        IEnumerator ShipDelayOut()
        {
            yield return new WaitForSeconds(3);
            LoadNext();
        }
    }

    void LoadNext()
    {       
        string next = "level_" + (lvls + 1);
        loadLevel.LoadLevel(next);
        // if lvls = 1 -> "level_" + (1 + 1) = "level_2"
        // if lvls = 2 -> "level_" + (2 + 1) = "level_3" etc.
    }

    void LoadEndgame()
    {
        loadLevel.LoadLevel("Endgame"); // scene to be created
    }
}


// FOR TESTING MAP MOVEMENT

/*
using UnityEngine;
using DG.Tweening;

public class MoveShip : MonoBehaviour
{
    private int lvls = 0; // track current level, initialise to 0

    private void Start()
    {
        MoveToNext();
    }

    void MoveToNext()
    {

        Vector3[] points = {
        new Vector3(-39, 121, 0), // A
        new Vector3(79, 1, 0), // B
        new Vector3(180, 209, 0), // C
        new Vector3(260, 50, 0), // D
        new Vector3(429, -2, 0), // E
        new Vector3(364, 248, 0), // F
        new Vector3(473, 383, 0), // G
        new Vector3(640, 295, 0), // H
        new Vector3(515, 172, 0), // I
        new Vector3(662, 108, 0), // J
        new Vector3(849, 186, 0), // K
        new Vector3(1016, 284, 0) // L
        };

        transform.DOMove(points[lvls], 3)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                lvls++;
                if (lvls < points.Length)
                {
                    MoveToNext();
                }
            });
    }
}
*/

