using UnityEngine;

// Selembar kertas catatan di atas meja, dibangun dari kode sebagai grid titik.
// Ujung kiri dan kanannya sedikit terangkat (melengkung) supaya tidak terlihat seperti stiker datar:
//   y = lengkung * (x / setengahLebar)^2   -> parabola, tengah menempel meja, tepi naik.
// Normal tiap titik dihitung dari turunan parabola itu, jadi cahaya jatuh halus mengikuti lengkungan.
public class KertasCatatan : MeshDariKode
{
    public float lebar = 0.15f;
    public float panjang = 0.1875f;      // rasio gambar 512 x 640
    public float lengkung = 0.008f;
    [Range(2, 24)] public int segmen = 10;
    public string namaTekstur = "T_CatatanIbu";

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(1);
        int nx = segmen, nz = 2;
        float hw = lebar * 0.5f, hp = panjang * 0.5f;
        var idx = new int[nx + 1, nz + 1];

        for (int i = 0; i <= nx; i++)
        for (int j = 0; j <= nz; j++)
        {
            float u = (float)i / nx, v = (float)j / nz;
            float x = Mathf.Lerp(-hw, hw, u);
            float z = Mathf.Lerp(-hp, hp, v);
            float r = x / hw;
            float y = lengkung * r * r + 0.001f;

            // turunan dy/dx = 2 * lengkung * x / hw^2 ; normal = (-dy/dx, 1, 0) dinormalkan
            float kemiringan = 2f * lengkung * x / (hw * hw);
            Vector3 normal = new Vector3(-kemiringan, 1f, 0f).normalized;
            idx[i, j] = b.Titik(new Vector3(x, y, z), normal, new Vector2(u, v));
        }

        // dilihat dari atas: a kiri bawah, b kanan bawah, c kanan atas, d kiri atas -> (a, d, c) dan (a, c, b)
        for (int i = 0; i < nx; i++)
        for (int j = 0; j < nz; j++)
        {
            int a = idx[i, j], bb = idx[i + 1, j], c = idx[i + 1, j + 1], d = idx[i, j + 1];
            b.Segitiga(0, a, d, c);
            b.Segitiga(0, a, c, bb);
        }
        return b.Jadi("Mesh_KertasCatatan");
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Kertas_Catatan", Color.white, BahanKode.Tekstur(namaTekstur), 0.1f),
    };
}
