using System;
using UnityEngine;

public enum TingkatKetenangan { Gelisah, Normal, Tenang }

// Nilai Ketenangan 0-100 (GDD slide 13-14). Tidak tampil di HUD, hanya di rekap malam.
public class KetenanganTracker : MonoBehaviour
{
    public static KetenanganTracker Instance { get; private set; }

    public int nilaiAwal = 50;

    // Nilai terendah. Dulu 20 (GDD versi awal); sekarang 0 supaya burnout bisa benar-benar habis.
    // Pengaman dari "spiral kesalahan" tetap ada: jendela Perfect tidak pernah lebih sempit dari 60%
    // (lihat PengaliJendelaPerfect), seberapa pun rendah nilainya.
    // Nama field sengaja baru (bukan batasBawah) supaya nilai 20 yang tersimpan di scene tidak terbawa.
    [Range(0, 39)] public int nilaiTerendah = 0;
    public int batasBawah => nilaiTerendah;   // nama lama, tetap bisa dibaca skrip lain
    public int batasTenang = 70;  // 70+ = Tenang
    public int batasNormal = 40;  // 40-69 = Normal, di bawahnya Gelisah

    public int Nilai { get; private set; }

    public TingkatKetenangan Tingkat =>
        Nilai >= batasTenang ? TingkatKetenangan.Tenang :
        Nilai >= batasNormal ? TingkatKetenangan.Normal :
        TingkatKetenangan.Gelisah;

    // --- Untuk Jalur A (rhythm) ---
    // Pengali lebar jendela Perfect. 1 = normal (Ketenangan 40 ke atas); turun linear sampai 0,6
    // saat Ketenangan mencapai nilai terendah. Tidak pernah di bawah 0,6.
    public float PengaliJendelaPerfect =>
        Tingkat == TingkatKetenangan.Gelisah
            ? Mathf.Lerp(0.6f, 1f, Mathf.InverseLerp(nilaiTerendah, batasNormal, Nilai))
            : 1f;

    // Pengali lebar jendela Good. Tenang = lebih lebar.
    public float PengaliJendelaGood => Tingkat == TingkatKetenangan.Tenang ? 1.2f : 1f;

    // nilai baru, perubahan, alasan
    public event Action<int, int, string> OnBerubah;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Nilai = nilaiAwal;
    }

    // Dipakai "Ulangi Hari": kembalikan ke nilai sebelum shift.
    public void Atur(int nilai, string alasan)
    {
        Ubah(nilai - Nilai, alasan);
    }

    public void Ubah(int delta, string alasan)
    {
        int lama = Nilai;
        Nilai = Mathf.Clamp(Nilai + delta, nilaiTerendah, 100);
        if (Nilai == lama) return;

        Debug.Log($"[Ketenangan] {lama} -> {Nilai} ({alasan})");
        OnBerubah?.Invoke(Nilai, Nilai - lama, alasan);
    }
}
