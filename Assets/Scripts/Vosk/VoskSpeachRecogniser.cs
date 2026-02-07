using UnityEngine;
using Vosk;
using System.Text;
using System.Collections;
using TMPro;

public class VoskSpeechRecognizer : MonoBehaviour
{
    [Header("Speech Settings")]
    public bool active = false;
    public float duration = 5f;
    public string keyword = "open";
    public bool victory = false;

    private Model model;
    private VoskRecognizer recognizer;
    private AudioClip micClip;
    private string micDevice;
    private int sampleRate = 16000;
    private float[] samples = new float[4096];
    private bool isListening = false;
    public TMP_Text MonitorText;
    void Start()
    {
        string modelPath = System.IO.Path.Combine(Application.streamingAssetsPath, "vosk-model-small-en-us-0.15");
        model = new Model(modelPath);
        recognizer = new VoskRecognizer(model, sampleRate);

        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            micClip = Microphone.Start(micDevice, true, 1, sampleRate);
            Debug.Log("Microphone started: " + micDevice);
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
    }

    void Update()
    {
        if (!active || isListening == false || micClip == null) return;

        int micPos = Microphone.GetPosition(micDevice);
        micClip.GetData(samples, 0);

        short[] intData = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * short.MaxValue);
        }

        byte[] byteData = new byte[intData.Length * 2];
        System.Buffer.BlockCopy(intData, 0, byteData, 0, byteData.Length);

        if (recognizer.AcceptWaveform(byteData, byteData.Length))
        {
            string text = recognizer.Result();
            if (!string.IsNullOrEmpty(text))
            {
                Debug.Log("Recognized: " + text);
                MonitorText.text = text;
                CheckKeyword(text);
            }
        }
        else
        {
            string partial = recognizer.PartialResult();
            if (!string.IsNullOrEmpty(partial))
            {
                Debug.Log("Partial: " + partial);
                MonitorText.text = partial;
                CheckKeyword(partial);
            }
        }
    }

    void CheckKeyword(string text)
    {
        if (text.ToLower().Contains(keyword.ToLower()))
        {
            victory = true;
            Debug.Log("Keyword detected! Victory set to true.");
        }
    }

    public void StartListening()
    {
        if (!isListening)
            StartCoroutine(ListenForDuration());
    }

    private IEnumerator ListenForDuration()
    {
        isListening = true;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isListening = false;
        active = false;
        Debug.Log("Finished listening.");
    }

    void OnDestroy()
    {
        recognizer?.Dispose();
        model?.Dispose();
    }
    public void activate()
    {
        active = true;
    }
}
