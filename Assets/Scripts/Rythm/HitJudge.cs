using UnityEngine;
using TMPro;

public class HitJudge : MonoBehaviour
{
    public Conductor conductor;
    public TMP_Text resultText;
    public ScoreManager score;

    public float perfectWindow = 0.05f;
    public float goodWindow = 0.12f;
    public float ignoreWindow = 0.30f;

    NoteMover[] notes;
    KeyCode[] keys = { KeyCode.D, KeyCode.F, KeyCode.J };
    bool finished;
    float PW => perfectWindow * (GameState.Instance != null ? GameState.Instance.PerfectMult : 1f);
    float GW => goodWindow * (GameState.Instance != null ? GameState.Instance.GoodMult : 1f);

    void Start()
    {
        if (notes == null)
            notes = FindObjectsByType<NoteMover>(FindObjectsSortMode.None);
    }

    public void SetNotes(NoteMover[] n)
    {
        notes = n;
    }

    float Diff(NoteMover n)
    {
        return (n.targetBeat - conductor.SongBeat) * conductor.SecPerBeat;
    }

    void Update()
    {
        if (notes == null || finished) return;

        for (int lane = 0; lane < keys.Length; lane++)
            if (Input.GetKeyDown(keys[lane])) TryHit(lane);

        foreach (var n in notes)
        {
            if (n == null || n.judged) continue;
            if (Diff(n) < -GW) Judge(n, "Miss");
        }
    }

    void TryHit(int lane)
    {
        NoteMover best = null;
        float bestAbs = 999f;

        foreach (var n in notes)
        {
            if (n == null || n.judged || n.lane != lane) continue;
            float a = Mathf.Abs(Diff(n));
            if (a < bestAbs) { best = n; bestAbs = a; }
        }

        if (best == null || bestAbs > ignoreWindow) return;

        if (bestAbs <= PW) Judge(best, "Perfect");
        else if (bestAbs <= GW) Judge(best, "Good");
        else Judge(best, "Miss");
    }

    void Judge(NoteMover n, string result)
    {
        n.judged = true;
        n.gameObject.SetActive(false);
        resultText.text = result;
        if (score != null) score.Register(result);
        Debug.Log(result);
        CheckFinished();
    }

    void CheckFinished()
    {
        foreach (var n in notes)
            if (n != null && !n.judged) return;

        finished = true;
        if (score != null) score.ShowSummary();
    }
}