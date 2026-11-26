using UnityEngine;

public class NoteMover : MonoBehaviour
{
    public Conductor conductor;
    public Transform origin;
    public int lane;
    public float laneSpacing = 2f;
    public float targetBeat = 4f;
    public float beatsShownInAdvance = 4f;
    public float spawnZ = 8f;
    public float hitZ = -3f;
    [HideInInspector] public bool judged;

    public Color[] laneColors =
    {
        new Color(0.42f, 0.59f, 0.72f),   // D: biru
        new Color(0.95f, 0.65f, 0.35f),   // F: oranye hangat
        new Color(0.95f, 0.87f, 0.72f)    // J: krem
    };

    void Start()
    {
        if (lane < 0 || lane >= laneColors.Length) return;
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.material.color = laneColors[lane];
    }

    void Update()
    {
        if (conductor == null) return;
        float t = (targetBeat - conductor.SongBeat) / beatsShownInAdvance;
        float z = Mathf.LerpUnclamped(hitZ, spawnZ, t);
        float x = (lane - 1) * laneSpacing;
        Vector3 o = origin != null ? origin.position : Vector3.zero;
        transform.position = o + new Vector3(x, 0f, z);
    }
}