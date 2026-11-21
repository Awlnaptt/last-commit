using UnityEngine;

public class NoteMover : MonoBehaviour
{
    public Conductor conductor;
    public int lane;                 // 0 = D, 1 = F, 2 = J
    public float laneSpacing = 2f;
    public float targetBeat = 4f;
    public float beatsShownInAdvance = 4f;
    public float spawnZ = 8f;
    public float hitZ = -3f;
    [HideInInspector] public bool judged;

    void Update()
    {
        if (conductor == null) return;
        float t = (targetBeat - conductor.SongBeat) / beatsShownInAdvance;
        float z = Mathf.LerpUnclamped(hitZ, spawnZ, t);
        float x = (lane - 1) * laneSpacing;
        transform.position = new Vector3(x, 0f, z);
    }
}