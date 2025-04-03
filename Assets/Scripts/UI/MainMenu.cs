using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private LevelLoader levelLoader;
    //[SerializeField] private Credits credits;

    [Header("Main")]
    [SerializeField] private RectTransform titleText;

    [SerializeField] private RectTransform startBtn; // UI element
    [SerializeField] private Button startButton; // control button interactability

    [SerializeField] private RectTransform creditsBtn;
    [SerializeField] private Button creditsButton;

    [SerializeField] private RectTransform quitBtn;
    [SerializeField] private Button quitButton;

    [Header("Misc")]
    [SerializeField] private AudioSource audioSource; // cannonball firing sound for when start is clicked
    [SerializeField] private Animator anim;

    private RectTransform canvasSize;
    private float canvasWidth;

    private void Start()
    {
        canvasSize = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        canvasWidth = canvasSize.rect.width;

        // set these to non-interactable so player can't click them while animation is playing
        startButton.interactable = false;
        quitButton.interactable = false;

        anim.SetTrigger("MainMenu");
        DOVirtual.DelayedCall(2f, AnimUIMainIn);
    }

    // DOTween transition that brings in the title and buttons
    public void AnimUIMainIn()
    {
        UIAnimator.AnimateUIIn(titleText, new RectTransform[] { startBtn, creditsBtn, quitBtn }, canvasWidth);
    }

    public void Play()
    {
        audioSource.Play();
        UIAnimator.AnimateUIOut(titleText, new RectTransform[] { startBtn, creditsBtn, quitBtn }, canvasWidth);

        // give time for transition to finish before loading next scene
        DOVirtual.DelayedCall(1.8f + 0.5f, () =>
        {
            DOTween.KillAll();
            levelLoader.LoadLevel("GoldShop");
        });
    }
    
    // transition to credits
    public void Credits()
    {
        UIAnimator.AnimateUIOut(titleText, new RectTransform[] { startBtn, creditsBtn, quitBtn }, canvasWidth);
        DOVirtual.DelayedCall(1.5f, () =>
        {
            anim.SetTrigger("ToCredits");
            //credits.RollCredits();
        });
    }

    // closes the game (build version only)
    public void Quit()
    {
        Application.Quit();
    }
}
