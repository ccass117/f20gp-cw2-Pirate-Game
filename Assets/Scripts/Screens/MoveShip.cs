using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MoveShip : MonoBehaviour
{
    public MapPoints mapPoints; // points on the map
    public LevelLoader loadLevel; // crossfade anim and level loading
    public float time; // time for ship to move on the map
    private int lvls = 0; // track current level, initialise to 0

    private void Start()
    {
        lvls = PlayerPrefs.GetInt("LevelsCompleted");
        MoveToNext();
    }

    void MoveToNext()
    {
        // if there are still points to be moved to on the map then trigger ship movement to the next point
        if (lvls < mapPoints.points.Length - 1)
        {
            StartCoroutine(ShipDelay());

            IEnumerator ShipDelay()
            {
                yield return new WaitForSeconds(2);

                Vector3 nextPoint = mapPoints.GetNextPoint(lvls + 1);

                // DOTween shortcut to move the ship to the next point on the map in 'time' seconds
                transform.DOMove(nextPoint, time)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(OnPoint);
            }

        }
        else
        {
            LoadEndgame(); // this should probably be placed in level 12's script
        }
    }
  
    void OnPoint()
    {
        // increments current level and stores it
        lvls++;
        PlayerPrefs.SetInt("LevelsCompleted", lvls);
        PlayerPrefs.Save();
        
        LoadNext();
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

