// PasangTycoon.cs — langkah 5-7: pelengkap tycoon. Tiap menu berdiri sendiri dan aman diulang.
//
//   5. Pasang Visual Upgrade      : wadah visual tiap upgrade + efek Dekorasi dan Lampu Hangat
//   6. Pasang Papan Harga dan Jeda : PapanHarga di objek PapanMenu, MenuJedaUI di JalurB_Managers
//   7. Masukkan Kedai ke Build     : scene Kedai jadi scene pertama di Build Profiles

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PasangTycoon
{
    const string UPGRADE = AlatEditor.DATA + "/Upgrade";

    // nama file aset upgrade -> nama wadah visual di Kedai > VisualUpgrade
    static readonly Dictionary<string, string> WADAH = new Dictionary<string, string>
    {
        { "KursiMeja",    "Visual_KursiMeja" },
        { "Dekorasi",     "Visual_Dekorasi" },
        { "MesinCepat",   "Visual_MesinCepat" },
        { "MesinManual",  "Visual_MesinManual" },
        { "MusikCepat",   "Visual_MusikCepat" },
        { "MusikAkustik", "Visual_MusikAkustik" },
        { "LampuHangat",  "Visual_LampuHangat" },
    };

    // ---------- 5 ----------
    [MenuItem("Last Commit/Jalur B/5. Pasang Visual Upgrade", false, 5)]
    public static void VisualUpgrade()
    {
        var kedai = AlatEditor.Kedai();
        if (kedai == null) return;

        var grup = AlatEditor.Grup(kedai.transform, "VisualUpgrade");
        var laporan = new List<string>();

        foreach (var kv in WADAH)
        {
            // Wadah kosong dan nonaktif. Taruh model dekor/mesin/speaker milik upgrade itu di dalamnya;
            // wadah otomatis menyala saat upgrade dibeli.
            var t = grup.Find(kv.Value);
            if (t == null)
            {
                var go = new GameObject(kv.Value);
                Undo.RegisterCreatedObjectUndo(go, "Buat " + kv.Value);
                go.transform.SetParent(grup, false);
                go.SetActive(false);
            }

            var u = AssetDatabase.LoadAssetAtPath<UpgradeData>(UPGRADE + "/Upgrade_" + kv.Key + ".asset");
            if (u == null) { laporan.Add("Upgrade_" + kv.Key + " TIDAK ditemukan (jalankan langkah 1)"); continue; }

            if (string.IsNullOrEmpty(u.nyalakanObjek)) u.nyalakanObjek = kv.Value;
            if (kv.Key == "LampuHangat") u.hangatkanLampu = true;
            if (kv.Key == "Dekorasi" && u.kurangiJedaPelanggan <= 0f)
            {
                u.kurangiJedaPelanggan = 0.15f;
                if (u.deskripsi == "Ruangan lebih hidup.")
                    u.deskripsi = "Ruangan lebih hidup, pelanggan datang lebih sering.";
            }
            EditorUtility.SetDirty(u);
            laporan.Add(u.namaTampil + " -> " + u.nyalakanObjek);
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(kedai.scene);
        EditorUtility.DisplayDialog("Visual upgrade",
            string.Join("\n", laporan) +
            "\n\nWadah ada di Kedai > VisualUpgrade (nonaktif). Masukkan model dekor tiap upgrade ke wadahnya." +
            "\nLampu Hangat juga mengubah warna lampu gantung. Jangan lupa Ctrl+S.", "OK");
    }

    // ---------- 6 ----------
    [MenuItem("Last Commit/Jalur B/6. Pasang Papan Harga dan Menu Jeda", false, 6)]
    public static void PapanDanJeda()
    {
        var kedai = AlatEditor.Kedai();
        if (kedai == null) return;
        var laporan = new List<string>();

        var papan = AlatEditor.Cari(kedai.transform, "PapanMenu");
        if (papan != null)
        {
            AlatEditor.Komponen<PapanHarga>(papan.gameObject);
            AlatEditor.PastikanCollider(papan.gameObject);
            laporan.Add("PapanMenu: PapanHarga terpasang");
        }
        else laporan.Add("PapanMenu TIDAK ditemukan");

        var m = GameObject.Find("JalurB_Managers");
        if (m != null) { AlatEditor.Komponen<MenuJedaUI>(m); laporan.Add("JalurB_Managers: MenuJedaUI terpasang"); }
        else laporan.Add("JalurB_Managers TIDAK ditemukan (jalankan langkah 1)");

        EditorSceneManager.MarkSceneDirty(kedai.scene);
        EditorUtility.DisplayDialog("Papan harga dan menu jeda",
            string.Join("\n", laporan) + "\n\nPagi: E di papan menu untuk atur harga. Esc: menu jeda.\nJangan lupa Ctrl+S.", "OK");
    }

    // ---------- 7 ----------
    [MenuItem("Last Commit/Jalur B/7. Masukkan Kedai ke Build", false, 7)]
    public static void KeBuild()
    {
        var kedai = AlatEditor.Kedai();
        if (kedai == null) return;

        string path = kedai.scene.path;
        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Scene belum disimpan", "Simpan scene Kedai dulu (Ctrl+S).", "OK");
            return;
        }

        // Kedai paling atas dan aktif; scene lain tetap ada tapi dimatikan.
        var daftar = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(path, true) };
        foreach (var s in EditorBuildSettings.scenes)
            if (s.path != path) daftar.Add(new EditorBuildSettingsScene(s.path, false));
        EditorBuildSettings.scenes = daftar.ToArray();

        EditorUtility.DisplayDialog("Build", path + " sekarang scene pertama di daftar Build.\n" +
            "Scene lain tetap di daftar tapi tidak dicentang.", "OK");
    }
}
