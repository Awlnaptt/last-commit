using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Intro sinematik sebelum hari 1: kamera bergerak pelan melewati beberapa sudut,
// teks cerita muncul huruf demi huruf, lalu kartu judul. Spasi / Enter / Esc untuk melewati.
//
// Cara kerja:
//   - Kamera pemain yang dipakai (jadi post-processing, cuaca, dan lalu lintas tetap terlihat).
//     Selama intro, skrip pemain (gerak, interaksi) dimatikan dan HUD disembunyikan.
//   - Hari 1 sudah dimulai di belakang layar (pagi, cuaca, dekor), hanya monolog pagi yang ditunda
//     sampai intro selesai. Setelah kartu judul, layar "Hari 1" langsung menyambung.
//   - Setiap shot: posisi awal ke posisi akhir (koordinat dunia) sambil melihat dari titik awal ke titik akhir.
//     Isi 'objekDilihat' supaya kamera mengarah ke objek itu (posisinya dicari otomatis).
//
// Mengatur shot: pilih JalurB_Managers > IntroSinematik. Tiap shot punya tombol
// "Lihat" (Scene view pindah ke sudut itu) dan "Ambil dari Scene view" (simpan sudut Scene view sekarang).
public class IntroSinematik : MonoBehaviour
{
    [Serializable]
    public class Shot
    {
        public string catatan;
        public Vector3 posisiAwal, posisiAkhir;
        public Vector3 lihatAwal, lihatAkhir;
        [Tooltip("Kosongkan untuk memakai titik 'lihat'. Kalau diisi dan objeknya ada, kamera mengarah ke objek ini.")]
        public string objekDilihat;
        [Range(20f, 80f)] public float fov = 55f;
        public float durasi = 6f;
        [TextArea(2, 3)] public string teks;
    }

    // Dibaca sistem lain: selama true, pemain tidak bisa berinteraksi dan monolog pagi ditunda.
    public static bool Berjalan { get; private set; }
    static Action setelahSelesai;
    static bool sudahDiputar;

    // Jalankan 'aksi' setelah intro selesai (langsung kalau tidak ada intro).
    public static void SetelahSelesai(Action aksi)
    {
        if (aksi == null) return;
        if (Berjalan) setelahSelesai += aksi;
        else aksi();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void BersihkanStatis()
    {
        Berjalan = false;
        setelahSelesai = null;
        sudahDiputar = false;
    }

    [Header("Umum")]
    public bool aktif = true;
    [Tooltip("Centang supaya intro hanya diputar sekali (tidak diulang setelah 'Main Lagi').")]
    public bool sekaliSaja = true;
    public float hurufPerDetik = 30f;
    public float pudar = 0.6f;

    public Shot[] shot = Bawaan();

    [Header("Kartu judul")]
    public string judul = "LAST COMMIT";
    public string subjudul = "Lima hari. Satu keputusan.";
    public float durasiJudul = 4f;

    [SerializeField, HideInInspector] int versiData;
    const int VERSI_DATA = 2;

    // Koordinat dunia scene Kedai: ruangan x -5..5, z -4..4, jendela di dinding utara (z = 4),
    // trotoar z 4,1..6,6, jalan z 6,6..13,6, gedung mulai z 17. Pintu di (-3,5; -4).
    public static Shot[] Bawaan() => new[]
    {
        new Shot { catatan = "Udara: jalan di depan kedai", durasi = 7f, fov = 55f,
            posisiAwal = new Vector3(-9f, 7.5f, 2.5f), posisiAkhir = new Vector3(-1f, 7f, 3f),
            lihatAwal = new Vector3(-4f, 0.5f, 15f), lihatAkhir = new Vector3(4f, 0.5f, 15f),
            teks = "Kota ini tidak pernah benar-benar berhenti." },
        new Shot { catatan = "Dari seberang trotoar ke jendela kedai", durasi = 6.5f, fov = 40f,
            posisiAwal = new Vector3(3.2f, 1.5f, 7.2f), posisiAkhir = new Vector3(0.6f, 1.45f, 7.2f),
            lihatAwal = new Vector3(1.8f, 1.4f, 4f), lihatAkhir = new Vector3(0.6f, 1.35f, 4f),
            teks = "Di sudut jalan ini, ada kedai kopi kecil yang baru dibuka." },
        new Shot { catatan = "Dari pintu menuju mesin kopi", durasi = 6.5f, fov = 55f, objekDilihat = "MesinKopi",
            posisiAwal = new Vector3(-3.4f, 1.7f, -3.2f), posisiAkhir = new Vector3(-1.2f, 1.6f, -1.6f),
            lihatAwal = new Vector3(4f, 1.1f, 0.5f), lihatAkhir = new Vector3(4.2f, 1.1f, 1.2f),
            teks = "Seminggu lalu ia wisuda. Bukan kantor yang ia pilih, tapi mesin kopi." },
        new Shot { catatan = "Meja sudut: laptop lama dan foto wisuda", durasi = 7f, fov = 45f, objekDilihat = "MejaSudut",
            posisiAwal = new Vector3(2.1f, 1.8f, -1.4f), posisiAkhir = new Vector3(2.8f, 1.55f, -2.3f),
            lihatAwal = new Vector3(3.55f, 1.1f, -3.7f), lihatAkhir = new Vector3(3.6f, 1.0f, -3.7f),
            teks = "Tawaran kerja di dunia IT masih menunggu. Lima hari untuk memutuskan." },
    };

    void PakaiBawaanBaru()
    {
        if (versiData >= VERSI_DATA) return;
        shot = Bawaan();
        versiData = VERSI_DATA;
    }

    void OnValidate() => PakaiBawaanBaru();

    void Awake()
    {
        PakaiBawaanBaru();
        // Ditandai sejak Awake supaya DayManager.Start sudah tahu monolog pagi harus menunggu.
        if (aktif && shot != null && shot.Length > 0 && !(sekaliSaja && sudahDiputar)) Berjalan = true;
    }

    // ---------- keadaan selama intro ----------
    Camera kamera;
    Transform indukKamera;
    Vector3 posLokal;
    Quaternion rotLokal;
    float fovAsli;
    readonly List<Behaviour> dimatikan = new List<Behaviour>();
    CanvasGroup hudGrup;
    float hudAlpha;
    bool hudGrupBaru;
    GameObject kanvas;
    Image hitam;
    TMP_Text teksCerita, teksJudul, teksSub;
    bool lewati;

    void Start()
    {
        if (!Berjalan) return;
        kamera = Camera.main;
        if (kamera == null) { Selesai(); return; }

        Siapkan();
        StartCoroutine(Putar());
    }

    void Update()
    {
        if (Berjalan && kanvas != null && TombolLewati()) lewati = true;
    }

    IEnumerator Putar()
    {
        for (int i = 0; i < shot.Length && !lewati; i++)
            yield return PutarShot(shot[i]);

        if (!lewati) yield return KartuJudul();
        if (lewati) yield return null;   // tunggu satu frame supaya tombol yang sama tidak membuka menu jeda / melewati monolog
        Selesai();
    }

    IEnumerator PutarShot(Shot s)
    {
        float dur = Mathf.Max(2f * pudar + 0.5f, s.durasi);
        Vector3 pusat = Vector3.zero;
        bool adaObjek = !string.IsNullOrEmpty(s.objekDilihat) && PusatObjek(s.objekDilihat, out pusat);
        kamera.fieldOfView = s.fov;
        teksCerita.text = s.teks;
        teksCerita.maxVisibleCharacters = 0;

        for (float t = 0f; t < dur && !lewati; t += Time.unscaledDeltaTime)
        {
            float u = Mulus(t / dur);
            Vector3 pos = Vector3.Lerp(s.posisiAwal, s.posisiAkhir, u);
            Vector3 lihat = adaObjek ? pusat : Vector3.Lerp(s.lihatAwal, s.lihatAkhir, u);
            kamera.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(lihat - pos, Vector3.up));

            hitam.color = new Color(0f, 0f, 0f, Mathf.Max(1f - t / pudar, 1f - (dur - t) / pudar, 0f));
            teksCerita.maxVisibleCharacters = Mathf.FloorToInt(Mathf.Max(0f, t - pudar * 0.5f) * hurufPerDetik);
            teksCerita.alpha = Mathf.Clamp01((dur - t) / pudar);
            yield return null;
        }
        hitam.color = Color.black;
        teksCerita.text = "";
    }

    IEnumerator KartuJudul()
    {
        hitam.color = Color.black;
        for (float t = 0f; t < durasiJudul && !lewati; t += Time.unscaledDeltaTime)
        {
            float a = Mathf.Clamp01(Mathf.Min(t / 0.8f, (durasiJudul - t) / 0.8f));
            teksJudul.alpha = a;
            teksSub.alpha = Mathf.Clamp01(Mathf.Min((t - 0.6f) / 0.8f, (durasiJudul - t) / 0.8f));
            teksJudul.characterSpacing = Mathf.Lerp(30f, 18f, Mulus(t / durasiJudul));
            yield return null;
        }
    }

    // ---------- persiapan dan pemulihan ----------

    void Siapkan()
    {
        indukKamera = kamera.transform.parent;
        posLokal = kamera.transform.localPosition;
        rotLokal = kamera.transform.localRotation;
        fovAsli = kamera.fieldOfView;

        // Matikan skrip pemain (gerak, lihat, interaksi) supaya kamera tidak ditarik balik.
        var cc = kamera.GetComponentInParent<CharacterController>();
        Transform pemain = cc != null ? cc.transform : kamera.transform;
        var skrip = new List<MonoBehaviour>(pemain.GetComponentsInChildren<MonoBehaviour>());
        if (cc == null && kamera.transform.parent != null && kamera.transform.parent.name != "Kedai") skrip.AddRange(kamera.transform.parent.GetComponents<MonoBehaviour>());
        foreach (var mb in skrip)
        {
            if (mb == this || !mb.enabled || !string.IsNullOrEmpty(mb.GetType().Namespace)) continue;   // komponen bawaan Unity/URP tetap jalan
            mb.enabled = false;
            dimatikan.Add(mb);
        }
        kamera.transform.SetParent(null, true);

        var ui = GameObject.Find("Kanvas_UI");
        if (ui != null)
        {
            hudGrup = ui.GetComponent<CanvasGroup>();
            hudGrupBaru = hudGrup == null;
            if (hudGrupBaru) hudGrup = ui.AddComponent<CanvasGroup>();
            hudAlpha = hudGrup.alpha;
            hudGrup.alpha = 0f;
        }

        DayManager.KunciKursor(true);
        BuatLayar();
    }

    void Selesai()
    {
        if (kamera != null)
        {
            kamera.transform.SetParent(indukKamera, false);
            kamera.transform.localPosition = posLokal;
            kamera.transform.localRotation = rotLokal;
            kamera.fieldOfView = fovAsli;
        }
        foreach (var b in dimatikan) if (b != null) b.enabled = true;
        dimatikan.Clear();

        if (hudGrup != null)
        {
            if (hudGrupBaru) Destroy(hudGrup);
            else hudGrup.alpha = hudAlpha;
        }
        if (kanvas != null) Destroy(kanvas);

        sudahDiputar = true;
        Berjalan = false;
        var aksi = setelahSelesai;
        setelahSelesai = null;
        aksi?.Invoke();
    }

    // Keluar Play / ganti scene di tengah intro: cukup bersihkan status, jangan memulai monolog.
    void OnDestroy()
    {
        if (!Berjalan || kanvas == null) return;
        Berjalan = false;
        setelahSelesai = null;
    }

    // Titik tengah objek bernama 'nama' (di mana pun di scene, termasuk yang nonaktif).
    static bool PusatObjek(string nama, out Vector3 pusat)
    {
        pusat = Vector3.zero;
        var kedai = GameObject.Find("Kedai");
        var t = kedai != null ? AlatKedai.Cari(kedai.transform, nama) : null;
        if (t == null) { var go = GameObject.Find(nama); t = go != null ? go.transform : null; }
        if (t == null) return false;
        pusat = AlatKedai.Ukur(t, out var b) ? b.center : t.position;
        return true;
    }

    // ---------- layar: hitam, pita sinematik, teks ----------

    void BuatLayar()
    {
        kanvas = new GameObject("Kanvas_Intro", typeof(Canvas), typeof(CanvasScaler));
        var c = kanvas.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 1000;
        var sk = kanvas.GetComponent<CanvasScaler>();
        sk.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sk.referenceResolution = new Vector2(1920f, 1080f);
        sk.matchWidthOrHeight = 0.5f;

        hitam = Kotak("Hitam", Vector2.zero, Vector2.one, 0f, Color.black);
        Kotak("PitaAtas", new Vector2(0f, 1f), Vector2.one, 110f, Color.black);
        Kotak("PitaBawah", Vector2.zero, new Vector2(1f, 0f), 110f, Color.black);

        teksCerita = Teks("Cerita", new Vector2(0.5f, 0f), new Vector2(0f, 175f), new Vector2(1500f, 90f), 44f);
        teksCerita.fontStyle = FontStyles.Italic;
        teksCerita.color = new Color(0.97f, 0.94f, 0.88f);
        teksCerita.outlineWidth = 0.15f;
        teksCerita.outlineColor = new Color32(0, 0, 0, 200);

        var petunjuk = Teks("Petunjuk", new Vector2(0f, 1f), new Vector2(40f, -55f), new Vector2(600f, 40f), 22f);
        petunjuk.alignment = TextAlignmentOptions.Left;
        petunjuk.rectTransform.pivot = new Vector2(0f, 0.5f);
        petunjuk.text = "Spasi / Enter / Esc = lewati intro";
        petunjuk.color = new Color(1f, 1f, 1f, 0.45f);

        teksJudul = Teks("Judul", new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1600f, 160f), 120f);
        teksJudul.fontStyle = FontStyles.Bold;
        teksJudul.text = judul;
        teksJudul.color = new Color(0.96f, 0.90f, 0.80f);
        teksJudul.alpha = 0f;
        teksSub = Teks("Subjudul", new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(1600f, 60f), 40f);
        teksSub.text = subjudul;
        teksSub.color = new Color(0.79f, 0.72f, 0.61f);
        teksSub.alpha = 0f;
    }

    Image Kotak(string nama, Vector2 min, Vector2 max, float tinggi, Color warna)
    {
        var go = new GameObject(nama, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(kanvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max;
        rt.pivot = new Vector2(0.5f, min.y);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        if (tinggi > 0f) rt.sizeDelta = new Vector2(0f, tinggi);
        var img = go.GetComponent<Image>();
        img.color = warna;
        img.raycastTarget = false;
        return img;
    }

    TMP_Text Teks(string nama, Vector2 jangkar, Vector2 posisi, Vector2 ukuran, float besar)
    {
        var go = new GameObject(nama, typeof(RectTransform));
        go.transform.SetParent(kanvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = jangkar;
        rt.anchoredPosition = posisi;
        rt.sizeDelta = ukuran;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = besar;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    static float Mulus(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }

    static bool TombolLewati()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        return k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame || k.escapeKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
