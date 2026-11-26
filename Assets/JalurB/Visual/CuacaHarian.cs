using System;
using UnityEngine;

// Cuaca yang berganti tiap hari (GDD slide 8 dan 19: "antar hari yang berganti cuma skybox,
// intensitas hujan, dan warna cahaya"; hari 3 bertema hujan deras).
//
// Dalam satu hari hujan TIDAK tetap: naik-turun dan kadang berhenti, tapi tetap dalam batas
// "watak" hari itu (hari 3 selalu deras, hari 4 lebih sering reda).
//   - Naik-turunnya memakai Perlin noise: angka acak yang berubah HALUS terhadap waktu,
//     jadi hujan menderas dan mereda pelan, tidak berkedip.
//   - Tiap hari diberi titik awal noise yang diacak, jadi pola hari ini beda dengan main sebelumnya.
//   - "reda" = kira-kira berapa bagian waktu hujan berhenti total. Noise di bawah ambang itu = kering.
//   - Gelap langit, kabut, dan petir ikut kuat-lemahnya hujan saat itu.
//
// Yang diatur:
//   - Hujan          : jumlah tetesan per detik di partikel Kedai > Cuaca > Hujan
//   - Kabut          : kepekatan kabut (hujan deras = pandangan ke luar lebih kabur)
//   - Warna & terang : diteruskan ke SiklusCahaya lewat Tint dan Terang (dikalikan ke matahari, ambien, langit)
//   - Petir          : kilatan cahaya singkat saat hujan lebat di hari yang diberi petir
//                      (OnPetir bisa dipakai Jalur A untuk suara guntur)
public class CuacaHarian : MonoBehaviour
{
    [Serializable]
    public struct Cuaca
    {
        public string nama;
        [Tooltip("Tetesan per detik paling sedikit dan paling banyak hari itu")]
        public float hujanMin, hujanMaks;
        [Range(0f, 0.9f), Tooltip("Perkiraan bagian waktu hujan berhenti total")]
        public float reda;
        [Tooltip("Kepekatan kabut saat hujan paling lebat")]
        public float kabut;
        [Tooltip("Warna cahaya saat hujan paling lebat")]
        public Color tint;
        [Range(0.2f, 1.5f), Tooltip("Terang langit saat hujan paling lebat (1 = normal)")]
        public float terang;
        public bool petir;
    }

    // Hujan dibuat lebih jarang (revisi): hari biasa lebih banyak reda, siklusnya lebih panjang.
    // Hari 3 tetap selalu hujan deras sesuai tema cerita.
    static Cuaca[] Bawaan() => new[]
    {
        new Cuaca { nama = "Gerimis pembukaan",   hujanMin = 0f,   hujanMaks = 240f,  reda = 0.55f, kabut = 0.015f, tint = new Color(0.93f, 0.96f, 1.00f), terang = 0.88f },
        new Cuaca { nama = "Mendung",             hujanMin = 0f,   hujanMaks = 150f,  reda = 0.72f, kabut = 0.012f, tint = new Color(0.96f, 0.97f, 0.99f), terang = 0.92f },
        new Cuaca { nama = "Hujan deras",         hujanMin = 450f, hujanMaks = 1100f, reda = 0f,    kabut = 0.034f, tint = new Color(0.74f, 0.82f, 0.96f), terang = 0.52f, petir = true },
        new Cuaca { nama = "Reda, langit hangat", hujanMin = 0f,   hujanMaks = 100f,  reda = 0.82f, kabut = 0.009f, tint = new Color(1.00f, 0.94f, 0.86f), terang = 1.05f },
        new Cuaca { nama = "Gerimis keputusan",   hujanMin = 0f,   hujanMaks = 200f,  reda = 0.60f, kabut = 0.012f, tint = new Color(1.00f, 0.92f, 0.84f), terang = 0.92f },
    };

    [Tooltip("Index 0 = hari 1")]
    public Cuaca[] cuacaPerHari = Bawaan();

    [Tooltip("Rata-rata detik untuk satu siklus menderas-mereda. Makin besar, hujan makin jarang berganti.")]
    public float periodeHujan = 80f;

    // Versi angka bawaan. Kalau scene masih menyimpan angka versi lama, diganti otomatis dengan yang baru.
    [SerializeField, HideInInspector] int versiData;
    const int VERSI_DATA = 2;

    void PakaiBawaanBaru()
    {
        if (versiData >= VERSI_DATA) return;
        cuacaPerHari = Bawaan();
        periodeHujan = 80f;
        versiData = VERSI_DATA;
    }

    void Awake() => PakaiBawaanBaru();
    void OnValidate() => PakaiBawaanBaru();

    public float kecepatanTransisi = 0.4f;     // bagian per detik
    public Color warnaKabutSiang = new Color(0.55f, 0.60f, 0.68f);
    public Color warnaKabutMalam = new Color(0.10f, 0.11f, 0.15f);
    public Vector2 jedaPetir = new Vector2(7f, 18f);

    // Dibaca SiklusCahaya dan KotaLuar.
    public static Color Tint { get; private set; } = Color.white;
    public static float Terang { get; private set; } = 1f;
    public static float Kilat { get; private set; }      // 0..1 saat petir menyambar
    public static event Action OnPetir;

    public string NamaCuacaHariIni => Ambil().nama;
    // 0 = kering, 1 = hujan paling lebat hari itu. Boleh dibaca Jalur A (misalnya volume suara hujan).
    public float KekuatanHujan { get; private set; }

    ParticleSystem hujan;
    SiklusCahaya siklus;
    float hujanSekarang, kabutSekarang, waktuPetir, titikNoise, waktuNoise;
    int hariNoise = -1;

    void Start()
    {
        hujan = CariHujan();
        if (hujan == null)
            Debug.LogWarning("[CuacaHarian] Partikel Kedai > Cuaca > Hujan tidak ada. Jalankan menu Last Commit > Buat Hujan di Luar Jendela.");
        siklus = FindAnyObjectByType<SiklusCahaya>();

        AcakPolaHari();
        var c = Ambil();
        KekuatanHujan = Kekuatan(c);
        Tint = Color.Lerp(Color.white, c.tint, KekuatanHujan);
        Terang = Mathf.Lerp(1f, c.terang, KekuatanHujan);
        hujanSekarang = Mathf.Lerp(c.hujanMin, c.hujanMaks, KekuatanHujan) * (KekuatanHujan > 0f ? 1f : 0f);
        kabutSekarang = c.kabut * KekuatanHujan;
        if (hujan != null) { var main = hujan.main; main.maxParticles = Mathf.Max(main.maxParticles, 2000); }
        RenderSettings.fog = true;
        JadwalkanPetir();
    }

    void OnDisable()
    {
        Tint = Color.white; Terang = 1f; Kilat = 0f;
    }

    void Update()
    {
        int hari = DayManager.Instance != null ? DayManager.Instance.HariIni : 1;
        bool hariBaru = hari != hariNoise;
        if (hariBaru) AcakPolaHari();

        // Noise hanya maju saat game berjalan (berhenti saat menu jeda, karena timeScale 0).
        waktuNoise += Time.deltaTime;

        var c = Ambil();
        float kuat = Kekuatan(c);
        KekuatanHujan = Mathf.MoveTowards(KekuatanHujan, kuat, Time.deltaTime * 0.5f);

        float targetHujan = kuat > 0f ? Mathf.Lerp(c.hujanMin, c.hujanMaks, kuat) : 0f;
        Color targetTint = Color.Lerp(Color.white, c.tint, KekuatanHujan);
        float targetTerang = Mathf.Lerp(1f, c.terang, KekuatanHujan);
        float targetKabut = Mathf.Lerp(0.006f, c.kabut, KekuatanHujan);

        // Hari baru: langsung pakai cuaca hari itu. Lompatannya tertutup layar hitam TransisiHari.
        float u = hariBaru ? 1f : kecepatanTransisi * Time.deltaTime;
        if (hariBaru) { KekuatanHujan = kuat; targetTint = Color.Lerp(Color.white, c.tint, kuat); targetTerang = Mathf.Lerp(1f, c.terang, kuat); targetKabut = Mathf.Lerp(0.006f, c.kabut, kuat); hujanSekarang = targetHujan; }
        Tint = Color.Lerp(Tint, targetTint, Mathf.Min(1f, u * 3f));
        Terang = Mathf.Lerp(Terang, targetTerang, Mathf.Min(1f, u * 3f));
        hujanSekarang = Mathf.MoveTowards(hujanSekarang, targetHujan, 600f * u);
        kabutSekarang = Mathf.Lerp(kabutSekarang, targetKabut, Mathf.Min(1f, u * 3f));

        if (hujan != null)
        {
            var em = hujan.emission;
            em.rateOverTime = hujanSekarang;
        }

        float t = siklus != null ? siklus.T : 0f;
        RenderSettings.fogDensity = kabutSekarang;
        RenderSettings.fogColor = Color.Lerp(warnaKabutSiang, warnaKabutMalam, t) * Tint;

        UrusPetir(c.petir && KekuatanHujan > 0.55f);
    }

    // 0..1 dari Perlin noise, dengan bagian "reda" dipotong jadi 0 (kering).
    // Hari tanpa reda (hari 3) tidak pernah kering: paling lemah tetap di hujanMin.
    float Kekuatan(Cuaca c)
    {
        float n = Mathf.PerlinNoise(titikNoise + waktuNoise / Mathf.Max(1f, periodeHujan), titikNoise * 0.37f);
        n = Mathf.Clamp01((n - 0.15f) / 0.7f);          // Perlin jarang menyentuh 0 dan 1, jadi direntangkan
        if (c.reda <= 0f) return Mathf.Max(n, 0.05f);
        return Mathf.Clamp01((n - c.reda) / (1f - c.reda));
    }

    void AcakPolaHari()
    {
        hariNoise = DayManager.Instance != null ? DayManager.Instance.HariIni : 1;
        titikNoise = UnityEngine.Random.Range(0f, 500f);
        waktuNoise = 0f;
    }

    void UrusPetir(bool aktif)
    {
        // Kilat meluruh cepat setelah menyambar (sekitar 0,3 detik).
        Kilat = Mathf.MoveTowards(Kilat, 0f, Time.deltaTime * 3.5f);
        if (!aktif || DayManager.Instance == null || DayManager.Instance.Fase == FaseHari.Selesai) return;

        waktuPetir -= Time.deltaTime;
        if (waktuPetir > 0f) return;

        Kilat = 1f;
        OnPetir?.Invoke();
        JadwalkanPetir();
        // kadang dua kali sambar berdekatan, seperti petir sungguhan
        if (UnityEngine.Random.value < 0.4f) waktuPetir = UnityEngine.Random.Range(0.15f, 0.35f);
    }

    void JadwalkanPetir() => waktuPetir = UnityEngine.Random.Range(jedaPetir.x, jedaPetir.y);

    Cuaca Ambil()
    {
        int hari = DayManager.Instance != null ? DayManager.Instance.HariIni : 1;
        if (cuacaPerHari == null || cuacaPerHari.Length == 0)
            return new Cuaca { nama = "-", tint = Color.white, terang = 1f };
        return cuacaPerHari[Mathf.Clamp(hari - 1, 0, cuacaPerHari.Length - 1)];
    }

    static ParticleSystem CariHujan()
    {
        var kedai = GameObject.Find("Kedai");
        if (kedai == null) return null;
        foreach (var ps in kedai.GetComponentsInChildren<ParticleSystem>(true))
            if (ps.name == "Hujan") return ps;
        return null;
    }
}
