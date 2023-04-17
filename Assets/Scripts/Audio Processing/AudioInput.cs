using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioInput : Singleton<AudioInput>
{
    // input audio is at least this many times louder than bg noise aka threshold
    [Range(1f, 3f)]
    [SerializeField] private float backgroundRatio = 2f;

    // multiplier on output audio
    [SerializeField] private float sensitivity = 20;

    // num of seconds of memory
    [SerializeField] private float window = 10;

    // 
    [SerializeField] private AudioSource audioSource;
    
    // input needs to be at least this out of max to be considered shouting
    [Range(0f, 1f)]
    [SerializeField] private float shoutingThreshold = 0.7f;

    public delegate void OnShoutDelegate();
    public event OnShoutDelegate OnShoutEvent;

    public float currRawVolume;
    public float loudestRawVolume;

    private string micName;
    private AudioClip micClip;
    private Queue<float> recentVolumes;
    private Queue<float> recentBackgroundVolumes;
    private Queue<float> recentLoudestVolumes;
    private bool wasShouting = true;

    private const int SAMPLE_WINDOW = 1024;
    private const int CLIP_LENGTH = 5;
    private const int FREQUENCY = 44100;
    private const float EPSILON = 0.0001f;
    private const float TEST_WINDOW = 0.2f;
    private const float MIC_BUFFER = 0.05f;

    void Start()
    {
        Reset();
        SetupMicrophone();
    }

    private void Reset()
    {
        loudestRawVolume = 0.01f;
        recentVolumes = new Queue<float>();
        recentVolumes.Enqueue(0);
        recentBackgroundVolumes = new Queue<float>();
        recentBackgroundVolumes.Enqueue(1f);
        recentLoudestVolumes = new Queue<float>();
        recentLoudestVolumes.Enqueue(0f);
    }

    private void SetupMicrophone()
    {
        StartCoroutine(SetupMicrophoneCo());
    }

    private IEnumerator SetupMicrophoneCo()
    {
        print(Microphone.devices.Length + " availiable Microphones:\n" + Microphone.devices.ToString<string>());
        
        string bestMicName = "";
        float softestMicVolume = 9999;

        foreach (string device in Microphone.devices)
        {
            micName = device;
            micClip = Microphone.Start(micName, true, CLIP_LENGTH, FREQUENCY);
            audioSource.clip = micClip;
            audioSource.Play();
            
            yield return new WaitForSecondsRealtime(TEST_WINDOW);

            float bgVolume = GetRecentAverageBackgroundVolume();
            if (bgVolume > EPSILON && currRawVolume > EPSILON)
            {
                print("Sucessful " + bgVolume + " on " + device);

                if (bgVolume < softestMicVolume)
                {
                    softestMicVolume = bgVolume;
                    bestMicName = micName;
                }
            }
            else
            {
                print("Rejected " + device);
            }

            Microphone.End(micName);
            audioSource.Stop();
            Reset();
        }

        if (bestMicName == "")
        {
            Debug.LogError("No suitable Microphone found.");
        }
        else 
        {
            ForceSetupMicrophone(bestMicName);
        }
    }

    private void ForceSetupMicrophone(string micName)
    {
        print("Force Setup " + micName);
        this.micName = micName;
        micClip = Microphone.Start(micName, true, CLIP_LENGTH, FREQUENCY);
        audioSource.clip = micClip;
        StartCoroutine(ForceSetupMicrophoneCo());
    }

    private IEnumerator ForceSetupMicrophoneCo()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        while(Microphone.GetPosition(micName) <= 0) yield return null;

        float delay = (float)Microphone.GetPosition(micName)/FREQUENCY;
        print(delay + "s input delay detected");
        
        audioSource.Play();
        if (delay > MIC_BUFFER) audioSource.time = delay - MIC_BUFFER;
    }

    void Update()
    {
        // Pause the microphone if you lose focus
        if (!Application.isPlaying)
        {
			StopMicrophone();
		}
        else
        {
			if (!Microphone.IsRecording(micName))
            {
				ForceSetupMicrophone(micName);
			}
		}

        currRawVolume = GetRawVolumeOfMic();

        if (currRawVolume > loudestRawVolume)
        {
            loudestRawVolume = currRawVolume;
        }
        
        AddToMemory(recentVolumes, currRawVolume);

        if (currRawVolume < GetThreshold())
        {
            AddToMemory(recentBackgroundVolumes, currRawVolume);
        }
        
        if (currRawVolume > GetRecentAverageLoudestVolume())
        {
            AddToMemory(recentLoudestVolumes, currRawVolume);
        }

        bool isShouting = IsShouting();
        if (!wasShouting && isShouting)
        {
            if (OnShoutEvent != null) OnShoutEvent.Invoke();
        }
        wasShouting = isShouting;
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

    // Reutrns a float from 0 to 1 from the current min to max
    public float GetParsedVolume()
    {
        return currRawVolume.Remap(GetThreshold(), GetRecentAverageLoudestVolume(), 0f, 1f);
    }
    
    // Reutrns a float from 0 to 1 from the current min to shouting threshold
    public float GetParsedShoutingVolume()
    {
        return currRawVolume.Remap(GetThreshold(), GetRecentAverageLoudestVolume() * shoutingThreshold, 0f, 1f);
    }
    
    public bool IsShouting()
    {
        return GetParsedVolume() > shoutingThreshold;
    }

    public float GetThreshold()
    {
        return backgroundRatio * GetRecentAverageBackgroundVolume();
    }

    public int GetFps()
    {
        int fps = (int)(1f / Time.unscaledDeltaTime);
        fps = fps <= 0 ? 1 : fps;
        return fps;
    }

    public float GetRecentAverageRawVolume()
    { 
        return recentVolumes.Average();
    }
    
    public float GetRecentAverageBackgroundVolume()
    { 
        return recentBackgroundVolumes.Average();
    }
    
    public float GetRecentAverageLoudestVolume()
    { 
        return recentLoudestVolumes.Average();
    }

    public float GetRawVolumeOfMic()
    {
        return GetVolumeOfAudioClip(micClip, Microphone.GetPosition(micName));
    }

    public float GetVolumeOfMic()
    {
        float volume = GetRawVolumeOfMic();
        volume *= sensitivity;
        volume = volume < GetThreshold() ? 0 : volume;
        
        return volume;
    }

    private float GetVolumeOfAudioClip(AudioClip audioClip, int clipPosition)
    {
        int startPosition = clipPosition - SAMPLE_WINDOW;
        startPosition = startPosition < 0 ? 0 : startPosition;

        float[] data = new float[SAMPLE_WINDOW];
        audioClip.GetData(data, startPosition);

        float volumeSum = 0;
        foreach (float s in data)
        {
            volumeSum += Mathf.Abs(s);
        }

        return volumeSum / SAMPLE_WINDOW;
    }

    void OnApplicationFocus(bool focused)
	{
		if (!focused) StopMicrophone();
	}
	
	void OnApplicationPause(bool focused)
	{
		if (!focused) StopMicrophone();
	}
    
    void OnApplicationQuit()
    {
        // Stop recording when the application is quitting
        StopMicrophone();
    }

    private void StopMicrophone()
    {
        if (micName == null) return;

        print("Stopped " + micName);
        audioSource.Stop();
        Microphone.End(micName);
    }
}