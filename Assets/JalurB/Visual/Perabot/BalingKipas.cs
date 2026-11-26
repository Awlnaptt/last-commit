using UnityEngine;

// Baling-baling kipas langit-langit (bagian yang BERPUTAR, lewat komponen Berputar di objek yang sama).
// Titik asal = poros putar, jadi baling-baling berputar di tempat tanpa bergeser.
// Tiap daun: balok tipis yang diputar (360 / jumlah) derajat mengelilingi poros
// dan dimiringkan sedikit di sumbu panjangnya, seperti daun kipas sungguhan.
public class BalingKipas : MeshDariKode
{
    [Range(3, 6)] public int jumlahDaun = 4;
    public float panjangDaun = 0.55f;
    public float lebarDaun = 0.13f;
    [Range(0f, 25f)] public float kemiringan = 12f;
    public Color warnaDaun = new Color(0.45f, 0.30f, 0.19f);
    public Color warnaLengan = new Color(0.16f, 0.15f, 0.14f);

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(2);   // 0 daun kayu, 1 lengan logam
        for (int i = 0; i < jumlahDaun; i++)
        {
            float sudut = 360f * i / jumlahDaun;
            b.Atur(Vector3.zero, new Vector3(kemiringan, sudut, 0f));
            b.Kotak(0, new Vector3(0.16f + panjangDaun * 0.5f, 0f, 0f), new Vector3(panjangDaun, 0.012f, lebarDaun));
            b.Atur(Vector3.zero, new Vector3(0f, sudut, 0f));
            b.Kotak(1, new Vector3(0.14f, 0.01f, 0f), new Vector3(0.12f, 0.012f, 0.04f));
        }
        b.Reset();
        return b.Jadi("Mesh_BalingKipas");
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Daun_Kipas", warnaDaun, null, 0.35f),
        BahanKode.Lit("Lengan_Kipas", warnaLengan, null, 0.5f),
    };
}
