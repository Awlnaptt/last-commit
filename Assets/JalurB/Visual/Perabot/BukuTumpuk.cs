using UnityEngine;

// Tumpukan beberapa buku tidur, tiap buku sedikit diputar supaya tidak terlihat seperti satu balok.
// Titik asal: tengah-bawah tumpukan.
public class BukuTumpuk : MeshDariKode
{
    public int jumlah = 4;
    public int seed = 3;
    public Color[] warnaSampul =
    {
        new Color(0.17f, 0.30f, 0.45f), new Color(0.80f, 0.66f, 0.38f), new Color(0.55f, 0.18f, 0.16f), new Color(0.90f, 0.87f, 0.80f),
    };

    protected override Mesh Bangun()
    {
        int nWarna = Mathf.Max(1, warnaSampul.Length);
        var b = new PembangunMesh(nWarna);
        var acak = new System.Random(seed);
        float R(float a, float c) => a + (float)acak.NextDouble() * (c - a);

        float y = 0f;
        for (int i = 0; i < jumlah; i++)
        {
            float tebal = R(0.03f, 0.05f), panjang = R(0.2f, 0.26f), dlm = R(0.15f, 0.19f);
            b.Atur(new Vector3(R(-0.015f, 0.015f), y, R(-0.015f, 0.015f)), new Vector3(0f, R(-12f, 12f), 0f));
            b.Kotak(i % nWarna, new Vector3(0f, tebal * 0.5f, 0f), new Vector3(panjang, tebal, dlm));
            b.Reset();
            y += tebal;
        }
        return b.Jadi("Mesh_BukuTumpuk");
    }

    protected override Material[] Bahan()
    {
        int nWarna = Mathf.Max(1, warnaSampul.Length);
        var m = new Material[nWarna];
        for (int i = 0; i < nWarna; i++) m[i] = BahanKode.Lit("Sampul_Tumpuk_" + i, warnaSampul.Length > 0 ? warnaSampul[i] : Color.gray, null, 0.3f);
        return m;
    }
}
