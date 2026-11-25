using System.Collections.Generic;
using UnityEngine;
using static AlatKedai;

// Mengganti dekor yang masih berupa kotak polos (atau rak Kenney yang kosong) dengan aset buatan kode,
// dan menambal celah struktur. Dipanggil PasangAtmosfer (menu 4 Dekorasi > 2, dan otomatis saat Play).
// Objek aslinya TIDAK dihapus: hanya tampilannya (renderer) yang dimatikan, jadi nama, posisi,
// collider, dan logika yang memakainya tetap utuh.
public static class GantiModelKosong
{
    public static void Jalankan(Transform kedai, List<string> laporan)
    {
        TambalSudut(kedai, laporan);
        IsiRakBuku(kedai, laporan);
        GantiLampuGantung(kedai, laporan);
        if (TempelGambar(kedai, "Poster_Timur", "T_Poster_Kopi")) laporan.Add("Poster_Timur: gambar \"Kopi Tubruk\"");
        if (TempelGambar(kedai, "Poster_Selatan", "T_Poster_Nusantara")) laporan.Add("Poster_Selatan: gambar \"Biji Kopi Nusantara\"");
        if (TempelGambar(kedai, "Keset", "T_Keset")) laporan.Add("Keset: tulisan \"Selamat Datang\"");
        GantiTumpukanBuku(kedai, laporan);
    }

    // Dinding utara/selatan sepanjang x -5..5 dan dinding barat/timur sepanjang z -4..4, tebal 0,1 m.
    // Akibatnya di keempat pojok luar ada lubang 0,1 x 0,1 m setinggi dinding. Ditambal tiang pojok.
    static void TambalSudut(Transform kedai, List<string> laporan)
    {
        var struktur = Cari(kedai, "Struktur");
        var contoh = Cari(kedai, "Dinding_Barat");
        if (struktur == null || contoh == null) return;
        var bahan = contoh.GetComponent<Renderer>() != null ? contoh.GetComponent<Renderer>().sharedMaterial : null;

        int n = 0;
        foreach (var sx in new[] { -1f, 1f })
        foreach (var sz in new[] { -1f, 1f })
        {
            string nama = $"TiangSudut_{(sx < 0 ? "Barat" : "Timur")}{(sz < 0 ? "Selatan" : "Utara")}";
            if (Cari(kedai, nama) != null) continue;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = nama;
            go.transform.SetParent(struktur, false);
            go.transform.position = new Vector3(5.05f * sx, 1.5f, 4.05f * sz);
            go.transform.localScale = new Vector3(0.12f, 3.2f, 0.12f);
            if (bahan != null) go.GetComponent<Renderer>().sharedMaterial = bahan;
            n++;
        }
        if (n > 0) laporan.Add($"{n} tiang pojok menambal lubang di sudut dinding");
    }

    // Rak Kenney (RakBuku, juga "bookcase..." dari upgrade) kosong: diganti rak berisi buku.
    static void IsiRakBuku(Transform kedai, List<string> laporan)
    {
        var target = new List<Transform>();
        var rak = Cari(kedai, "RakBuku");
        if (rak != null) target.Add(rak);
        foreach (var t in kedai.GetComponentsInChildren<Transform>(true))
            if (t.name.StartsWith("bookcase") && !target.Contains(t)) target.Add(t);

        foreach (var t in target)
        {
            if (t.Find("RakBukuKode") != null) continue;
            if (!Ukur(t, out var b)) continue;
            MatikanTampilan(t);

            // Menghadap ke tengah ruangan, dari dinding terdekat.
            bool dindingUtaraSelatan = Mathf.Abs(b.center.z) / 4f > Mathf.Abs(b.center.x) / 5f;
            Vector3 hadap = dindingUtaraSelatan ? new Vector3(0f, 0f, -Mathf.Sign(b.center.z)) : new Vector3(-Mathf.Sign(b.center.x), 0f, 0f);
            float lebar = dindingUtaraSelatan ? b.size.x : b.size.z;
            float dalam = dindingUtaraSelatan ? b.size.z : b.size.x;

            var go = new GameObject("RakBukuKode");
            go.transform.SetParent(t, false);
            go.transform.position = new Vector3(b.center.x, b.min.y, b.center.z);
            go.transform.rotation = Quaternion.LookRotation(hadap);
            go.transform.localScale = new Vector3(1f / t.lossyScale.x, 1f / t.lossyScale.y, 1f / t.lossyScale.z);
            var r = go.AddComponent<RakBukuIsi>();
            r.lebar = lebar; r.tinggi = b.size.y; r.dalam = Mathf.Clamp(dalam, 0.25f, 0.5f);
            r.jumlahRuang = Mathf.Clamp(Mathf.RoundToInt(b.size.y / 0.38f), 2, 6);
            r.seed = Mathf.Abs(t.name.GetHashCode()) % 1000;
            r.BangunUlang();
            laporan.Add($"{t.name}: rak buku berisi buku");
        }
    }

    // Lampu gantung sementara (kotak Kabel + kotak Kap) diganti lampu bulat dari kode.
    static void GantiLampuGantung(Transform kedai, List<string> laporan)
    {
        int n = 0;
        foreach (var t in kedai.GetComponentsInChildren<Transform>(true))
        {
            if (!t.name.StartsWith("LampuGantung_") || t.Find("LampuKode") != null) continue;
            var kap = t.Find("Kap");
            if (kap == null || !Ukur(kap, out var bk)) continue;

            foreach (var nama in new[] { "Kap", "Kabel" })
            {
                var anak = t.Find(nama);
                var rend = anak != null ? anak.GetComponent<Renderer>() : null;
                if (rend != null) rend.enabled = false;
            }

            var go = new GameObject("LampuKode");
            go.transform.SetParent(t, false);
            go.transform.position = new Vector3(bk.center.x, 3.0f, bk.center.z);
            go.transform.rotation = Quaternion.identity;
            var l = go.AddComponent<LampuGantungKode>();
            l.panjangKabel = Mathf.Max(0.1f, 3.0f - bk.max.y);
            l.tinggiKap = bk.size.y;
            l.BangunUlang();
            n++;
        }
        if (n > 0) laporan.Add($"{n} lampu gantung diganti model buatan kode");
    }

    // Tempel gambar di sisi kotak yang menghadap ke dalam ruangan (atau ke atas, kalau kotaknya tipis mendatar).
    // Kotaknya tetap terlihat sebagai bingkai/alas. Gambar dibuat SAUDARA kotak (bukan anaknya)
    // supaya tidak ikut skala kotak, dan ikut menyala/mati bersama grupnya.
    static bool TempelGambar(Transform kedai, string namaKotak, string tekstur)
    {
        var kotak = Cari(kedai, namaKotak);
        string namaGambar = namaKotak + "_Gambar";
        if (kotak == null || Cari(kedai, namaGambar) != null || !Ukur(kotak, out var b)) return false;

        Vector3 s = b.size;
        Vector3 pos; Quaternion rot; float lebar, tinggi;
        if (s.y <= s.x && s.y <= s.z)
        {
            pos = new Vector3(b.center.x, b.max.y + 0.002f, b.center.z);
            rot = Quaternion.LookRotation(Vector3.up, Vector3.forward);   // tulisan terbaca dari arah pintu
            lebar = s.x; tinggi = s.z;
        }
        else
        {
            bool tipisX = s.x <= s.z;
            Vector3 n = tipisX ? new Vector3(-Mathf.Sign(b.center.x), 0f, 0f) : new Vector3(0f, 0f, -Mathf.Sign(b.center.z));
            pos = b.center + Vector3.Scale(n, b.extents) + n * 0.003f;
            rot = Quaternion.LookRotation(n);
            lebar = tipisX ? s.z : s.x; tinggi = s.y;
        }

        var go = new GameObject(namaGambar);
        go.transform.SetParent(kotak.parent, false);
        go.transform.SetPositionAndRotation(pos, rot);
        var p = go.AddComponent<PanelGambar>();
        p.lebar = lebar * 0.92f; p.tinggi = tinggi * 0.92f;
        p.namaTekstur = tekstur; p.namaTeksturCahaya = ""; p.halus = 0.1f;
        p.BangunUlang();
        return true;
    }

    static void GantiTumpukanBuku(Transform kedai, List<string> laporan)
    {
        var kotak = Cari(kedai, "TumpukanBuku");
        if (kotak == null || Cari(kedai, "TumpukanBuku_Kode") != null || !Ukur(kotak, out var b)) return;
        MatikanTampilan(kotak);
        var go = new GameObject("TumpukanBuku_Kode");
        go.transform.SetParent(kotak.parent, false);
        go.transform.SetPositionAndRotation(new Vector3(b.center.x, b.min.y, b.center.z), Quaternion.Euler(0f, 15f, 0f));
        go.AddComponent<BukuTumpuk>().BangunUlang();
        laporan.Add("TumpukanBuku: tumpukan buku dari kode");
    }

    // ---------- pembantu ----------

    static void MatikanTampilan(Transform t)
    {
        foreach (var r in t.GetComponentsInChildren<Renderer>(true))
            if (r.GetComponentInParent<MeshDariKode>() == null) r.enabled = false;
    }
}
