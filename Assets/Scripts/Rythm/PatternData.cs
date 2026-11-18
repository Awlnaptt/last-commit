using UnityEngine;

[System.Serializable]
public struct NoteEntry
{
    public float beat;
    public int lane;   // 0 = D, 1 = F, 2 = J
}

[CreateAssetMenu(menuName = "Last Commit/Pola Note")]
public class PatternData : ScriptableObject
{
    public float bpm = 100f;
    public NoteEntry[] notes;
}