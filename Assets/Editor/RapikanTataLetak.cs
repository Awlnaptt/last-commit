// RapikanTataLetak.cs
// Memperbaiki dua hal tanpa membongkar model yang sudah terpasang:
//   1. Jendela dilebarkan dan ditinggikan
//   2. Rak buku digeser ke kanan supaya tidak menutupi jendela
//
// Taruh di: Assets/Editor/RapikanTataLetak.cs
// Jalankan lewat menu: Last Commit > Rapikan Tata Letak
//
// CATATAN: kalau suatu saat kamu menekan "Bangun ulang kotak blockout",
// jalankan alat ini lagi sesudahnya, karena bangun ulang memakai ukuran lama.

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class RapikanTataLetak
{
    // Jendela baru: membentang dari x -4.0 sampai x +1.2, tinggi 0.8 sampai 2.5
    const float JENDELA_KIRI  = -4.0f;
    const float JENDELA_KANAN =  1.2f;
    const float JENDELA_BAWAH =  0.8f;
    const float JENDELA_ATAS  =  2.5f;

    const float RUANG_KIRI  = -5f;
    const float RUANG_KANAN =  5f;
    const float TINGGI      =  3f;
    const float Z_DINDING   =  4.05f;

    // Posisi baru rak buku, digeser ke kanan supaya lepas dari jendela
    const float RAK_X = 3.1f;

    [MenuItem("Last Commit/Rapikan Tata Letak")]
    public static void Rapikan()
    {
        var akar = GameObject.Find("Kedai");
        if (akar == null)
        {
            EditorUtility.DisplayDialog("Objek 'Kedai' tidak ada",
                "Buka dulu scene Kedai yang sudah ada blockout-nya.", "OK");
            return;
        }

        float lebarJendela = JENDELA_KANAN - JENDELA_KIRI;          // 5.2
        float tengahJendelaX = (JENDELA_KIRI + JENDELA_KANAN) * 0.5f; // -1.4
        float tinggiJendela = JENDELA_ATAS - JENDELA_BAWAH;          // 1.7
        float tengahJendelaY = (JENDELA_BAWAH + JENDELA_ATAS) * 0.5f; // 1.65

        // --- Dinding utara dipotong ulang mengelilingi jendela baru ---

        float lebarKiri = JENDELA_KIRI - RUANG_KIRI;                 // 1.0
        Atur(akar, "Dinding_Utara_Kiri",
            new Vector3(RUANG_KIRI + lebarKiri * 0.5f, TINGGI * 0.5f, Z_DINDING),
            new Vector3(lebarKiri, TINGGI, 0.1f));

        float lebarKanan = RUANG_KANAN - JENDELA_KANAN;              // 3.8
        Atur(akar, "Dinding_Utara_Kanan",
            new Vector3(JENDELA_KANAN + lebarKanan * 0.5f, TINGGI * 0.5f, Z_DINDING),
            new Vector3(lebarKanan, TINGGI, 0.1f));

        Atur(akar, "Dinding_Utara_Bawah",
            new Vector3(tengahJendelaX, JENDELA_BAWAH * 0.5f, Z_DINDING),
            new Vector3(lebarJendela, JENDELA_BAWAH, 0.1f));

        float tinggiAtas = TINGGI - JENDELA_ATAS;                    // 0.5
        Atur(akar, "Dinding_Utara_Atas",
            new Vector3(tengahJendelaX, JENDELA_ATAS + tinggiAtas * 0.5f, Z_DINDING),
            new Vector3(lebarJendela, tinggiAtas, 0.1f));

        // --- Kaca jendelanya sendiri ---

        var jendela = Cari(akar, "JendelaHujan");
        if (jendela != null)
        {
            Undo.RecordObject(jendela, "Rapikan jendela");
            jendela.position = new Vector3(tengahJendelaX, tengahJendelaY, 4f);
            jendela.localScale = new Vector3(lebarJendela, tinggiJendela, 0.06f);

            // Terangkan sedikit supaya kacanya terbaca sebagai jendela, bukan lubang hitam
            var r = jendela.GetComponent<Renderer>();
            if (r != null && r.sharedMaterial != null)
            {
                var m = r.sharedMaterial;
                var warna = new Color(0.33f, 0.52f, 0.70f);
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", warna);
                m.color = warna;
            }
        }

        // --- Geser rak buku ke kanan, keluar dari depan jendela ---

        var rak = Cari(akar, "RakBuku");
        if (rak != null)
        {
            Undo.RecordObject(rak, "Geser rak buku");
            var p = rak.position;
            rak.position = new Vector3(RAK_X, p.y, p.z);
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("Tata letak dirapikan: jendela jadi " + lebarJendela.ToString("0.#") +
                  " m x " + tinggiJendela.ToString("0.#") + " m, rak buku digeser ke x = " + RAK_X);

        EditorUtility.DisplayDialog("Selesai",
            "Jendela dilebarkan jadi " + lebarJendela.ToString("0.#") + " meter dan ditinggikan.\n" +
            "Rak buku digeser ke kanan supaya tidak menutupi jendela.\n\n" +
            "Jangan lupa Ctrl+S.", "OK");
    }

    static void Atur(GameObject akar, string nama, Vector3 posisi, Vector3 skala)
    {
        var t = Cari(akar, nama);
        if (t == null)
        {
            Debug.LogWarning("Objek '" + nama + "' tidak ketemu, dilewati.");
            return;
        }

        Undo.RecordObject(t, "Rapikan " + nama);
        t.position = posisi;
        t.localScale = skala;
    }

    static Transform Cari(GameObject akar, string nama)
    {
        foreach (var t in akar.GetComponentsInChildren<Transform>(true))
            if (t.name == nama) return t;
        return null;
    }
}
