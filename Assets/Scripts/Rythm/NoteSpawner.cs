using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public Conductor conductor;
    public HitJudge judge;
    public NoteMover notePrefab;
    public PatternData pattern;

    void Start()
    {
        conductor.bpm = pattern.bpm;
        var list = new List<NoteMover>();

        foreach (var e in pattern.notes)
        {
            NoteMover n = Instantiate(notePrefab);
            n.conductor = conductor;
            n.lane = e.lane;
            n.targetBeat = e.beat;
            list.Add(n);
        }

        judge.SetNotes(list.ToArray());
    }
}