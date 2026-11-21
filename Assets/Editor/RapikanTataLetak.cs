// RapikanTataLetak.cs — versi 2
//
// Taruh di: Assets/Editor/RapikanTataLetak.cs  (timpa yang lama)
// Jalankan lewat menu: Last Commit > Rapikan Tata Letak
//
// Empat perbaikan, semuanya menghitung dari posisi objek YANG SEKARANG,
// bukan dari angka tetap. Jadi kalau kamu menggeser meja lagi besok,
// jalankan ulang dan semuanya ikut menyesuaikan.
//
//   1. Jendela dilebarkan jadi 6 x 1,7 m, dinding utara dipotong ulang
//   2. Gelas didudukkan ulang di atas mejanya masing-masing
//   3. Lampu gantung dipindah ke atas mejanya
//   4. Tanaman diubah jadi ukuran pot lantai, bukan pot meja
//
// Yang TIDAK disentuh: posisi meja, kursi, rak buku, counter. Itu tata
// ruang, bukan kesalahan.

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class RapikanTataLetak
{
    // Jendela baru. Batas kanan dijaga di 1,8 supaya tidak menabrak rak buku
    // yang berdiri di dinding utara sekitar x 2,1 sampai 4,1.
    const float JENDELA_KIRI  = -4.2f;
    const float JENDELA_KANAN =  1.8f;
    const float JENDELA_BAWAH =  0.85f;
    const float JENDELA_ATAS  =  2.55f;

    const float RUANG_KIRI  = -5f;
    const float RUANG_KANAN =  5f;
    const float TINGGI      =  3f;
    const float Z_DINDING   =  4.05f;

    // Tinggi tanaman pot lantai, dari dasar pot sampai pucuk daun
    const float TINGGI_TANAMAN = 1.35f;

    [MenuItem("Last Commit/Rapikan Tata Letak")]
    public static void Rapikan()
    {
        var akar = GameObject.Find("Kedai");
        if (akar == null)
        {
            EditorUtility.DisplayDialog("Objek 'Kedai' tidak ada",
                "Buka dulu scene Kedai.", "OK");
            return;
        }

        var laporan = new System.Text.StringBuilder();

        RapikanJendela(akar, laporan);
        RapikanGelas(akar, laporan);
        RapikanLampu(akar, laporan);
        RapikanTanaman(akar, laporan);

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("Rapikan Tata Letak:\n" + laporan);

        EditorUtility.DisplayDialog("Selesai",
            laporan.ToString() + "\nJangan lupa Ctrl+S.", "OK");
    }

    // ---------- 1. Jendela ----------

    static void RapikanJendela(GameObject akar, System.Text.StringBuilder log)
    {
        float lebar  = JENDELA_KANAN - JENDELA_KIRI;
        float tinggi = JENDELA_ATAS - JENDELA_BAWAH;
        float tengahX = (JENDELA_KIRI + JENDELA_KANAN) * 0.5f;
        float tengahY = (JENDELA_BAWAH + JENDELA_ATAS) * 0.5f;

        float lebarKiri = JENDELA_KIRI - RUANG_KIRI;
        Atur(akar, "Dinding_Utara_Kiri",
            new Vector3(RUANG_KIRI + lebarKiri * 0.5f, TINGGI * 0.5f, Z_DINDING),
            new Vector3(lebarKiri, TINGGI, 0.1f));

        float lebarKanan = RUANG_KANAN - JENDELA_KANAN;
        Atur(akar, "Dinding_Utara_Kanan",
            new Vector3(JENDELA_KANAN + lebarKanan * 0.5f, TINGGI * 0.5f, Z_DINDING),
            new Vector3(lebarKanan, TINGGI, 0.1f));

        Atur(akar, "Dinding_Utara_Bawah",
            new Vector3(tengahX, JENDELA_BAWAH * 0.5f, Z_DINDING),
            new Vector3(lebar, JENDELA_BAWAH, 0.1f));

        float tinggiAtas = TINGGI - JENDELA_ATAS;
        Atur(akar, "Dinding_Utara_Atas",
            new Vector3(tengahX, JENDELA_ATAS + tinggiAtas * 0.5f, Z_DINDING),
            new Vector3(lebar, tinggiAtas, 0.1f));

        var kaca = Cari(akar, "JendelaHujan");
        if (kaca != null)
        {
            Undo.RecordObject(kaca, "Rapikan jendela");
            kaca.position = new Vector3(tengahX, tengahY, 4f);
            kaca.localScale = new Vector3(lebar, tinggi, 0.06f);

            var r = kaca.GetComponent<Renderer>();
            if (r != null && r.sharedMaterial != null)
            {
                var m = r.sharedMaterial;
                var warna = new Color(0.33f, 0.52f, 0.70f);
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", warna);
                m.color = warna;
            }
        }

        log.AppendLine("Jendela: " + lebar.ToString("0.#") + " x " + tinggi.ToString("0.#") +
                       " m, dari x " + JENDELA_KIRI.ToString("0.#") +
                       " sampai " + JENDELA_KANAN.ToString("0.#") + ".");
    }

    // ---------- 2. Gelas di atas meja ----------

    static void RapikanGelas(GameObject akar, System.Text.StringBuilder log)
    {
        int n = 0;
        n += Dudukkan(akar, "Gelas_Meja1", "Meja1");
        n += Dudukkan(akar, "Gelas_Meja2", "Meja2");
        n += Dudukkan(akar, "Gelas_Meja3", "Meja3");
        n += Dudukkan(akar, "TumpukanBuku", "RakBuku");

        // Mesin kopi dan toples sama-sama di counter. Tanpa geseran keduanya
        // dipusatkan di titik yang sama dan saling menindih.
        n += Dudukkan(akar, "MesinKopi", "Counter", 0.3f);
        n += Dudukkan(akar, "ToplesBiji", "Counter", -0.3f);
        n += Dudukkan(akar, "Laptop", "MejaSudut");

        log.AppendLine(n + " benda kecil didudukkan ulang di atas permukaannya.");
    }

    // ---------- 3. Lampu gantung di atas meja ----------

    static void RapikanLampu(GameObject akar, System.Text.StringBuilder log)
    {
        int n = 0;
        n += PindahkanKeAtas(akar, "LampuGantung_Meja1", "Meja1");
        n += PindahkanKeAtas(akar, "LampuGantung_Meja2", "Meja2");
        n += PindahkanKeAtas(akar, "LampuGantung_MejaSudut", "MejaSudut");
        n += PindahkanKeAtas(akar, "LampuGantung_Counter", "Counter");

        log.AppendLine(n + " lampu gantung dipindah ke atas perabotnya. " +
                       "LampuGantung_Seberang sengaja dibiarkan karena bukan lampu meja.");
    }

    // ---------- 4. Tanaman pot lantai ----------

    static void RapikanTanaman(GameObject akar, System.Text.StringBuilder log)
    {
        int n = 0;
        n += SetinggiLantai(akar, "Tanaman_Pot1");
        n += SetinggiLantai(akar, "Tanaman_Pot2");

        log.AppendLine(n + " tanaman diubah jadi tinggi " +
                       TINGGI_TANAMAN.ToString("0.##") + " m dan didudukkan di lantai.");
    }

    static int SetinggiLantai(GameObject akar, string nama)
    {
        var t = Cari(akar, nama);
        if (t == null) { Debug.LogWarning("'" + nama + "' tidak ketemu."); return 0; }

        Undo.RecordObject(t, "Ukur ulang " + nama);

        // Ukur pada skala 1 dulu, supaya faktornya tidak menumpuk tiap kali dijalankan
        t.localScale = Vector3.one;

        Bounds b;
        if (!HitungBounds(t.gameObject, out b) || b.size.y < 0.0001f)
        {
            Debug.LogWarning("'" + nama + "' tidak punya Renderer, dilewati.");
            return 0;
        }

        float f = TINGGI_TANAMAN / b.size.y;
        if (f < 0.001f || f > 1000f) { Debug.LogWarning("Skala " + nama + " tidak masuk akal, dilewati."); return 0; }

        t.localScale = Vector3.one * f;

        // dudukkan dasar pot tepat di lantai
        Bounds b2;
        if (HitungBounds(t.gameObject, out b2))
            t.position += new Vector3(0f, -b2.min.y, 0f);

        return 1;
    }

    // ---------- Pembantu ----------

    static int Dudukkan(GameObject akar, string namaAtas, string namaBawah, float bagian = 0f)
    {
        var atas = Cari(akar, namaAtas);
        var bawah = Cari(akar, namaBawah);
        if (atas == null || bawah == null) return 0;

        Bounds bA, bB;
        if (!HitungBounds(atas.gameObject, out bA)) return 0;
        if (!HitungBounds(bawah.gameObject, out bB)) return 0;

        Undo.RecordObject(atas, "Dudukkan " + namaAtas);
        atas.position += new Vector3(
            bB.center.x - bA.center.x,
            bB.max.y - bA.min.y,
            bB.center.z - bA.center.z) + Geseran(bB, bA, bagian);

        return 1;
    }


    // Geser benda di sepanjang sisi terpanjang penopangnya, supaya dua benda
    // di permukaan yang sama tidak saling menindih. bagian: -0.5 sampai 0.5
    // dari panjang yang tersisa setelah dikurangi ukuran bendanya sendiri.
    static Vector3 Geseran(Bounds penopang, Bounds benda, float bagian)
    {
        if (Mathf.Abs(bagian) < 0.0001f) return Vector3.zero;

        bool panjangKeZ = penopang.size.z >= penopang.size.x;
        float ruang = panjangKeZ
            ? penopang.size.z - benda.size.z
            : penopang.size.x - benda.size.x;

        if (ruang <= 0f) return Vector3.zero;   // tidak muat, biarkan di tengah

        float d = Mathf.Clamp(bagian, -0.5f, 0.5f) * ruang;
        return panjangKeZ ? new Vector3(0f, 0f, d) : new Vector3(d, 0f, 0f);
    }

    static int PindahkanKeAtas(GameObject akar, string namaLampu, string namaPerabot)
    {
        var lampu = Cari(akar, namaLampu);
        var perabot = Cari(akar, namaPerabot);
        if (lampu == null || perabot == null) return 0;

        Bounds b;
        if (!HitungBounds(perabot.gameObject, out b)) return 0;

        Undo.RecordObject(lampu, "Pindahkan " + namaLampu);
        var p = lampu.position;
        lampu.position = new Vector3(b.center.x, p.y, b.center.z);

        return 1;
    }

    static void Atur(GameObject akar, string nama, Vector3 posisi, Vector3 skala)
    {
        var t = Cari(akar, nama);
        if (t == null) { Debug.LogWarning("Objek '" + nama + "' tidak ketemu, dilewati."); return; }

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

    static bool HitungBounds(GameObject go, out Bounds hasil)
    {
        hasil = new Bounds(go.transform.position, Vector3.zero);
        var rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return false;

        hasil = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) hasil.Encapsulate(rends[i].bounds);
        return true;
    }
}
