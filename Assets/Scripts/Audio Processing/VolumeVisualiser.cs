using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeVisualiser : MonoBehaviour
{
    [SerializeField] private Image rawVolumeBar;
    [SerializeField] private Transform thresholdMark;
    [SerializeField] private Transform averageMark;
    [SerializeField] private Transform loudestMark;

    private RectTransform barRectTransform;

    void Start()
    {
        barRectTransform = rawVolumeBar.GetComponent<RectTransform>();
        rawVolumeBar.fillAmount = 0;
        UpdateMarker(thresholdMark, 0);
        UpdateMarker(averageMark, 0);
        UpdateMarker(loudestMark, 0);
    }

    void Update()
    {
        rawVolumeBar.fillAmount = Mathf.Lerp(0f, 1f,
                AudioInput.Instance.GetRawVolumeOfMic() / AudioInput.Instance.loudestRawVolume);
        
        UpdateMarker(thresholdMark, AudioInput.Instance.GetThreshold());
        UpdateMarker(averageMark, AudioInput.Instance.GetRecentAverageRawVolume());
        UpdateMarker(loudestMark, AudioInput.Instance.GetRecentAverageLoudestVolume());
    }

    private void UpdateMarker(Transform markerTransform, float value)
    {
        Vector3 newPos = markerTransform.position;
        newPos.x = Mathf.Lerp(0, barRectTransform.rect.width,
                value / AudioInput.Instance.loudestRawVolume);
        markerTransform.position = newPos;
    }
}
