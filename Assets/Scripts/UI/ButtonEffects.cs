using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

// scale buttons when mouse is over them
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private Vector3 original;

    void Start()
    {
        button = GetComponent<Button>();
        original = transform.localScale;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            transform.DOScale(original * 1.2f, 0.2f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            transform.DOScale(original, 0.2f);
        }
    }
}
