using UnityEngine;

// Pasang di objek Pintu. Pagi: E untuk membuka kedai dan memulai shift.
public class PintuKedai : MonoBehaviour, IInteraktif
{
    public string Petunjuk => "[E] Buka kedai";
    public bool BisaDipakai => DayManager.Instance != null && DayManager.Instance.BisaBukaKedai;
    public void Interaksi() => DayManager.Instance.BukaKedai();
}
