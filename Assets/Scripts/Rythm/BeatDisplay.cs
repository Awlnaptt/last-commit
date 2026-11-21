using UnityEngine;
using TMPro;

public class BeatDisplay : MonoBehaviour
{
    public Conductor conductor;
    public TMP_Text text;

    void Update()
    {
        text.text = "Beat: " + conductor.SongBeat.ToString("F2");
    }
}