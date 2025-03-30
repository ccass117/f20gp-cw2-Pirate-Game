using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIAnimator : MonoBehaviour
{
    private const float moveTime = 1.8f; // duration of animation

    // brings UI elements into view
    public static void AnimateUI(RectTransform titleText, RectTransform[] buttons, float width)
    {
        titleText.anchoredPosition = new Vector2(-width, titleText.anchoredPosition.y);
        titleText.DOAnchorPosX(0, moveTime)
            .SetEase(Ease.OutBack);

        // to handle any number of buttons
        foreach (RectTransform button in buttons)
        {
            button.anchoredPosition = new Vector2(width, button.anchoredPosition.y);
            button.DOAnchorPosX(0, moveTime)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    Button btn = button.GetComponent<Button>();
                    btn.interactable = true;
                });
        }
    }

    // takes UI elements out of view
    public static void AnimateUIOut(RectTransform title, RectTransform[] buttons, float width)
    {
        title.DOAnchorPosX(-width, moveTime)
            .SetEase(Ease.InBack);

        foreach (RectTransform button in buttons)
        {
            button.DOAnchorPosX(width, moveTime)
                .SetEase(Ease.InBack);
        }
    }
}
