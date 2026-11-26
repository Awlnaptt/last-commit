using UnityEngine;

// Satu panel datar bergambar (papan kapur, papan neon), menghadap +z lokal (panah biru).
// Kalau 'namaTeksturCahaya' diisi, bagian gambar itu menyala sendiri (emission) dan ikut berpendar
// lewat bloom post-processing. Nyalanya lebih kuat saat malam (mengikuti SiklusCahaya.T).
public class PanelGambar : MeshDariKode
{
    public float lebar = 1.3f;
    public float tinggi = 0.4f;
    public string namaTekstur = "T_Neon";
    public string namaTeksturCahaya = "";
    public float terangSiang = 1.2f, terangMalam = 3f;
    [Range(0f, 1f)] public float halus = 0.2f;

    SiklusCahaya siklus;

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(1);
        float w = lebar * 0.5f, h = tinggi * 0.5f;
        // dilihat dari +z, kiri gambar ada di +x
        b.Quad(0, new Vector3(w, -h, 0f), new Vector3(-w, -h, 0f), new Vector3(-w, h, 0f), new Vector3(w, h, 0f),
               new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
        return b.Jadi("Mesh_Panel_" + namaTekstur);
    }

    protected override Material[] Bahan()
    {
        var m = BahanKode.Lit("Panel_" + namaTekstur, Color.white, BahanKode.Tekstur(namaTekstur), halus);
        if (!string.IsNullOrEmpty(namaTeksturCahaya))
            BahanKode.Menyala(m, BahanKode.Tekstur(namaTeksturCahaya), Color.white * terangSiang);
        return new[] { m };
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    protected override void Update()
    {
        base.Update();
        if (!Application.isPlaying || string.IsNullOrEmpty(namaTeksturCahaya)) return;

        if (siklus == null) siklus = FindAnyObjectByType<SiklusCahaya>();
        float t = siklus != null ? siklus.T : 0f;
        var m = BahanKe(0);
        if (m != null) m.SetColor("_EmissionColor", Color.white * Mathf.Lerp(terangSiang, terangMalam, t));
    }
}
