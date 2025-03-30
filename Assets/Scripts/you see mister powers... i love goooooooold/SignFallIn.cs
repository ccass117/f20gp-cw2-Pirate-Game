using UnityEngine;
using System.Collections;

public class SignFallIn : MonoBehaviour
{
    public RectTransform sign;
    public float goalY;

    void Start()
    {
        sign = GetComponent<RectTransform>();
        goalY = transform.position.y-540; //get position relative to UI
        StartCoroutine(MoveSign());
    }

    IEnumerator MoveSign()
    {
        // Move down to y = 30 over 0.5 seconds
        yield return MoveOverTime(sign, goalY + 970f, goalY - 100f, 0.5f);

        // Move up to y = 130 over 0.25 seconds
        yield return MoveOverTime(sign, goalY - 100f, goalY + 20f, 0.25f);

        yield return MoveOverTime(sign, goalY + 20f, goalY, 0.2f);
    }

    IEnumerator MoveOverTime(RectTransform rect, float startY, float endY, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float newY = Mathf.Lerp(startY, endY, time / duration);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, newY);
            yield return null;
        }
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, endY);
    }
}