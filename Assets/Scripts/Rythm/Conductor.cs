using UnityEngine;

public class Conductor : MonoBehaviour
{
    public AudioSource music;
    public float bpm = 100f;
    public float firstBeatOffset = 0f;

    double startDsp;
    bool running;

    public float SecPerBeat => 60f / bpm;

    public float SongBeat =>
        running ? (float)((AudioSettings.dspTime - startDsp - firstBeatOffset) / SecPerBeat) : 0f;

    public void Begin()
    {
        startDsp = AudioSettings.dspTime + 0.2;
        if (music != null && music.clip != null)
        {
            music.Stop();
            music.PlayScheduled(startDsp);
        }
        running = true;
    }

    public void Stop()
    {
        running = false;
        if (music != null) music.Stop();
    }
}