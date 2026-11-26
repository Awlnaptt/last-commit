// Inspector IntroSinematik: tombol per shot untuk melihat dan mengambil sudut kamera dari Scene view,
// plus menu "2 Sistem dan UI > 6. Pasang Intro Sinematik".
//
// Cara mengatur satu shot:
//   1. Gerakkan Scene view ke posisi AWAL yang diinginkan (klik kanan + WASD), tekan "Ambil Awal".
//   2. Gerakkan ke posisi AKHIR, tekan "Ambil Akhir".
//   3. "Lihat Awal" / "Lihat Akhir" untuk mengecek. Play untuk melihat gerakannya.
// Titik 'lihat' diambil 5 m di depan kamera. Kalau 'objekDilihat' diisi, kamera mengarah ke objek itu saja.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(IntroSinematik))]
public class IntroSinematikEditor : Editor
{
    const float JARAK_LIHAT = 5f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var intro = (IntroSinematik)target;
        if (intro.shot == null) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Atur sudut kamera dari Scene view", EditorStyles.boldLabel);
        for (int i = 0; i < intro.shot.Length; i++)
        {
            var s = intro.shot[i];
            EditorGUILayout.LabelField($"Shot {i + 1}: {s.catatan}", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Lihat Awal")) Lihat(s.posisiAwal, s.lihatAwal, s.objekDilihat);
                if (GUILayout.Button("Lihat Akhir")) Lihat(s.posisiAkhir, s.lihatAkhir, s.objekDilihat);
                if (GUILayout.Button("Ambil Awal")) Ambil(intro, i, true);
                if (GUILayout.Button("Ambil Akhir")) Ambil(intro, i, false);
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Kembalikan semua shot ke bawaan"))
        {
            Undo.RecordObject(intro, "Shot bawaan");
            intro.shot = IntroSinematik.Bawaan();
            EditorUtility.SetDirty(intro);
        }
    }

    static void Lihat(Vector3 posisi, Vector3 lihat, string objek)
    {
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) return;
        if (!string.IsNullOrEmpty(objek))
        {
            var t = Cari(objek);
            if (t != null) lihat = AlatKedai.Ukur(t, out var b) ? b.center : t.position;
        }
        var bantu = new GameObject("_SudutIntro") { hideFlags = HideFlags.HideAndDontSave };
        bantu.transform.SetPositionAndRotation(posisi, Quaternion.LookRotation(lihat - posisi, Vector3.up));
        sv.AlignViewToObject(bantu.transform);
        DestroyImmediate(bantu);
    }

    static void Ambil(IntroSinematik intro, int i, bool awal)
    {
        var sv = SceneView.lastActiveSceneView;
        if (sv == null) return;
        var cam = sv.camera.transform;
        Undo.RecordObject(intro, "Ambil sudut intro");
        var s = intro.shot[i];
        if (awal) { s.posisiAwal = cam.position; s.lihatAwal = cam.position + cam.forward * JARAK_LIHAT; }
        else { s.posisiAkhir = cam.position; s.lihatAkhir = cam.position + cam.forward * JARAK_LIHAT; }
        EditorUtility.SetDirty(intro);
    }

    static Transform Cari(string nama)
    {
        var kedai = GameObject.Find("Kedai");
        var t = kedai != null ? AlatKedai.Cari(kedai.transform, nama) : null;
        if (t != null) return t;
        var go = GameObject.Find(nama);
        return go != null ? go.transform : null;
    }

    [MenuItem(MenuLastCommit.SISTEM + "6. Pasang Intro Sinematik", false, 6)]
    static void Pasang()
    {
        var m = GameObject.Find("JalurB_Managers");
        if (m == null)
        {
            EditorUtility.DisplayDialog("Intro sinematik", "JalurB_Managers belum ada. Jalankan dulu 2 Sistem dan UI > 2. Pasang Manager dan Data.", "OK");
            return;
        }
        var intro = m.GetComponent<IntroSinematik>();
        bool baru = intro == null;
        if (baru) intro = Undo.AddComponent<IntroSinematik>(m);
        Selection.activeObject = intro;
        EditorSceneManager.MarkSceneDirty(m.scene);
        EditorUtility.DisplayDialog("Intro sinematik",
            (baru ? "IntroSinematik dipasang di JalurB_Managers." : "IntroSinematik sudah terpasang.") +
            "\n\nAtur teks dan sudut kamera di Inspector (tombol Lihat / Ambil per shot).\n" +
            "Matikan sementara dengan menghapus centang 'Aktif'.\n\nJangan lupa Ctrl+S.", "OK");
    }
}
