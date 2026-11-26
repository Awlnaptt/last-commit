using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Post-processing (GDD slide 23: "post-processing hanya bloom serta color grading").
//
// Post-processing = efek yang diterapkan ke GAMBAR AKHIR kamera, setelah semua objek digambar:
//   Bloom            : piksel yang sangat terang "berpendar" ke sekitarnya (lampu, jendela gedung, kilat)
//   White Balance    : suhu warna. Negatif = kebiruan/dingin, positif = kekuningan/hangat
//   Color Adjustments: saturasi (kepekatan warna), kontras, dan filter warna
//   Vignette         : tepi layar digelapkan supaya perhatian ke tengah
//   Tonemapping      : memetakan warna HDR (bisa > 1) ke layar (0..1) supaya lampu tidak putih pecah
//
// Gaya mengikuti cerita (GDD slide 19):
//   Tahap 1 (hari 1-3)  "sepi dan dingin"  -> suhu negatif, saturasi rendah, vinyet lebih tebal
//   Tahap 2 (setelah renovasi dibeli, paling cepat hari 4) "hangat dan penuh" -> suhu positif, saturasi naik, bloom lebih kuat
// Ditambah dua pengubah:
//   - Ketenangan rendah  -> warna makin pudar dan tepi layar makin gelap (kesan lelah / burnout)
//   - Saat meracik       -> vinyet menebal sedikit (fokus, tegang)
// Semua nilai berpindah pelan ke target supaya tidak terasa lompat.
public class AtmosferPasca : MonoBehaviour
{
    [System.Serializable]
    public struct Gaya
    {
        [Range(-100f, 100f)] public float suhu;
        [Range(-100f, 100f)] public float saturasi;
        [Range(-100f, 100f)] public float kontras;
        [Range(0f, 1f)] public float vinyet;
        [Range(0f, 3f)] public float bloom;
        public Color filter;
    }

    public Gaya tahap1 = new Gaya { suhu = -18f, saturasi = -12f, kontras = 8f, vinyet = 0.30f, bloom = 0.55f, filter = new Color(0.94f, 0.97f, 1f) };
    public Gaya tahap2 = new Gaya { suhu = 14f, saturasi = 8f, kontras = 10f, vinyet = 0.20f, bloom = 0.95f, filter = new Color(1f, 0.97f, 0.92f) };

    [Header("Pengubah")]
    public int ketenanganMulaiPudar = 40;
    public float pudarMaks = 22f;           // pengurangan saturasi saat Ketenangan 0
    public float vinyetLelah = 0.12f;
    public float vinyetMeracik = 0.10f;
    public float kecepatan = 1.5f;

    [Header("Bloom")]
    public float ambangBloom = 0.95f;
    public float sebaranBloom = 0.65f;
    public Color warnaBloom = new Color(1f, 0.9f, 0.78f);

    Volume volume;
    Bloom bloom;
    WhiteBalance wb;
    ColorAdjustments warna;
    Vignette vinyet;
    Gaya sekarang;
    float cekKamera;
    int hariSebelum = -1;

    void Start()
    {
        var go = new GameObject("Volume_Atmosfer");
        go.transform.SetParent(transform, false);
        go.layer = 0; // Default, sudah termasuk Volume Mask kamera bawaan

        volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;   // menang dari volume bawaan scene

        var profil = ScriptableObject.CreateInstance<VolumeProfile>();
        profil.name = "Profil_Atmosfer (dari kode)";
        volume.profile = profil;

        var tm = profil.Add<Tonemapping>(true);
        tm.mode.value = TonemappingMode.Neutral;

        bloom = profil.Add<Bloom>(true);
        bloom.threshold.value = ambangBloom;
        bloom.scatter.value = sebaranBloom;
        bloom.tint.value = warnaBloom;
        bloom.highQualityFiltering.value = false;  // lebih ringan untuk laptop

        wb = profil.Add<WhiteBalance>(true);
        warna = profil.Add<ColorAdjustments>(true);
        warna.postExposure.value = 0f;
        warna.hueShift.overrideState = false;

        vinyet = profil.Add<Vignette>(true);
        vinyet.smoothness.value = 0.45f;
        vinyet.color.value = new Color(0.05f, 0.04f, 0.06f);
        vinyet.rounded.value = false;

        sekarang = Target();
        Terapkan(sekarang);
        NyalakanDiKamera();
    }

    void Update()
    {
        float u = 1f - Mathf.Exp(-kecepatan * Time.deltaTime);
        Gaya t = Target();

        // Hari baru (misalnya masuk tahap 2): ganti gaya langsung, tertutup layar hitam TransisiHari.
        int hari = DayManager.Instance != null ? DayManager.Instance.HariIni : 0;
        if (hari != hariSebelum) { hariSebelum = hari; u = 1f; }
        sekarang.suhu = Mathf.Lerp(sekarang.suhu, t.suhu, u);
        sekarang.saturasi = Mathf.Lerp(sekarang.saturasi, t.saturasi, u);
        sekarang.kontras = Mathf.Lerp(sekarang.kontras, t.kontras, u);
        sekarang.vinyet = Mathf.Lerp(sekarang.vinyet, t.vinyet, u);
        sekarang.bloom = Mathf.Lerp(sekarang.bloom, t.bloom, u);
        sekarang.filter = Color.Lerp(sekarang.filter, t.filter, u);
        Terapkan(sekarang);

        // Kamera pemain bisa dibuat belakangan oleh Jalur A, jadi dicek ulang tiap detik.
        cekKamera -= Time.deltaTime;
        if (cekKamera <= 0f) { cekKamera = 1f; NyalakanDiKamera(); }
    }

    Gaya Target()
    {
        var hari = DayManager.Instance;
        bool tahapDua = hari != null && hari.Tahap2Aktif;   // hangat setelah renovasi dibeli
        Gaya g = tahapDua ? tahap2 : tahap1;

        var ket = KetenanganTracker.Instance;
        if (ket != null && ket.Nilai < ketenanganMulaiPudar)
        {
            float lelah = 1f - (float)ket.Nilai / Mathf.Max(1, ketenanganMulaiPudar);
            g.saturasi -= pudarMaks * lelah;
            g.vinyet += vinyetLelah * lelah;
        }

        if (OrderManager.Instance != null && OrderManager.Instance.SedangMeracik) g.vinyet += vinyetMeracik;
        if (hari != null && hari.Fase == FaseHari.Malam) g.bloom += 0.2f;   // lampu malam lebih berpendar
        g.bloom += CuacaHarian.Kilat * 1.5f;
        return g;
    }

    void Terapkan(Gaya g)
    {
        if (bloom == null) return;
        bloom.intensity.value = g.bloom;
        wb.temperature.value = g.suhu;
        warna.saturation.value = g.saturasi;
        warna.contrast.value = g.kontras;
        warna.colorFilter.value = g.filter;
        vinyet.intensity.value = Mathf.Clamp01(g.vinyet);
    }

    // Post-processing di URP harus dinyalakan per kamera (bawaannya mati).
    static void NyalakanDiKamera()
    {
        foreach (var cam in Camera.allCameras)
        {
            var data = cam.GetUniversalAdditionalCameraData();
            if (data != null && data.renderType == CameraRenderType.Base && !data.renderPostProcessing)
                data.renderPostProcessing = true;
        }
    }

    void OnDestroy()
    {
        if (volume != null && volume.profile != null) Destroy(volume.profile);
    }
}
