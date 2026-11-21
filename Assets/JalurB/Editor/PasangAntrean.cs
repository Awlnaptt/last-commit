// PasangAntrean.cs — langkah 4: pelanggan umum yang datang, antre di counter, lalu pulang.
//
// Menu: Last Commit > Jalur B > 4. Pasang Antrean Pelanggan
//
// Membuat Kedai > AlurPelanggan berisi:
//   TitikMasuk  : muncul dan hilang di sini (dalam pintu)
//   TitikLorong : jalur aman di antara meja
//   TitikAntre1..3 : tempat antre, menghadap counter
// Geser titiknya di Scene view kalau ada yang menembus perabot. Titik yang sudah ada tidak dipindah.
//
// Grup lama Kedai > Pelanggan (model statis dari Pasang Pelanggan) DINONAKTIFKAN, tidak dihapus.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PasangAntrean
{
    [MenuItem("Last Commit/Jalur B/4. Pasang Antrean Pelanggan", false, 4)]
    public static void Pasang()
    {
        var kedai = AlatEditor.Kedai();
        if (kedai == null) return;
        var k = kedai.transform;
        var laporan = new List<string>();

        // ---------- Titik ----------
        var grup = AlatEditor.Grup(k, "AlurPelanggan");

        Vector3 pintu = new Vector3(-3.5f, 0f, -4f);
        if (AlatEditor.Ukur(AlatEditor.Cari(k, "Pintu"), out var bp)) pintu = new Vector3(bp.center.x, 0f, bp.center.z);

        Vector3 counter = new Vector3(3.4f, 0f, 0.5f);
        if (AlatEditor.Ukur(AlatEditor.Cari(k, "Counter"), out var bc)) counter = new Vector3(bc.center.x, 0f, bc.center.z);

        var lorong = new Vector3(0.3f, 0f, -1.2f);
        var masuk = AlatEditor.Titik(grup, "TitikMasuk", pintu + new Vector3(0f, 0f, 0.7f), lorong);
        var tLorong = AlatEditor.Titik(grup, "TitikLorong", lorong, counter);

        // Pakai posisi titik antre lama kalau ada (sudah kamu atur 19 November).
        Vector3[] awal = { new Vector3(2.15f, 0f, 2.00f), new Vector3(2.15f, 0f, 1.05f), new Vector3(1.25f, 0f, 1.55f) };
        var antre = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            var lama = AlatEditor.Cari(k, "TitikAntre" + (i + 1));
            Vector3 pos = lama != null && !lama.IsChildOf(grup) ? new Vector3(lama.position.x, 0f, lama.position.z) : awal[i];
            antre[i] = AlatEditor.Titik(grup, "TitikAntre" + (i + 1), pos, new Vector3(counter.x, 0f, pos.z));
        }

        // ---------- Komponen ----------
        var q = AlatEditor.Komponen<AntreanPelanggan>(grup.gameObject);
        Undo.RecordObject(q, "Isi antrean");
        q.titikMasuk = masuk;
        q.titikLorong = tLorong;
        q.titikAntre = antre;

        // Model pelanggan umum: semua karakter pria/wanita biasa, kecuali yang dipakai NPC.
        var semua = PasangKarakter.Buat();
        var dipakaiNPC = new HashSet<ModelKarakter>();
        foreach (string guid in AssetDatabase.FindAssets("t:NPCData"))
        {
            var n = AssetDatabase.LoadAssetAtPath<NPCData>(AssetDatabase.GUIDToAssetPath(guid));
            if (n != null && n.karakter != null) dipakaiNPC.Add(n.karakter);
        }

        // Susun ulang daftar model kalau kosong atau ada yang modelnya sudah hilang.
        bool perluUlang = q.pilihanModel == null || q.pilihanModel.Count == 0;
        if (!perluUlang)
            foreach (var m in q.pilihanModel)
                if (m == null || m.model == null) { perluUlang = true; break; }

        if (perluUlang)
        {
            q.pilihanModel = new List<ModelKarakter>();
            foreach (var m in semua)
            {
                if (!m.pelangganUmum || dipakaiNPC.Contains(m)) continue;
                if (m.jalan == null || m.diam == null)
                {
                    laporan.Add(m.name + " dilewati: tidak punya animasi (cek Rig FBX-nya, harus Generic)");
                    continue;
                }
                q.pilihanModel.Add(m);
            }
        }
        laporan.Add(q.pilihanModel.Count + " model pelanggan umum");

        if (q.kalimat == null)
            q.kalimat = AlatEditor.Aset<KalimatPelanggan>(AlatEditor.DATA + "/Kalimat_Pelanggan.asset", d => { });
        laporan.Add("Kalimat pelanggan: " + q.kalimat.name);

        // ---------- Model statis lama ----------
        var pelangganLama = k.Find("Pelanggan");
        if (pelangganLama != null && pelangganLama.gameObject.activeSelf)
        {
            Undo.RecordObject(pelangganLama.gameObject, "Matikan pelanggan lama");
            pelangganLama.gameObject.SetActive(false);
            laporan.Add("Grup lama 'Pelanggan' dinonaktifkan (tidak dihapus)");
        }

        EditorSceneManager.MarkSceneDirty(kedai.scene);
        EditorUtility.DisplayDialog("Antrean pelanggan terpasang",
            string.Join("\n", laporan) +
            "\n\nTitik ada di Kedai > AlurPelanggan. Geser kalau jalurnya menembus meja.\n" +
            "Model pelanggan bisa diatur di komponen AntreanPelanggan.\nJangan lupa Ctrl+S.", "OK");
    }
}
