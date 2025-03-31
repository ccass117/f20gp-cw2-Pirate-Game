using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIAnimator : MonoBehaviour
{
    // brings UI elements into view
    public static void AnimateUI(RectTransform titleText, RectTransform[] buttons, float width)
    {
        titleText.anchoredPosition = new Vector2(-width, titleText.anchoredPosition.y);
        titleText.DOAnchorPosX(0, 2.2f)
            .SetEase(Ease.OutBack);

        // to handle any number of buttons
        foreach (RectTransform button in buttons)
        {
            button.anchoredPosition = new Vector2(width, button.anchoredPosition.y);
            button.DOAnchorPosX(0, 2.2f)
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
        title.DOAnchorPosX(-width, 1.8f)
            .SetEase(Ease.InBack);

        foreach (RectTransform button in buttons)
        {
            button.DOAnchorPosX(width, 1.8f)
                .SetEase(Ease.InBack);
        }
    }
}
