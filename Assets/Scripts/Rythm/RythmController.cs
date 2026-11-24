using System.Collections;
using UnityEngine;
using TMPro;

public class RhythmController : MonoBehaviour
{
    [Header("Pemain")]
    public GameObject player;
    public GameObject playerCamera;

    [Header("Rhythm")]
    public GameObject rhythmStage;
    public Camera rhythmCamera;
    public GameObject rhythmCanvas;
    public NoteSpawner spawner;
    public Conductor conductor;
    public ScoreManager score;
    public TMP_Text customerText;

    public bool IsRunning { get; private set; }

    MonoBehaviour[] playerScripts;
    bool finishing;

    bool Check()
    {
        bool ok = true;
        if (player == null) { Debug.LogError("RhythmController: kolom Player kosong"); ok = false; }
        if (playerCamera == null) { Debug.LogError("RhythmController: kolom Player Camera kosong"); ok = false; }
        if (rhythmStage == null) { Debug.LogError("RhythmController: kolom Rhythm Stage kosong"); ok = false; }
        if (rhythmCamera == null) { Debug.LogError("RhythmController: kolom Rhythm Camera kosong"); ok = false; }
        if (rhythmCanvas == null) { Debug.LogError("RhythmController: kolom Rhythm Canvas kosong"); ok = false; }
        if (spawner == null) { Debug.LogError("RhythmController: kolom Spawner kosong"); ok = false; }
        if (conductor == null) { Debug.LogError("RhythmController: kolom Conductor kosong"); ok = false; }
        if (score == null) { Debug.LogError("RhythmController: kolom Score kosong"); ok = false; }
        if (customerText == null) { Debug.LogError("RhythmController: kolom Customer Text kosong"); ok = false; }
        return ok;
    }

    void Start()
    {
        if (!Check()) return;
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
        if (!Check()) return;
        if (IsRunning || score.coffeeReady) return;
        IsRunning = true;
        finishing = false;

        foreach (var s in playerScripts) s.enabled = false;
        playerCamera.SetActive(false);

        SetRhythmVisible(true);
        rhythmCamera.depth = 10;
        var al = rhythmCamera.GetComponent<AudioListener>();
        if (al == null) al = rhythmCamera.gameObject.AddComponent<AudioListener>();
        al.enabled = true;

        customerText.text = "";
        score.ResetAll();
        spawner.Begin();
    }

    void Update()
    {
        if (IsRunning && !finishing && score != null && score.coffeeReady)
            StartCoroutine(Finish());
    }

    IEnumerator Finish()
    {
        finishing = true;
        yield return new WaitForSeconds(3f);

        conductor.Stop();
        SetRhythmVisible(false);
        playerCamera.SetActive(true);
        foreach (var s in playerScripts) s.enabled = true;

        IsRunning = false;
        customerText.text = "Kopi selesai! Tekan E untuk mengantar ke pelanggan.";
    }
}