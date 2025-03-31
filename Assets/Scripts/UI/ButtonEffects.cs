using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

// scale buttons when mouse is over them
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(1.2f, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1, 0.2f);
    }
}
