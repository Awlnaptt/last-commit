using UnityEngine;
using UnityEngine.SceneManagement;

// Memasang komponen Jalur B yang belum ada di scene secara otomatis saat Play,
// supaya fitur tetap jalan walaupun menu "Last Commit > Jalur B > 6" belum dijalankan / belum disimpan.
//   - PapanHarga di objek PapanMenu (plus collider supaya bisa ditekan E)
//   - MenuJedaUI di JalurB_Managers
// Kalau komponennya sudah ada di scene, tidak ada yang diubah.
public static class PasangOtomatis
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Mulai()
    {
        Pasang(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        SceneManager.sceneLoaded -= Pasang;
        SceneManager.sceneLoaded += Pasang;   // juga setelah "Main Lagi" memuat ulang scene
    }

    static void Pasang(Scene scene, LoadSceneMode mode)
    {
        var managers = GameObject.Find("JalurB_Managers");
        if (managers != null && managers.GetComponent<MenuJedaUI>() == null)
            managers.AddComponent<MenuJedaUI>();

        var kedai = GameObject.Find("Kedai");
        if (kedai == null) return;

        Transform papan = null;
        foreach (var t in kedai.GetComponentsInChildren<Transform>(true))
            if (t.name == "PapanMenu") { papan = t; break; }

        if (papan != null && papan.GetComponentInParent<PapanHarga>() == null)
        {
            papan.gameObject.AddComponent<PapanHarga>();
            PastikanCollider(papan.gameObject);
        }
    }

    static void PastikanCollider(GameObject go)
    {
        if (go.GetComponentInChildren<Collider>(true) != null) return;
        var rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return;

        Bounds b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);

        var box = go.AddComponent<BoxCollider>();
        box.center = go.transform.InverseTransformPoint(b.center);
        var s = go.transform.lossyScale;
        box.size = new Vector3(
            b.size.x / Mathf.Max(0.0001f, Mathf.Abs(s.x)),
            b.size.y / Mathf.Max(0.0001f, Mathf.Abs(s.y)),
            b.size.z / Mathf.Max(0.0001f, Mathf.Abs(s.z)));
    }
}
