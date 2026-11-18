using UnityEngine;

public class BeatTick : MonoBehaviour
{
    public AudioSource source;      // Audio Source di objek Tick
    public bool tickOn = true;
    [Range(0f, 1f)] public float volume = 0.6f;

    KeyCode[] keys = { KeyCode.D, KeyCode.F, KeyCode.J };
    float[] freqs = { 800f, 1000f, 1250f };   // nada beda per lane
    AudioClip[] clips;

    void Start()
    {
        clips = new AudioClip[keys.Length];
        for (int i = 0; i < keys.Length; i++)
            clips[i] = MakeClick(freqs[i]);
    }

    AudioClip MakeClick(float freq)
    {
        int rate = AudioSettings.outputSampleRate;
        int len = rate / 50;   // 20 ms
        float[] data = new float[len];
        for (int i = 0; i < len; i++)
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / rate) * (1f - (float)i / len) * 0.5f;

        AudioClip c = AudioClip.Create("click", len, 1, rate, false);
        c.SetData(data, 0);
        return c;
    }

    void Update()
    {
        if (!tickOn || source == null) return;

        for (int i = 0; i < keys.Length; i++)
            if (Input.GetKeyDown(keys[i]))
                source.PlayOneShot(clips[i], volume);
    }
}