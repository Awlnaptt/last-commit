using UnityEngine;

// Pembantu bersama untuk skrip yang menata isi Kedai (saat Play maupun dari menu editor).
// Dulu fungsi-fungsi ini disalin di beberapa file; sekarang cukup di sini.
public static class AlatKedai
{
    // Cari objek bernama 'nama' di mana pun di bawah 'akar', termasuk yang sedang nonaktif.
    public static Transform Cari(Transform akar, string nama)
    {
        if (akar == null) return null;
        foreach (var t in akar.GetComponentsInChildren<Transform>(true))
            if (t.name == nama) return t;
        return null;
    }

    // Kotak pembatas (bounds) bentuk yang terlihat, dalam koordinat dunia.
    // Dihitung dari mesh, bukan Renderer.bounds, supaya tetap benar walau objeknya nonaktif
    // (misalnya isi Tahap2 sebelum renovasi) atau tampilannya sedang dimatikan.
    // Aset buatan kode (MeshDariKode) tidak ikut diukur.
    public static bool Ukur(Transform t, out Bounds b)
    {
        // Dikumpulkan di variabel lokal dulu: parameter 'out' tidak boleh dipakai di dalam fungsi lokal.
        Bounds hasil = default;
        bool ada = false;

        void Tambahkan(Mesh mesh, Transform tr)
        {
            if (mesh == null || tr.GetComponentInParent<MeshDariKode>() != null) return;
            var mb = mesh.bounds;
            var m = tr.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                var sudut = mb.center + Vector3.Scale(mb.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                var p = m.MultiplyPoint3x4(sudut);
                if (!ada) { hasil = new Bounds(p, Vector3.zero); ada = true; } else hasil.Encapsulate(p);
            }
        }

        if (t != null)
        {
            foreach (var mf in t.GetComponentsInChildren<MeshFilter>(true)) Tambahkan(mf.sharedMesh, mf.transform);
            foreach (var sk in t.GetComponentsInChildren<SkinnedMeshRenderer>(true)) Tambahkan(sk.sharedMesh, sk.transform);
        }
        b = hasil;
        return ada;
    }

    // Objek kosong baru. Posisi nol = ikut posisi dan arah induknya.
    public static GameObject Baru(string nama, Transform induk, Vector3 posisiDunia, float rotY)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(induk, false);
        if (posisiDunia == Vector3.zero) return go;
        go.transform.position = posisiDunia;
        go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        return go;
    }

    // Tambah komponen kalau belum ada. true = baru ditambahkan.
    public static bool Tambah<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() != null) return false;
        go.AddComponent<T>();
        return true;
    }
}
