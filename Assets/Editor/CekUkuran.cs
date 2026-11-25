// CekUkuran.cs — mengukur model yang sudah terpasang dan menghitung Scale Factor
// Taruh di: Assets/Editor/CekUkuran.cs
// Buka lewat menu: Last Commit > Cek Ukuran Model
//
// Alat ini TIDAK mengubah apa pun sampai kamu klik tombolnya.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CekUkuranWindow : EditorWindow
{
    class Baris
    {
        public string nama;
        public string status;          // pesan kalau ada masalah
        public bool bisaDihitung;
        public Vector3 ukuran;         // ukuran model sekarang, meter
        public float targetTinggi;
        public float sfSekarang = 1f;
        public float sfSaran = 1f;
        public bool sudahPas;
        public string pathAsset;
    }

    // nama objek di scene, dan tinggi wajarnya dalam meter
    static readonly string[] NAMA = {
        // perabot utama
        "MesinKopi", "Counter", "Pintu", "Speaker", "PapanMenu",
        "Laptop", "Meja1", "Kursi1a", "RakBuku",
        // hiasan Tahap 2
        "Tanaman_Pot1", "Tanaman_Pot2", "KursiBar1",
        "Poster_Timur", "Poster_Selatan", "Keset",
        "Gelas_Meja1", "TumpukanBuku", "ToplesBiji",
        "LampuGantung_Counter"
    };
    static readonly float[] TARGET = {
        0.50f, 1.00f, 2.10f, 1.00f, 0.80f,
        0.25f, 0.75f, 0.90f, 2.00f,
        1.10f, 0.90f, 0.75f,
        0.90f, 0.95f, 0.04f,
        0.12f, 0.22f, 0.30f,
        0.25f
    };

    // Tinggi wajar untuk objek yang tidak ada di daftar tetap, ditebak dari namanya
    static float TinggiWajar(string nama)
    {
        for (int i = 0; i < NAMA.Length; i++)
            if (NAMA[i] == nama) return TARGET[i];

        // Pelanggan berdiri di objek bernama Titik*.
        // 1,25 m, bukan 1,70 m: karakter chibi proporsinya 3-4 kali tinggi kepala,
        // bukan 7-8 seperti manusia. Dipaksa setinggi manusia, kepalanya jadi raksasa.
        if (nama.StartsWith("Titik"))        return 1.25f;

        if (nama.StartsWith("Gelas"))        return 0.12f;
        if (nama.StartsWith("Kursi"))        return 0.90f;
        if (nama.StartsWith("Meja"))         return 0.75f;
        if (nama.StartsWith("Poster"))       return 0.90f;
        if (nama.StartsWith("Tanaman"))      return 1.00f;
        if (nama.StartsWith("LampuGantung")) return 0.25f;
        if (nama.StartsWith("Toples"))       return 0.30f;
        if (nama.StartsWith("Tumpukan"))     return 0.22f;
        if (nama.StartsWith("Keset"))        return 0.04f;
        if (nama.StartsWith("Titik"))        return 1.75f;   // titik pelanggan: tinggi orang dewasa

        return 1.00f;   // tidak dikenali, anggap setinggi 1 meter
    }

    List<Baris> hasil = new List<Baris>();
    Vector2 scroll;

    [MenuItem(MenuLastCommit.ALAT + "Cek Ukuran Model", false, 1)]
    public static void Buka()
    {
        var w = GetWindow<CekUkuranWindow>("Cek Ukuran");
        w.minSize = new Vector2(600, 560);
        w.Hitung();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Ukur ulang", GUILayout.Height(26)))
            Hitung();

        EditorGUILayout.Space(6);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        int perluUbah = 0;

        foreach (var b in hasil)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(b.nama, EditorStyles.boldLabel);

            if (!b.bisaDihitung)
            {
                EditorGUILayout.LabelField(b.status, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                continue;
            }

            EditorGUILayout.LabelField(
                "Ukuran sekarang : " +
                b.ukuran.x.ToString("0.###") + " x " +
                b.ukuran.y.ToString("0.###") + " x " +
                b.ukuran.z.ToString("0.###") + " meter");

            EditorGUILayout.LabelField(
                "Tinggi wajar    : sekitar " + b.targetTinggi.ToString("0.##") + " meter");

            EditorGUILayout.LabelField(
                "Scale Factor sekarang : " + b.sfSekarang.ToString("0.####"));

            if (b.sudahPas)
            {
                var gaya = new GUIStyle(EditorStyles.label);
                gaya.normal.textColor = new Color(0.35f, 0.7f, 0.35f);
                EditorGUILayout.LabelField("UKURANNYA SUDAH WAJAR. Jangan diubah.", gaya);
            }
            else
            {
                perluUbah++;
                var gaya = new GUIStyle(EditorStyles.boldLabel);
                gaya.normal.textColor = new Color(0.9f, 0.55f, 0.2f);
                EditorGUILayout.LabelField(
                    "ISI SCALE FACTOR DENGAN : " + b.sfSaran.ToString("0.####"), gaya);
                EditorGUILayout.LabelField("File : " + b.pathAsset, EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Perbaiki langsung di scene", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Menyetel ukuran SEMUA objek yang sudah punya model, satu per satu, " +
            "tanpa menyentuh Scale Factor file FBX. Dipakai kalau satu file dipakai " +
            "beberapa objek dengan ukuran berbeda.",
            EditorStyles.wordWrappedMiniLabel);

        var warnaBiru = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.6f, 0.75f, 0.95f);
        if (GUILayout.Button("Setel ukuran semua objek di scene", GUILayout.Height(34)))
            SetelSemuaDiScene();
        GUI.backgroundColor = warnaBiru;

        EditorGUILayout.Space(6);

        if (perluUbah == 0)
        {
            EditorGUILayout.HelpBox(
                "Semua model ukurannya sudah wajar. Tidak ada yang perlu diubah.",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                perluUbah + " model ukurannya meleset. Tombol di bawah akan mengisi Scale Factor " +
                "dan menekan Apply otomatis untuk semuanya.", MessageType.Warning);

            var warnaLama = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.55f, 0.85f, 0.55f);
            if (GUILayout.Button("Terapkan semua Scale Factor yang disarankan", GUILayout.Height(34)))
                Terapkan();
            GUI.backgroundColor = warnaLama;

            EditorGUILayout.LabelField(
                "Setelah ini, buka Last Commit > Pasang Model ke Blockout lalu klik tombol hijau, " +
                "supaya collidernya ikut menyesuaikan.", EditorStyles.miniLabel);
        }
    }

    // Menyetel skala tiap objek sendiri-sendiri. Berbeda dari Scale Factor,
    // yang berlaku untuk seluruh objek yang memakai file FBX yang sama.
    void SetelSemuaDiScene()
    {
        var akar = GameObject.Find("Kedai");
        if (akar == null)
        {
            EditorUtility.DisplayDialog("Objek 'Kedai' tidak ada", "Buka dulu scene Kedai.", "OK");
            return;
        }

        int diubah = 0, sudahPas = 0, dilewati = 0;
        var catatan = new System.Text.StringBuilder();

        foreach (var t in akar.GetComponentsInChildren<Transform>(true))
        {
            Transform model = null;
            for (int c = 0; c < t.childCount; c++)
                if (t.GetChild(c).name == "Model") model = t.GetChild(c);
            if (model == null) continue;

            if (Dilewati(t.name)) { dilewati++; continue; }

            Bounds b;
            if (!HitungBounds(model.gameObject, out b) || b.size.y < 0.0001f) { dilewati++; continue; }

            float target = TinggiWajar(t.name);
            float f = target / b.size.y;

            if (f < 0.01f || f > 100f) { dilewati++; continue; }
            if (Mathf.Abs(f - 1f) < 0.03f) { sudahPas++; continue; }

            Undo.RecordObject(model, "Setel ukuran " + t.name);

            float alasLama = b.min.y;
            model.localScale = new Vector3(
                model.localScale.x * f, model.localScale.y * f, model.localScale.z * f);

            // jaga supaya alasnya tetap di tempat semula, tidak melayang atau tenggelam
            Bounds b2;
            if (HitungBounds(model.gameObject, out b2))
                model.position += new Vector3(0f, alasLama - b2.min.y, 0f);

            PasangUlangCollider(t.gameObject);

            catatan.AppendLine("  " + t.name + " : " + b.size.y.ToString("0.00") +
                               " m jadi " + target.ToString("0.00") + " m");
            diubah++;
        }

        EditorSceneManager.MarkAllScenesDirty();
        Hitung();

        Debug.Log("Setel ukuran di scene:\n" + catatan);
        EditorUtility.DisplayDialog("Selesai",
            diubah + " objek disetel ulang ukurannya.\n" +
            sudahPas + " sudah pas.\n" +
            dilewati + " dilewati (benda pipih atau bagian bangunan).\n\n" +
            "Benda yang menumpang di atas meja mungkin perlu didudukkan ulang: " +
            "jalankan Last Commit > Rapikan Tata Letak sesudah ini.\n\n" +
            "Salah? Ctrl+Z. Jangan lupa Ctrl+S.", "OK");
    }

    // Benda pipih dan bagian bangunan tidak boleh diukur dari tingginya
    static bool Dilewati(string nama)
    {
        return nama.StartsWith("Keset")
            || nama.StartsWith("Poster")
            || nama.StartsWith("Jendela")
            || nama.StartsWith("Dinding")
            || nama.StartsWith("Lantai")
            || nama.StartsWith("Plafon");
    }

    static void PasangUlangCollider(GameObject target)
    {
        var bc = target.GetComponent<BoxCollider>();
        var cap = target.GetComponent<CapsuleCollider>();
        if (bc == null && cap == null) return;

        Bounds b;
        if (!HitungBounds(target, out b) || b.size.sqrMagnitude < 0.000001f) return;

        var skala = target.transform.lossyScale;

        if (bc != null)
        {
            Undo.RecordObject(bc, "Sesuaikan collider");
            bc.center = target.transform.InverseTransformPoint(b.center);
            bc.size = new Vector3(
                b.size.x / Mathf.Max(0.0001f, skala.x),
                b.size.y / Mathf.Max(0.0001f, skala.y),
                b.size.z / Mathf.Max(0.0001f, skala.z));
        }

        if (cap != null)
        {
            Undo.RecordObject(cap, "Sesuaikan collider");
            float tinggi = Mathf.Max(0.5f, b.size.y / Mathf.Max(0.0001f, skala.y));
            cap.direction = 1;                     // sumbu Y
            cap.height = tinggi;
            cap.center = new Vector3(0f, tinggi * 0.5f, 0f);
            cap.radius = Mathf.Clamp(
                Mathf.Max(b.size.x, b.size.z) * 0.5f / Mathf.Max(0.0001f, skala.x),
                0.15f, 0.45f);
        }
    }

    static bool HitungBounds(GameObject go, out Bounds hasil)
    {
        hasil = new Bounds(go.transform.position, Vector3.zero);
        var rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return false;

        hasil = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) hasil.Encapsulate(rends[i].bounds);
        return true;
    }

    void Hitung()
    {
        hasil.Clear();

        var akar = GameObject.Find("Kedai");
        if (akar == null)
        {
            hasil.Add(new Baris {
                nama = "Objek 'Kedai' tidak ada di scene",
                status = "Jalankan dulu Last Commit > Bangun Blockout Kedai.",
                bisaDihitung = false });
            return;
        }

        // Telusuri SELURUH isi Kedai, bukan daftar tetap. Setiap objek yang
        // punya anak bernama "Model" berarti sudah diganti modelnya.
        var daftar = new List<Transform>();
        foreach (var t in akar.GetComponentsInChildren<Transform>(true))
        {
            for (int c = 0; c < t.childCount; c++)
                if (t.GetChild(c).name == "Model") { daftar.Add(t); break; }
        }

        if (daftar.Count == 0)
        {
            hasil.Add(new Baris {
                nama = "Belum ada model terpasang",
                status = "Pasang model dulu lewat Last Commit > Pasang Model ke Blockout.",
                bisaDihitung = false });
            return;
        }

        daftar.Sort((x, y) => string.Compare(x.name, y.name, System.StringComparison.Ordinal));

        for (int i = 0; i < daftar.Count; i++)
        {
            Transform target = daftar[i];
            var b = new Baris { nama = target.name, targetTinggi = TinggiWajar(target.name) };

            Transform model = null;
            for (int c = 0; c < target.childCount; c++)
                if (target.GetChild(c).name == "Model") model = target.GetChild(c);

            if (model == null) continue;

            var rends = model.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0)
            {
                b.status = "Model tidak punya Renderer, ukurannya tidak bisa diukur.";
                hasil.Add(b);
                continue;
            }

            Bounds bb = rends[0].bounds;
            for (int r = 1; r < rends.Length; r++) bb.Encapsulate(rends[r].bounds);
            b.ukuran = bb.size;

            if (b.ukuran.y < 0.0001f)
            {
                b.status = "Tinggi model nyaris nol, tidak bisa dihitung.";
                hasil.Add(b);
                continue;
            }

            b.pathAsset = PathAsset(model.gameObject);
            var mi = string.IsNullOrEmpty(b.pathAsset)
                ? null
                : AssetImporter.GetAtPath(b.pathAsset) as ModelImporter;
            if (mi != null) b.sfSekarang = mi.globalScale;

            float rasio = b.targetTinggi / b.ukuran.y;
            b.sudahPas = (rasio > 0.7f && rasio < 1.45f);
            b.sfSaran = b.sfSekarang * rasio;
            b.bisaDihitung = true;

            hasil.Add(b);
        }
    }

    void Terapkan()
    {
        int n = 0;
        var sudah = new HashSet<string>();

        foreach (var b in hasil)
        {
            if (!b.bisaDihitung || b.sudahPas) continue;
            if (string.IsNullOrEmpty(b.pathAsset)) continue;
            if (sudah.Contains(b.pathAsset)) continue;

            var mi = AssetImporter.GetAtPath(b.pathAsset) as ModelImporter;
            if (mi == null) continue;

            mi.globalScale = b.sfSaran;
            mi.SaveAndReimport();
            sudah.Add(b.pathAsset);
            n++;
        }

        AssetDatabase.Refresh();
        Hitung();

        EditorUtility.DisplayDialog("Selesai",
            n + " file model sudah diubah Scale Factor-nya dan di-Apply.\n\n" +
            "Sekarang buka Last Commit > Pasang Model ke Blockout, klik tombol hijau, " +
            "lalu simpan dengan Ctrl+S.", "OK");
    }

    static string PathAsset(GameObject inst)
    {
        var src = PrefabUtility.GetCorrespondingObjectFromOriginalSource(inst);
        if (src != null)
        {
            string p = AssetDatabase.GetAssetPath(src);
            if (!string.IsNullOrEmpty(p)) return p;
        }

        var mf = inst.GetComponentInChildren<MeshFilter>(true);
        if (mf != null && mf.sharedMesh != null)
        {
            string p = AssetDatabase.GetAssetPath(mf.sharedMesh);
            if (!string.IsNullOrEmpty(p)) return p;
        }
        return null;
    }

    static Transform CariAnak(Transform akar, string nama)
    {
        foreach (var t in akar.GetComponentsInChildren<Transform>(true))
            if (t.name == nama) return t;
        return null;
    }
}
