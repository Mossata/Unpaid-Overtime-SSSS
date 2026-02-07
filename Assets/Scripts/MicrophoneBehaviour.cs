using UnityEngine;
using UnityEngine.Android;
using System.Collections;
using UnityEngine.SceneManagement;
using Meta.XR.MRUtilityKit.SceneDecorator;

public class MicrophoneBehaviour : MonoBehaviour
{
    AudioClip micClip;
    string micDevice;
    bool micStarted = false;
    int sampleRate = 44100;
    public int normalized = 0;
    public int lossThreshould = 50;
    public MonsterBehaviour monsterBehaviour;
    public AudioSource audioSource;


    public bool debug;
    public bool canDie;
    void Start()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone devices found!");
            return;
        }

        micDevice = Microphone.devices[0];
        if (debug) Debug.Log("Mic found for volume detection: " + micDevice);

        sampleRate = AudioSettings.outputSampleRate;
        micClip = Microphone.Start(micDevice, true, 1, sampleRate);
        StartCoroutine(WaitForMicStart());
        
    }

    IEnumerator WaitForMicStart()
    {
        while (Microphone.GetPosition(micDevice) <= 0)
        {
            yield return null;
        }

        micStarted = true;
        if(debug) Debug.Log("Microphone recording started!");
    }

    void Update()
    {
        if (!micStarted || micClip == null) return;

            float rawLoudness = GetLoudness();
            normalized = NormalizeLoudnessTo100(rawLoudness);
            if (debug) Debug.Log("loudness: " + normalized);

        if (normalized > lossThreshould && canDie)
        {
            Debug.Log("Lost game because player got too loud");
            DeathIntegerAccess.deathInt = 2;
            monsterBehaviour.timer = 99.0f;
            monsterBehaviour.cooldownTimer = 99.0f;
            audioSource.Play();
            //PLAY SOUND EFFECT HERE

            // JumpScare js = FindFirstObjectByType<JumpScare>();
            // js.activate();
        }
    }

    float GetLoudness()
    {
        int sampleWindow = 128;
        float[] data = new float[sampleWindow];
        int micPosition = Microphone.GetPosition(micDevice);

        if (micPosition < sampleWindow)
        {
            return 0f;
        }

        int startPosition = micPosition - sampleWindow;
        if (startPosition < 0)
        {
            startPosition += micClip.samples;
        }

        micClip.GetData(data, startPosition);

        float totalLoudness = 0f;
        foreach (float sample in data)
        {
            totalLoudness += Mathf.Abs(sample);
        }

        return totalLoudness / sampleWindow;
    }

    int NormalizeLoudnessTo100(float loudness)
    {
        float amplified = loudness * 1000f;
        float safeLoudness = Mathf.Max(amplified, 1f);
        float scaled = Mathf.Log10(safeLoudness) * 20f;
        return Mathf.Clamp(Mathf.CeilToInt(scaled), 1, 100);
    }
}

