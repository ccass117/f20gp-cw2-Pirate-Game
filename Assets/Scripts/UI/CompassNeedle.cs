using UnityEngine;
using System.Linq;

public class CompassNeedle : MonoBehaviour
{
    private Transform player;
    private RectTransform needle;
    public float spinSpeed = -60f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        needle = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (player == null)
            return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closestEnemy = GetClosestEnemy(enemies);

        if (closestEnemy != null)
        {
            Vector3 directionToEnemy = (closestEnemy.position - player.position).normalized;
            float angleToEnemy = Mathf.Atan2(directionToEnemy.x, directionToEnemy.z) * Mathf.Rad2Deg;
            float playerYRotation = player.eulerAngles.y;
            float adjustedAngle = angleToEnemy - playerYRotation;
            needle.rotation = Quaternion.Euler(0, 0, -adjustedAngle);
        }
        else
        {
            needle.Rotate(0, 0, spinSpeed * Time.deltaTime);
        }
    }

    Transform GetClosestEnemy(GameObject[] enemies)
    {
        if (enemies.Length == 0)
            return null;

        return enemies
            .Select(e => e.transform)
            .OrderBy(t => Vector3.SqrMagnitude(t.position - player.position))
            .FirstOrDefault();
    }
}
