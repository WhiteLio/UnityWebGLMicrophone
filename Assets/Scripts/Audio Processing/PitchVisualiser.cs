using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PitchVisualiser : MonoBehaviour
{
    [SerializeField] private Image rawPitchBar;
    [SerializeField] private Transform minMark;
    [SerializeField] private Transform maxMark;
    
    private RectTransform barRectTransform;

    void Start()
    {
        barRectTransform = rawPitchBar.GetComponent<RectTransform>();
        rawPitchBar.fillAmount = 0;
        UpdateMarker(minMark, FastYinSystem.Instance.GetRecentAverageLowestPitch());
        UpdateMarker(maxMark, FastYinSystem.Instance.GetRecentAverageHighestPitch());
    }

    void Update()
    {
        float midiFloat = FastYinSystem.Instance.midiNote;

        rawPitchBar.fillAmount = FastYinSystem.Instance.GetRemappedPitch();
        UpdateMarker(minMark, FastYinSystem.Instance.GetRecentAverageLowestPitch());
        UpdateMarker(maxMark, FastYinSystem.Instance.GetRecentAverageHighestPitch());
    }

    private void UpdateMarker(Transform markerTransform, float value)
    {
        Vector3 newPos = markerTransform.position;
        newPos.y = value.Remap(PitchDsp.kMinMidiNote, PitchDsp.kMaxMidiNote,
                0f, barRectTransform.rect.height);
        markerTransform.position = newPos;
    }

    public float GetRemappedPitch(float midiNote)
    {
        return midiNote.Remap(PitchDsp.kMinMidiNote, PitchDsp.kMaxMidiNote, 0f, 1f);
    }
}
