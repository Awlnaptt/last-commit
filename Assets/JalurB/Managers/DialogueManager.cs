using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Siapa yang bicara: nama, keterangan kecil di samping nama, dan potret wajah.
public class InfoPembicara
{
    public string nama;
    public string peran;
    public Sprite potret;
}

// Satu kotak dialog untuk SEMUA ucapan di game (GDD slide 17, 21), dua mode:
//
//   Mulai(...)    : percakapan. Pemain menekan Space untuk lanjut, waktu shift berhenti,
//                   boleh ada maksimal dua pilihan jawaban (1 / 2).
//   Ucapkan(...)  : satu kalimat singkat (pesanan pelanggan, reaksi, sapaan NPC).
//                   Tidak menghentikan permainan, hilang sendiri, diantrekan kalau banyak.
//
// Memakai prefab Dialog di Kanvas_UI. Potret dibuat otomatis di sisi kiri panel kalau belum ada.
//
// UNTUK JALUR A: PlayerController sebaiknya berhenti bergerak selama
// DialogueManager.Instance.IsActive bernilai true (hanya mode percakapan).
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI (kosongkan untuk cari otomatis di Kanvas_UI/Dialog)")]
    public GameObject layar;
    public TMP_Text teksNama;
    public TMP_Text teksIsi;
    public TMP_Text teksLanjut;
    public Image potret;

    public float hurufPerDetik = 45f;
    public Color warnaPeran = new Color(0.79f, 0.72f, 0.61f);

    [Header("Ucapan singkat")]
    public float durasiUcapan = 2.5f;
    public int maksAntreUcapan = 3;

    public bool IsActive { get; private set; }

    // --- percakapan ---
    DialogueData data;
    InfoPembicara info;
    BarisDialog[] baris;
    int index;
    bool modePilih;
    bool sudahMemilih;
    int pilihanDiambil = -1;
    Action<int> selesai;
    Coroutine mengetik;
    bool barisPenuh;
    int frameMulai;

    // --- ucapan singkat ---
    struct Ucapan { public InfoPembicara siapa; public string isi; public float durasi; }
    readonly Queue<Ucapan> antreUcapan = new Queue<Ucapan>();
    bool sedangUcapan;
    float sisaUcapan;

    // --- tata letak ---
    const float GESER_POTRET = 240f;
    float xNamaAwal, xIsiAwal, lebarIsiAwal;
    bool tataSiap;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Ikat();
        if (layar != null) layar.SetActive(false);
    }

    void Ikat()
    {
        if (layar == null)
        {
            var kanvas = GameObject.Find("Kanvas_UI");
            var t = kanvas != null ? kanvas.transform.Find("Dialog") : null;
            if (t != null) layar = t.gameObject;
        }
        if (layar == null) { Debug.LogError("[Dialog] Kanvas_UI/Dialog tidak ditemukan."); return; }

        var panel = layar.transform.Find("Panel");
        if (panel == null) return;
        if (teksNama == null) teksNama = panel.Find("TeksNama")?.GetComponent<TMP_Text>();
        if (teksIsi == null) teksIsi = panel.Find("TeksIsi")?.GetComponent<TMP_Text>();
        if (teksLanjut == null) teksLanjut = panel.Find("TeksLanjut")?.GetComponent<TMP_Text>();
        if (potret == null) potret = panel.Find("Potret")?.GetComponent<Image>();
        if (potret == null) potret = BuatPotret(panel);

        if (teksNama != null) teksNama.fontStyle |= FontStyles.Bold;

        if (teksNama != null && teksIsi != null)
        {
            var rn = teksNama.rectTransform;
            var ri = teksIsi.rectTransform;
            xNamaAwal = rn.anchoredPosition.x;
            xIsiAwal = ri.anchoredPosition.x;
            lebarIsiAwal = ri.sizeDelta.x;
            tataSiap = true;
        }
    }

    static Image BuatPotret(Transform panel)
    {
        var go = new GameObject("Potret", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(panel, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(36f, 0f);
        rt.sizeDelta = new Vector2(190f, 190f);

        var img = go.GetComponent<Image>();
        img.preserveAspect = true;
        img.raycastTarget = false;
        img.enabled = false;
        return img;
    }

    // ================= Mode percakapan =================

    // selesai menerima indeks pilihan (0/1), atau -1 kalau dialog tanpa pilihan.
    // info: pembicara utama. Baris yang pembicaranya sama dengan info.nama memakai potret dan perannya.
    public void Mulai(DialogueData dialog, Action<int> selesai = null, InfoPembicara info = null)
    {
        if (dialog == null || dialog.baris == null || dialog.baris.Length == 0 || layar == null)
        {
            selesai?.Invoke(-1);
            return;
        }

        // Percakapan menggeser ucapan singkat yang sedang tampil.
        sedangUcapan = false;
        antreUcapan.Clear();

        data = dialog;
        this.info = info;
        baris = dialog.baris;
        index = 0;
        modePilih = false;
        sudahMemilih = false;
        pilihanDiambil = -1;
        this.selesai = selesai;
        frameMulai = Time.frameCount;

        IsActive = true;
        layar.SetActive(true);
        TampilkanBaris();
    }

    void Update()
    {
        if (!IsActive)
        {
            JalankanUcapan();
            return;
        }
        if (Time.frameCount == frameMulai || MenuJedaUI.Terbuka) return;

        if (modePilih)
        {
            int jumlah = Mathf.Min(2, data.pilihan.Length);
            if (Ditekan(1) && jumlah >= 1) Pilih(0);
            else if (Ditekan(2) && jumlah >= 2) Pilih(1);
            return;
        }

        if (TombolLanjut()) Lanjut();
    }

    void TampilkanBaris()
    {
        BarisDialog b = baris[index];

        Sprite gambar = b.potret;
        string peran = null;
        if (info != null && b.pembicara == info.nama)
        {
            if (gambar == null) gambar = info.potret;
            peran = info.peran;
        }
        IsiKepala(b.pembicara, peran, gambar);
        if (teksLanjut != null) teksLanjut.text = "[Space]";

        if (mengetik != null) StopCoroutine(mengetik);
        mengetik = StartCoroutine(Ketik(GantiToken(b.isi)));
    }

    IEnumerator Ketik(string isi)
    {
        barisPenuh = false;
        teksIsi.text = isi;
        teksIsi.maxVisibleCharacters = 0;

        float jeda = 1f / Mathf.Max(1f, hurufPerDetik);
        for (int i = 1; i <= isi.Length; i++)
        {
            teksIsi.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(jeda);
        }

        barisPenuh = true;
        mengetik = null;
    }

    void Lanjut()
    {
        if (!barisPenuh)
        {
            if (mengetik != null) StopCoroutine(mengetik);
            mengetik = null;
            teksIsi.maxVisibleCharacters = 99999;
            barisPenuh = true;
            return;
        }

        index++;
        if (index < baris.Length) { TampilkanBaris(); return; }

        bool adaPilihan = !sudahMemilih && data.pilihan != null && data.pilihan.Length > 0;
        if (adaPilihan) TampilkanPilihan();
        else Akhiri();
    }

    void TampilkanPilihan()
    {
        modePilih = true;
        IsiKepala("Pilih jawaban", null, null);

        string isi = "[1] " + data.pilihan[0].teks;
        if (data.pilihan.Length > 1) isi += "\n[2] " + data.pilihan[1].teks;
        teksIsi.text = isi;
        teksIsi.maxVisibleCharacters = 99999;
        if (teksLanjut != null) teksLanjut.text = data.pilihan.Length > 1 ? "[1] / [2]" : "[1]";
    }

    void Pilih(int i)
    {
        modePilih = false;
        sudahMemilih = true;
        pilihanDiambil = i;

        PilihanDialog p = data.pilihan[i];
        if (p.ubahKetenangan != 0)
            KetenanganTracker.Instance.Ubah(p.ubahKetenangan, "pilihan dialog: " + p.teks);

        if (string.IsNullOrEmpty(p.balasan)) { Akhiri(); return; }

        BarisDialog akhir = data.baris[data.baris.Length - 1];
        baris = new[] { new BarisDialog { pembicara = akhir.pembicara, potret = akhir.potret, isi = p.balasan } };
        index = 0;
        TampilkanBaris();
    }

    void Akhiri()
    {
        IsActive = false;
        layar.SetActive(false);

        if (data.ketenanganSaatSelesai != 0)
            KetenanganTracker.Instance.Ubah(data.ketenanganSaatSelesai, "percakapan selesai");

        Action<int> cb = selesai;
        selesai = null;
        cb?.Invoke(pilihanDiambil);
    }

    // ================= Mode ucapan singkat =================

    public void Ucapkan(InfoPembicara siapa, string isi, float durasi = -1f)
    {
        if (string.IsNullOrEmpty(isi) || layar == null || siapa == null) return;
        while (antreUcapan.Count >= Mathf.Max(1, maksAntreUcapan)) antreUcapan.Dequeue();
        antreUcapan.Enqueue(new Ucapan { siapa = siapa, isi = isi, durasi = durasi > 0f ? durasi : durasiUcapan });
    }

    void JalankanUcapan()
    {
        if (sedangUcapan)
        {
            sisaUcapan -= Time.deltaTime;
            if (sisaUcapan > 0f) return;
            sedangUcapan = false;
            if (antreUcapan.Count == 0) { layar.SetActive(false); return; }
        }
        if (antreUcapan.Count == 0) return;

        Ucapan u = antreUcapan.Dequeue();
        layar.SetActive(true);
        IsiKepala(u.siapa.nama, u.siapa.peran, u.siapa.potret);
        teksIsi.text = GantiToken(u.isi);
        teksIsi.maxVisibleCharacters = 99999;
        if (teksLanjut != null) teksLanjut.text = "";
        sisaUcapan = u.durasi;
        sedangUcapan = true;
    }

    // ================= Pembantu =================

    void IsiKepala(string nama, string peran, Sprite gambar)
    {
        if (teksNama != null)
        {
            string warna = ColorUtility.ToHtmlStringRGB(warnaPeran);
            teksNama.text = string.IsNullOrEmpty(peran)
                ? nama
                : $"{nama}  <size=65%><color=#{warna}>{peran}</color></size>";
        }

        bool ada = gambar != null && potret != null;
        if (potret != null)
        {
            potret.sprite = gambar;
            potret.enabled = ada;
        }

        if (!tataSiap) return;
        float geser = ada ? GESER_POTRET : 0f;
        var rn = teksNama.rectTransform;
        var ri = teksIsi.rectTransform;
        rn.anchoredPosition = new Vector2(xNamaAwal + geser, rn.anchoredPosition.y);
        ri.anchoredPosition = new Vector2(xIsiAwal + geser, ri.anchoredPosition.y);
        ri.sizeDelta = new Vector2(lebarIsiAwal - geser, ri.sizeDelta.y);
    }

    static string GantiToken(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var eco = EconomyManager.Instance;
        if (eco != null)
        {
            s = s.Replace("{gaji}", EconomyManager.Rupiah(eco.gajiTawaran));
            s = s.Replace("{proyeksi}", EconomyManager.Rupiah(eco.ProyeksiBulanan));
        }
        if (DayManager.Instance != null) s = s.Replace("{hari}", DayManager.Instance.HariIni.ToString());
        return s;
    }

    // ---------- Input ----------

    static bool TombolLanjut()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        var m = Mouse.current;
        return (k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame))
            || (m != null && m.leftButton.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0);
#endif
    }

    static bool Ditekan(int angka)
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return false;
        return angka == 1 ? k.digit1Key.wasPressedThisFrame : k.digit2Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(angka == 1 ? KeyCode.Alpha1 : KeyCode.Alpha2);
#endif
    }
}
