using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastYinSystem : Singleton<FastYinSystem>
{
    public AudioInput audioInput;
    public AudioSource audioSource;
    public float pitch;
    public int midiNote = 60;
    public int midiCents;
    public string midiNoteText;
    public int currentMinMidiNote = 55; // A3
    public int currentMaxMidiNote = 63; // F4
    
    // num of seconds of memory
    [SerializeField] private float window = 10;

    private FastYin fastYin;
    private Queue<float> recentLowestPitches;
    private Queue<float> recentHighestPitches;

    void Start()
    {
        fastYin = new FastYin(44100, 1024);
        Reset();
    }
    
    private void Reset()
    {
        currentMinMidiNote = 55; // A3
        currentMaxMidiNote = 63; // F4
        recentLowestPitches = new Queue<float>();
        recentLowestPitches.Enqueue(55);
        recentHighestPitches = new Queue<float>();
        recentHighestPitches.Enqueue(63);
    }

    void Update()
    {
        if (audioSource == null) return;

        if (audioSource.clip == null) return;

        if (audioInput.GetParsedVolume() <= 0) return;

        var buffer = new float[1024];
        audioSource.GetOutputData(buffer, 0);

        var result = fastYin.getPitch(buffer);
        
        var pitch = result.getPitch();
        var midiNote = 0;
        var midiCents = 0;

        PitchDsp.PitchToMidiNote(pitch, out midiNote, out midiCents);

        if (midiNote < PitchDsp.kMinMidiNote || midiNote > PitchDsp.kMaxMidiNote) return;

        this.pitch = pitch;
        this.midiNote = midiNote;
        this.midiCents = midiCents;
        this.midiNoteText = PitchDsp.GetNoteName(midiNote, true, true);

        
        if (midiNote < currentMinMidiNote)
        {
            currentMinMidiNote = midiNote;
        }

        if (midiNote > currentMaxMidiNote)
        {
            currentMaxMidiNote = midiNote;
        }
        
        if (midiNote < GetRecentAverageLowestPitch())
        {
            AddToMemory(recentLowestPitches, midiNote);
        }
        
        if (midiNote > GetRecentAverageHighestPitch())
        {
            AddToMemory(recentHighestPitches, midiNote);
        }
    }

    // Reutrns a float from 0 to 1 from the absolute min to max
    public float GetRemappedPitch()
    {
        return ((float)midiNote).Remap(PitchDsp.kMinMidiNote, PitchDsp.kMaxMidiNote, 0f, 1f);
    }
    
    // Reutrns a float from 0 to 1 from the current min to max
    public float GetParsedPitch()
    {
        return ((float)midiNote).Remap(GetRecentAverageLowestPitch(), GetRecentAverageHighestPitch(), 0f, 1f);
    }

    public string GetRange()
    {
        return PitchDsp.GetNoteName((int)GetRecentAverageLowestPitch(), true, true) + " ~ " + PitchDsp.GetNoteName((int)GetRecentAverageHighestPitch(), true, true);
    }
    
    public float GetRecentAverageLowestPitch()
    { 
        return recentLowestPitches.Average();
    }
    
    public float GetRecentAverageHighestPitch()
    { 
        return recentHighestPitches.Average();
    }
    
    private void AddToMemory(Queue<float> memory, float newValue)
    {
        memory.Enqueue(newValue);
        float amountToDequeue = memory.Count() - GetFps() * window;
        for (int i = 0; i < amountToDequeue; i++)
        {
            memory.Dequeue();
        }
    }
    
    public int GetFps()
    {
        int fps = (int)(1f / Time.unscaledDeltaTime);
        fps = fps <= 0 ? 1 : fps;
        return fps;
    }
}
