using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParsedVolumeVisualiser : MonoBehaviour
{
    [SerializeField] private RectTransform indicatorParent;
    [SerializeField] private RectTransform indicator;
    [SerializeField] private Image[] indicatorFills;
    [SerializeField] private GameObject shoutingIndicator;

    void Update()
    {
        if (indicator != null)
        {
            Vector3 newPos = indicator.anchoredPosition;
            newPos.x = Mathf.Lerp(0, indicatorParent.rect.width,
                    AudioInput.Instance.GetParsedVolume());
            indicator.anchoredPosition = newPos;
        }

        float fillAmount = AudioInput.Instance.GetParsedShoutingVolume();
        foreach (Image indicatorFill in indicatorFills)
        {
            indicatorFill.fillAmount = fillAmount;
        }

        if (shoutingIndicator != null)
        {
            shoutingIndicator.SetActive(AudioInput.Instance.IsShouting());
        }
    }
}
