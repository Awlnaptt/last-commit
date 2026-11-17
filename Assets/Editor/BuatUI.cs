// BuatUI.cs — Tugas 6: bikin semua layar UI sekaligus
//
// Taruh di: Assets/Editor/BuatUI.cs
// Jalankan lewat menu: Last Commit > Buat Layar UI
//
// Hasilnya: 1 Canvas di scene + 8 prefab di Assets/Prefabs/UI/
// Yang aktif cuma HUD, sisanya dimatikan supaya Game view tidak penuh.
// Semua teks dan angka di sini cuma contoh tampilan. Rizki yang menyambungkan
// ke logika lewat script-nya, kamu cukup serahkan prefab-nya.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class BuatUI
{
    // Palet dari GDD
    static readonly Color GELAP    = new Color32(0x1E, 0x16, 0x13, 0xFF);
    static readonly Color PANEL    = new Color32(0x2A, 0x1F, 0x1A, 0xFF);
    static readonly Color EMAS     = new Color32(0xE8, 0xA3, 0x3D, 0xFF);
    static readonly Color KRIM     = new Color32(0xF5, 0xEB, 0xDD, 0xFF);
    static readonly Color PASIR    = new Color32(0xD8, 0xC7, 0xB5, 0xFF);
    static readonly Color HIJAU    = new Color32(0x7F, 0xB0, 0x69, 0xFF);
    static readonly Color MERAH    = new Color32(0xE0, 0x80, 0x6B, 0xFF);
    static readonly Color BIRU     = new Color32(0x6B, 0x96, 0xB8, 0xFF);

    const string FOLDER = "Assets/Prefabs/UI";

    [MenuItem("Last Commit/Buat Layar UI")]
    public static void Buat()
    {
        if (TMP_Settings.defaultFontAsset == null)
        {
            EditorUtility.DisplayDialog("Font TextMeshPro belum ada",
                "Buka menu Window > TextMeshPro > Import TMP Essential Resources dulu, " +
                "lalu jalankan lagi Last Commit > Buat Layar UI.\n\n" +
                "Tanpa itu semua teks akan kosong.", "OK");
            return;
        }

        SiapkanFolder();
        var kanvas = SiapkanKanvas();
        SiapkanEventSystem();

        var dibuat = new (GameObject go, bool aktif)[]
        {
            (Hud(kanvas),            true),
            (FeedbackRitme(kanvas),  false),
            (Dialog(kanvas),         false),
            (MenuUpgrade(kanvas),    false),
            (RekapMalam(kanvas),     false),
            (EndingTenang(kanvas),   false),
            (EndingBurnout(kanvas),  false),
            (MenuJeda(kanvas),       false),
        };

        foreach (var (go, aktif) in dibuat)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(
                go, FOLDER + "/" + go.name + ".prefab", InteractionMode.AutomatedAction);
            go.SetActive(aktif);
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog("Selesai",
            dibuat.Length + " layar UI dibuat dan disimpan sebagai prefab di\n" + FOLDER +
            "\n\nYang aktif cuma HUD. Untuk melihat layar lain, centang kotaknya " +
            "di Hierarchy (anak dari Kanvas_UI).\n\nJangan lupa Ctrl+S.", "OK");
    }

    // ---------- Layar ----------

    static GameObject Hud(Transform k)
    {
        var akar = Layar("HUD", k, Color.clear);

        Teks("TeksHari", akar, "Hari 1  ·  07:30", 44, KRIM,
             TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(48, -40), new Vector2(600, 60));

        Teks("TeksUang", akar, "Rp 0", 44, EMAS,
             TextAlignmentOptions.TopRight, new Vector2(1, 1), new Vector2(-48, -40), new Vector2(400, 60));

        // Meter ketenangan: lebar isi diatur lewat anchorMax.x (0 sampai 1)
        var meter = Kotak("MeterKetenangan", akar, new Color(0, 0, 0, 0.45f),
                          new Vector2(0, 0), new Vector2(48, 60), new Vector2(460, 34));
        var isi = Kotak("Isi", meter, HIJAU, Vector2.zero, Vector2.zero, Vector2.zero);
        var ri = isi.GetComponent<RectTransform>();
        ri.anchorMin = Vector2.zero; ri.anchorMax = new Vector2(0.7f, 1f);
        ri.offsetMin = Vector2.zero; ri.offsetMax = Vector2.zero;

        Teks("LabelKetenangan", akar, "KETENANGAN", 26, PASIR,
             TextAlignmentOptions.BottomLeft, new Vector2(0, 0), new Vector2(48, 102), new Vector2(460, 36));

        Teks("TeksPesanan", akar, "Pesanan:  Espresso  ·  Latte", 34, PASIR,
             TextAlignmentOptions.BottomRight, new Vector2(1, 0), new Vector2(-48, 60), new Vector2(760, 50));

        Teks("TeksPetunjuk", akar, "[E] Seduh", 32, EMAS,
             TextAlignmentOptions.Center, new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(500, 50));

        return akar.gameObject;
    }

    static GameObject FeedbackRitme(Transform k)
    {
        var akar = Layar("FeedbackRitme", k, Color.clear);
        var t = Teks("TeksHasil", akar, "PERFECT", 110, EMAS,
                     TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f),
                     new Vector2(0, 160), new Vector2(900, 150));
        t.fontStyle = FontStyles.Bold;

        Teks("TeksKombo", akar, "x3", 54, KRIM,
             TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(400, 80));

        return akar.gameObject;
    }

    static GameObject Dialog(Transform k)
    {
        var akar = Layar("Dialog", k, Color.clear);

        var panel = Kotak("Panel", akar, PANEL,
                          new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(1500, 260));
        Garis(panel, EMAS);

        Teks("TeksNama", panel, "Pelanggan", 38, EMAS,
             TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(44, -28), new Vector2(700, 50));

        Teks("TeksIsi", panel, "Satu americano, jangan terlalu pahit ya.", 36, KRIM,
             TextAlignmentOptions.TopLeft, new Vector2(0, 1), new Vector2(44, -90), new Vector2(1400, 120));

        Teks("TeksLanjut", panel, "[Space]", 28, PASIR,
             TextAlignmentOptions.BottomRight, new Vector2(1, 0), new Vector2(-44, 24), new Vector2(300, 40));

        return akar.gameObject;
    }

    static GameObject MenuUpgrade(Transform k)
    {
        var akar = Layar("MenuUpgrade", k, new Color(0, 0, 0, 0.72f));

        Teks("Judul", akar, "UPGRADE KEDAI", 72, EMAS,
             TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(1200, 100));

        var baris = Kotak("Baris", akar, Color.clear,
                          new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(1440, 620));
        var h = baris.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 40; h.childAlignment = TextAnchor.MiddleCenter;
        h.childForceExpandWidth = true; h.childForceExpandHeight = true;

        KartuUpgrade(baris, "Mesin Espresso", "Jendela timing Perfect melebar.", "Rp 250.000");
        KartuUpgrade(baris, "Kursi Empuk",     "Pelanggan betah, sabar bertambah.", "Rp 180.000");
        KartuUpgrade(baris, "Pemutar Musik",   "Ketenangan turun lebih lambat.",   "Rp 320.000");

        Tombol("TombolTutup", akar, "Tutup", PANEL, KRIM,
               new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(360, 86));

        return akar.gameObject;
    }

    static void KartuUpgrade(RectTransform induk, string nama, string isi, string harga)
    {
        var kartu = Kotak("Kartu_" + nama.Replace(" ", ""), induk, PANEL,
                          new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(440, 600));
        Garis(kartu, PASIR);

        Teks("Nama", kartu, nama, 44, EMAS,
             TextAlignmentOptions.Top, new Vector2(0.5f, 1), new Vector2(0, -44), new Vector2(380, 110));

        Teks("Isi", kartu, isi, 32, KRIM,
             TextAlignmentOptions.Top, new Vector2(0.5f, 1), new Vector2(0, -180), new Vector2(380, 200));

        Teks("Harga", kartu, harga, 38, PASIR,
             TextAlignmentOptions.Center, new Vector2(0.5f, 0), new Vector2(0, 160), new Vector2(380, 60));

        Tombol("TombolBeli", kartu, "Beli", EMAS, GELAP,
               new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(300, 76));
    }

    static GameObject RekapMalam(Transform k)
    {
        var akar = Layar("RekapMalam", k, new Color(0, 0, 0, 0.8f));

        var panel = Kotak("Panel", akar, PANEL,
                          new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 760));
        Garis(panel, EMAS);

        Teks("Judul", panel, "TUTUP TOKO", 64, EMAS,
             TextAlignmentOptions.Top, new Vector2(0.5f, 1), new Vector2(0, -56), new Vector2(900, 90));

        Teks("Subjudul", panel, "Hari 1", 34, PASIR,
             TextAlignmentOptions.Top, new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(900, 50));

        BarisStat(panel, "Pelanggan dilayani", "12", -240);
        BarisStat(panel, "Pesanan Perfect",    "7",  -310);
        BarisStat(panel, "Pendapatan",         "Rp 340.000", -380);
        BarisStat(panel, "Ketenangan akhir",   "68 / 100",   -450);

        Tombol("TombolLanjut", panel, "Lanjut ke Hari 2", EMAS, GELAP,
               new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(520, 90));

        return akar.gameObject;
    }

    static void BarisStat(RectTransform panel, string label, string nilai, float y)
    {
        Teks("Label_" + label.Replace(" ", ""), panel, label, 36, KRIM,
             TextAlignmentOptions.Left, new Vector2(0, 1), new Vector2(80, y), new Vector2(620, 54));

        Teks("Nilai_" + label.Replace(" ", ""), panel, nilai, 36, EMAS,
             TextAlignmentOptions.Right, new Vector2(1, 1), new Vector2(-80, y), new Vector2(400, 54));
    }

    static GameObject EndingTenang(Transform k)
    {
        return Ending("EndingTenang", k, "KEDAI YANG BERTAHAN", HIJAU,
            "Kamu menutup pintu, mematikan lampu, dan untuk pertama kalinya " +
            "sejak keluar dari kantor, besok terasa seperti sesuatu yang kamu tunggu.");
    }

    static GameObject EndingBurnout(Transform k)
    {
        return Ending("EndingBurnout", k, "COMMIT TERAKHIR", MERAH,
            "Pesanan terus datang dan kamu berhenti menghitung. " +
            "Mesinnya masih menyala. Kamu yang tidak.");
    }

    static GameObject Ending(string nama, Transform k, string judul, Color warna, string isi)
    {
        var akar = Layar(nama, k, GELAP);

        var t = Teks("Judul", akar, judul, 86, warna,
                     TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f),
                     new Vector2(0, 180), new Vector2(1500, 130));
        t.fontStyle = FontStyles.Bold;

        Teks("Isi", akar, isi, 38, KRIM,
             TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(1200, 220));

        Tombol("TombolMenu", akar, "Kembali ke Menu", PANEL, KRIM,
               new Vector2(0.5f, 0), new Vector2(0, 170), new Vector2(480, 90));

        return akar.gameObject;
    }

    static GameObject MenuJeda(Transform k)
    {
        var akar = Layar("MenuJeda", k, new Color(0, 0, 0, 0.78f));

        var panel = Kotak("Panel", akar, PANEL,
                          new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 620));
        Garis(panel, EMAS);

        Teks("Judul", panel, "JEDA", 64, EMAS,
             TextAlignmentOptions.Top, new Vector2(0.5f, 1), new Vector2(0, -56), new Vector2(560, 90));

        Tombol("TombolLanjut", panel, "Lanjutkan",     EMAS,  GELAP, new Vector2(0.5f, 1), new Vector2(0, -210), new Vector2(480, 86));
        Tombol("TombolUlang",  panel, "Ulangi Hari",   PANEL, KRIM,  new Vector2(0.5f, 1), new Vector2(0, -320), new Vector2(480, 86));
        Tombol("TombolKeluar", panel, "Keluar Game",   PANEL, MERAH, new Vector2(0.5f, 1), new Vector2(0, -430), new Vector2(480, 86));

        return akar.gameObject;
    }

    // ---------- Pembantu ----------

    static void SiapkanFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(FOLDER))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
    }

    static Transform SiapkanKanvas()
    {
        var lama = GameObject.Find("Kanvas_UI");
        if (lama != null) Undo.DestroyObjectImmediate(lama);

        var go = new GameObject("Kanvas_UI",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(go, "Buat kanvas UI");

        var c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;

        var s = go.GetComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920, 1080);
        s.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        s.matchWidthOrHeight = 0.5f;

        return go.transform;
    }

    static void SiapkanEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        var go = new GameObject("EventSystem", typeof(EventSystem));
        Undo.RegisterCreatedObjectUndo(go, "Buat EventSystem");
#if ENABLE_INPUT_SYSTEM
        go.AddComponent<InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    // Layar penuh, jadi akar tiap prefab
    static RectTransform Layar(string nama, Transform induk, Color bg)
    {
        var go = new GameObject(nama, typeof(RectTransform), typeof(CanvasGroup));
        go.transform.SetParent(induk, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        if (bg.a > 0.001f)
        {
            var img = go.AddComponent<Image>();
            img.color = bg;
        }
        return rt;
    }

    static RectTransform Kotak(string nama, Transform induk, Color warna,
        Vector2 anchor, Vector2 posisi, Vector2 ukuran)
    {
        var go = new GameObject(nama, typeof(RectTransform));
        go.transform.SetParent(induk, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = posisi;
        rt.sizeDelta = ukuran;

        if (warna.a > 0.001f) go.AddComponent<Image>().color = warna;
        return rt;
    }

    static TextMeshProUGUI Teks(string nama, Transform induk, string isi, int ukuran,
        Color warna, TextAlignmentOptions rata, Vector2 anchor, Vector2 posisi, Vector2 ukuranKotak)
    {
        var go = new GameObject(nama, typeof(RectTransform));
        go.transform.SetParent(induk, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = posisi;
        rt.sizeDelta = ukuranKotak;

        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = isi;
        t.fontSize = ukuran;
        t.color = warna;
        t.alignment = rata;
        return t;
    }

    static void Tombol(string nama, Transform induk, string label, Color bg, Color warnaTeks,
        Vector2 anchor, Vector2 posisi, Vector2 ukuran)
    {
        var rt = Kotak(nama, induk, bg, anchor, posisi, ukuran);
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = rt.GetComponent<Image>();

        var t = Teks("Label", rt, label, 34, warnaTeks,
                     TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), Vector2.zero, ukuran);
        t.raycastTarget = false;
    }

    // Garis tepi tipis supaya panel tidak terlihat seperti kotak datar
    static void Garis(RectTransform panel, Color warna)
    {
        var o = new GameObject("Tepi", typeof(RectTransform));
        o.transform.SetParent(panel, false);
        o.transform.SetAsFirstSibling();

        var rt = (RectTransform)o.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-3, -3); rt.offsetMax = new Vector2(3, 3);

        var img = o.AddComponent<Image>();
        img.color = new Color(warna.r, warna.g, warna.b, 0.55f);
        img.raycastTarget = false;
    }
}
