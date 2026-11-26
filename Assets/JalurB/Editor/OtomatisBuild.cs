// OtomatisBuild.cs — memastikan scene Kedai selalu ada di daftar Build (paling atas, dicentang).
// Berjalan sendiri setiap Unity selesai compile, jadi tombol "Main Lagi" di ending tidak error.
// Sama dengan menu "Last Commit > 2 Sistem dan UI > 5. Masukkan Kedai ke Build", hanya otomatis.

using System.Collections.Generic;
using UnityEditor;

[InitializeOnLoad]
public static class OtomatisBuild
{
    const string KEDAI = "Assets/Scenes/Kedai.unity";

    static OtomatisBuild()
    {
        EditorApplication.delayCall += Cek;
    }

    static void Cek()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(KEDAI) == null) return;

        var lama = EditorBuildSettings.scenes;
        if (lama.Length > 0 && lama[0].path == KEDAI && lama[0].enabled) return;

        var baru = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(KEDAI, true) };
        foreach (var s in lama)
            if (s.path != KEDAI) baru.Add(new EditorBuildSettingsScene(s.path, false));
        EditorBuildSettings.scenes = baru.ToArray();
        UnityEngine.Debug.Log("[Jalur B] Scene Kedai dimasukkan ke daftar Build sebagai scene pertama.");
    }
}
