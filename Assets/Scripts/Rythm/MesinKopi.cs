using UnityEngine;

public class MesinKopiInteraksi : MonoBehaviour, IInteraktif
{
    public RhythmController rhythm;

    public string Petunjuk => "[E] Seduh kopi";

    public bool BisaDipakai =>
        rhythm != null && !rhythm.IsRunning &&
        OrderManager.Instance != null &&
        OrderManager.Instance.PesananAktif != null &&
        !OrderManager.Instance.SedangMeracik;

    public void Interaksi() => rhythm.StartRhythm();
}