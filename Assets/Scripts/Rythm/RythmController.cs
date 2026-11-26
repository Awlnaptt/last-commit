using System.Collections;
using UnityEngine;
using TMPro;

public class RhythmController : MonoBehaviour
{
    [Header("Pemain")]
    public GameObject player;

    [Header("Rhythm")]
    public GameObject rhythmStage;
    public GameObject rhythmCanvas;
    public NoteSpawner spawner;
    public Conductor conductor;
    public ScoreManager score;
    public TMP_Text orderLabel;       // opsional

    [Header("Batas hasil (akurasi %)")]
    public float perfectThreshold = 85f;
    public float goodThreshold = 50f;

    public bool IsRunning { get; private set; }

    MonoBehaviour[] playerScripts;
    bool finishing;

    void Start()
    {
        playerScripts = player.GetComponents<MonoBehaviour>();
        SetRhythmVisible(false);
    }

    void SetRhythmVisible(bool on)
    {
        rhythmStage.SetActive(on);
        rhythmCanvas.SetActive(on);
    }

    public void StartRhythm()
    {
        if (IsRunning) return;

        Pesanan p = OrderManager.Instance.MulaiRacik();
        if (p == null) return;

        var pola = p.menu.polaRitme as PatternData;
        if (pola != null) spawner.pattern = pola;

        IsRunning = true;
        finishing = false;

        foreach (var s in playerScripts) s.enabled = false;

        SetRhythmVisible(true);
        if (orderLabel != null) orderLabel.text = "Meracik: " + p.menu.namaTampil;


        Debug.Log("Rhythm mulai. Pola: " + spawner.pattern.name +
          ", jumlah note: " + spawner.pattern.notes.Length +
          ", bpm: " + spawner.pattern.bpm);

        score.ResetAll();
        spawner.Begin();
    }

    void Update()
    {
        if (IsRunning && !finishing && score.coffeeReady)
            StartCoroutine(Finish());
    }

    IEnumerator Finish()
    {
        finishing = true;
        yield return new WaitForSeconds(3f);

        conductor.Stop();
        SetRhythmVisible(false);
        foreach (var s in playerScripts) s.enabled = true;

        HasilRacik hasil = HasilRacik.Miss;
        if (score.lastAccuracy >= perfectThreshold) hasil = HasilRacik.Perfect;
        else if (score.lastAccuracy >= goodThreshold) hasil = HasilRacik.Good;

        score.coffeeReady = false;
        OrderManager.Instance.SelesaiRacik(hasil);
        IsRunning = false;
    }
}