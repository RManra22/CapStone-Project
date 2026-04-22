using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TooltipV2 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipObject;
    public GameObject tooltipObject2;
    public GameObject tooltipObject3;
    public GameObject tooltipObject4;
    public GameObject tooltipObject5;
    

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipObject.SetActive(true);
        tooltipObject2.SetActive(true);
        tooltipObject3.SetActive(true);
        tooltipObject4.SetActive(true);
        tooltipObject5.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipObject.SetActive(false);
        tooltipObject2.SetActive(false);
        tooltipObject3.SetActive(false);
        tooltipObject4.SetActive(false);
        tooltipObject5.SetActive(false);
    }
}