using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public Conductor conductor;
    public HitJudge judge;
    public NoteMover notePrefab;
    public PatternData pattern;
    public Transform origin;
    public bool autoStart = true;

    List<NoteMover> spawned = new List<NoteMover>();

    void Start()
    {
        if (autoStart) Begin();
    }

    public void Begin()
    {
        foreach (var n in spawned)
            if (n != null) Destroy(n.gameObject);
        spawned.Clear();

        conductor.bpm = pattern.bpm;
        conductor.Begin();

        foreach (var e in pattern.notes)
        {
            NoteMover n = Instantiate(notePrefab);
            n.conductor = conductor;
            n.origin = origin;
            n.lane = e.lane;
            n.targetBeat = e.beat;
            n.judged = false;
            n.gameObject.SetActive(true);
            spawned.Add(n);
        }

        judge.SetNotes(spawned.ToArray());
    }
}