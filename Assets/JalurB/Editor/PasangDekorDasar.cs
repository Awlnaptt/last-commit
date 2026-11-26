// Menu 4 Dekorasi > 4: dekor model Kenney, dimasukkan ke grup per hari (Kedai > DekorBertahap > HariN).
// Dekor buatan kode (kusen, lampu dinding, etalase, rak, payung, bangku, vas) dipasang menu 4 Dekorasi > 3.
// Alasan posisi dan hari tiap benda: Assets/JalurB/DEKOR_TATA_LETAK.md
//
// Menjalankan menu ini MENGGANTI isi lama: grup lama "DekorDasar" dan model Kenney di DekorBertahap
// dihapus dulu (bisa di-Undo), lalu dipasang ulang di posisi yang sudah dirapikan.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PasangDekorDasar
{
    const string FURNITUR = "Furniture Kit", MAKANAN = "Food Kit";
    const string PENANDA = "Kenney_";   // awalan nama supaya bisa dibersihkan saat dipasang ulang

    [MenuItem(MenuLastCommit.DEKORASI + "4. Pasang Dekor Kenney per Hari", false, 4)]
    public static void Pasang()
    {
        var kedai = AlatEditor.Kedai();
        if (kedai == null) return;
        var k = kedai.transform;

        PasangAtmosfer.Pastikan(PasangAtmosfer.Bagian.Dekor);   // pastikan DekorBertahap > Hari1..Hari5 sudah ada
        var bertahap = k.Find("DekorBertahap");

        // bersihkan versi lama
        var lama = k.Find("DekorDasar");
        if (lama != null) Undo.DestroyObjectImmediate(lama.gameObject);
        foreach (var t in bertahap.GetComponentsInChildren<Transform>(true))
            if (t != null && t.name.StartsWith(PENANDA)) Undo.DestroyObjectImmediate(t.gameObject);

        bool ada = AlatEditor.Ukur(AlatEditor.Cari(k, "Counter"), out var bc);
        float atas = ada ? bc.max.y : 1f;
        float xBarista = ada ? bc.max.x - 0.25f : 3.65f;   // sisi counter milik barista (dekat mesin)
        float zUtara = ada ? bc.max.z - 0.15f : 1.95f;

        var hasil = new List<string>();
        void T(int hari, string kit, string nama, Vector3 pos, float rotY, float tinggi, string ket)
        {
            var go = AlatEditor.Taruh(bertahap.Find("Hari" + hari), kit, nama, pos, rotY, tinggi);
            if (go == null) return;
            go.name = PENANDA + nama;
            hasil.Add("Hari " + hari + ": " + ket);
        }

        // ---------- Hari 1: perlengkapan wajib kedai baru ----------
        // Kipas di tengah area duduk (bukan di lorong), di antara tiga meja dan tidak menabrak lampu gantung.
        var kipas = AlatEditor.Taruh(bertahap.Find("Hari1"), FURNITUR, "ceilingFan", new Vector3(-2.3f, 2.62f, 1.2f), 0f, 0.38f);
        if (kipas != null && AlatEditor.Ukur(kipas.transform, out var bk))
        {
            var poros = new GameObject(PENANDA + "Kipas");
            Undo.RegisterCreatedObjectUndo(poros, "Kipas");
            poros.transform.SetParent(bertahap.Find("Hari1"), false);
            poros.transform.position = bk.center;
            kipas.transform.SetParent(poros.transform, true);
            Undo.AddComponent<Berputar>(poros);
            hasil.Add("Hari 1: kipas langit-langit di tengah area duduk");
        }
        T(1, FURNITUR, "trashcan", new Vector3(2.6f, 0f, -1.45f), 0f, 0.5f, "tempat sampah di ujung counter, sisi lorong keluar");
        T(1, MAKANAN, "cup-saucer", new Vector3(xBarista, atas, 0.32f), 0f, 0.07f, "cangkir siap pakai di samping mesin");
        T(1, MAKANAN, "cup-saucer", new Vector3(xBarista, atas, 0.15f), 30f, 0.07f, "cangkir kedua");

        // ---------- Hari 2: pelanggan tetap mulai datang ----------
        T(2, FURNITUR, "coatRackStanding", new Vector3(-4.62f, 0f, -3.6f), 0f, 1.75f, "gantungan jaket di pojok samping pintu");
        T(2, FURNITUR, "plantSmall1", new Vector3(xBarista, atas, zUtara), 0f, 0.3f, "tanaman kecil di ujung counter");

        // ---------- Hari 3: variasi menu ----------
        T(3, MAKANAN, "bag", new Vector3(4.72f, 0f, -1.6f), -20f, 0.45f, "karung biji kopi di bawah rak bahan");

        EditorSceneManager.MarkSceneDirty(kedai.scene);
        EditorUtility.DisplayDialog("Dekor bertahap",
            "Dipasang di Kedai > DekorBertahap:\n- " + string.Join("\n- ", hasil) +
            "\n\nDi Edit Mode semua hari terlihat sekaligus supaya mudah ditata.\nSaat Play, grup HariN baru muncul di hari ke-N.\nJangan lupa Ctrl+S.", "OK");
    }
}
