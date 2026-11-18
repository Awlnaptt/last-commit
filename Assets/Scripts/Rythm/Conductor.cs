using UnityEngine;

public class Conductor : MonoBehaviour
{
    public AudioSource music;          // boleh dikosongkan dulu
    public float bpm = 100f;
    public float firstBeatOffset = 0f; // detik sampai beat pertama

    double startDsp;
    bool running;

    public float SecPerBeat => 60f / bpm;

    public float SongBeat =>
        running ? (float)((AudioSettings.dspTime - startDsp - firstBeatOffset) / SecPerBeat) : 0f;

    void Start()
    {
        startDsp = AudioSettings.dspTime + 0.2;
        if (music != null) music.PlayScheduled(startDsp);
        running = true;
    }
}