using System;
using System.Collections.Generic;
using UnityEngine;

// Upgrade linear dan bercabang (GDD slide 12).
//
// UNTUK JALUR A: efek upgrade ke rhythm dibaca lewat PilihanCabang:
//   UpgradeManager.Instance.PilihanCabang("Mesin") -> "A" (pola pendek), "B" (pola panjang), atau null
//   UpgradeManager.Instance.PilihanCabang("Musik") -> "A" (musik cepat), "B" (akustik), atau null
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public List<UpgradeData> semuaUpgrade = new List<UpgradeData>();

    [Tooltip("Akar scene untuk efek visual. Kosong = cari objek bernama 'Kedai'")]
    public Transform akarKedai;

    [Header("Lampu dan upgrade Lampu Hangat")]
    [Tooltip("Tidak dicentang (bawaan): lampu memakai setelan scene apa adanya, upgrade membuatnya lebih terang dan hangat.\n" +
             "Dicentang: lampu mulai putih redup, upgrade mengembalikannya ke setelan scene.")]
    public bool lampuMulaiRedup = false;
    public Color warnaLampuAwal = new Color(0.85f, 0.9f, 1f);
    [Range(0.2f, 1f)] public float pengaliTerangAwal = 0.7f;
    [Tooltip("Dipakai kalau lampuMulaiRedup mati: seberapa terang lampu setelah upgrade")]
    public float pengaliTerangUpgrade = 1.25f;
    public Color warnaLampuUpgrade = new Color(1f, 0.62f, 0.32f);
    [Range(0f, 1f)] public float campurWarnaUpgrade = 0.35f;

    struct LampuAsli { public Light lampu; public Color warna; public float terang; }
    readonly List<LampuAsli> lampu = new List<LampuAsli>();

    readonly HashSet<UpgradeData> dimiliki = new HashSet<UpgradeData>();
    readonly Dictionary<string, UpgradeData> cabangDipilih = new Dictionary<string, UpgradeData>();

    public event Action<UpgradeData> OnUpgradeDibeli;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (akarKedai == null)
        {
            var k = GameObject.Find("Kedai");
            if (k != null) akarKedai = k.transform;
        }
        SiapkanLampu();
    }

    // Simpan warna asli (hangat) semua lampu titik/sorot di kedai, lalu buat versi "belum di-upgrade".
    void SiapkanLampu()
    {
        if (akarKedai == null) return;
        foreach (var l in akarKedai.GetComponentsInChildren<Light>(true))
        {
            if (l.type == LightType.Directional) continue;
            lampu.Add(new LampuAsli { lampu = l, warna = l.color, terang = l.intensity });
            if (lampuMulaiRedup)
            {
                l.color = warnaLampuAwal;
                l.intensity *= pengaliTerangAwal;
            }
        }
    }

    void HangatkanLampu()
    {
        foreach (var a in lampu)
        {
            if (a.lampu == null) continue;
            if (lampuMulaiRedup)
            {
                a.lampu.color = a.warna;
                a.lampu.intensity = a.terang;
            }
            else
            {
                a.lampu.color = Color.Lerp(a.warna, warnaLampuUpgrade, campurWarnaUpgrade);
                a.lampu.intensity = a.terang * pengaliTerangUpgrade;
            }
        }
    }

    public bool Dimiliki(UpgradeData u) => dimiliki.Contains(u);

    public string PilihanCabang(string grup) =>
        cabangDipilih.TryGetValue(grup, out var u) ? u.labelCabang : null;

    public bool AdaBonusKetenanganHarian
    {
        get
        {
            foreach (var u in dimiliki) if (u.ketenanganHarian) return true;
            return false;
        }
    }

    // Muncul di menu Tab: belum dimiliki, harinya sudah sampai, dan cabangnya belum dipilih.
    public bool Tersedia(UpgradeData u)
    {
        if (u == null || Dimiliki(u)) return false;
        int hari = DayManager.Instance != null ? DayManager.Instance.HariIni : 1;
        if (hari < u.hariTersedia) return false;
        if (!string.IsNullOrEmpty(u.grupCabang) && cabangDipilih.ContainsKey(u.grupCabang)) return false;
        return true;
    }

    public bool BisaBeli(UpgradeData u) => Tersedia(u) && EconomyManager.Instance.Uang >= u.harga;

    public List<UpgradeData> DaftarTersedia()
    {
        var hasil = new List<UpgradeData>();
        foreach (var u in semuaUpgrade) if (Tersedia(u)) hasil.Add(u);
        return hasil;
    }

    public bool CobaBeli(UpgradeData u)
    {
        if (!BisaBeli(u)) return false;
        if (!EconomyManager.Instance.CobaBelanja(u.harga)) return false;

        dimiliki.Add(u);
        if (!string.IsNullOrEmpty(u.grupCabang)) cabangDipilih[u.grupCabang] = u;

        Terapkan(u);
        OnUpgradeDibeli?.Invoke(u);
        return true;
    }

    void Terapkan(UpgradeData u)
    {
        var eco = EconomyManager.Instance;
        eco.pengaliHarga += u.tambahPengaliHarga;
        eco.persenTip = Mathf.Max(0f, eco.persenTip + u.tambahPersenTip);
        OrderManager.Instance.kapasitas += u.tambahKapasitas;
        if (u.kurangiJedaPelanggan > 0f)
            OrderManager.Instance.jedaPelanggan *= 1f - u.kurangiJedaPelanggan;
        if (u.hangatkanLampu) HangatkanLampu();

        SetAktif(u.nyalakanObjek, true);
        SetAktif(u.matikanObjek, false);
    }

    void SetAktif(string nama, bool aktif)
    {
        if (string.IsNullOrEmpty(nama) || akarKedai == null) return;

        // GetComponentsInChildren(true) ikut mencari objek yang sedang nonaktif.
        foreach (var t in akarKedai.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == nama)
            {
                t.gameObject.SetActive(aktif);
                return;
            }
        }
        Debug.LogWarning($"[Upgrade] Objek '{nama}' tidak ditemukan di bawah Kedai.");
    }
}
