using UnityEngine;

// Rak dinding dua tingkat berisi toples biji kopi, di dinding belakang counter.
// Toples = silinder kaca bening berisi silinder cokelat (biji kopi) dan tutup gelap.
// Tinggi isi tiap toples sengaja dibuat beda supaya terlihat "dipakai".
// +z lokal = menjauhi dinding.
public class RakDinding : MeshDariKode
{
    public float lebar = 1.0f, dalam = 0.22f;
    public float[] tinggiRak = { 0f, 0.42f };
    public Color warnaKayu = new Color(0.45f, 0.30f, 0.18f);
    public Color warnaBiji = new Color(0.30f, 0.17f, 0.09f);
    public Color warnaTutup = new Color(0.15f, 0.13f, 0.12f);

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(4);   // 0 kayu, 1 biji, 2 tutup, 3 kaca
        float[] isi = { 0.6f, 0.35f, 0.8f, 0.5f, 0.7f, 0.25f };
        int k = 0;
        foreach (float y in tinggiRak)
        {
            b.Kotak(0, new Vector3(0f, y, dalam * 0.5f), new Vector3(lebar, 0.03f, dalam));              // papan
            b.Kotak(0, new Vector3(-lebar * 0.38f, y - 0.07f, 0.06f), new Vector3(0.03f, 0.12f, 0.12f)); // siku
            b.Kotak(0, new Vector3( lebar * 0.38f, y - 0.07f, 0.06f), new Vector3(0.03f, 0.12f, 0.12f));

            for (int i = 0; i < 3; i++, k++)
            {
                var alas = new Vector3(-lebar * 0.3f + i * lebar * 0.3f, y + 0.015f, dalam * 0.5f);
                float t = 0.2f;
                b.Silinder(3, alas, 0.06f, 0.06f, t, 12, false);                                    // kaca
                b.Silinder(1, alas + Vector3.up * 0.002f, 0.055f, 0.055f, t * isi[k % isi.Length], 12, true); // biji kopi
                b.Silinder(2, alas + Vector3.up * t, 0.064f, 0.064f, 0.03f, 12, true);             // tutup
            }
        }
        return b.Jadi("Mesh_RakDinding");
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Kayu_Rak", warnaKayu, null, 0.25f),
        BahanKode.Lit("Biji_Kopi", warnaBiji, null, 0.35f),
        BahanKode.Lit("Tutup_Toples", warnaTutup, null, 0.4f),
        BahanKode.Transparan("Kaca_Toples", new Color(0.9f, 0.95f, 1f, 0.22f)),
    };
}
