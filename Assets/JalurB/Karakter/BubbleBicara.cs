using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Gelembung teks di atas kepala karakter, selalu menghadap kamera.
// Dipakai pelanggan umum (pesanan, reaksi) dan NPC tetap (sapaan).
// Dibuat lewat kode, jadi tidak perlu prefab:
//   var b = BubbleBicara.Pasang(transformKarakter, 1.4f);
//   b.Tampil("Espresso, ya!", 3f);   // durasi 0 = tampil terus sampai Sembunyi()
public class BubbleBicara : MonoBehaviour
{
    public float tinggi = 1.4f;       // meter di atas kaki
    public float lebarMaks = 420f;    // piksel kanvas
    public float skala = 0.0035f;     // 1 piksel kanvas = 3,5 mm

    static TMP_FontAsset fontAcuan;
    static bool sudahCariFont;

    RectTransform akar;
    RectTransform latar;
    TMP_Text teks;
    float sisa;
    Transform kamera;

    public bool Tampak => akar != null && akar.gameObject.activeSelf;

    public static BubbleBicara Pasang(Transform target, float tinggi)
    {
        var b = target.GetComponent<BubbleBicara>();
        if (b == null) b = target.gameObject.AddComponent<BubbleBicara>();
        b.tinggi = tinggi;
        if (b.akar != null) b.akar.localPosition = Vector3.up * tinggi;
        return b;
    }

    void Awake()
    {
        Bangun();
        Sembunyi();
    }

    void Bangun()
    {
        var go = new GameObject("Bubble", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        akar = (RectTransform)go.transform;
        akar.localPosition = Vector3.up * tinggi;
        akar.localScale = Vector3.one * skala;
        akar.sizeDelta = new Vector2(lebarMaks, 120f);

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        var bg = new GameObject("Latar", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(akar, false);
        latar = (RectTransform)bg.transform;
        latar.anchorMin = latar.anchorMax = new Vector2(0.5f, 0f);
        latar.pivot = new Vector2(0.5f, 0f);
        bg.GetComponent<Image>().color = new Color(1f, 0.97f, 0.9f, 0.95f);

        var tg = new GameObject("Teks", typeof(RectTransform));
        tg.transform.SetParent(latar, false);
        var rt = (RectTransform)tg.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(18f, 10f);
        rt.offsetMax = new Vector2(-18f, -10f);

        teks = tg.AddComponent<TextMeshProUGUI>();
        teks.fontSize = 34;
        teks.color = new Color(0.22f, 0.15f, 0.1f);
        teks.alignment = TextAlignmentOptions.Center;
        teks.raycastTarget = false;

        // Samakan font dengan UI game kalau ada.
        if (!sudahCariFont)
        {
            sudahCariFont = true;
            var kanvas = GameObject.Find("Kanvas_UI");
            var acuan = kanvas != null ? kanvas.GetComponentInChildren<TMP_Text>(true) : null;
            if (acuan != null) fontAcuan = acuan.font;
        }
        if (fontAcuan != null) teks.font = fontAcuan;
    }

    public void Tampil(string isi, float durasi)
    {
        if (string.IsNullOrEmpty(isi)) { Sembunyi(); return; }

        akar.gameObject.SetActive(true);
        teks.text = isi;
        float isiMaks = lebarMaks - 36f;
        Vector2 ukuran = teks.GetPreferredValues(isi, isiMaks, 0f);
        latar.sizeDelta = new Vector2(Mathf.Min(ukuran.x, isiMaks) + 36f, ukuran.y + 20f);
        sisa = durasi;
    }

    public void Sembunyi()
    {
        if (akar != null) akar.gameObject.SetActive(false);
        sisa = 0f;
    }

    void Update()
    {
        if (sisa <= 0f) return;
        sisa -= Time.deltaTime;
        if (sisa <= 0f) Sembunyi();
    }

    void LateUpdate()
    {
        if (!Tampak) return;
        if (kamera == null && Camera.main != null) kamera = Camera.main.transform;
        if (kamera != null) akar.rotation = kamera.rotation;
    }

    // Tinggi kepala dari bentuk model yang terlihat.
    public static float TinggiKepala(Transform model)
    {
        var rs = model.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return 1.4f;
        Bounds b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b.max.y - model.position.y + 0.15f;
    }
}
