using UnityEngine;

// Bangku panjang di bawah jendela (tahap 2: "kursi tambahan").
// Dudukan kayu, empat kaki, bantal duduk, dan dua bantal sandaran yang miring menempel ke dinding.
// -z lokal = sisi dinding/jendela, +z lokal = menghadap ke dalam ruangan.
public class BangkuJendela : MeshDariKode
{
    public float panjang = 1.6f, dalam = 0.42f, tinggiDuduk = 0.45f;
    public Color warnaKayu = new Color(0.40f, 0.26f, 0.15f);
    public Color warnaBantal = new Color(0.72f, 0.45f, 0.30f);
    public Color warnaBantal2 = new Color(0.36f, 0.45f, 0.42f);

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(3);   // 0 kayu, 1 bantal hangat, 2 bantal hijau
        float p = panjang * 0.5f, d = dalam * 0.5f, papan = 0.05f;
        b.Kotak(0, new Vector3(0f, tinggiDuduk - papan * 0.5f, 0f), new Vector3(panjang, papan, dalam));
        foreach (float x in new[] { -p + 0.06f, p - 0.06f })
        foreach (float z in new[] { -d + 0.05f, d - 0.05f })
            b.Kotak(0, new Vector3(x, (tinggiDuduk - papan) * 0.5f, z), new Vector3(0.06f, tinggiDuduk - papan, 0.06f));

        b.Kotak(1, new Vector3(0f, tinggiDuduk + 0.04f, 0.01f), new Vector3(panjang - 0.06f, 0.08f, dalam - 0.04f));

        // bantal sandaran: dimiringkan 15 derajat ke arah dinding
        for (int i = 0; i < 2; i++)
        {
            b.Atur(new Vector3(i == 0 ? -0.38f : 0.38f, tinggiDuduk + 0.26f, -d + 0.08f), new Vector3(-15f, 0f, 0f));
            b.Kotak(i == 0 ? 2 : 1, Vector3.zero, new Vector3(0.62f, 0.36f, 0.1f));
            b.Reset();
        }
        return b.Jadi("Mesh_BangkuJendela");
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Kayu_Bangku", warnaKayu, null, 0.3f),
        BahanKode.Lit("Bantal_Hangat", warnaBantal, null, 0.05f),
        BahanKode.Lit("Bantal_Hijau", warnaBantal2, null, 0.05f),
    };
}
