using UnityEngine;

// Etalase kue kecil di counter: alas kayu, kotak kaca, dan beberapa kue bolu di dalamnya.
// Ditaruh di dekat titik pesan supaya pelanggan melihat kue sambil menunggu (seperti kafe sungguhan).
// +z lokal = sisi yang menghadap pelanggan.
public class EtalaseKue : MeshDariKode
{
    public Vector3 ukuran = new Vector3(0.42f, 0.26f, 0.3f);
    public Color warnaKayu = new Color(0.42f, 0.27f, 0.16f);
    public Color warnaKue = new Color(0.72f, 0.48f, 0.26f);
    public Color warnaKrim = new Color(0.97f, 0.92f, 0.85f);

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(4);   // 0 kayu, 1 kue, 2 krim, 3 kaca
        float alas = 0.04f;
        b.Kotak(0, new Vector3(0f, alas * 0.5f, 0f), new Vector3(ukuran.x, alas, ukuran.z));
        b.Kotak(3, new Vector3(0f, alas + (ukuran.y - alas) * 0.5f, 0f), new Vector3(ukuran.x - 0.01f, ukuran.y - alas, ukuran.z - 0.01f));
        b.Kotak(0, new Vector3(0f, ukuran.y + 0.008f, 0f), new Vector3(ukuran.x + 0.01f, 0.016f, ukuran.z + 0.01f)); // tutup kayu

        // dua baris kue bolu: badan (silinder) + krim di atasnya
        for (int baris = 0; baris < 2; baris++)
        for (int i = 0; i < 3; i++)
        {
            var p = new Vector3(-0.13f + i * 0.13f, alas, -0.06f + baris * 0.12f);
            b.Silinder(1, p, 0.045f, 0.04f, 0.05f, 10, false);
            b.Silinder(2, p + Vector3.up * 0.05f, 0.042f, 0.03f, 0.018f, 10, true);
        }
        return b.Jadi("Mesh_EtalaseKue");
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Kayu_Etalase", warnaKayu, null, 0.3f),
        BahanKode.Lit("Kue", warnaKue, null, 0.2f),
        BahanKode.Lit("Krim", warnaKrim, null, 0.4f),
        BahanKode.Transparan("Kaca_Etalase", new Color(0.85f, 0.95f, 1f, 0.18f)),
    };
}
