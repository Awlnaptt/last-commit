using UnityEngine;

// Vas bunga di meja yang nanti diduduki Ibu (hari 5): kedai "dirapikan" karena Ibu datang.
// Vas = silinder mengerucut, batang = balok tipis miring, bunga = kubus kecil diputar 45 derajat
// (bentuk berlian low-poly).
public class VasBunga : MeshDariKode
{
    public Color warnaVas = new Color(0.85f, 0.82f, 0.74f);
    public Color warnaBatang = new Color(0.30f, 0.50f, 0.28f);
    public Color warnaBunga = new Color(0.95f, 0.62f, 0.55f);
    public Color warnaBunga2 = new Color(0.98f, 0.85f, 0.42f);

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(4);   // 0 vas, 1 batang, 2 bunga, 3 bunga kuning
        b.Silinder(0, Vector3.zero, 0.045f, 0.035f, 0.15f, 12, true, true);

        float[] sudut = { 0f, 72f, 144f, 216f, 288f };
        for (int i = 0; i < sudut.Length; i++)
        {
            float tinggi = 0.13f + 0.03f * (i % 2);
            b.Atur(new Vector3(0f, 0.12f, 0f), new Vector3(14f, sudut[i], 0f));
            b.Kotak(1, new Vector3(0f, tinggi * 0.5f, 0f), new Vector3(0.006f, tinggi, 0.006f));
            b.Atur(b.Matriks.MultiplyPoint3x4(new Vector3(0f, tinggi, 0f)), new Vector3(45f, sudut[i], 45f));
            b.Kotak(i % 2 == 0 ? 2 : 3, Vector3.zero, Vector3.one * 0.035f);
            b.Reset();
        }
        return b.Jadi("Mesh_VasBunga");
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Vas", warnaVas, null, 0.7f),
        BahanKode.Lit("Batang", warnaBatang, null, 0.2f),
        BahanKode.Lit("Bunga_Merah_Muda", warnaBunga, null, 0.2f),
        BahanKode.Lit("Bunga_Kuning", warnaBunga2, null, 0.2f),
    };
}
