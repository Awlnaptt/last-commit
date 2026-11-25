// Menu untuk PasangAtmosfer, dipecah per kategori. Semua aman diulang; yang sudah ada tidak diubah.
//   4 Dekorasi > 2. Ganti Dekor Polos dan Tambal Pojok   (rak berisi buku, lampu gantung, poster, keset, tiang pojok)
//   4 Dekorasi > 3. Pasang Dekor Kode per Hari           (kusen pintu, papan kapur, jam, dekor hari 2-5)
//   4 Dekorasi > 5. Pasang Objek Cerita                  (foto wisuda, catatan Ibu)
//   5 Suasana  > 2. Pasang Cahaya, Cuaca, Post-processing (cuaca harian, post-processing, layar "Hari N", uap, kota luar)

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PasangAtmosferMenu
{
    const string TEKSTUR = "Assets/JalurB/Visual/Resources/JalurB/";

    [MenuItem(MenuLastCommit.DEKORASI + "2. Ganti Dekor Polos dan Tambal Pojok", false, 2)]
    static void GantiModel() => Jalankan(PasangAtmosfer.Bagian.GantiModel, "Ganti dekor polos",
        "Objek aslinya tidak dihapus, hanya tampilannya yang dimatikan.");

    [MenuItem(MenuLastCommit.DEKORASI + "3. Pasang Dekor Kode per Hari", false, 3)]
    static void Dekor() => Jalankan(PasangAtmosfer.Bagian.Dekor, "Dekor kode per hari",
        "Hari 1 gratis. Hari 2-5 dibeli otomatis tiap pagi (atur harga di Kedai > DekorBertahap).\nAlasan posisinya: Assets/JalurB/DEKOR_TATA_LETAK.md");

    [MenuItem(MenuLastCommit.DEKORASI + "5. Pasang Objek Cerita (Foto, Catatan Ibu)", false, 5)]
    static void ObjekCerita() => Jalankan(PasangAtmosfer.Bagian.ObjekCerita, "Objek cerita",
        "Geser di Kedai > ObjekCerita kalau kurang pas. Teks dialognya ada di komponen ObjekCerita (Inspector).");

    [MenuItem(MenuLastCommit.SUASANA + "2. Pasang Cahaya, Cuaca, dan Post-processing", false, 2)]
    static void Suasana() => Jalankan(PasangAtmosfer.Bagian.Suasana, "Suasana",
        "Cuaca per hari: JalurB_Managers > CuacaHarian. Post-processing: AtmosferPasca.\nPost-processing dan uap hanya terlihat saat Play, di Game view.");

    static void Jalankan(PasangAtmosfer.Bagian bagian, string judul, string catatan)
    {
        var kedai = AlatEditor.Kedai();
        if (kedai == null) return;

        AturTekstur("T_FotoWisuda", TextureWrapMode.Clamp);
        AturTekstur("T_CatatanIbu", TextureWrapMode.Clamp);
        AturTekstur("T_FasadGedung", TextureWrapMode.Repeat);
        AturTekstur("T_FasadGedung_Cahaya", TextureWrapMode.Repeat);

        List<string> laporan = PasangAtmosfer.Pastikan(bagian);
        EditorSceneManager.MarkSceneDirty(kedai.scene);

        EditorUtility.DisplayDialog(judul,
            (laporan.Count == 0 ? "Semua sudah terpasang sebelumnya." : string.Join("\n", laporan)) +
            "\n\n" + catatan + "\n\nJangan lupa Ctrl+S.", "OK");
    }

    static void AturTekstur(string nama, TextureWrapMode wrap)
    {
        var imp = AssetImporter.GetAtPath(TEKSTUR + nama + ".png") as TextureImporter;
        if (imp == null || imp.wrapMode == wrap) return;
        imp.wrapMode = wrap;
        imp.SaveAndReimport();
    }
}
