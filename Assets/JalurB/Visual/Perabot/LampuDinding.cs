using UnityEngine;

// Lampu dinding (sconce) dari kode: pelat dinding, lengan, kap kerucut, dan bohlam menyala.
// +z lokal = menjauhi dinding. Lampu titik kecil ditambahkan sebagai anak "Cahaya".
public class LampuDinding : MeshDariKode
{
    public Color warnaLogam = new Color(0.18f, 0.16f, 0.14f);
    public Color warnaKap = new Color(0.92f, 0.80f, 0.60f);
    public Color warnaCahaya = new Color(1f, 0.72f, 0.42f);
    public float terang = 1.2f, jangkauan = 3.2f;

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(3);   // 0 logam, 1 kap, 2 bohlam
        b.Kotak(0, new Vector3(0f, 0f, 0.01f), new Vector3(0.1f, 0.16f, 0.02f));        // pelat dinding
        b.Kotak(0, new Vector3(0f, 0f, 0.09f), new Vector3(0.025f, 0.025f, 0.15f));     // lengan
        b.Silinder(1, new Vector3(0f, -0.06f, 0.18f), 0.09f, 0.045f, 0.13f, 14, false, false); // kap terbuka
        b.Silinder(2, new Vector3(0f, -0.04f, 0.18f), 0.03f, 0.03f, 0.05f, 10, true, true);    // bohlam
        return b.Jadi("Mesh_LampuDinding");
    }

    protected override Material[] Bahan()
    {
        var bohlam = BahanKode.Lit("Bohlam", warnaCahaya, null, 0.5f);
        BahanKode.Menyala(bohlam, null, warnaCahaya * 2.5f);
        return new[] { BahanKode.Lit("Logam_Lampu", warnaLogam, null, 0.5f), BahanKode.Lit("Kap_Lampu", warnaKap, null, 0.2f), bohlam };
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        var t = transform.Find("Cahaya");
        if (t == null)
        {
            t = new GameObject("Cahaya").transform;
            t.SetParent(transform, false);
            t.localPosition = new Vector3(0f, -0.02f, 0.2f);
            var l = t.gameObject.AddComponent<Light>();
            l.type = LightType.Point;
            l.shadows = LightShadows.None;   // hemat untuk laptop
        }
        var cahaya = t.GetComponent<Light>();
        if (cahaya != null) { cahaya.color = warnaCahaya; cahaya.intensity = terang; cahaya.range = jangkauan; }
    }
}
