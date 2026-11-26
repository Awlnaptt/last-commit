using UnityEngine;

// Pemandangan kota di luar jendela, seluruhnya dibangun dari kode (GDD: "biru dingin di luar jendela").
//
//   - Trotoar dan jalan: dua persegi datar.
//   - Gedung: balok dengan tinggi dan lebar acak, dalam beberapa baris. Baris yang lebih jauh lebih tinggi
//     supaya tetap terlihat di atas baris depan.
//   - Jendela gedung: satu tekstur kecil yang DIULANG. UV dihitung dalam meter
//     (1 ulangan tekstur = 16 m lebar x 24 m tinggi = 8 jendela x 8 lantai), jadi ukuran jendela
//     sama di gedung besar maupun kecil.
//   - Lampu jendela: emission map. Kecerahannya mengikuti waktu (SiklusCahaya.T): redup siang, menyala malam.
//
// Angka acak memakai seed tetap, jadi bentuk kota selalu sama setiap kali dibangun ulang.
public class KotaLuar : MeshDariKode
{
    [Header("Bentuk")]
    public int seed = 12;
    public int jumlahBaris = 3;
    public float zBarisPertama = 17f;
    public float jarakBaris = 10f;
    public float xMin = -26f, xMax = 24f;
    public Vector2 lebarGedung = new Vector2(4f, 9f);
    public Vector2 tinggiGedung = new Vector2(3.5f, 9f);   // baris depan: ruko rendah
    public float tambahTinggiPerBaris = 8f;               // baris belakang: gedung tinggi

    [Header("Tanah (z mulai dari luar dinding utara)")]
    public float zTanah = 4.12f;
    public float lebarTrotoar = 2.5f;
    public float lebarJalan = 7f;

    [Header("Warna")]
    public Color[] warnaGedung =
    {
        new Color(0.55f, 0.60f, 0.70f),
        new Color(0.62f, 0.62f, 0.66f),
        new Color(0.48f, 0.54f, 0.62f),
    };
    public Color warnaTrotoar = new Color(0.42f, 0.42f, 0.44f);
    public Color warnaJalan = new Color(0.16f, 0.17f, 0.19f);
    public Color warnaLampuJendela = new Color(1f, 0.78f, 0.5f);
    public float terangLampuSiang = 0.05f, terangLampuMalam = 1.8f;

    static readonly Vector2 ULANG = new Vector2(16f, 24f);
    SiklusCahaya siklus;

    protected override Mesh Bangun()
    {
        int nGedung = Mathf.Max(1, warnaGedung.Length);
        var b = new PembangunMesh(nGedung + 2); // gedung per warna, lalu trotoar, lalu jalan
        int sTrotoar = nGedung, sJalan = nGedung + 1;

        float lebarTanah = xMax - xMin + 30f;
        float xc = (xMin + xMax) * 0.5f;
        float zJalan = zTanah + lebarTrotoar;
        float zUjung = zBarisPertama + jarakBaris * jumlahBaris + 20f;
        Datar(b, sTrotoar, xc, lebarTanah, zTanah, zJalan, -0.02f);
        Datar(b, sJalan, xc, lebarTanah, zJalan, zJalan + lebarJalan, -0.03f);
        Datar(b, sTrotoar, xc, lebarTanah, zJalan + lebarJalan, zUjung, -0.02f);

        var acak = new System.Random(seed);
        float R(float a, float c) => a + (float)acak.NextDouble() * (c - a);

        for (int baris = 0; baris < jumlahBaris; baris++)
        {
            float z = zBarisPertama + baris * jarakBaris;
            float x = xMin + R(0f, 3f);
            while (x < xMax)
            {
                float lebar = R(lebarGedung.x, lebarGedung.y);
                float tinggi = R(tinggiGedung.x, tinggiGedung.y) + baris * tambahTinggiPerBaris;
                float dalam = R(5f, 8f);
                int warna = acak.Next(nGedung);
                b.Kotak(warna, new Vector3(x + lebar * 0.5f, tinggi * 0.5f, z + dalam * 0.5f),
                        new Vector3(lebar, tinggi, dalam), ULANG, atapPolos: true);
                x += lebar + R(0.6f, 2.5f);
            }
        }
        return b.Jadi("Mesh_KotaLuar");
    }

    // Persegi datar menghadap atas, dari z0 ke z1.
    static void Datar(PembangunMesh b, int s, float xc, float lebar, float z0, float z1, float y)
    {
        float x0 = xc - lebar * 0.5f, x1 = xc + lebar * 0.5f;
        b.Quad(s, new Vector3(x0, y, z0), new Vector3(x1, y, z0), new Vector3(x1, y, z1), new Vector3(x0, y, z1),
               Vector2.zero);
    }

    protected override Material[] Bahan()
    {
        var dasar = BahanKode.Tekstur("T_FasadGedung");
        var lampu = BahanKode.Tekstur("T_FasadGedung_Cahaya");
        int nGedung = Mathf.Max(1, warnaGedung.Length);
        var hasil = new Material[nGedung + 2];
        for (int i = 0; i < nGedung; i++)
        {
            var m = BahanKode.Lit("Gedung_" + i, warnaGedung.Length > 0 ? warnaGedung[i] : Color.gray, dasar, 0.2f);
            m.SetTextureScale("_BaseMap", Vector2.one);
            BahanKode.Menyala(m, lampu, warnaLampuJendela * terangLampuMalam);
            hasil[i] = m;
        }
        hasil[nGedung] = BahanKode.Lit("Trotoar", warnaTrotoar, null, 0.15f);
        hasil[nGedung + 1] = BahanKode.Lit("Jalan_Basah", warnaJalan, null, 0.75f); // basah = mengilap
        return hasil;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        var r = GetComponent<MeshRenderer>();
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // hemat untuk laptop Iris Xe
        r.receiveShadows = false;
    }

    protected override void Update()
    {
        base.Update();
        if (!Application.isPlaying) return;

        if (siklus == null) siklus = FindAnyObjectByType<SiklusCahaya>();
        float t = siklus != null ? siklus.T : 1f;
        // lampu mulai menyala sore (t 0,6) dan penuh saat malam; hujan tebal membuat lampu menyala lebih awal
        float nyala = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 1f, t));
        nyala = Mathf.Max(nyala, 1f - CuacaHarian.Terang);
        Color c = warnaLampuJendela * Mathf.Lerp(terangLampuSiang, terangLampuMalam, nyala);

        int nGedung = Mathf.Max(1, warnaGedung.Length);
        for (int i = 0; i < nGedung; i++)
        {
            var m = BahanKe(i);
            if (m != null) m.SetColor("_EmissionColor", c);
        }
    }
}
