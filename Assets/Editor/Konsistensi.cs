
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using UnityEditor;
using UnityEditor.SceneManagement;

public class KonsistensiWindow : EditorWindow
{
    const string FOLDER_UI = "Assets/Prefabs/UI";
    const string FOLDER_FONT = "Assets/Fonts";

    // Teks sebesar ini ke atas dianggap judul
    const float BATAS_JUDUL = 44f;

    // Palet ruangan, diturunkan dari palet GDD
    static readonly Color LANTAI  = new Color32(0x3A, 0x2A, 0x1F, 0xFF);
    static readonly Color DINDING = new Color32(0x4A, 0x3A, 0x30, 0xFF);
    static readonly Color LANGIT  = new Color32(0x2A, 0x1F, 0x1A, 0xFF);

    TMP_FontAsset fontJudul;
    TMP_FontAsset fontIsi;
    bool sudahTebak;

    [MenuItem(MenuLastCommit.SISTEM + "4. Konsistensi Font dan Warna", false, 4)]
    public static void Buka()
    {
        var w = GetWindow<KonsistensiWindow>("Konsistensi");
        w.minSize = new Vector2(440, 420);
    }

    void OnGUI()
    {
        if (!sudahTebak) { Tebak(); sudahTebak = true; }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("1. Dari .ttf jadi TMP Font Asset", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Taruh file .ttf di " + FOLDER_FONT + " dulu, lalu klik tombol ini. " +
            "Unduh Space Grotesk dan IBM Plex Sans di fonts.google.com.",
            EditorStyles.wordWrappedMiniLabel);

        if (GUILayout.Button("Bikin TMP Font Asset dari semua .ttf", GUILayout.Height(28)))
            BikinFontAsset();

        EditorGUILayout.Space(16);
        EditorGUILayout.LabelField("2. Pasang font ke semua teks UI", EditorStyles.boldLabel);

        fontJudul = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "Font judul", fontJudul, typeof(TMP_FontAsset), false);
        fontIsi = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "Font isi", fontIsi, typeof(TMP_FontAsset), false);

        EditorGUILayout.LabelField(
            "Teks ukuran " + BATAS_JUDUL + " ke atas pakai font judul, sisanya font isi. " +
            "Kalau salah satu kosong, yang terisi dipakai untuk semuanya.",
            EditorStyles.wordWrappedMiniLabel);

        GUI.enabled = fontJudul != null || fontIsi != null;
        var warnaLama = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
        if (GUILayout.Button("Terapkan font ke semua UI", GUILayout.Height(34)))
            TerapkanFont();
        GUI.backgroundColor = warnaLama;
        GUI.enabled = true;

        EditorGUILayout.Space(16);
        EditorGUILayout.LabelField("3. Warna ruangan", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Lantai, dinding, dan langit-langit diganti ke palet GDD. " +
            "Model Kenney tidak disentuh.",
            EditorStyles.wordWrappedMiniLabel);

        if (GUILayout.Button("Samakan warna ruangan dengan palet GDD", GUILayout.Height(28)))
            WarnaRuangan();

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Kalau teks di scene belum berubah padahal prefab sudah, klik objeknya di " +
            "Hierarchy lalu Prefab > Revert All.", MessageType.Info);
    }

    // ---------- 1. Bikin font asset ----------

    void BikinFontAsset()
    {
        if (!AssetDatabase.IsValidFolder(FOLDER_FONT))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Fonts"))
                AssetDatabase.CreateFolder("Assets", "Fonts");

            EditorUtility.DisplayDialog("Folder Fonts baru dibuat",
                "Folder " + FOLDER_FONT + " sudah saya buat tapi masih kosong.\n\n" +
                "Unduh Space Grotesk dan IBM Plex Sans di fonts.google.com, " +
                "masukkan file .ttf-nya ke folder itu, lalu klik tombol ini lagi.", "OK");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Font", new[] { FOLDER_FONT });
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Belum ada file font",
                "Tidak ada .ttf atau .otf di " + FOLDER_FONT + ".\n\n" +
                "Unduh Space Grotesk dan IBM Plex Sans di fonts.google.com, " +
                "masukkan ke folder itu, lalu klik lagi.", "OK");
            return;
        }

        int dibuat = 0, dilewati = 0;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var font = AssetDatabase.LoadAssetAtPath<Font>(path);
            if (font == null) continue;

            var pathSdf = System.IO.Path.GetDirectoryName(path).Replace('\\', '/')
                        + "/" + System.IO.Path.GetFileNameWithoutExtension(path) + " SDF.asset";

            // Selalu dibuat ulang. Path-nya sama, jadi GUID tetap dan referensi
            // di prefab tidak putus.
            bool menimpa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(pathSdf) != null;

            var fa = TMP_FontAsset.CreateFontAsset(
                font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
            if (fa == null) { Debug.LogWarning("Gagal membuat font asset dari " + path); continue; }

            AssetDatabase.CreateAsset(fa, pathSdf);

            // Panggang huruf yang dipakai, supaya atlasnya terisi sebelum disimpan
            fa.TryAddCharacters(HurufDipakai());

            // INI bagian yang dulu terlewat: tekstur dan material harus
            // ditanam ke dalam file .asset, kalau tidak hilang saat Unity ditutup.
            if (fa.atlasTextures != null && fa.atlasTextures.Length > 0 && fa.atlasTextures[0] != null)
            {
                fa.atlasTextures[0].name = fa.name + " Atlas";
                AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
            }
            if (fa.material != null)
            {
                fa.material.name = fa.name + " Material";
                AssetDatabase.AddObjectToAsset(fa.material, fa);
            }

            // Static berarti huruf sudah jadi di dalam asset dan tidak perlu
            // file .ttf lagi saat game berjalan.
            fa.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(fa);

            if (menimpa) dilewati++;
            dibuat++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        sudahTebak = false;   // tebak ulang supaya slot langsung terisi

        EditorUtility.DisplayDialog("Selesai",
            dibuat + " font asset dibuat ulang (" + dilewati + " menimpa yang lama).\n\n" +
            "Cek slot Font judul dan Font isi di atas, lalu klik tombol hijau.", "OK");
    }

    // Huruf yang perlu dipanggang ke atlas: ASCII plus tanda yang dipakai HUD
    static string HurufDipakai()
    {
        var sb = new System.Text.StringBuilder();
        for (int c = 32; c <= 126; c++) sb.Append((char)c);
        sb.Append("\u00B7\u00D7\u00B0\u2013\u2014\u2018\u2019\u201C\u201D\u2026\u2192");
        return sb.ToString();
    }

    // ---------- 2. Terapkan font ----------

    void TerapkanFont()
    {
        var judul = fontJudul != null ? fontJudul : fontIsi;
        var isi = fontIsi != null ? fontIsi : fontJudul;

        int diubah = 0, prefab = 0;

        // Prefab dulu, karena itu sumber kebenarannya
        if (AssetDatabase.IsValidFolder(FOLDER_UI))
        {
            foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { FOLDER_UI }))
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var akar = PrefabUtility.LoadPrefabContents(path);

                int n = Pasang(akar.GetComponentsInChildren<TextMeshProUGUI>(true), judul, isi);
                if (n > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(akar, path);
                    diubah += n;
                    prefab++;
                }
                PrefabUtility.UnloadPrefabContents(akar);
            }
        }

        // Lalu teks yang ada di scene, supaya Game view langsung ikut berubah
        var diScene = Object.FindObjectsByType<TextMeshProUGUI>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in diScene) Undo.RecordObject(t, "Ganti font");
        diubah += Pasang(diScene, judul, isi);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog("Selesai",
            diubah + " teks diganti fontnya di " + prefab + " prefab dan di scene.\n\n" +
            "Judul: " + judul.name + "\nIsi: " + isi.name + "\n\nJangan lupa Ctrl+S.", "OK");
    }

    static int Pasang(IReadOnlyList<TextMeshProUGUI> daftar, TMP_FontAsset judul, TMP_FontAsset isi)
    {
        int n = 0;
        foreach (var t in daftar)
        {
            if (t == null) continue;
            var mau = t.fontSize >= BATAS_JUDUL ? judul : isi;
            if (t.font == mau) continue;
            t.font = mau;
            EditorUtility.SetDirty(t);
            n++;
        }
        return n;
    }

    // ---------- 3. Warna ruangan ----------

    void WarnaRuangan()
    {
        var akar = GameObject.Find("Kedai");
        if (akar == null)
        {
            EditorUtility.DisplayDialog("Objek 'Kedai' tidak ada",
                "Buka dulu scene Kedai yang sudah ada blockout-nya.", "OK");
            return;
        }

        int n = 0;
        foreach (var t in akar.GetComponentsInChildren<Transform>(true))
        {
            Color warna;
            if (t.name == "Lantai") warna = LANTAI;
            else if (t.name == "Langit" || t.name == "LangitLangit") warna = LANGIT;
            else if (t.name.StartsWith("Dinding")) warna = DINDING;
            else continue;

            var r = t.GetComponent<Renderer>();
            if (r == null || r.sharedMaterial == null) continue;

            var m = r.sharedMaterial;
            Undo.RecordObject(m, "Warna ruangan");
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", warna);
            m.color = warna;
            EditorUtility.SetDirty(m);
            n++;
        }

        EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog(n > 0 ? "Selesai" : "Tidak ada yang cocok",
            n > 0
                ? n + " permukaan diganti warnanya ke palet GDD.\n\nJangan lupa Ctrl+S."
                : "Tidak ketemu objek bernama Lantai, Langit, atau Dinding_* di dalam Kedai.",
            "OK");
    }

    // ---------- Tebakan awal ----------

    void Tebak()
    {
        foreach (var g in AssetDatabase.FindAssets("t:TMP_FontAsset"))
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (path.StartsWith("Packages/")) continue;   // lewati font bawaan TMP

            var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fa == null) continue;

            var nama = fa.name.ToLower().Replace(" ", "").Replace("-", "");
            if (fontJudul == null && nama.Contains("spacegrotesk")) fontJudul = fa;
            else if (fontIsi == null && nama.Contains("plex")) fontIsi = fa;
        }
    }
}
