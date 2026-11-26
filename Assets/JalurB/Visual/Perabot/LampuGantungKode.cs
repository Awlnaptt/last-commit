using UnityEngine;

// Lampu gantung dari kode (pengganti kotak "Kabel" dan "Kap" sementara).
// Titik asal: titik gantung di plafon. Dari atas ke bawah: dudukan plafon, kabel, kap kerucut,
// lalu kaca peredam di dasar kap yang menyala (emission), jadi dari bawah terlihat bercahaya
// dan ikut berpendar lewat bloom.
public class LampuGantungKode : MeshDariKode
{
    public float panjangKabel = 0.55f;
    public float tinggiKap = 0.22f;
    public float jariBawah = 0.19f, jariAtas = 0.05f;
    public Color warnaLogam = new Color(0.12f, 0.11f, 0.10f);
    public Color warnaKap = new Color(0.20f, 0.33f, 0.30f);
    public Color warnaCahaya = new Color(1f, 0.78f, 0.5f);

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(3);   // 0 logam, 1 kap, 2 kaca menyala
        b.Silinder(0, new Vector3(0f, -0.025f, 0f), 0.06f, 0.06f, 0.025f, 12, false, true);   // dudukan plafon
        b.Kotak(0, new Vector3(0f, -panjangKabel * 0.5f, 0f), new Vector3(0.01f, panjangKabel, 0.01f));
        float yKap = -panjangKabel - tinggiKap;
        b.Silinder(1, new Vector3(0f, yKap, 0f), jariBawah, jariAtas, tinggiKap, 18, true, false);
        b.Silinder(2, new Vector3(0f, yKap + 0.01f, 0f), jariBawah * 0.92f, jariBawah * 0.92f, 0.005f, 18, false, true);
        return b.Jadi("Mesh_LampuGantung");
    }

    protected override Material[] Bahan()
    {
        var kaca = BahanKode.Lit("Kaca_Lampu", warnaCahaya, null, 0.6f);
        BahanKode.Menyala(kaca, null, warnaCahaya * 2.2f);
        return new[] { BahanKode.Lit("Logam_Gantung", warnaLogam, null, 0.5f), BahanKode.Lit("Kap_Gantung", warnaKap, null, 0.35f), kaca };
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
}
