// PasangNPC.cs — langkah 3: tiga pelanggan tetap, masing-masing berdiri sendiri.
//
// Menu: Last Commit > Jalur B > 3. Pasang NPC
//
// Tiap NPC = satu aset NPCData (Assets/JalurB/Data/NPC) + satu titik di Kedai > NPC.
//   Ganti model/hari/dialog satu NPC -> ubah aset NPC_<nama> saja
//   Pindah posisi satu NPC           -> geser Titik_<nama> di Scene view
// Titik yang sudah ada tidak dipindah, aset yang sudah ada tidak ditimpa.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PasangNPC
{
    const string FOLDER = AlatEditor.DATA + "/NPC";
    const string DIALOG = AlatEditor.DATA + "/Dialog";

    struct Rencana
    {
        public string file, nama, peran, karakter, dialog, meja, sapaan;
        public int hari;
        public Vector3 arah, cadangan;
    }

    // Masing-masing di meja yang berbeda, satu NPC per hari jadi tidak pernah bertumpuk.
    // Skin Kenney Blocky: q = setelan kantor (Kak Raka kerja kantoran),
    // c = kaus gamer (Dimas, anak IT), i = berkacamata, rambut putih (Ibu). Ganti di aset NPC_* kalau kurang pas.
    static readonly Rencana[] NPC =
    {
        new Rencana { file = "KakakTingkat", nama = "Kak Raka", peran = "Kakak tingkat", karakter = "q", hari = 2, sapaan = "Eh, adik tingkat! Sini bentar.", dialog = "Dialog_NPC1_KakakTingkat", meja = "Meja3", arah = Vector3.right, cadangan = new Vector3(-3.45f, 0f, -1.20f) },
        new Rencana { file = "TemanKuliah",  nama = "Dimas",    peran = "Teman kuliah", karakter = "c", hari = 3, sapaan = "Bro... boleh ngobrol bentar?", dialog = "Dialog_NPC2_TemanKuliah",  meja = "Meja2", arah = Vector3.back,  cadangan = new Vector3(-1.70f, 0f,  0.95f) },
        new Rencana { file = "Ibu",          nama = "Ibu",      peran = "Keluarga", karakter = "i", hari = 5, sapaan = "Nak, Ibu mampir.", dialog = "Dialog_NPC3_Ibu",          meja = "Meja1", arah = Vector3.right, cadangan = new Vector3(-3.45f, 0f,  1.70f) },
    };

    [MenuItem("Last Commit/Jalur B/3. Pasang NPC", false, 3)]
    public static void Pasang()
    {
        var kedai = AlatEditor.Kedai();
        if (kedai == null) return;

        if (PasangKarakter.Ambil("a") == null) PasangKarakter.Buat();

        var grup = AlatEditor.Grup(kedai.transform, "NPC");
        var laporan = new List<string>();

        foreach (var r in NPC)
        {
            var karakter = PasangKarakter.Ambil(r.karakter);
            var dialog = AssetDatabase.LoadAssetAtPath<DialogueData>(DIALOG + "/" + r.dialog + ".asset");

            var data = AlatEditor.Aset<NPCData>(FOLDER + "/NPC_" + r.file + ".asset", d =>
            {
                d.namaTampil = r.nama;
                d.hariMuncul = r.hari;
            });
            // Kosong, atau modelnya sudah hilang (pack lama dihapus) -> pakai karakter baru.
            if (data.karakter == null || data.karakter.model == null) data.karakter = karakter;
            if (data.dialog == null) data.dialog = dialog;
            if (string.IsNullOrEmpty(data.peran) || data.peran == "Pelanggan tetap") data.peran = r.peran;
            // Sapaan bawaan diganti sapaan khas NPC ini; sapaan yang sudah kamu ubah tidak disentuh.
            if (data.sapaan == null || data.sapaan.Length == 0 ||
                (data.sapaan.Length == 1 && data.sapaan[0] == "Eh, sini sebentar."))
                data.sapaan = new[] { r.sapaan };
            EditorUtility.SetDirty(data);

            Vector3 pos = AlatEditor.SampingPerabot(kedai.transform, r.meja, r.arah, r.cadangan, out Vector3 tengahMeja);
            var titik = AlatEditor.Titik(grup, "Titik_" + r.file, pos, tengahMeja);

            var npc = AlatEditor.Komponen<NPCTetap>(titik.gameObject);
            Undo.RecordObject(npc, "Isi NPC");
            npc.data = data;

            string catatan = r.nama + " (hari " + r.hari + ", " + r.meja + ")";
            if (data.karakter == null) catatan += " — model BELUM ada";
            if (data.dialog == null) catatan += " — dialog BELUM ada, jalankan langkah 1";
            laporan.Add(catatan);
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(kedai.scene);
        EditorUtility.DisplayDialog("NPC terpasang",
            string.Join("\n", laporan) +
            "\n\nModel muncul saat Play, hanya di hari masing-masing.\n" +
            "Ganti model di aset " + FOLDER + "/NPC_*.\nJangan lupa Ctrl+S.", "OK");
    }
}
