using System;
using System.Collections.Generic;
using UnityEngine;

// Hasil satu racikan. Diputuskan Jalur A (rhythm), dipakai Jalur B (uang, Ketenangan).
public enum HasilRacik { Perfect, Good, Miss }

public class Pesanan
{
    public MenuItemData menu;
    public float sabar;    // sisa detik
    public float sabarMaks;
}

// Antrean pesanan selama shift (GDD slide 9).
//
// KONTRAK DENGAN JALUR A:
//   1. Pemain menekan E di MesinKopi  -> panggil OrderManager.Instance.MulaiRacik()
//      Hasilnya pesanan yang diracik (null kalau antrean kosong). Pola: pesanan.menu.polaRitme
//   2. Pola selesai                    -> panggil OrderManager.Instance.SelesaiRacik(hasil)
//   Selama diracik, kesabaran pesanan itu berhenti berkurang.
public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    public List<MenuItemData> semuaMenu = new List<MenuItemData>();

    [Header("Pesanan")]
    public float jedaPelanggan = 12f;  // satu pesanan sekitar 10-15 detik
    public int kapasitas = 3;          // naik lewat upgrade Kursi dan Meja
    public float kesabaran = 40f;

    readonly List<Pesanan> antrean = new List<Pesanan>();

    public IReadOnlyList<Pesanan> Antrean => antrean;
    public Pesanan PesananAktif => antrean.Count > 0 ? antrean[0] : null;
    public bool SedangMeracik { get; private set; }

    public event Action OnAntreanBerubah;
    public event Action<Pesanan> OnPesananMasuk;
    public event Action<Pesanan, HasilRacik, int> OnPesananSelesai; // int = uang masuk
    public event Action<Pesanan> OnPelangganPergi;

    bool menerima;
    float timerDatang;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void MulaiTerima()
    {
        menerima = true;
        timerDatang = 2f;
    }

    public void BerhentiTerima()
    {
        menerima = false;
    }

    // Tutup shift: sisa antrean pulang tanpa dihitung kecewa.
    public void KosongkanAntrean()
    {
        antrean.Clear();
        SedangMeracik = false;
        OnAntreanBerubah?.Invoke();
    }

    void Update()
    {
        // Waktu berhenti saat dialog terbuka, supaya ngobrol dengan pelanggan tetap tidak dihukum.
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive) return;

        if (menerima && !SedangMeracik)
        {
            timerDatang -= Time.deltaTime;
            if (timerDatang <= 0f)
            {
                // Harga mahal membuat pelanggan lebih jarang datang (EconomyManager.PengaliJedaHarga).
                timerDatang = jedaPelanggan * EconomyManager.Instance.PengaliJedaHarga;
                CobaTambahPesanan();
            }
        }

        for (int i = antrean.Count - 1; i >= 0; i--)
        {
            if (i == 0 && SedangMeracik) continue;

            antrean[i].sabar -= Time.deltaTime;
            if (antrean[i].sabar > 0f) continue;

            Pesanan pergi = antrean[i];
            antrean.RemoveAt(i);
            EconomyManager.Instance.CatatPelangganPergi();
            OnPelangganPergi?.Invoke(pergi);
            OnAntreanBerubah?.Invoke();
        }
    }

    void CobaTambahPesanan()
    {
        if (antrean.Count >= kapasitas) return;

        var tersedia = MenuTersedia();
        if (tersedia.Count == 0) return;

        var p = new Pesanan
        {
            menu = tersedia[UnityEngine.Random.Range(0, tersedia.Count)],
            sabar = kesabaran,
            sabarMaks = kesabaran
        };
        antrean.Add(p);
        OnPesananMasuk?.Invoke(p);
        OnAntreanBerubah?.Invoke();
    }

    public List<MenuItemData> MenuTersedia()
    {
        int hari = DayManager.Instance != null ? DayManager.Instance.HariIni : 1;
        var hasil = new List<MenuItemData>();
        foreach (var m in semuaMenu)
            if (m != null && m.hariTersedia <= hari) hasil.Add(m);
        return hasil;
    }

    public Pesanan MulaiRacik()
    {
        if (PesananAktif == null) return null;
        SedangMeracik = true;
        return PesananAktif;
    }

    public void SelesaiRacik(HasilRacik hasil)
    {
        Pesanan p = PesananAktif;
        SedangMeracik = false;
        if (p == null) return;

        antrean.RemoveAt(0);
        int uang = EconomyManager.Instance.CatatPenjualan(p.menu, hasil);
        OnPesananSelesai?.Invoke(p, hasil, uang);
        OnAntreanBerubah?.Invoke();
    }

    // Kalau pemain membatalkan racikan di tengah jalan.
    public void BatalRacik()
    {
        SedangMeracik = false;
    }
}
