using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Menu Tab (GDD slide 7, 21). Hanya bisa dibuka saat fase Pagi.
// Memakai prefab MenuUpgrade: kartu pertama dijadikan cetakan, lalu digandakan
// sebanyak upgrade yang tersedia hari itu.
//
// UNTUK JALUR A: PlayerController sebaiknya berhenti saat MenuUpgradeUI.Terbuka.
public class MenuUpgradeUI : MonoBehaviour
{
    public static bool Terbuka { get; private set; }

    [Tooltip("Kosongkan untuk cari otomatis Kanvas_UI/MenuUpgrade")]
    public GameObject layar;

    Transform baris;
    GameObject cetakanKartu;
    TMP_Text judul;
    readonly List<GameObject> kartuAktif = new List<GameObject>();

    void Start()
    {
        if (layar == null)
        {
            var kanvas = GameObject.Find("Kanvas_UI");
            var t = kanvas != null ? kanvas.transform.Find("MenuUpgrade") : null;
            if (t != null) layar = t.gameObject;
        }
        if (layar == null) { Debug.LogError("[MenuUpgrade] Kanvas_UI/MenuUpgrade tidak ditemukan."); enabled = false; return; }

        baris = layar.transform.Find("Baris");
        judul = layar.transform.Find("Judul")?.GetComponent<TMP_Text>();

        // Kartu pertama jadi cetakan, kartu bawaan BuatUI disembunyikan.
        for (int i = 0; i < baris.childCount; i++)
        {
            var anak = baris.GetChild(i).gameObject;
            if (cetakanKartu == null) cetakanKartu = anak;
            anak.SetActive(false);
        }

        var tutup = layar.transform.Find("TombolTutup")?.GetComponent<Button>();
        if (tutup != null) tutup.onClick.AddListener(Tutup);

        layar.SetActive(false);
        Terbuka = false;
    }

    void OnDestroy()
    {
        Terbuka = false;
    }

    void Update()
    {
        if (TabDitekan())
        {
            if (Terbuka) Tutup();
            else if (DayManager.Instance.Fase == FaseHari.Pagi && !UIStatus.MenghalangiPemain) Buka();
        }
    }

    public void Buka()
    {
        Terbuka = true;
        layar.SetActive(true);
        DayManager.KunciKursor(false);
        Segarkan();
    }

    public void Tutup()
    {
        Terbuka = false;
        layar.SetActive(false);
        DayManager.KunciKursor(true);
    }

    void Segarkan()
    {
        // SetActive(false) dulu supaya layout tidak menghitung kartu yang baru dihapus.
        foreach (var k in kartuAktif) { k.SetActive(false); Destroy(k); }
        kartuAktif.Clear();

        var eco = EconomyManager.Instance;
        if (judul != null) judul.text = "UPGRADE KEDAI  ·  " + EconomyManager.Rupiah(eco.Uang) + StatistikKemarin();

        var daftar = UpgradeManager.Instance.DaftarTersedia();
        foreach (var u in daftar)
        {
            var kartu = Instantiate(cetakanKartu, baris);
            kartu.name = "Kartu_" + u.name;
            kartu.SetActive(true);

            string nama = u.namaTampil;
            if (!string.IsNullOrEmpty(u.grupCabang)) nama += "  (cabang " + u.labelCabang + ")";
            Isi(kartu, "Nama", nama);
            Isi(kartu, "Isi", u.deskripsi);
            Isi(kartu, "Harga", EconomyManager.Rupiah(u.harga));

            var tombol = kartu.transform.Find("TombolBeli")?.GetComponent<Button>();
            if (tombol != null)
            {
                UpgradeData salinan = u;
                tombol.interactable = UpgradeManager.Instance.BisaBeli(u);
                tombol.onClick.AddListener(() => { UpgradeManager.Instance.CobaBeli(salinan); Segarkan(); });
            }
            kartuAktif.Add(kartu);
        }

        // Kartu berukuran 440 px, area 1440 px: perkecil kalau tidak muat.
        int n = Mathf.Max(1, daftar.Count);
        float lebar = n * 440f + (n - 1) * 40f;
        float skala = Mathf.Min(1f, 1440f / lebar);
        baris.localScale = new Vector3(skala, skala, 1f);
        ((RectTransform)baris).sizeDelta = new Vector2(Mathf.Max(1440f, lebar), 620f);

        if (daftar.Count == 0 && judul != null) judul.text += "\n<size=60%>Tidak ada upgrade baru hari ini</size>";
    }

    // Statistik hari sebelumnya (GDD slide 21), ditampilkan kecil di bawah judul.
    static string StatistikKemarin()
    {
        var k = DayManager.Instance != null ? DayManager.Instance.Kemarin : null;
        if (k == null) return "\n<size=50%>Belum ada data kemarin</size>";
        return $"\n<size=50%>Kemarin (hari {k.hari}): {k.dilayani} dilayani · {k.perfect} Perfect · " +
               $"{k.kecewa} kecewa · {EconomyManager.Rupiah(k.pemasukan)} · Ketenangan {k.ketenangan}</size>";
    }

    static void Isi(GameObject kartu, string nama, string teks)
    {
        var t = kartu.transform.Find(nama)?.GetComponent<TMP_Text>();
        if (t != null) t.text = teks;
    }

    static bool TabDitekan()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Tab);
#endif
    }
}
