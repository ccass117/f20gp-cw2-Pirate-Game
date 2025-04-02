using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private RectTransform titleText;
    [SerializeField] private RectTransform quitBtn; // UI element
    [SerializeField] private Button quitButton; // control button interactability

    private RectTransform canvasSize;
    private float canvasWidth;

    private void Start()
    {
        canvasSize = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        canvasWidth = canvasSize.rect.width;

        // set to false until the moving animation is complete
        quitButton.interactable = false;
        AnimUIIn();
    }

    void AnimUIIn()
    {
        UIAnimator.AnimateUIIn(titleText, new RectTransform[] { quitBtn }, canvasWidth);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
