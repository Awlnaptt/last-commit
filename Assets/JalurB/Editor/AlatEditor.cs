// Pembantu bersama untuk menu editor Last Commit (lihat MenuLastCommit.cs).
// Cari dan Ukur meneruskan ke AlatKedai supaya hasilnya sama dengan saat Play.

using UnityEditor;
using UnityEngine;

public static class AlatEditor
{
    public const string DATA = "Assets/JalurB/Data";

    public static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int i = path.LastIndexOf('/');
        Folder(path.Substring(0, i));
        AssetDatabase.CreateFolder(path.Substring(0, i), path.Substring(i + 1));
    }

    // Buat aset hanya kalau belum ada. Yang sudah ada dibiarkan, jadi hasil editanmu aman.
    public static T Aset<T>(string path, System.Action<T> isi) where T : ScriptableObject
    {
        var ada = AssetDatabase.LoadAssetAtPath<T>(path);
        if (ada != null) return ada;

        Folder(path.Substring(0, path.LastIndexOf('/')));
        var baru = ScriptableObject.CreateInstance<T>();
        isi(baru);
        AssetDatabase.CreateAsset(baru, path);
        return baru;
    }

    public static T Komponen<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : Undo.AddComponent<T>(go);
    }

    public static Transform Cari(Transform akar, string nama) => AlatKedai.Cari(akar, nama);

    public static Transform Grup(Transform akar, string nama)
    {
        var t = akar.Find(nama);
        if (t != null) return t;

        var go = new GameObject(nama);
        Undo.RegisterCreatedObjectUndo(go, "Buat " + nama);
        go.transform.SetParent(akar, false);
        return go.transform;
    }

    // Titik baru dibuat di posisi awal. Titik yang SUDAH ADA tidak dipindah,
    // supaya hasil geseranmu di Scene view tidak tertimpa.
    public static Transform Titik(Transform induk, string nama, Vector3 posisi, Vector3 lihatKe)
    {
        var ada = induk.Find(nama);
        if (ada != null) return ada;

        var go = new GameObject(nama);
        Undo.RegisterCreatedObjectUndo(go, "Buat " + nama);
        go.transform.SetParent(induk, true);
        go.transform.position = posisi;

        Vector3 arah = lihatKe - posisi;
        arah.y = 0f;
        if (arah.sqrMagnitude > 0.0001f) go.transform.rotation = Quaternion.LookRotation(arah);

        var ikon = EditorGUIUtility.IconContent("sv_label_3");
        if (ikon != null && ikon.image is Texture2D tex) EditorGUIUtility.SetIconForObject(go, tex);
        return go.transform;
    }

    public static bool Ukur(Transform t, out Bounds b) => AlatKedai.Ukur(t, out b);

    // Titik berdiri di samping perabot, dihitung dari bentuk yang TERLIHAT
    // (pivot model Kenney bisa meleset sekitar 1 m dari bendanya).
    public static Vector3 SampingPerabot(Transform kedai, string nama, Vector3 arah, Vector3 cadangan, out Vector3 tengah)
    {
        const float RENGGANG = 0.55f;
        tengah = cadangan;
        if (!Ukur(Cari(kedai, nama), out var b)) return cadangan;

        tengah = new Vector3(b.center.x, 0f, b.center.z);
        Vector3 tepi = new Vector3(Mathf.Abs(arah.x) * b.extents.x, 0f, Mathf.Abs(arah.z) * b.extents.z);
        return tengah + Vector3.Scale(arah, tepi) + arah * RENGGANG;
    }

    // Raycast butuh collider. Kalau belum ada, buat BoxCollider seukuran renderer anaknya.
    public static void PastikanCollider(GameObject go)
    {
        if (go.GetComponentInChildren<Collider>(true) != null) return;
        if (!Ukur(go.transform, out var b)) return;

        var box = Undo.AddComponent<BoxCollider>(go);
        box.center = go.transform.InverseTransformPoint(b.center);
        var s = go.transform.lossyScale;
        box.size = new Vector3(
            b.size.x / Mathf.Max(0.0001f, Mathf.Abs(s.x)),
            b.size.y / Mathf.Max(0.0001f, Mathf.Abs(s.y)),
            b.size.z / Mathf.Max(0.0001f, Mathf.Abs(s.z)));
    }

    public static GameObject Kedai()
    {
        var k = GameObject.Find("Kedai");
        if (k == null)
            EditorUtility.DisplayDialog("Scene belum siap",
                "Buka scene Kedai yang sudah punya objek 'Kedai'.", "OK");
        return k;
    }

    // Instansiasi model dari folder Art, skalakan ke tinggi (meter), letakkan alasnya di posisi.y,
    // tengahnya di posisi.x / posisi.z (dihitung dari bentuk yang terlihat, bukan pivot).
    // Dikerjakan di luar induk dulu karena bounds objek nonaktif tidak bisa diukur.
    public static GameObject Taruh(Transform induk, string kit, string nama, Vector3 posisi, float rotY, float tinggi)
    {
        var sumber = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/" + kit + "/" + nama + ".fbx");
        if (sumber == null) { Debug.LogWarning("[Jalur B] Model " + kit + "/" + nama + " tidak ditemukan."); return null; }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(sumber);
        Undo.RegisterCreatedObjectUndo(go, "Taruh " + nama);
        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, rotY, 0f));

        if (Ukur(go.transform, out var b) && b.size.y > 0.0001f)
            go.transform.localScale *= tinggi / b.size.y;
        if (Ukur(go.transform, out b))
            go.transform.position += new Vector3(posisi.x - b.center.x, posisi.y - b.min.y, posisi.z - b.center.z);

        go.transform.SetParent(induk, true);
        return go;
    }
}
