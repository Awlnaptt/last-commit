// CekUkuran.cs — mengukur model yang sudah terpasang dan menghitung Scale Factor
// Taruh di: Assets/Editor/CekUkuran.cs
// Buka lewat menu: Last Commit > Cek Ukuran Model
//
// Alat ini TIDAK mengubah apa pun sampai kamu klik tombolnya.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

        if (nama.StartsWith("Gelas"))        return 0.12f;
        if (nama.StartsWith("Kursi"))        return 0.90f;
        if (nama.StartsWith("Meja"))         return 0.75f;
        if (nama.StartsWith("Poster"))       return 0.90f;
        if (nama.StartsWith("Tanaman"))      return 1.00f;
        if (nama.StartsWith("LampuGantung")) return 0.25f;
        if (nama.StartsWith("Toples"))       return 0.30f;
        if (nama.StartsWith("Tumpukan"))     return 0.22f;
        if (nama.StartsWith("Keset"))        return 0.04f;

        return 1.00f;   // tidak dikenali, anggap setinggi 1 meter
    }

    List<Baris> hasil = new List<Baris>();
    Vector2 scroll;

    [MenuItem("Last Commit/Cek Ukuran Model")]
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
