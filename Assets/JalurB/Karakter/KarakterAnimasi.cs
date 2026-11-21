using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

// Memutar satu klip animasi pada model karakter tanpa perlu Animator Controller.
// Klip diambil dari FBX model itu sendiri, jadi aman untuk rig Generic maupun Humanoid.
public class KarakterAnimasi : MonoBehaviour
{
    public ModelKarakter Karakter { get; private set; }

    Animator animator;
    PlayableGraph graph;
    AnimationPlayableOutput output;
    AnimationClipPlayable playable;
    AnimationClip sekarang;
    bool ulang;

    // Membuat model di posisi tertentu, telapak kaki didudukkan di posisi.y.
    public static KarakterAnimasi Buat(ModelKarakter k, Transform induk, Vector3 posisi, Quaternion rotasi)
    {
        var go = Instantiate(k.model, posisi, rotasi, induk);
        go.name = "Model_" + k.name;

        // Skalakan ke tinggi yang diminta di aset karakter (0 = ukuran asli FBX).
        if (k.tinggi > 0f && Ukur(go, out Bounds awal) && awal.size.y > 0.01f)
            go.transform.localScale *= k.tinggi / awal.size.y;

        // Dudukkan telapak kaki di lantai.
        if (Ukur(go, out Bounds b))
            go.transform.position += new Vector3(0f, posisi.y - b.min.y, 0f);

        var a = go.AddComponent<KarakterAnimasi>();
        a.Karakter = k;
        return a;
    }

    static bool Ukur(GameObject go, out Bounds b)
    {
        b = default;
        var rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return false;
        b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return true;
    }

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();
        animator.applyRootMotion = false;

        graph = PlayableGraph.Create(name);
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        output = AnimationPlayableOutput.Create(graph, "Animasi", animator);
    }

    public void Main(AnimationClip clip, bool ulangi)
    {
        if (clip == null || (clip == sekarang && ulang == ulangi)) return;

        if (playable.IsValid()) playable.Destroy();
        playable = AnimationClipPlayable.Create(graph, clip);
        output.SetSourcePlayable(playable);
        sekarang = clip;
        ulang = ulangi;
        graph.Play();
    }

    void Update()
    {
        // Klip FBX Kenney tidak diset loop, jadi diulang manual.
        if (!ulang || !playable.IsValid() || sekarang == null || sekarang.length <= 0f) return;
        double t = playable.GetTime();
        if (t >= sekarang.length) playable.SetTime(t % sekarang.length);
    }

    void OnEnable()
    {
        if (graph.IsValid()) graph.Play();
    }

    void OnDestroy()
    {
        if (graph.IsValid()) graph.Destroy();
    }
}
