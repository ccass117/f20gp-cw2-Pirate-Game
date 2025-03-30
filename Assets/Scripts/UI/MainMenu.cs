using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private RectTransform titleText;
    [SerializeField] private RectTransform startBtn; // UI element
    [SerializeField] private RectTransform quitBtn;

    [SerializeField] private AudioSource clickStartSound;
    [SerializeField] private Animator cameraAnim;

    [SerializeField] private Button startButton; // control button interactability
    [SerializeField] private Button quitButton;

    [SerializeField] private LevelLoader levelLoader;

    private RectTransform canvasSize;
    private float canvasWidth;
    private float moveTime = 1.8f;

    private void Start()
    {
        canvasSize = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        canvasWidth = canvasSize.rect.width;

        startButton.interactable = false;
        quitButton.interactable = false;

        cameraAnim.SetTrigger("MainMenu");
        DOVirtual.DelayedCall(2f, AnimUIIn);
    }

    void AnimUIIn()
    {
        UIAnimator.AnimateUI(titleText, new RectTransform[] { startBtn, quitBtn }, canvasWidth);
    }


    public void Play()
    {
        clickStartSound.Play();
        UIAnimator.AnimateUIOut(titleText, new RectTransform[] { startBtn, quitBtn }, canvasWidth);

        DOVirtual.DelayedCall(moveTime + 0.5f, () =>
        {
            DOTween.KillAll();
            levelLoader.LoadLevel("level_1");
        });
    }

    public void Quit()
    {
        Application.Quit();
    }
}
