using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlotPoint : MonoBehaviour
{
    RectTransform rT;

    void Awake() 
    {
        rT = GetComponent<RectTransform>();
    }

    public Vector2 AnchoredPosition => rT.anchoredPosition;

    public void PlacePoint(Vector2 position)
    {
        rT.sizeDelta = new Vector2(11,11);
        rT.anchorMin = Vector2.zero;
        rT.anchorMax = Vector2.zero;
        rT.anchoredPosition = position;
    }
}
