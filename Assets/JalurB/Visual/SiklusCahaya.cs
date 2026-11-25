using UnityEngine;

// Siklus cahaya pagi -> siang -> sore -> malam mengikuti fase hari.
//
// Cara kerja (bahan penjelasan saat demo):
//   1. Waktu hari diubah jadi satu angka t dari 0 sampai 1:
//        Pagi  -> t = 0
//        Shift -> t = 0,05 sampai 0,94 mengikuti sisa waktu shift (rata, tidak menumpuk di akhir)
//        Malam -> t = 1
//   2. Ada beberapa "titik kunci" cahaya (pagi, siang, sore, malam). Tiap titik kunci menyimpan
//      warna dan terang matahari, sudut matahari, warna cahaya ambien, dan terang langit.
//   3. Untuk t di antara dua titik kunci, semua nilai dihitung dengan interpolasi linear (Lerp):
//        nilai = kunciA + (kunciB - kunciA) * u,  u = posisi t di antara dua kunci (0..1)
//   4. Hasilnya dipasang ke Directional Light (matahari), RenderSettings.ambientLight, dan skybox.
//      Sudut matahari diputar lewat rotasi Euler sumbu X (tinggi matahari).
//   5. Perpindahan antarfase tidak langsung lompat: t bergerak pelan ke target (MoveTowards).
//   6. Cuaca hari itu (CuacaHarian) ikut dikalikan: Tint mewarnai cahaya, Terang meredupkan matahari
//      dan langit (hujan deras = lebih gelap), Kilat menambah cahaya sesaat saat petir.
//
// Lampu gantung di dalam kedai tidak disentuh (diatur upgrade Lampu Hangat), jadi saat matahari
// meredup di sore hari, cahaya lampu kedai otomatis terasa lebih dominan.
public class SiklusCahaya : MonoBehaviour
{
    [System.Serializable]
    public struct KunciCahaya
    {
        public string nama;
        [Range(0f, 1f)] public float t;
        public Color warnaMatahari;
        public float terangMatahari;
        [Tooltip("Tinggi matahari (derajat). 0 = di cakrawala, 90 = tepat di atas")]
        public float sudutMatahari;
        public Color warnaAmbien;
        public float terangLangit;
    }

    [Tooltip("Kosongkan untuk memakai Directional Light di scene")]
    public Light matahari;

    // Titik kunci dibuat berjarak rata supaya perubahan terasa SEPANJANG shift,
    // bukan menumpuk di akhir. Cahaya ambien paling berpengaruh di dalam kedai
    // (matahari hanya masuk lewat jendela), jadi rentang ambiennya dibuat lebar.
    [Tooltip("Urutkan dari t kecil ke besar")]
    public KunciCahaya[] kunciWaktu =
    {
        new KunciCahaya { nama = "Pagi",  t = 0f,    warnaMatahari = new Color(1f, 0.90f, 0.76f),    terangMatahari = 1.0f,  sudutMatahari = 18f, warnaAmbien = new Color(0.44f, 0.42f, 0.40f), terangLangit = 1.05f },
        new KunciCahaya { nama = "Siang", t = 0.3f,  warnaMatahari = new Color(1f, 0.98f, 0.95f),    terangMatahari = 1.3f,  sudutMatahari = 62f, warnaAmbien = new Color(0.50f, 0.50f, 0.50f), terangLangit = 1.3f },
        new KunciCahaya { nama = "Sore",  t = 0.6f,  warnaMatahari = new Color(1f, 0.80f, 0.55f),    terangMatahari = 1.05f, sudutMatahari = 32f, warnaAmbien = new Color(0.40f, 0.35f, 0.30f), terangLangit = 1.0f },
        new KunciCahaya { nama = "Senja", t = 0.82f, warnaMatahari = new Color(1f, 0.52f, 0.30f),    terangMatahari = 0.6f,  sudutMatahari = 8f,  warnaAmbien = new Color(0.28f, 0.22f, 0.22f), terangLangit = 0.6f },
        new KunciCahaya { nama = "Malam", t = 1f,    warnaMatahari = new Color(0.12f, 0.15f, 0.54f), terangMatahari = 0.3f,  sudutMatahari = 50f, warnaAmbien = new Color(0.17f, 0.16f, 0.18f), terangLangit = 0.25f },
    };

    [Tooltip("Kecepatan perpindahan antarfase (bagian t per detik). Kecil = senja turun pelan saat kedai tutup.")]
    public float kecepatanTransisi = 0.12f;

    [Tooltip("Shift dipetakan ke t dari nilai ini ...")]
    public float tAwalShift = 0.05f;
    [Tooltip("... sampai nilai ini. Sisanya (sampai 1) dilalui pelan setelah kedai tutup.")]
    public float tAkhirShift = 0.94f;

    public float T { get; private set; }

    FaseHari faseSebelum;
    int hariSebelum;

    float arahMatahari;      // rotasi Y asli matahari, dipertahankan
    Material langit;
    Color warnaLangitAsli = Color.white;

    void Start()
    {
        if (matahari == null) matahari = CariMatahari();
        if (matahari != null)
        {
            arahMatahari = matahari.transform.eulerAngles.y;
            RenderSettings.sun = matahari;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        // Salinan skybox supaya material bawaan Unity tidak ikut berubah.
        if (RenderSettings.skybox != null)
        {
            langit = new Material(RenderSettings.skybox);
            RenderSettings.skybox = langit;
            if (langit.HasProperty("_SkyTint")) warnaLangitAsli = langit.GetColor("_SkyTint");
            else if (langit.HasProperty("_Tint")) warnaLangitAsli = langit.GetColor("_Tint");
        }

        T = TargetT();
        Terapkan(T);
        Ingat();
    }

    void Update()
    {
        var hari = DayManager.Instance;
        // Pagi baru (hari berganti / hari diulang): lompat langsung ke pagi.
        // Lompatannya tertutup layar hitam TransisiHari, jadi tidak terlihat mendadak.
        bool pagiBaru = hari != null && hari.Fase == FaseHari.Pagi &&
                        (faseSebelum != FaseHari.Pagi || hari.HariIni != hariSebelum);
        if (pagiBaru) T = TargetT();
        else T = Mathf.MoveTowards(T, TargetT(), KecepatanSekarang() * Time.deltaTime);

        Terapkan(T);
        Ingat();
    }

    void Ingat()
    {
        if (DayManager.Instance == null) return;
        faseSebelum = DayManager.Instance.Fase;
        hariSebelum = DayManager.Instance.HariIni;
    }

    // Saat shift, t sudah naik sedikit demi sedikit tiap frame (mengikuti jam), jadi cukup dikejar cepat.
    float KecepatanSekarang()
    {
        var hari = DayManager.Instance;
        return hari != null && hari.Fase == FaseHari.Shift ? Mathf.Max(kecepatanTransisi, 0.5f) : kecepatanTransisi;
    }

    float TargetT()
    {
        var hari = DayManager.Instance;
        if (hari == null) return 0f;
        switch (hari.Fase)
        {
            case FaseHari.Pagi: return 0f;
            case FaseHari.Shift: return Mathf.Lerp(tAwalShift, tAkhirShift, Mathf.Clamp01(hari.ProgresShift));
            default: return 1f;
        }
    }

    void Terapkan(float t)
    {
        if (kunciWaktu == null || kunciWaktu.Length == 0) return;
        KunciCahaya k = Interpolasi(t);

        Color tint = CuacaHarian.Tint;
        float terang = CuacaHarian.Terang;
        float kilat = CuacaHarian.Kilat;

        if (matahari != null)
        {
            matahari.color = Color.Lerp(k.warnaMatahari * tint, new Color(0.85f, 0.9f, 1f), kilat);
            matahari.intensity = k.terangMatahari * terang + kilat * 3f;
            matahari.transform.rotation = Quaternion.Euler(k.sudutMatahari, arahMatahari, 0f);
        }

        // Ambien hanya ikut separuh redupnya cuaca: dalam kedai tetap terang oleh lampu.
        RenderSettings.ambientLight = k.warnaAmbien * tint * Mathf.Lerp(1f, terang, 0.5f) + Color.white * (kilat * 0.35f);
        if (langit != null)
        {
            if (langit.HasProperty("_Exposure")) langit.SetFloat("_Exposure", k.terangLangit * terang + kilat * 1.5f);
            if (langit.HasProperty("_SkyTint")) langit.SetColor("_SkyTint", warnaLangitAsli * tint);
            else if (langit.HasProperty("_Tint")) langit.SetColor("_Tint", warnaLangitAsli * tint);
        }
    }

    // Interpolasi linear antara dua kunci yang mengapit t.
    KunciCahaya Interpolasi(float t)
    {
        if (t <= kunciWaktu[0].t) return kunciWaktu[0];
        for (int i = 0; i < kunciWaktu.Length - 1; i++)
        {
            KunciCahaya a = kunciWaktu[i], b = kunciWaktu[i + 1];
            if (t > b.t) continue;

            float u = Mathf.InverseLerp(a.t, b.t, t);
            return new KunciCahaya
            {
                t = t,
                warnaMatahari = Color.Lerp(a.warnaMatahari, b.warnaMatahari, u),
                terangMatahari = Mathf.Lerp(a.terangMatahari, b.terangMatahari, u),
                sudutMatahari = Mathf.Lerp(a.sudutMatahari, b.sudutMatahari, u),
                warnaAmbien = Color.Lerp(a.warnaAmbien, b.warnaAmbien, u),
                terangLangit = Mathf.Lerp(a.terangLangit, b.terangLangit, u),
            };
        }
        return kunciWaktu[kunciWaktu.Length - 1];
    }

    static Light CariMatahari()
    {
        if (RenderSettings.sun != null) return RenderSettings.sun;
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional) return l;
        return null;
    }
}
