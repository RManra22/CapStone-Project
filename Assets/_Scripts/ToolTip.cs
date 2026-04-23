/*
    This script is apart of allowing the player 
    to naviate the UI and understand what each button does.
*/
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipObject;
    public GameObject tooltipObject2;

    

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipObject.SetActive(true);
        tooltipObject2.SetActive(true);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipObject.SetActive(false);
        tooltipObject2.SetActive(false);
    }

    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
        tooltipObject2.SetActive(false);
    }
}