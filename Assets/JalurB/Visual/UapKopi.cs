using UnityEngine;

// Uap dari mesin kopi, dibuat seluruhnya dari kode (tanpa file tekstur atau prefab).
//
//   - Tekstur asap: lingkaran lembut 64 x 64 yang dihitung per piksel,
//       alpha = (1 - jarak dari tengah)^2  -> tengah pekat, tepi memudar ke 0.
//   - Partikel naik pelan, membesar, dan memudar sepanjang umurnya (Size / Color over Lifetime).
//   - Saat pemain meracik (OrderManager.SedangMeracik), uap jauh lebih banyak.
// Dipasang otomatis di objek MesinKopi oleh PasangOtomatis.
public class UapKopi : MonoBehaviour
{
    public float uapDiam = 2f;       // partikel per detik saat mesin hanya hangat
    public float uapMeracik = 16f;   // partikel per detik saat meracik
    public Vector3 geser = new Vector3(0f, 0.02f, 0f);

    ParticleSystem ps;
    static Texture2D teksturAsap;

    void Start()
    {
        var go = new GameObject("Uap");
        go.transform.SetParent(transform, false);
        go.transform.position = TitikAtas() + geser;
        go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // memancar ke atas

        ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.duration = 3f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.11f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new Color(1f, 1f, 1f, 0.35f);
        main.gravityModifier = -0.02f;
        main.maxParticles = 120;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission;
        em.rateOverTime = uapDiam;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 8f;
        shape.radius = 0.03f;

        var ukuran = ps.sizeOverLifetime;
        ukuran.enabled = true;
        ukuran.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 2.4f));

        var warna = ps.colorOverLifetime;
        warna.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
        warna.color = g;

        var putar = ps.rotationOverLifetime;
        putar.enabled = true;
        putar.z = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.08f;
        noise.frequency = 0.8f;

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        r.sharedMaterial = BahanAsap();

        ps.Play();
    }

    void Update()
    {
        if (ps == null) return;
        bool racik = OrderManager.Instance != null && OrderManager.Instance.SedangMeracik;
        var em = ps.emission;
        float target = racik ? uapMeracik : uapDiam;
        em.rateOverTime = Mathf.MoveTowards(em.rateOverTime.constant, target, 40f * Time.deltaTime);
    }

    // Titik tengah-atas mesin, dari bentuk yang terlihat (renderer), bukan pivot.
    Vector3 TitikAtas()
    {
        var rs = GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return transform.position;
        Bounds b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return new Vector3(b.center.x, b.max.y, b.center.z);
    }

    static Material BahanAsap()
    {
        var sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (sh == null) sh = Shader.Find("Sprites/Default");
        var m = new Material(sh) { name = "M_Uap" };
        if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);   // transparan
        if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);       // alpha blend
        m.SetOverrideTag("RenderType", "Transparent");
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        var tex = TeksturAsap();
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
        m.mainTexture = tex;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", new Color(0.95f, 0.95f, 0.95f, 1f));
        return m;
    }

    static Texture2D TeksturAsap()
    {
        if (teksturAsap != null) return teksturAsap;
        const int N = 64;
        teksturAsap = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "T_Asap" };
        var px = new Color32[N * N];
        for (int y = 0; y < N; y++)
        for (int x = 0; x < N; x++)
        {
            float dx = (x + 0.5f) / N * 2f - 1f, dy = (y + 0.5f) / N * 2f - 1f;
            float jarak = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Clamp01(1f - jarak);
            px[y * N + x] = new Color32(255, 255, 255, (byte)(a * a * 255f));
        }
        teksturAsap.SetPixels32(px);
        teksturAsap.Apply();
        return teksturAsap;
    }
}
