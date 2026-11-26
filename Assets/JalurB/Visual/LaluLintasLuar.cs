using System.Collections.Generic;
using UnityEngine;

// Kehidupan di luar jendela: pejalan kaki di trotoar dan mobil di jalan.
//
// Mengikuti cuaca (CuacaHarian.KekuatanHujan, 0 = kering, 1 = hujan paling lebat hari itu):
//   - Kering        : pejalan kaki ramai, tanpa payung.
//   - Hujan         : pejalan kaki jauh lebih jarang, semuanya memakai payung dan berjalan lebih cepat.
//   - Hujan lebat   : tidak ada pejalan kaki sama sekali. Mobil tetap lewat.
//   Pejalan yang sudah di jalan saat hujan turun langsung membuka payungnya, dan menutupnya saat reda.
// Lampu depan mobil menyala saat malam atau hujan.
//
// Pejalan kaki memakai model karakter pelanggan umum (AntreanPelanggan.pilihanModel) dengan animasi jalan.
// Payung dan mobil dibangun dari kode (PembangunMesh), satu mesh dipakai bersama oleh semua salinannya.
// Dipasang di objek LuarJendela.
public class LaluLintasLuar : MonoBehaviour
{
    [Header("Pejalan kaki")]
    public int maksPejalan = 6;
    public float pejalanPerMenit = 10f;
    [Tooltip("Di atas kekuatan hujan ini tidak ada pejalan kaki yang lewat")]
    [Range(0f, 1f)] public float hujanTanpaPejalan = 0.65f;
    public Vector2 kecepatanJalan = new Vector2(1.1f, 1.5f);
    public float[] lajurTrotoar = { 5.0f, 5.9f };     // z, dekat dinding utara (trotoar KotaLuar: 4,12..6,62)

    [Header("Mobil")]
    public int maksMobil = 4;
    public float mobilPerMenit = 9f;
    public Vector2 kecepatanMobil = new Vector2(6f, 9f);
    public float[] lajurJalan = { 8.6f, 11.6f };       // z, jalan KotaLuar: 6,62..13,62

    [Header("Batas lewat (x)")]
    public float xMulai = -16f, xAkhir = 14f;

    class Pejalan { public Transform t; public float laju; public float arah; public GameObject payung; }
    class Mobil { public Transform t; public float laju; public float arah; }

    readonly List<Pejalan> pejalan = new List<Pejalan>();
    readonly List<Mobil> mobil = new List<Mobil>();
    readonly List<ModelKarakter> model = new List<ModelKarakter>();
    CuacaHarian cuaca;
    SiklusCahaya siklus;
    float jedaPejalan, jedaMobil;

    static Mesh meshPayung, meshMobil;
    static Material[] bahanPayung;
    static Material bahanTangkai, bahanKaca, bahanRoda, bahanLampuDepan, bahanLampuBelakang;
    static readonly Color[] WARNA_PAYUNG = { new Color(0.75f, 0.22f, 0.20f), new Color(0.18f, 0.32f, 0.55f), new Color(0.90f, 0.72f, 0.25f), new Color(0.20f, 0.20f, 0.22f) };
    static readonly Color[] WARNA_MOBIL = { new Color(0.85f, 0.85f, 0.88f), new Color(0.15f, 0.16f, 0.18f), new Color(0.65f, 0.15f, 0.14f), new Color(0.20f, 0.35f, 0.55f), new Color(0.75f, 0.70f, 0.55f) };
    static Material[] bahanBodiMobil;

    void Start()
    {
        cuaca = FindAnyObjectByType<CuacaHarian>();
        siklus = FindAnyObjectByType<SiklusCahaya>();
        var antre = FindAnyObjectByType<AntreanPelanggan>();
        if (antre != null)
            foreach (var m in antre.pilihanModel)
                if (m != null && m.model != null && m.jalan != null && m.pelangganUmum) model.Add(m);
        SiapkanAset();
        jedaPejalan = 1f; jedaMobil = 2f;
    }

    void Update()
    {
        float hujan = cuaca != null ? cuaca.KekuatanHujan : 0f;
        bool adaHujan = hujan > 0.05f;
        float dt = Time.deltaTime;

        // ---- muncul ----
        jedaPejalan -= dt;
        float laju = pejalanPerMenit * FaktorWaktu() * (adaHujan ? Mathf.Clamp01(1f - hujan / hujanTanpaPejalan) * 0.4f : 1f);
        if (jedaPejalan <= 0f)
        {
            if (laju > 0.01f && pejalan.Count < maksPejalan && model.Count > 0) BuatPejalan(adaHujan);
            jedaPejalan = laju > 0.01f ? Random.Range(0.6f, 1.4f) * 60f / laju : 2f;
        }

        jedaMobil -= dt;
        if (jedaMobil <= 0f)
        {
            if (mobil.Count < maksMobil) BuatMobil();
            jedaMobil = Random.Range(0.6f, 1.4f) * 60f / Mathf.Max(1f, mobilPerMenit * Mathf.Lerp(1f, 0.6f, Malam()));
        }

        // ---- bergerak ----
        for (int i = pejalan.Count - 1; i >= 0; i--)
        {
            var p = pejalan[i];
            if (p.t == null) { pejalan.RemoveAt(i); continue; }
            if (p.payung != null && p.payung.activeSelf != adaHujan) p.payung.SetActive(adaHujan);   // buka/tutup payung
            p.t.position += Vector3.right * p.arah * p.laju * (adaHujan ? 1.4f : 1f) * dt;              // hujan: buru-buru
            if (Lewat(p.t.position.x, p.arah)) { Destroy(p.t.gameObject); pejalan.RemoveAt(i); }
        }

        float lampu = Mathf.Max(Malam(), hujan);
        if (bahanLampuDepan != null) bahanLampuDepan.SetColor("_EmissionColor", new Color(1f, 0.92f, 0.75f) * Mathf.Lerp(0.2f, 3f, lampu));
        for (int i = mobil.Count - 1; i >= 0; i--)
        {
            var m = mobil[i];
            if (m.t == null) { mobil.RemoveAt(i); continue; }
            m.t.position += Vector3.right * m.arah * m.laju * dt;
            if (Lewat(m.t.position.x, m.arah)) { Destroy(m.t.gameObject); mobil.RemoveAt(i); }
        }
    }

    bool Lewat(float x, float arah) => arah > 0 ? x > xAkhir : x < xMulai;

    float Malam() => siklus != null ? Mathf.InverseLerp(0.65f, 1f, siklus.T) : 0f;

    float FaktorWaktu()
    {
        var hari = DayManager.Instance;
        if (hari == null) return 1f;
        switch (hari.Fase)
        {
            case FaseHari.Pagi: return 0.6f;
            case FaseHari.Shift: return 1f;
            case FaseHari.Malam: return 0.25f;
            default: return 0f;
        }
    }

    void BuatPejalan(bool pakaiPayung)
    {
        var k = model[Random.Range(0, model.Count)];
        float arah = Random.value < 0.5f ? 1f : -1f;
        float z = lajurTrotoar[Random.Range(0, lajurTrotoar.Length)] + Random.Range(-0.15f, 0.15f);
        var pos = transform.TransformPoint(new Vector3(arah > 0 ? xMulai : xAkhir, 0f, z));
        var anim = KarakterAnimasi.Buat(k, transform, pos, Quaternion.LookRotation(Vector3.right * arah));
        anim.name = "Pejalan_" + k.name;
        anim.Main(k.jalan, true);
        foreach (var r in anim.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // Payung: tangkai dari tangan kanan, kanopi di atas kepala. Disembunyikan saat tidak hujan.
        float tinggi = k.tinggi > 0f ? k.tinggi : 1.7f;
        var payung = new GameObject("Payung", typeof(MeshFilter), typeof(MeshRenderer));
        payung.transform.SetParent(anim.transform, false);
        payung.transform.position = anim.transform.position + anim.transform.right * 0.18f + Vector3.up * (tinggi * 0.55f);
        payung.transform.localScale = Vector3.one / Mathf.Max(0.0001f, anim.transform.lossyScale.y);
        payung.GetComponent<MeshFilter>().sharedMesh = meshPayung;
        var rp = payung.GetComponent<MeshRenderer>();
        rp.sharedMaterials = new[] { bahanPayung[Random.Range(0, bahanPayung.Length)], bahanTangkai };
        rp.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        payung.SetActive(pakaiPayung);

        pejalan.Add(new Pejalan { t = anim.transform, laju = Random.Range(kecepatanJalan.x, kecepatanJalan.y), arah = arah, payung = payung });
    }

    void BuatMobil()
    {
        int lajur = Random.Range(0, lajurJalan.Length);
        float arah = lajur == 0 ? 1f : -1f;   // lajur dekat ke kanan, lajur jauh ke kiri
        var go = new GameObject("Mobil", typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(arah > 0 ? xMulai - 3f : xAkhir + 3f, 0f, lajurJalan[lajur]);
        go.transform.localRotation = Quaternion.Euler(0f, arah > 0 ? 0f : 180f, 0f);
        go.GetComponent<MeshFilter>().sharedMesh = meshMobil;
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterials = new[] { bahanBodiMobil[Random.Range(0, bahanBodiMobil.Length)], bahanKaca, bahanRoda, bahanLampuDepan, bahanLampuBelakang };
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mobil.Add(new Mobil { t = go.transform, laju = Random.Range(kecepatanMobil.x, kecepatanMobil.y), arah = arah });
    }

    // ---------- aset dari kode (dibuat sekali, dipakai bersama) ----------

    static void SiapkanAset()
    {
        if (meshPayung != null) return;

        // Payung terbuka: kanopi kerucut rendah (8 sisi, seperti payung sungguhan) + tangkai.
        var b = new PembangunMesh(2);
        b.Silinder(0, new Vector3(0f, 0.95f, 0f), 0.55f, 0.03f, 0.25f, 8, true, true);
        b.Kotak(1, new Vector3(0f, 0.55f, 0f), new Vector3(0.025f, 1.0f, 0.025f));
        b.Kotak(1, new Vector3(0.04f, 0.06f, 0f), new Vector3(0.09f, 0.025f, 0.025f));
        meshPayung = b.Jadi("Mesh_PayungTerbuka");

        bahanPayung = new Material[WARNA_PAYUNG.Length];
        for (int i = 0; i < WARNA_PAYUNG.Length; i++) bahanPayung[i] = BahanKode.Lit("Payung_" + i, WARNA_PAYUNG[i], null, 0.55f);
        bahanTangkai = BahanKode.Lit("Tangkai", new Color(0.2f, 0.15f, 0.1f), null, 0.3f);

        // Mobil low-poly, menghadap +x: bodi, kabin kaca, 4 roda, lampu depan dan belakang.
        var m = new PembangunMesh(5);
        m.Kotak(0, new Vector3(0f, 0.55f, 0f), new Vector3(4.0f, 0.7f, 1.8f));
        m.Kotak(1, new Vector3(-0.25f, 1.17f, 0f), new Vector3(2.1f, 0.55f, 1.6f));
        foreach (float x in new[] { -1.3f, 1.3f })
        foreach (float z in new[] { -0.85f, 0.85f })
        {
            m.Atur(new Vector3(x, 0.33f, z), new Vector3(90f, 0f, 0f));
            m.Silinder(2, new Vector3(0f, -0.12f, 0f), 0.33f, 0.33f, 0.24f, 10, true, true);
            m.Reset();
        }
        m.Kotak(3, new Vector3(2.0f, 0.65f, -0.6f), new Vector3(0.04f, 0.16f, 0.35f));
        m.Kotak(3, new Vector3(2.0f, 0.65f, 0.6f), new Vector3(0.04f, 0.16f, 0.35f));
        m.Kotak(4, new Vector3(-2.0f, 0.7f, -0.65f), new Vector3(0.04f, 0.14f, 0.3f));
        m.Kotak(4, new Vector3(-2.0f, 0.7f, 0.65f), new Vector3(0.04f, 0.14f, 0.3f));
        meshMobil = m.Jadi("Mesh_Mobil");

        bahanBodiMobil = new Material[WARNA_MOBIL.Length];
        for (int i = 0; i < WARNA_MOBIL.Length; i++) bahanBodiMobil[i] = BahanKode.Lit("Mobil_" + i, WARNA_MOBIL[i], null, 0.75f); // basah: mengilap
        bahanKaca = BahanKode.Lit("Kaca_Mobil", new Color(0.12f, 0.16f, 0.2f), null, 0.9f);
        bahanRoda = BahanKode.Lit("Roda", new Color(0.08f, 0.08f, 0.09f), null, 0.2f);
        bahanLampuDepan = BahanKode.Lit("Lampu_Depan", new Color(1f, 0.95f, 0.8f), null, 0.6f);
        BahanKode.Menyala(bahanLampuDepan, null, new Color(1f, 0.92f, 0.75f));
        bahanLampuBelakang = BahanKode.Lit("Lampu_Belakang", new Color(0.8f, 0.1f, 0.08f), null, 0.6f);
        BahanKode.Menyala(bahanLampuBelakang, null, new Color(1f, 0.12f, 0.08f) * 1.5f);
    }
}
