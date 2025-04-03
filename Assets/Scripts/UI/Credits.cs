using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    [SerializeField] private Animator credAnim;
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private Animator camAnim;
    [SerializeField] private Button backButton;

    private bool rollingCredits;

    private void Awake()
    {
        // disable to prevent it playing automatically
        credAnim.enabled = false;
    }
    public void RollCredits()
    {
        credAnim.enabled = true;

        // delay to give time for camera to move
        DOVirtual.DelayedCall(2.5f, () =>
        {
            credAnim.SetTrigger("RollCredits");
        });
    }

    // button to return to the main menu
    public void Back()
    {
        camAnim.SetTrigger("BackCredits");
        DOVirtual.DelayedCall(2.2f + 0.5f, () =>
        {
            credAnim.enabled = false;
            mainMenu.AnimUIMainIn();
        });
    }
}
