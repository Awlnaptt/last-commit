// PasangTycoon.cs — pelengkap tycoon (menu 4 Dekorasi > Visual Upgrade, 2 Sistem dan UI > 3 dan 5). Tiap menu berdiri sendiri dan aman diulang.
//
//   5. Pasang Visual Upgrade      : wadah visual tiap upgrade, diisi model Kenney (draf posisi),
//                                   + efek Dekorasi dan Lampu Hangat
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
    [MenuItem(MenuLastCommit.DEKORASI + "Pasang Visual Upgrade", false, 21)]
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
                t = go.transform;
            }
            // Wadah kosong diisi model contoh. Wadah yang sudah ada isinya tidak disentuh.
            if (t.childCount == 0) IsiWadah(kedai.transform, kv.Key, t);

            var u = AssetDatabase.LoadAssetAtPath<UpgradeData>(UPGRADE + "/Upgrade_" + kv.Key + ".asset");
            if (u == null) { laporan.Add("Upgrade_" + kv.Key + " TIDAK ditemukan (jalankan 2 Sistem dan UI > 2. Pasang Manager dan Data)"); continue; }

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
            "\n\nWadah ada di Kedai > VisualUpgrade (nonaktif) dan sudah diisi model contoh." +
            "\nCek posisinya lewat 4 Dekorasi > Pratinjau Visual Upgrade, geser kalau ada yang menembus." +
            "\nLampu Hangat juga mengubah warna lampu gantung. Jangan lupa Ctrl+S.", "OK");
    }

    // Menyalakan/mematikan semua wadah visual di Editor untuk mengecek posisi (tidak berpengaruh saat Play).
    [MenuItem(MenuLastCommit.DEKORASI + "Pratinjau Visual Upgrade (nyala-mati)", false, 22)]
    public static void Pratinjau()
    {
        var kedai = AlatEditor.Kedai();
        if (kedai == null) return;
        var grup = kedai.transform.Find("VisualUpgrade");
        if (grup == null) { EditorUtility.DisplayDialog("Pratinjau", "Jalankan 4 Dekorasi > Pasang Visual Upgrade dulu.", "OK"); return; }

        bool nyala = false;
        foreach (Transform c in grup) if (!c.gameObject.activeSelf) { nyala = true; break; }
        foreach (Transform c in grup) { Undo.RecordObject(c.gameObject, "Pratinjau"); c.gameObject.SetActive(nyala); }
        EditorUtility.DisplayDialog("Pratinjau", nyala
            ? "Semua visual upgrade DINYALAKAN. Cek posisinya, geser kalau menembus sesuatu.\nJalankan menu ini lagi untuk mematikan sebelum Ctrl+S."
            : "Semua visual upgrade dimatikan lagi. Sekarang aman untuk Ctrl+S.", "OK");
    }

    // ---------- Isi contoh tiap wadah ----------
    // Posisi dihitung dari denah kedai (lantai x -5..5, z -4..4) dan bentuk perabot yang terlihat.
    // Ini DRAF: cek lewat Pratinjau Visual Upgrade, geser kalau ada yang menembus.
    static void IsiWadah(Transform kedai, string upgrade, Transform wadah)
    {
        float atasCounter = AlatEditor.Ukur(AlatEditor.Cari(kedai, "Counter"), out var bc) ? bc.max.y : 1.0f;
        float xCounter = bc.size.x > 0f ? bc.center.x : 3.4f;

        switch (upgrade)
        {
            case "KursiMeja":   // satu meja bundar + dua kursi di sisi selatan yang masih kosong
                Taruh(wadah, "Furniture Kit", "tableRound",   new Vector3(0.6f, 0f, -2.9f), 0f, 0.75f);
                Taruh(wadah, "Furniture Kit", "chairCushion", new Vector3(-0.05f, 0f, -2.9f), 90f, 0.9f);
                Taruh(wadah, "Furniture Kit", "chairCushion", new Vector3(1.25f, 0f, -2.9f), -90f, 0.9f);
                break;

            case "Dekorasi":    // rak buku di dinding barat + tanaman di dinding selatan
                Taruh(wadah, "Furniture Kit", "bookcaseOpen", new Vector3(-4.75f, 0f, 0.25f), 90f, 1.8f);
                Taruh(wadah, "Furniture Kit", "pottedPlant",  new Vector3(-0.8f, 0f, -3.7f), 0f, 1.0f);
                break;

            case "MesinCepat":  // mesin kopi kedua di ujung selatan counter
                Taruh(wadah, "Furniture Kit", "kitchenCoffeeMachine", new Vector3(xCounter, atasCounter, -0.85f), -90f, 0.45f);
                break;

            case "MesinManual": // alat seduh manual di ujung selatan counter
                Taruh(wadah, "Food Kit", "mortar-pestle", new Vector3(xCounter - 0.15f, atasCounter, -0.8f), 0f, 0.15f);
                Taruh(wadah, "Food Kit", "mug",           new Vector3(xCounter + 0.15f, atasCounter, -0.95f), 0f, 0.12f);
                break;

            case "MusikCepat":  // speaker besar di sudut timur laut
                Taruh(wadah, "Furniture Kit", "speaker", new Vector3(4.6f, 0f, 3.6f), -135f, 1.0f);
                break;

            case "MusikAkustik": // meja kecil + radio di sudut yang sama (cabang musik saling mengunci)
                Taruh(wadah, "Furniture Kit", "sideTable", new Vector3(4.6f, 0f, 3.5f), -135f, 0.55f);
                Taruh(wadah, "Furniture Kit", "radio",     new Vector3(4.6f, 0.55f, 3.5f), -135f, 0.3f);
                break;

            case "LampuHangat": // lampu meja di tiga meja pelanggan
                foreach (var meja in new[] { "Meja1", "Meja2", "Meja3" })
                    if (AlatEditor.Ukur(AlatEditor.Cari(kedai, meja), out var bm))
                        Taruh(wadah, "Furniture Kit", "lampRoundTable",
                              new Vector3(bm.center.x + 0.25f, bm.max.y, bm.center.z), 0f, 0.45f);
                break;
        }
    }

    static void Taruh(Transform wadah, string kit, string nama, Vector3 posisi, float rotY, float tinggi) =>
        AlatEditor.Taruh(wadah, kit, nama, posisi, rotY, tinggi);

    // ---------- 6 ----------
    [MenuItem(MenuLastCommit.SISTEM + "3. Pasang Papan Harga dan Menu Jeda", false, 3)]
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
        else laporan.Add("JalurB_Managers TIDAK ditemukan (jalankan 2 Sistem dan UI > 2. Pasang Manager dan Data)");

        EditorSceneManager.MarkSceneDirty(kedai.scene);
        EditorUtility.DisplayDialog("Papan harga dan menu jeda",
            string.Join("\n", laporan) + "\n\nPagi: E di papan menu untuk atur harga. Esc: menu jeda.\nJangan lupa Ctrl+S.", "OK");
    }

    // ---------- 7 ----------
    [MenuItem(MenuLastCommit.SISTEM + "5. Masukkan Kedai ke Build", false, 5)]
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
