using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Layar hitam "Hari N" setiap pagi baru (juga saat hari diulang).
//
// Gunanya dua:
//   1. Pergantian malam -> pagi, cuaca, dan gaya warna terjadi DI BALIK layar hitam,
//      jadi pemain tidak melihat cahaya lompat mendadak.
//   2. Penanda bab: judul hari dan temanya (GDD slide 8).
//
// Layar ini ada di BELAKANG Kanvas_UI, jadi monolog pagi tetap terbaca di atas layar hitam.
// Layar baru memudar setelah monolog pagi selesai (paling lama 'maksTahan' detik).
public class TransisiHari : MonoBehaviour
{
    [Tooltip("Index 0 = hari 1 (GDD slide 8)")]
    public string[] temaHari = { "Pembukaan", "Pelanggan tetap", "Hujan deras", "Tawaran", "Keputusan" };
    public float minTahan = 1.5f;
    public float maksTahan = 30f;
    public float durasiPudar = 1.8f;

    CanvasGroup grup;
    TMP_Text judul, sub;
    FaseHari faseSebelum;
    int hariSebelum = -1;
    float waktu;
    enum Keadaan { Diam, Tahan, Pudar }
    Keadaan keadaan;

    void Start()
    {
        Buat();
        if (DayManager.Instance != null && DayManager.Instance.Fase == FaseHari.Pagi && DayManager.Instance.HariIni > 0)
            Tampilkan(false);
        Ingat();
    }

    void Update()
    {
        var dm = DayManager.Instance;
        if (dm == null || grup == null) return;

        bool pagiBaru = dm.Fase == FaseHari.Pagi && dm.HariIni > 0 &&
                        (faseSebelum != FaseHari.Pagi || dm.HariIni != hariSebelum);
        if (pagiBaru) Tampilkan(faseSebelum == FaseHari.Shift && dm.HariIni == hariSebelum);
        Ingat();

        switch (keadaan)
        {
            case Keadaan.Tahan:
                // Selama intro sinematik layar ini disembunyikan (kalau tidak, menutupi gambar intro)
                // dan waktu tahannya belum dihitung.
                if (IntroSinematik.Berjalan) { grup.alpha = 0f; waktu = 0f; break; }
                grup.alpha = 1f;
                waktu += Time.deltaTime;
                bool monologSelesai = !IntroSinematik.Berjalan &&
                                      (DialogueManager.Instance == null || !DialogueManager.Instance.IsActive);
                if ((waktu >= minTahan && monologSelesai) || waktu >= maksTahan) { keadaan = Keadaan.Pudar; waktu = 0f; }
                break;

            case Keadaan.Pudar:
                grup.alpha = Mathf.MoveTowards(grup.alpha, 0f, Time.deltaTime / Mathf.Max(0.01f, durasiPudar));
                if (grup.alpha <= 0f) keadaan = Keadaan.Diam;
                break;
        }
    }

    void Tampilkan(bool diulang)
    {
        int hari = DayManager.Instance.HariIni;
        judul.text = "Hari " + hari;
        string tema = hari - 1 < temaHari.Length ? temaHari[hari - 1] : "";
        sub.text = diulang ? tema + "  (diulang)" : tema;
        grup.alpha = 1f;
        waktu = 0f;
        keadaan = Keadaan.Tahan;
    }

    void Ingat()
    {
        if (DayManager.Instance == null) return;
        faseSebelum = DayManager.Instance.Fase;
        hariSebelum = DayManager.Instance.HariIni;
    }

    void Buat()
    {
        var go = new GameObject("Kanvas_TransisiHari", typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        go.transform.SetParent(transform, false);
        var kanvas = go.GetComponent<Canvas>();
        kanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Tepat di belakang Kanvas_UI supaya kotak dialog tetap terlihat di atasnya.
        var ui = GameObject.Find("Kanvas_UI");
        var kanvasUI = ui != null ? ui.GetComponent<Canvas>() : null;
        kanvas.sortingOrder = kanvasUI != null ? kanvasUI.sortingOrder - 1 : 50;

        // Skala ikut layar (sama dengan Kanvas_UI, acuan 1920 x 1080), supaya tulisan tidak mengecil di layar besar.
        var skala = go.GetComponent<CanvasScaler>();
        skala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        skala.referenceResolution = new Vector2(1920f, 1080f);
        skala.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        skala.matchWidthOrHeight = 0.5f;
        var skalaUI = ui != null ? ui.GetComponent<CanvasScaler>() : null;
        if (skalaUI != null)
        {
            skala.referenceResolution = skalaUI.referenceResolution;
            skala.matchWidthOrHeight = skalaUI.matchWidthOrHeight;
        }

        grup = go.GetComponent<CanvasGroup>();
        grup.alpha = 0f;
        grup.blocksRaycasts = false;
        grup.interactable = false;

        var hitam = new GameObject("Hitam", typeof(RectTransform), typeof(Image));
        hitam.transform.SetParent(go.transform, false);
        Penuh(hitam.GetComponent<RectTransform>());
        hitam.GetComponent<Image>().color = new Color(0.03f, 0.03f, 0.05f, 1f);
        hitam.GetComponent<Image>().raycastTarget = false;

        judul = Teks(go.transform, "Judul", 150f, new Vector2(0f, 90f), new Color(0.96f, 0.90f, 0.80f));
        judul.fontStyle = FontStyles.Bold;
        judul.characterSpacing = 6f;
        sub = Teks(go.transform, "Tema", 54f, new Vector2(0f, -30f), new Color(0.79f, 0.72f, 0.61f));

        var garis = new GameObject("Garis", typeof(RectTransform), typeof(Image));
        garis.transform.SetParent(go.transform, false);
        var rg = garis.GetComponent<RectTransform>();
        rg.anchorMin = rg.anchorMax = new Vector2(0.5f, 0.62f);
        rg.sizeDelta = new Vector2(420f, 4f);
        rg.anchoredPosition = new Vector2(0f, 22f);
        garis.GetComponent<Image>().color = new Color(0.79f, 0.72f, 0.61f, 0.8f);
        garis.GetComponent<Image>().raycastTarget = false;
    }

    static TMP_Text Teks(Transform induk, string nama, float ukuran, Vector2 posisi, Color warna)
    {
        var go = new GameObject(nama, typeof(RectTransform));
        go.transform.SetParent(induk, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.62f);
        rt.sizeDelta = new Vector2(1400f, ukuran * 1.4f);
        rt.anchoredPosition = posisi;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = ukuran;
        t.alignment = TextAlignmentOptions.Center;
        t.color = warna;
        t.raycastTarget = false;
        return t;
    }

    static void Penuh(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
