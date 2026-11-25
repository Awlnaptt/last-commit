using UnityEngine;

// Tempat payung di samping pintu, muncul di hari hujan deras.
// Ember silinder + dua payung tertutup yang bersandar miring. Payung tertutup = kerucut panjang
// (silinder dengan jari-jari atas kecil) yang diputar memakai matriks transformasi PembangunMesh.
public class TempatPayung : MeshDariKode
{
    public Color warnaEmber = new Color(0.20f, 0.24f, 0.28f);
    public Color warnaPayung1 = new Color(0.75f, 0.22f, 0.20f);
    public Color warnaPayung2 = new Color(0.18f, 0.32f, 0.55f);
    public Color warnaGagang = new Color(0.25f, 0.17f, 0.10f);

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(4);   // 0 ember, 1 payung merah, 2 payung biru, 3 gagang
        b.Silinder(0, Vector3.zero, 0.13f, 0.15f, 0.42f, 14, true, true);

        Payung(b, 1, new Vector3(-0.03f, 0.05f, 0.02f), new Vector3(0f, 0f, 9f));
        Payung(b, 2, new Vector3(0.04f, 0.05f, -0.03f), new Vector3(-7f, 0f, -6f));
        return b.Jadi("Mesh_TempatPayung");
    }

    // Payung berdiri terbalik: ujung runcing di bawah (dalam ember), gagang di atas.
    static void Payung(PembangunMesh b, int s, Vector3 alas, Vector3 miring)
    {
        b.Atur(alas, miring);
        b.Silinder(s, Vector3.zero, 0.015f, 0.06f, 0.62f, 8, true, true);              // kain terlipat
        b.Kotak(3, new Vector3(0f, 0.72f, 0f), new Vector3(0.022f, 0.2f, 0.022f));      // tangkai
        b.Kotak(3, new Vector3(0.035f, 0.83f, 0f), new Vector3(0.09f, 0.025f, 0.025f)); // pegangan
        b.Reset();
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Ember_Payung", warnaEmber, null, 0.5f),
        BahanKode.Lit("Payung_Merah", warnaPayung1, null, 0.45f),   // basah: agak mengilap
        BahanKode.Lit("Payung_Biru", warnaPayung2, null, 0.45f),
        BahanKode.Lit("Gagang", warnaGagang, null, 0.3f),
    };
}
