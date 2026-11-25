using System.Collections.Generic;
using UnityEngine;

// Pembuat mesh dari kode (bahan penjelasan Grafikom).
//
// Mesh di Unity hanyalah empat daftar:
//   vertices  : posisi titik sudut (x, y, z)
//   normals   : arah hadap permukaan di tiap titik, dipakai untuk menghitung cahaya
//   uv        : koordinat tekstur (u, v) 0..1, menentukan bagian gambar mana yang menempel
//   triangles : tiap 3 angka = satu segitiga, berisi nomor urut titik
//
// Unity menggambar sisi DEPAN segitiga yang urutan titiknya searah jarum jam bila dilihat dari depan.
// Karena itu satu persegi (quad) a-b-c-d (kiri bawah, kanan bawah, kanan atas, kiri atas)
// dipecah jadi segitiga (a, d, c) dan (a, c, b).
// Normal dihitung dengan perkalian silang: n = (d - a) x (b - a).
//
// Tiap sisi kotak punya titik sendiri (tidak dibagi dengan sisi tetangga) supaya normalnya tegas
// dan kotak terlihat bersudut (flat shading, khas low-poly).
public class PembangunMesh
{
    readonly List<Vector3> v = new List<Vector3>();
    readonly List<Vector3> n = new List<Vector3>();
    readonly List<Vector2> uv = new List<Vector2>();
    readonly List<int>[] sub;

    // Transformasi untuk bagian berikutnya (geser + putar). Dipakai untuk bagian miring,
    // misalnya gagang payung. Titik dikali matriks, normal hanya diputar.
    public Matrix4x4 Matriks = Matrix4x4.identity;
    public void Atur(Vector3 posisi, Vector3 rotasiEuler) => Matriks = Matrix4x4.TRS(posisi, Quaternion.Euler(rotasiEuler), Vector3.one);
    public void Reset() => Matriks = Matrix4x4.identity;

    public PembangunMesh(int jumlahSubmesh = 1)
    {
        sub = new List<int>[jumlahSubmesh];
        for (int i = 0; i < jumlahSubmesh; i++) sub[i] = new List<int>();
    }

    // Satu persegi. uvA..uvD mengikuti titik a..d.
    public void Quad(int s, Vector3 a, Vector3 b, Vector3 c, Vector3 d,
                     Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD)
    {
        a = Matriks.MultiplyPoint3x4(a); b = Matriks.MultiplyPoint3x4(b);
        c = Matriks.MultiplyPoint3x4(c); d = Matriks.MultiplyPoint3x4(d);
        Vector3 normal = Vector3.Cross(d - a, b - a).normalized;
        int i = v.Count;
        v.Add(a); v.Add(b); v.Add(c); v.Add(d);
        for (int k = 0; k < 4; k++) n.Add(normal);
        uv.Add(uvA); uv.Add(uvB); uv.Add(uvC); uv.Add(uvD);
        sub[s].AddRange(new[] { i, i + 3, i + 2, i, i + 2, i + 1 });
    }

    // Persegi dengan UV penuh 0..1, atau UV dalam meter dibagi skalaUV (untuk tekstur berulang).
    public void Quad(int s, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 skalaUV)
    {
        if (skalaUV.x <= 0f || skalaUV.y <= 0f)
        {
            Quad(s, a, b, c, d, Vector2.zero, Vector2.right, Vector2.one, Vector2.up);
            return;
        }
        float u = Vector3.Distance(a, b) / skalaUV.x;
        float w = Vector3.Distance(a, d) / skalaUV.y;
        Quad(s, a, b, c, d, Vector2.zero, new Vector2(u, 0f), new Vector2(u, w), new Vector2(0f, w));
    }

    // Balok dengan 6 sisi. Tiap sisi ditulis "dilihat dari luar": kiri bawah, kanan bawah, kanan atas, kiri atas.
    // atapPolos: sisi atas dan bawah memakai satu titik UV saja (tidak ikut motif jendela).
    public void Kotak(int s, Vector3 pusat, Vector3 ukuran, Vector2 skalaUV = default, bool atapPolos = false)
    {
        Vector3 h = ukuran * 0.5f;
        Vector3 P(float x, float y, float z) => pusat + new Vector3(x * h.x, y * h.y, z * h.z);

        Quad(s, P(-1, -1, -1), P(1, -1, -1), P(1, 1, -1), P(-1, 1, -1), skalaUV);   // depan  (-z)
        Quad(s, P(1, -1, 1), P(-1, -1, 1), P(-1, 1, 1), P(1, 1, 1), skalaUV);       // belakang (+z)
        Quad(s, P(1, -1, -1), P(1, -1, 1), P(1, 1, 1), P(1, 1, -1), skalaUV);       // kanan  (+x)
        Quad(s, P(-1, -1, 1), P(-1, -1, -1), P(-1, 1, -1), P(-1, 1, 1), skalaUV);   // kiri   (-x)

        if (atapPolos)
        {
            Vector2 t = new Vector2(0.01f, 0.01f);
            Quad(s, P(-1, 1, -1), P(1, 1, -1), P(1, 1, 1), P(-1, 1, 1), t, t, t, t);
            Quad(s, P(-1, -1, 1), P(1, -1, 1), P(1, -1, -1), P(-1, -1, -1), t, t, t, t);
        }
        else
        {
            Quad(s, P(-1, 1, -1), P(1, 1, -1), P(1, 1, 1), P(-1, 1, 1), skalaUV);   // atas   (+y)
            Quad(s, P(-1, -1, 1), P(1, -1, 1), P(1, -1, -1), P(-1, -1, -1), skalaUV); // bawah (-y)
        }
    }

    // Titik dan segitiga mentah, untuk bentuk lengkung (misalnya kertas).
    public int Titik(Vector3 posisi, Vector3 normal, Vector2 koordinatUV)
    {
        v.Add(Matriks.MultiplyPoint3x4(posisi)); n.Add(Matriks.MultiplyVector(normal).normalized); uv.Add(koordinatUV);
        return v.Count - 1;
    }

    public void Segitiga(int s, int a, int b, int c) { sub[s].Add(a); sub[s].Add(b); sub[s].Add(c); }

    // Silinder tegak (sumbu y), boleh mengerucut: jari-jari bawah r0, atas r1.
    // Titik tepi lingkaran: (r cos a, y, r sin a). Sisi = persegi per segmen (flat shading),
    // tutup = triangle fan. Dipakai untuk toples, vas, kap lampu, ember payung.
    public void Silinder(int s, Vector3 alas, float r0, float r1, float tinggi, int segmen = 12,
                         bool tutupAtas = true, bool tutupBawah = false, int sTutup = -1)
    {
        if (sTutup < 0) sTutup = s;
        Vector3 P(float r, float a, float y) => alas + new Vector3(r * Mathf.Cos(a), y, r * Mathf.Sin(a));
        for (int i = 0; i < segmen; i++)
        {
            float a0 = i * Mathf.PI * 2f / segmen, a1 = (i + 1) * Mathf.PI * 2f / segmen;
            Quad(s, P(r0, a0, 0f), P(r0, a1, 0f), P(r1, a1, tinggi), P(r1, a0, tinggi),
                 new Vector2(i / (float)segmen, 0f), new Vector2((i + 1) / (float)segmen, 0f),
                 new Vector2((i + 1) / (float)segmen, 1f), new Vector2(i / (float)segmen, 1f));
        }
        if (tutupAtas) Tutup(sTutup, alas + Vector3.up * tinggi, r1, segmen, true);
        if (tutupBawah) Tutup(sTutup, alas, r0, segmen, false);
    }

    void Tutup(int s, Vector3 pusat, float r, int segmen, bool keAtas)
    {
        Vector3 nn = keAtas ? Vector3.up : Vector3.down;
        int c = Titik(pusat, nn, new Vector2(0.5f, 0.5f));
        int awal = v.Count;
        for (int i = 0; i < segmen; i++)
        {
            float a = i * Mathf.PI * 2f / segmen;
            Titik(pusat + new Vector3(r * Mathf.Cos(a), 0f, r * Mathf.Sin(a)), nn,
                  new Vector2(0.5f + 0.5f * Mathf.Cos(a), 0.5f + 0.5f * Mathf.Sin(a)));
        }
        for (int i = 0; i < segmen; i++)
        {
            int p0 = awal + i, p1 = awal + (i + 1) % segmen;
            if (keAtas) Segitiga(s, c, p1, p0); else Segitiga(s, c, p0, p1);
        }
    }

    public Mesh Jadi(string nama)
    {
        var m = new Mesh { name = nama, hideFlags = HideFlags.DontSave };
        if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.SetVertices(v);
        m.SetNormals(n);
        m.SetUVs(0, uv);
        m.subMeshCount = sub.Length;
        for (int i = 0; i < sub.Length; i++) m.SetTriangles(sub[i], i);
        m.RecalculateBounds();
        m.RecalculateTangents();
        return m;
    }
}

// Material URP yang dibuat dari kode, supaya tidak perlu membuat file .mat satu per satu.
public static class BahanKode
{
    static Shader lit;

    public static Material Lit(string nama, Color warna, Texture tekstur = null, float halus = 0.3f)
    {
        if (lit == null) lit = Shader.Find("Universal Render Pipeline/Lit");
        var m = new Material(lit != null ? lit : Shader.Find("Standard")) { name = nama, hideFlags = HideFlags.DontSave };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", warna);
        m.color = warna;
        if (tekstur != null)
        {
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tekstur);
            m.mainTexture = tekstur;
        }
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", halus);
        return m;
    }

    // Material tembus pandang (kaca etalase, toples). Alpha warna = tingkat bening.
    public static Material Transparan(string nama, Color warna, float halus = 0.9f)
    {
        var m = Lit(nama, warna, null, halus);
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.SetOverrideTag("RenderType", "Transparent");
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return m;
    }

    // Emission: bagian tekstur yang menyala sendiri (lampu jendela, layar). Kecerahannya bisa diubah tiap frame.
    public static void Menyala(Material m, Texture peta, Color warna)
    {
        m.EnableKeyword("_EMISSION");
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        if (peta != null) m.SetTexture("_EmissionMap", peta);
        m.SetColor("_EmissionColor", warna);
    }

    // Tekstur di Assets/JalurB/Visual/Resources/JalurB, bisa dimuat saat main maupun di build.
    public static Texture2D Tekstur(string nama) => Resources.Load<Texture2D>("JalurB/" + nama);

    public static Sprite Gambar(string nama)
    {
        var t = Tekstur(nama);
        return t == null ? null : Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
    }
}

// Dasar komponen yang membangun mesh-nya sendiri. Ubah angka di Inspector, bentuk langsung berubah
// (juga di Edit Mode). Mesh tidak disimpan sebagai file: kodenya adalah asetnya.
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public abstract class MeshDariKode : MonoBehaviour
{
    Mesh mesh;
    Material[] bahan;
    bool perluBangun;

    protected abstract Mesh Bangun();
    protected abstract Material[] Bahan();

    protected virtual void OnEnable() => BangunUlang();
    void OnValidate() => perluBangun = true;

    protected virtual void Update()
    {
        if (!perluBangun) return;
        perluBangun = false;
        BangunUlang();
    }

    public void BangunUlang()
    {
        BersihkanLama();
        mesh = Bangun();
        bahan = Bahan();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterials = bahan;

        // Collider kotak seukuran mesh, supaya bisa kena raycast tombol E.
        var box = GetComponent<BoxCollider>();
        if (box != null) { box.center = mesh.bounds.center; box.size = mesh.bounds.size + Vector3.one * 0.02f; }
    }

    protected virtual void OnDestroy() => BersihkanLama();

    // Material pertama (misalnya untuk mengubah kecerahan lampu jendela saat main).
    protected Material BahanKe(int i) => bahan != null && i < bahan.Length ? bahan[i] : null;

    void BersihkanLama()
    {
        if (mesh != null) Hancurkan(mesh);
        if (bahan != null) foreach (var m in bahan) if (m != null) Hancurkan(m);
        mesh = null; bahan = null;
    }

    protected static void Hancurkan(Object o)
    {
        if (Application.isPlaying) Destroy(o); else DestroyImmediate(o);
    }
}
