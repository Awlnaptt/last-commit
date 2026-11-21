using System;
using System.Collections.Generic;
using UnityEngine;

// Uang, tip, statistik harian, dan perbandingan pendapatan vs gaji tawaran (GDD slide 9, 10, 15).
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Uang")]
    public int uangAwal = 50000;

    [Header("Tawaran kerja (email hari 4)")]
    public int gajiTawaran = 8000000;
    [Tooltip("Proyeksi bulanan = rata-rata pemasukan harian x angka ini")]
    public int hariKerjaPerBulan = 26;

    [Header("Hasil racik (diubah upgrade)")]
    public float pengaliHarga = 1f;      // upgrade mesin/musik cabang A
    public float pengaliPremium = 1.2f;  // Perfect = kopi premium
    public float persenTip = 0.15f;      // tip hanya dari Perfect
    public float pengaliMiss = 0.5f;     // kopi pahit tetap dibayar setengah
    public float bonusTipTenang = 0.10f; // Ketenangan 70+ : tip +10%

    public int Uang { get; private set; }

    // Statistik hari ini
    public int PendapatanHariIni { get; private set; }
    public int TipHariIni { get; private set; }
    public int DilayaniHariIni { get; private set; }
    public int PerfectHariIni { get; private set; }
    public int KecewaHariIni { get; private set; }

    public int PemasukanHariIni => PendapatanHariIni + TipHariIni;

    readonly List<int> riwayatHarian = new List<int>();
    readonly Dictionary<MenuItemData, int> hargaKustom = new Dictionary<MenuItemData, int>();

    public int RataRataHarian
    {
        get
        {
            if (riwayatHarian.Count == 0) return 0;
            long total = 0;
            foreach (int x in riwayatHarian) total += x;
            return (int)(total / riwayatHarian.Count);
        }
    }

    public int ProyeksiBulanan => RataRataHarian * hariKerjaPerBulan;

    [Header("Dampak harga ke jumlah pelanggan")]
    [Tooltip("Jeda antarpelanggan bertambah sampai sekian saat harga rata-rata di batas wajar (0.3 = 30% lebih jarang)")]
    public float tambahJedaSampaiWajar = 0.3f;
    [Tooltip("Tambahan jeda untuk tiap kelipatan selisih di atas batas wajar")]
    public float tambahJedaDiAtasWajar = 1.0f;
    public float pengaliJedaMaks = 3f;
    public bool KedaiMengalahkanGaji => ProyeksiBulanan > gajiTawaran;

    public event Action<int> OnUangBerubah;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Uang = uangAwal;
    }

    public void ResetHarian()
    {
        PendapatanHariIni = 0;
        TipHariIni = 0;
        DilayaniHariIni = 0;
        PerfectHariIni = 0;
        KecewaHariIni = 0;
    }

    // Dipanggil DayManager saat shift ditutup.
    public void TutupHari()
    {
        riwayatHarian.Add(PemasukanHariIni);
    }

    // 0 = harga rekomendasi (atau lebih murah), 1 = tepat di batas wajar, >1 = di atas wajar.
    public float TingkatMahal(MenuItemData menu)
    {
        float rentang = Mathf.Max(1f, menu.hargaWajarMaks - menu.hargaDasar);
        return Mathf.Max(0f, (HargaMenu(menu) - menu.hargaDasar) / rentang);
    }

    public bool TerlaluMahal(MenuItemData menu) => HargaMenu(menu) > menu.hargaWajarMaks;

    // Pengali jeda kedatangan pelanggan dari rata-rata tingkat mahal menu yang dijual hari ini.
    // 1 = normal, 1.3 = 30% lebih jarang, dst.
    public float PengaliJedaHarga
    {
        get
        {
            var menu = OrderManager.Instance != null ? OrderManager.Instance.MenuTersedia() : null;
            if (menu == null || menu.Count == 0) return 1f;

            float total = 0f;
            foreach (var m in menu) total += TingkatMahal(m);
            float t = total / menu.Count;

            float pengali = 1f + tambahJedaSampaiWajar * Mathf.Min(t, 1f)
                               + tambahJedaDiAtasWajar * Mathf.Max(0f, t - 1f);
            return Mathf.Min(pengali, pengaliJedaMaks);
        }
    }

    public void KembalikanHarga(MenuItemData menu) => hargaKustom.Remove(menu);

    public int HargaMenu(MenuItemData menu) =>
        hargaKustom.TryGetValue(menu, out int h) ? h : menu.hargaDasar;

    // Papan menu (should have). Menaikkan harga di atas wajar: Ketenangan -5.
    public void AturHarga(MenuItemData menu, int hargaBaru)
    {
        int lama = HargaMenu(menu);
        if (hargaBaru > lama && hargaBaru > menu.hargaWajarMaks)
            KetenanganTracker.Instance.Ubah(-5, "harga " + menu.namaTampil + " di atas wajar");
        hargaKustom[menu] = Mathf.Max(0, hargaBaru);
    }

    // Dipanggil OrderManager setelah racikan selesai. Mengembalikan uang yang masuk.
    public int CatatPenjualan(MenuItemData menu, HasilRacik hasil)
    {
        float harga = HargaMenu(menu) * pengaliHarga;
        int bayar;
        int tip = 0;

        switch (hasil)
        {
            case HasilRacik.Perfect:
                bayar = Mathf.RoundToInt(harga * pengaliPremium);
                float persen = persenTip;
                if (KetenanganTracker.Instance.Tingkat == TingkatKetenangan.Tenang)
                    persen *= 1f + bonusTipTenang;
                tip = Mathf.RoundToInt(bayar * Mathf.Max(0f, persen));
                PerfectHariIni++;
                break;

            case HasilRacik.Good:
                bayar = Mathf.RoundToInt(harga);
                break;

            default: // Miss
                bayar = Mathf.RoundToInt(harga * pengaliMiss);
                KecewaHariIni++;
                KetenanganTracker.Instance.Ubah(-5, "kopi pahit");
                break;
        }

        PendapatanHariIni += bayar;
        TipHariIni += tip;
        DilayaniHariIni++;
        UbahUang(bayar + tip);
        return bayar + tip;
    }

    // Pelanggan pergi karena menunggu terlalu lama.
    public void CatatPelangganPergi()
    {
        KecewaHariIni++;
        KetenanganTracker.Instance.Ubah(-3, "pelanggan menunggu terlalu lama");
    }

    // Dipakai "Ulangi Hari": kembalikan saldo ke nilai sebelum shift.
    public void AturUang(int nilai)
    {
        UbahUang(nilai - Uang);
    }

    public bool CobaBelanja(int jumlah)
    {
        if (jumlah < 0 || jumlah > Uang) return false;
        UbahUang(-jumlah);
        return true;
    }

    void UbahUang(int delta)
    {
        Uang += delta;
        OnUangBerubah?.Invoke(Uang);
    }

    // 1250000 -> "Rp 1.250.000"
    public static string Rupiah(int x) =>
        "Rp " + x.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', '.');
}
