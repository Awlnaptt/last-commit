using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Papan menu untuk mengatur harga (GDD slide 13, should have). Pasang di objek PapanMenu.
// Pagi hari: E di papan -> panel harga terbuka. Tombol - / + mengubah harga Rp 1.000.
// Tiap menu menampilkan harga rekomendasi dan batas wajar, plus dampak harga ke jumlah pelanggan.
// Di atas batas wajar: Ketenangan -5 saat dinaikkan, pelanggan jauh lebih jarang datang, dan komplain.
// Panelnya dibuat lewat kode di Kanvas_UI, jadi tidak perlu prefab.
public class PapanHarga : MonoBehaviour, IInteraktif
{
    public static bool Terbuka { get; private set; }

    public int langkahHarga = 1000;
    public int hargaMinimum = 1000;

    static readonly Color PANEL = new Color(0.12f, 0.10f, 0.09f, 0.96f);
    static readonly Color KRIM = new Color(0.96f, 0.91f, 0.82f);
    static readonly Color EMAS = new Color(0.90f, 0.70f, 0.35f);
    static readonly Color PASIR = new Color(0.79f, 0.72f, 0.61f);
    static readonly Color MERAH = new Color(0.90f, 0.40f, 0.35f);
    static readonly Color HIJAU = new Color(0.55f, 0.80f, 0.50f);

    GameObject layar;
    RectTransform daftar;
    TMP_Text teksDampak;
    TMP_FontAsset font;
    readonly List<GameObject> barisAktif = new List<GameObject>();

    public string Petunjuk => Terbuka ? "[E] Tutup papan harga" : "[E] Atur harga menu";

    public bool BisaDipakai =>
        DayManager.Instance != null && DayManager.Instance.Fase == FaseHari.Pagi &&
        (Terbuka || (!MenuUpgradeUI.Terbuka && !MenuJedaUI.Terbuka &&
                     DialogueManager.Instance != null && !DialogueManager.Instance.IsActive));

    public void Interaksi()
    {
        if (!BisaDipakai) return;
        if (Terbuka) Tutup();
        else Buka();
    }

    static PapanHarga aktif;

    [Header("Dialog seputar harga (teks bisa diubah di Inspector)")]
    [TextArea(2, 3)]
    public string[] kalimatPenjelasan =
    {
        "Ini papan harga. Angka hijau kecil di tiap menu itu harga rekomendasi.",
        "Kalau kemahalan, pelanggan bakal jarang datang dan protes. Kalau kemurahan, aku yang rugi."
    };
    [TextArea(2, 3)]
    public string kalimatBelumDicek = "Harga {menu} belum kupasang di papan. Cek papan menu dulu sebelum buka.";
    [TextArea(2, 3)]
    public string kalimatMahal = "Harga {menu} di atas wajar. Pelanggan bakal jarang datang dan protes soal harga.";
    public string pilihanTetapBuka = "Tetap buka kedai";
    public string pilihanAturUlang = "Atur ulang harga dulu";

    static readonly InfoPembicara Batin = new InfoPembicara { nama = "Aku", peran = "dalam hati" };
    static readonly HashSet<MenuItemData> sudahDicek = new HashSet<MenuItemData>();
    static bool sudahDijelaskan;
    static int hariSetujuMahal = -1;

    // Dipanggil DayManager sebelum kedai dibuka. Mengembalikan true kalau sedang menampilkan dialog
    // (kedai belum dibuka). lanjut dipanggil kalau pemain memilih tetap buka meski harga mahal.
    public static bool Periksa(System.Action lanjut)
    {
        if (aktif == null || DialogueManager.Instance == null) return false;
        var eco = EconomyManager.Instance;
        var menu = OrderManager.Instance.MenuTersedia();

        // 1. Ada menu yang harganya belum pernah dilihat di papan (hari 1, atau menu baru hari 3).
        var belum = new List<string>();
        foreach (var m in menu) if (!sudahDicek.Contains(m)) belum.Add(m.namaTampil);
        if (belum.Count > 0)
        {
            DialogueManager.Instance.Mulai(aktif.Satu(aktif.kalimatBelumDicek.Replace("{menu}", Gabung(belum))), null, Batin);
            return true;
        }

        // 2. Ada menu di atas batas wajar: minta konfirmasi sekali per hari.
        int hari = DayManager.Instance.HariIni;
        var mahal = new List<string>();
        foreach (var m in menu) if (eco.TerlaluMahal(m)) mahal.Add(m.namaTampil);
        if (mahal.Count > 0 && hariSetujuMahal != hari)
        {
            var d = aktif.Satu(aktif.kalimatMahal.Replace("{menu}", Gabung(mahal)));
            d.pilihan = new[]
            {
                new PilihanDialog { teks = aktif.pilihanTetapBuka },
                new PilihanDialog { teks = aktif.pilihanAturUlang }
            };
            DialogueManager.Instance.Mulai(d, pilih =>
            {
                if (pilih == 0) { hariSetujuMahal = hari; lanjut?.Invoke(); }
            }, Batin);
            return true;
        }
        return false;
    }

    DialogueData Satu(params string[] isi)
    {
        var d = ScriptableObject.CreateInstance<DialogueData>();
        d.baris = new BarisDialog[isi.Length];
        for (int i = 0; i < isi.Length; i++) d.baris[i] = new BarisDialog { pembicara = "Aku", isi = isi[i] };
        d.pilihan = new PilihanDialog[0];
        return d;
    }

    static string Gabung(List<string> nama)
    {
        if (nama.Count == 1) return nama[0];
        return string.Join(", ", nama.GetRange(0, nama.Count - 1)) + " dan " + nama[nama.Count - 1];
    }

    // Dipanggil MenuJedaUI saat Esc ditekan ketika papan terbuka.
    public static void TutupYangTerbuka()
    {
        if (aktif != null && Terbuka) aktif.Tutup();
    }

    void Awake()
    {
        aktif = this;
        // Variabel static tidak ter-reset saat scene dimuat ulang ("Main Lagi"), jadi dibersihkan di sini.
        sudahDicek.Clear();
        sudahDijelaskan = false;
        hariSetujuMahal = -1;
    }

    void OnDestroy()
    {
        Terbuka = false;
        if (aktif == this) aktif = null;
    }

    void Update()
    {
        if (Terbuka && DayManager.Instance.Fase != FaseHari.Pagi) Tutup();
    }

    void Buka()
    {
        // Pertama kali membuka papan: penjelasan singkat dulu, baru panelnya.
        if (!sudahDijelaskan && kalimatPenjelasan != null && kalimatPenjelasan.Length > 0)
        {
            sudahDijelaskan = true;
            DialogueManager.Instance.Mulai(Satu(kalimatPenjelasan), _ => Buka(), Batin);
            return;
        }

        if (layar == null) Bangun();
        if (layar == null) return;

        Terbuka = true;
        layar.SetActive(true);
        DayManager.KunciKursor(false);
        foreach (var m in OrderManager.Instance.MenuTersedia()) sudahDicek.Add(m);
        Segarkan();
    }

    void Tutup()
    {
        Terbuka = false;
        if (layar != null) layar.SetActive(false);
        DayManager.KunciKursor(true);
    }

    void Segarkan()
    {
        foreach (var b in barisAktif) { b.SetActive(false); Destroy(b); }
        barisAktif.Clear();

        var eco = EconomyManager.Instance;
        var menu = OrderManager.Instance.MenuTersedia();
        for (int i = 0; i < menu.Count; i++)
            barisAktif.Add(BuatBaris(menu[i], i, eco));

        if (teksDampak != null)
        {
            float p = eco.PengaliJedaHarga;
            int persen = Mathf.RoundToInt((1f - 1f / p) * 100f);
            bool adaMahal = false;
            foreach (var m in menu) if (eco.TerlaluMahal(m)) adaMahal = true;

            if (persen <= 0)
            {
                teksDampak.text = "Pelanggan datang seperti biasa.";
                teksDampak.color = HIJAU;
            }
            else
            {
                teksDampak.text = $"Pelanggan sekitar {persen}% lebih jarang datang." +
                                  (adaMahal ? " Menu di atas batas wajar akan dikomplain." : "");
                teksDampak.color = adaMahal ? MERAH : EMAS;
            }
        }
    }

    void PakaiRekomendasi()
    {
        var eco = EconomyManager.Instance;
        foreach (var m in OrderManager.Instance.MenuTersedia()) eco.KembalikanHarga(m);
        Segarkan();
    }

    // ---------- Pembuatan UI ----------

    void Bangun()
    {
        var kanvas = GameObject.Find("Kanvas_UI");
        if (kanvas == null) { Debug.LogError("[PapanHarga] Kanvas_UI tidak ditemukan."); return; }

        var acuan = kanvas.GetComponentInChildren<TMP_Text>(true);
        if (acuan != null) font = acuan.font;

        // Latar gelap penuh layar
        layar = new GameObject("PapanHarga", typeof(RectTransform), typeof(Image));
        layar.transform.SetParent(kanvas.transform, false);
        var rl = (RectTransform)layar.transform;
        rl.anchorMin = Vector2.zero; rl.anchorMax = Vector2.one;
        rl.offsetMin = Vector2.zero; rl.offsetMax = Vector2.zero;
        layar.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        var panel = Kotak("Panel", rl, PANEL, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 700f));
        Teks("Judul", panel, "PAPAN HARGA", 54, EMAS, TextAlignmentOptions.Top,
             new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(900f, 70f));
        Teks("Catatan", panel, "Hijau = harga rekomendasi. Makin mahal, makin jarang pelanggan datang.", 26, PASIR, TextAlignmentOptions.Top,
             new Vector2(0.5f, 1f), new Vector2(0f, -108f), new Vector2(900f, 40f));

        daftar = Kotak("Daftar", panel, Color.clear, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(920f, 330f));

        teksDampak = Teks("Dampak", panel, "", 28, HIJAU, TextAlignmentOptions.Center,
                          new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(920f, 44f));

        var rekom = Tombol("TombolRekomendasi", panel, "Pakai Rekomendasi", new Color(0.2f, 0.17f, 0.15f), KRIM,
                           new Vector2(0.5f, 0f), new Vector2(-200f, 40f), new Vector2(360f, 80f));
        rekom.onClick.AddListener(PakaiRekomendasi);

        var tutup = Tombol("TombolTutup", panel, "Selesai", EMAS, new Color(0.1f, 0.08f, 0.07f),
                           new Vector2(0.5f, 0f), new Vector2(200f, 40f), new Vector2(360f, 80f));
        tutup.onClick.AddListener(Tutup);

        layar.SetActive(false);
    }

    GameObject BuatBaris(MenuItemData m, int urutan, EconomyManager eco)
    {
        var baris = Kotak("Baris_" + m.name, daftar, new Color(1f, 1f, 1f, 0.04f),
                          new Vector2(0.5f, 1f), new Vector2(0f, -urutan * 100f), new Vector2(920f, 88f));

        int harga = eco.HargaMenu(m);
        bool mahal = harga > m.hargaWajarMaks;
        Color warnaHarga = mahal ? MERAH : harga <= m.hargaDasar ? HIJAU : EMAS;

        Teks("Nama", baris, m.namaTampil, 32, KRIM, TextAlignmentOptions.MidlineLeft,
             new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(300f, 80f));
        Teks("Harga", baris, EconomyManager.Rupiah(harga), 36, warnaHarga, TextAlignmentOptions.Midline,
             new Vector2(0.5f, 0.5f), new Vector2(10f, 12f), new Vector2(260f, 50f));
        Teks("Wajar", baris, "rekomendasi " + EconomyManager.Rupiah(m.hargaDasar) + "  ·  wajar s.d. " + EconomyManager.Rupiah(m.hargaWajarMaks),
             20, mahal ? MERAH : PASIR, TextAlignmentOptions.Midline,
             new Vector2(0.5f, 0.5f), new Vector2(10f, -24f), new Vector2(420f, 30f));

        var kurang = Tombol("Kurang", baris, "-", PANEL, KRIM, new Vector2(1f, 0.5f), new Vector2(-130f, 0f), new Vector2(80f, 64f));
        var tambah = Tombol("Tambah", baris, "+", PANEL, KRIM, new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(80f, 64f));

        MenuItemData salinan = m;
        kurang.onClick.AddListener(() => UbahHarga(salinan, -langkahHarga));
        tambah.onClick.AddListener(() => UbahHarga(salinan, +langkahHarga));
        return baris.gameObject;
    }

    void UbahHarga(MenuItemData m, int selisih)
    {
        var eco = EconomyManager.Instance;
        eco.AturHarga(m, Mathf.Max(hargaMinimum, eco.HargaMenu(m) + selisih));
        Segarkan();
    }

    RectTransform Kotak(string nama, Transform induk, Color warna, Vector2 jangkar, Vector2 posisi, Vector2 ukuran)
    {
        var go = new GameObject(nama, typeof(RectTransform));
        go.transform.SetParent(induk, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = jangkar;
        rt.anchoredPosition = posisi;
        rt.sizeDelta = ukuran;
        if (warna.a > 0.001f) go.AddComponent<Image>().color = warna;
        return rt;
    }

    TMP_Text Teks(string nama, Transform induk, string isi, float ukuranHuruf, Color warna,
                  TextAlignmentOptions rata, Vector2 jangkar, Vector2 posisi, Vector2 ukuran)
    {
        var rt = Kotak(nama, induk, Color.clear, jangkar, posisi, ukuran);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = isi;
        t.fontSize = ukuranHuruf;
        t.color = warna;
        t.alignment = rata;
        t.raycastTarget = false;
        return t;
    }

    Button Tombol(string nama, Transform induk, string label, Color latar, Color warnaLabel,
                  Vector2 jangkar, Vector2 posisi, Vector2 ukuran)
    {
        var rt = Kotak(nama, induk, latar, jangkar, posisi, ukuran);
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = rt.GetComponent<Image>();
        var t = Teks("Label", rt, label, 34, warnaLabel, TextAlignmentOptions.Center,
                     new Vector2(0.5f, 0.5f), Vector2.zero, ukuran);
        t.fontStyle = FontStyles.Bold;
        return b;
    }
}
