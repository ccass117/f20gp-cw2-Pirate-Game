using UnityEngine;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private RectTransform titleText;
    [SerializeField] private RectTransform startBtn;
    [SerializeField] private RectTransform quitBtn;
    [SerializeField] private float moveDuration = 1.2f;
    [SerializeField] private float buttonDelay = 0.3f;
    [SerializeField] private LevelLoader levelLoader;

    private void Start()
    {
        AnimUIIn();
    }
    void AnimUIIn()
    {
        titleText.anchoredPosition = new Vector2(-Screen.width, titleText.anchoredPosition.y);
        titleText.DOAnchorPosX(0, moveDuration).SetEase(Ease.OutBack);

        startBtn.anchoredPosition = new Vector2(Screen.width, startBtn.anchoredPosition.y);
        startBtn.DOAnchorPosX(0, moveDuration).SetEase(Ease.OutBack).SetDelay(buttonDelay);

        quitBtn.anchoredPosition = new Vector2(Screen.width, quitBtn.anchoredPosition.y);
        quitBtn.DOAnchorPosX(0, moveDuration).SetEase(Ease.OutBack).SetDelay(buttonDelay);
    }
    void AnimUIOut()
    {
        titleText.DOAnchorPosX(-Screen.width, moveDuration).SetEase(Ease.InBack);
        startBtn.DOAnchorPosX(Screen.width, moveDuration).SetEase(Ease.InBack).SetDelay(buttonDelay);
        quitBtn.DOAnchorPosX(Screen.width, moveDuration).SetEase(Ease.InBack).SetDelay(buttonDelay);
    }

    public void Play()
    {
        AnimUIOut();
        DOVirtual.DelayedCall(moveDuration, () =>
        {
            levelLoader.LoadLevel("level_1");
        });
    }
    public void Quit()
    {
        Application.Quit();
    }
}
