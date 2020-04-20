using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class OnHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI targetText;

    [SerializeField] private string text;

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetText.text = text;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetText.text = "";
    }
}
