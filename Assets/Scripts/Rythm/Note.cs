using UnityEngine;

public class Note : MonoBehaviour
{
    public Conductor conductor;
    public float targetBeat = 4f;
    public float beatsShownInAdvance = 4f;
    public float spawnZ = 8f;
    public float hitZ = -3f;

    void Update()
    {
        float t = (targetBeat - conductor.SongBeat) / beatsShownInAdvance;
        float z = Mathf.LerpUnclamped(hitZ, spawnZ, t);
        transform.position = new Vector3(transform.position.x, 0f, z);
    }
}