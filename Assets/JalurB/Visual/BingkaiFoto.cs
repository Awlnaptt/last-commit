using UnityEngine;

// Bingkai foto wisuda yang dibangun dari kode: 4 balok kayu + papan belakang + 1 persegi foto.
// Sumbu lokal: belakang bingkai menempel ke dinding di z = 0, foto menghadap +z (arah biru gizmo).
// Taruh objeknya di permukaan dinding lalu putar supaya panah biru menunjuk ke dalam ruangan.
public class BingkaiFoto : MeshDariKode
{
    [Header("Ukuran (meter)")]
    public float lebarFoto = 0.32f;
    public float tinggiFoto = 0.40f;   // rasio gambar 512 x 640
    public float lebarKayu = 0.035f;
    public float tebal = 0.03f;

    [Header("Tampilan")]
    public string namaTekstur = "T_FotoWisuda";
    public Color warnaKayu = new Color(0.36f, 0.22f, 0.13f);

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(2);   // submesh 0 = kayu, 1 = foto
        float w = lebarFoto * 0.5f, h = tinggiFoto * 0.5f, k = lebarKayu, d = tebal;

        // papan belakang tipis
        b.Kotak(0, new Vector3(0f, 0f, d * 0.15f), new Vector3(lebarFoto, tinggiFoto, d * 0.3f));
        // empat sisi kayu
        b.Kotak(0, new Vector3(0f,  h + k * 0.5f, d * 0.5f), new Vector3(lebarFoto + 2 * k, k, d));
        b.Kotak(0, new Vector3(0f, -h - k * 0.5f, d * 0.5f), new Vector3(lebarFoto + 2 * k, k, d));
        b.Kotak(0, new Vector3(-w - k * 0.5f, 0f, d * 0.5f), new Vector3(k, tinggiFoto, d));
        b.Kotak(0, new Vector3( w + k * 0.5f, 0f, d * 0.5f), new Vector3(k, tinggiFoto, d));

        // foto: dilihat dari +z, kiri gambar ada di +x
        float z = d * 0.35f;
        b.Quad(1,
            new Vector3( w, -h, z), new Vector3(-w, -h, z), new Vector3(-w, h, z), new Vector3( w, h, z),
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
        return b.Jadi("Mesh_BingkaiFoto");
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Kayu_Bingkai", warnaKayu, null, 0.25f),
        BahanKode.Lit("Foto_Wisuda", Color.white, BahanKode.Tekstur(namaTekstur), 0.65f), // kertas foto mengilap
    };
}
