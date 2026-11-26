// PasangKarakter.cs — menu 3 Karakter dan NPC > 1: satu aset ModelKarakter untuk tiap FBX karakter.
//
// Mencari semua FBX bernama character-*.fbx di Assets/Art/Characters, lalu mengambil
// klip idle, walk, sit, emote-yes, emote-no dari FBX itu sendiri.
// Slot yang sudah terisi tidak ditimpa.
//
// Aset karakter yang modelnya sudah hilang (pack lama dihapus) ikut dibersihkan,
// setelah konfirmasi.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PasangKarakter
{
    public const string FOLDER = AlatEditor.DATA + "/Karakter";
    const string SUMBER = "Assets/Art/Characters";

    // Kenney Blocky Characters: skin yang TIDAK cocok jadi pelanggan kafe
    // (robot, boneka uji tabrak, polisi, zombie, hantu, bajak laut, ninja).
    // Tetap dibuat asetnya, hanya pelangganUmum = false. Kalau mau dipakai juga,
    // hapus hurufnya dari daftar ini lalu jalankan 3 Karakter dan NPC > 1 lagi.
    static readonly HashSet<string> BUKAN_PELANGGAN = new HashSet<string>
    {
        "d", "g", "h", "j", "l", "n", "o", "p", "r"
    };

    [MenuItem(MenuLastCommit.KARAKTER + "1. Buat Data Karakter", false, 1)]
    public static void Menu()
    {
        int dihapus = BersihkanYatim(true);
        var hasil = Buat();

        int pelanggan = 0;
        foreach (var k in hasil) if (k.pelangganUmum) pelanggan++;

        EditorUtility.DisplayDialog("Data karakter",
            hasil.Count + " karakter siap di " + FOLDER + "\n" +
            pelanggan + " di antaranya boleh jadi pelanggan umum.\n" +
            (dihapus > 0 ? dihapus + " aset karakter lama (modelnya sudah tidak ada) dihapus.\n" : "") +
            "\nKalau ada model yang tidak punya animasi jalan, cek Console.", "OK");
    }

    public static List<ModelKarakter> Buat()
    {
        AlatEditor.Folder(FOLDER);
        var hasil = new List<ModelKarakter>();

        foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { SUMBER }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string nama = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!nama.StartsWith("character-")) continue;

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null) continue;

            string pendek = nama.Replace("character-", "");
            var k = AlatEditor.Aset<ModelKarakter>(FOLDER + "/Karakter_" + pendek + ".asset", d =>
            {
                d.pelangganUmum = !BUKAN_PELANGGAN.Contains(pendek);
            });
            if (k.model == null) k.model = model;
            if (BUKAN_PELANGGAN.Contains(pendek)) k.pelangganUmum = false;

            var klip = new Dictionary<string, AnimationClip>();
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
                if (o is AnimationClip c && !c.name.StartsWith("__preview__")) klip[c.name] = c;

            if (k.diam == null) klip.TryGetValue("idle", out k.diam);
            if (k.jalan == null) klip.TryGetValue("walk", out k.jalan);
            if (k.duduk == null) klip.TryGetValue("sit", out k.duduk);
            if (k.senang == null) klip.TryGetValue("emote-yes", out k.senang);
            if (k.kecewa == null) klip.TryGetValue("emote-no", out k.kecewa);

            if (k.jalan == null) Debug.LogWarning("[Karakter] " + nama + " tidak punya klip 'walk'. Cek tab Rig (harus Generic) dan Animation di import FBX-nya.");
            EditorUtility.SetDirty(k);
            hasil.Add(k);
        }

        AssetDatabase.SaveAssets();
        return hasil;
    }

    // Hapus aset ModelKarakter yang modelnya sudah tidak ada.
    static int BersihkanYatim(bool tanya)
    {
        var yatim = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:ModelKarakter", new[] { FOLDER }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var k = AssetDatabase.LoadAssetAtPath<ModelKarakter>(path);
            if (k != null && k.model == null) yatim.Add(path);
        }
        if (yatim.Count == 0) return 0;

        if (tanya && !EditorUtility.DisplayDialog("Bersihkan aset karakter lama",
                yatim.Count + " aset karakter sudah tidak punya model (pack lama dihapus).\nHapus asetnya?",
                "Hapus", "Biarkan"))
            return 0;

        foreach (var p in yatim) AssetDatabase.DeleteAsset(p);
        return yatim.Count;
    }

    public static ModelKarakter Ambil(string namaPendek) =>
        AssetDatabase.LoadAssetAtPath<ModelKarakter>(FOLDER + "/Karakter_" + namaPendek + ".asset");
}
