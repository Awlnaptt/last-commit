using UnityEngine;

// Kusen pintu dari kode. Menutup celah antara daun pintu dan lubang di dinding,
// lalu memberi "teras" gelap di luar supaya kalau ada celah tersisa yang terlihat adalah luar ruangan,
// bukan ruang kosong.
//
// Titik asal objek: tengah-bawah lubang pintu, di tengah tebal dinding. +z lokal = ke dalam ruangan.
// Lubang pintu kedai: lebar 1,4 m (x -4,2..-2,8), tinggi 2,1 m, dinding tebal 0,1 m.
public class KusenPintu : MeshDariKode
{
    public float lebarLubang = 1.4f;
    public float tinggiLubang = 2.1f;
    [Tooltip("Seberapa jauh kusen masuk ke lubang (menutup celah di tepi daun pintu)")]
    public float masuk = 0.07f;
    [Tooltip("Seberapa jauh kusen menutup dinding di sampingnya")]
    public float keluar = 0.08f;
    public float dalam = 0.18f;   // lebih tebal dari dinding (0,1) supaya menonjol di dua sisi
    public Color warnaKusen = new Color(0.30f, 0.20f, 0.13f);
    public Color warnaLuar = new Color(0.16f, 0.18f, 0.22f);

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(2);   // 0 = kusen, 1 = teras luar
        float w = lebarLubang * 0.5f, lebarTiang = masuk + keluar;
        float xTiang = w + (keluar - masuk) * 0.5f;

        b.Kotak(0, new Vector3(-xTiang, (tinggiLubang + keluar) * 0.5f, 0f), new Vector3(lebarTiang, tinggiLubang + keluar, dalam));
        b.Kotak(0, new Vector3( xTiang, (tinggiLubang + keluar) * 0.5f, 0f), new Vector3(lebarTiang, tinggiLubang + keluar, dalam));
        b.Kotak(0, new Vector3(0f, tinggiLubang + (keluar - masuk) * 0.5f, 0f), new Vector3(lebarLubang + 2 * keluar, lebarTiang, dalam));
        b.Kotak(0, new Vector3(0f, 0.012f, 0f), new Vector3(lebarLubang, 0.024f, dalam + 0.04f));   // ambang bawah

        // teras luar: lantai + dinding gelap 0,6 m di balik pintu
        b.Kotak(1, new Vector3(0f, -0.03f, -0.4f), new Vector3(lebarLubang + 1.2f, 0.06f, 0.7f));
        b.Kotak(1, new Vector3(0f, (tinggiLubang + 0.6f) * 0.5f, -0.75f), new Vector3(lebarLubang + 1.2f, tinggiLubang + 0.6f, 0.05f));
        return b.Jadi("Mesh_KusenPintu");
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Kusen", warnaKusen, null, 0.3f),
        BahanKode.Lit("Teras_Luar", warnaLuar, null, 0.6f),
    };
}
