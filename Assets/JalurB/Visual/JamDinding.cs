using UnityEngine;

// Jam dinding yang dibangun dari kode dan jarumnya mengikuti waktu permainan.
//
// Lingkaran dibuat dari segitiga-segitiga kipas (triangle fan): titik ke-i di tepi lingkaran
//   p_i = (r * cos(sudut_i), r * sin(sudut_i)),  sudut_i = i / segmen * 360 derajat
// lalu tiap segitiga = (pusat, p_i, p_i+1). Makin banyak segmen, makin bulat.
//
// Jam permainan: pagi 07.00, shift berjalan sampai sekitar 20.00, malam 21.00
//   jam = 7 + 14 * SiklusCahaya.T
// Jarum diputar mengelilingi sumbu z lokal. Jam menghadap +z, jadi dari depan sumbu x terlihat terbalik
// (kanan pemain = -x); karena itu sudut POSITIF terlihat searah jarum jam.
public class JamDinding : MeshDariKode
{
    public float jariJari = 0.17f;
    public float lebarBingkai = 0.02f;
    public float tebal = 0.04f;
    [Range(12, 96)] public int segmen = 48;
    public Color warnaMuka = new Color(0.95f, 0.92f, 0.84f);
    public Color warnaBingkai = new Color(0.25f, 0.17f, 0.11f);
    public float jamPagi = 7f, jamMalam = 21f;

    Transform jarumJam, jarumMenit;
    SiklusCahaya siklus;

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(2); // 0 = muka jam, 1 = bingkai + penanda + jarum
        float zMuka = tebal, r = jariJari, R = jariJari + lebarBingkai;

        // muka jam (triangle fan)
        int pusat = b.Titik(new Vector3(0f, 0f, zMuka), Vector3.forward, new Vector2(0.5f, 0.5f));
        int pertama = -1, sebelum = -1;
        for (int i = 0; i <= segmen; i++)
        {
            float a = i / (float)segmen * Mathf.PI * 2f;
            var p = new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, zMuka);
            int idx = i < segmen ? b.Titik(p, Vector3.forward, new Vector2(0.5f + p.x / (2 * r), 0.5f + p.y / (2 * r))) : pertama;
            if (i == 0) pertama = idx;
            else b.Segitiga(0, pusat, sebelum, idx);
            sebelum = idx;
        }

        // cincin bingkai di depan + dinding samping silinder
        for (int i = 0; i < segmen; i++)
        {
            float a0 = i / (float)segmen * Mathf.PI * 2f, a1 = (i + 1) / (float)segmen * Mathf.PI * 2f;
            Vector3 d0 = new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0f), d1 = new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f);
            Vector3 z1 = Vector3.forward * (zMuka + 0.006f);
            b.Quad(1, d1 * r + z1, d1 * R + z1, d0 * R + z1, d0 * r + z1, Vector2.zero);       // menghadap +z
            b.Quad(1, d1 * R, d0 * R, d0 * R + z1, d1 * R + z1, Vector2.zero);                 // menghadap keluar
        }

        // 12 penanda jam; jam 12, 3, 6, 9 lebih besar
        for (int i = 0; i < 12; i++)
        {
            float a = i / 12f * Mathf.PI * 2f;
            bool besar = i % 3 == 0;
            float panjang = besar ? 0.03f : 0.015f;
            Vector3 arah = new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0f);
            Vector3 pos = arah * (r - 0.012f - panjang * 0.5f) + Vector3.forward * (zMuka + 0.002f);
            float lebar = besar ? 0.008f : 0.004f;
            // kotak kecil diputar searah jari-jari: dibuat lewat 4 titik manual
            Vector3 samping = new Vector3(-arah.y, arah.x, 0f) * lebar * 0.5f, ke = arah * panjang * 0.5f;
            b.Quad(1, pos - ke - samping, pos - ke + samping, pos + ke + samping, pos + ke - samping, Vector2.zero);
        }
        return b.Jadi("Mesh_JamDinding");
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Muka_Jam", warnaMuka, null, 0.5f),
        BahanKode.Lit("Bingkai_Jam", warnaBingkai, null, 0.35f),
    };

    protected override void Update()
    {
        base.Update();
        if (!Application.isPlaying) return;

        if (jarumJam == null)
        {
            jarumJam = BuatJarum("JarumJam", 0.09f, 0.012f, tebal + 0.01f);
            jarumMenit = BuatJarum("JarumMenit", 0.135f, 0.007f, tebal + 0.016f);
        }

        if (siklus == null) siklus = FindAnyObjectByType<SiklusCahaya>();
        float jam = Mathf.Lerp(jamPagi, jamMalam, siklus != null ? siklus.T : 0f);
        jarumJam.localRotation = Quaternion.Euler(0f, 0f, (jam % 12f) / 12f * 360f);
        jarumMenit.localRotation = Quaternion.Euler(0f, 0f, (jam % 1f) * 360f);
    }

    Transform BuatJarum(string nama, float panjang, float lebar, float z)
    {
        var go = new GameObject(nama, typeof(MeshFilter), typeof(MeshRenderer));
        go.hideFlags = HideFlags.DontSave;
        go.transform.SetParent(transform, false);
        var b = new PembangunMesh(1);
        b.Kotak(0, new Vector3(0f, panjang * 0.5f - 0.015f, z), new Vector3(lebar, panjang, 0.004f));
        go.GetComponent<MeshFilter>().sharedMesh = b.Jadi("Mesh_" + nama);
        go.GetComponent<MeshRenderer>().sharedMaterial = BahanKode.Lit("Jarum", warnaBingkai, null, 0.4f);
        return go.transform;
    }
}
