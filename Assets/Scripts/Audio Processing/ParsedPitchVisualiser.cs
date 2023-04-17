using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParsedPitchVisualiser : MonoBehaviour
{
    [SerializeField] private RectTransform indicatorParent;
    [SerializeField] private RectTransform indicator;

    void Update()
    {
        Vector3 newPos = indicator.anchoredPosition;
        newPos.y = Mathf.Lerp(0, indicatorParent.rect.height,
                FastYinSystem.Instance.GetParsedPitch());
        indicator.anchoredPosition = newPos;
    }
}
