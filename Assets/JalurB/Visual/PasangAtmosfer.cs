using System.Collections.Generic;
using UnityEngine;
using static AlatKedai;

// Memasang suasana, dekor buatan kode, dan objek cerita. Dipanggil dari dua tempat:
//   - menu editor Last Commit > 4 Dekorasi / 5 Suasana (per bagian; lalu Ctrl+S supaya tersimpan)
//   - PasangOtomatis saat Play (semua bagian, jaga-jaga kalau menunya belum dijalankan / belum disimpan)
// Yang SUDAH ADA tidak diubah, jadi posisi hasil geseranmu dan teks yang kamu tulis ulang aman.
public static class PasangAtmosfer
{
    [System.Flags]
    public enum Bagian
    {
        Suasana = 1,        // cuaca, post-processing, layar "Hari N", uap kopi, kota luar jendela
        GantiModel = 2,     // tiang pojok, rak berisi buku, lampu gantung, poster, keset, tumpukan buku
        Dekor = 4,          // kusen pintu dan dekor bertahap per hari
        ObjekCerita = 8,    // foto wisuda dan catatan Ibu
        Semua = 15,
    }

    public static List<string> Pastikan(Bagian bagian = Bagian.Semua)
    {
        var laporan = new List<string>();
        var kedai = GameObject.Find("Kedai");
        var managers = GameObject.Find("JalurB_Managers");

        bool suasana = (bagian & Bagian.Suasana) != 0;
        if (suasana && managers != null)
        {
            if (Tambah<CuacaHarian>(managers)) laporan.Add("CuacaHarian di JalurB_Managers");
            if (Tambah<AtmosferPasca>(managers)) laporan.Add("AtmosferPasca (post-processing) di JalurB_Managers");
            if (Tambah<TransisiHari>(managers)) laporan.Add("TransisiHari (layar \"Hari N\") di JalurB_Managers");
        }
        else if (suasana) laporan.Add("JalurB_Managers TIDAK ditemukan (jalankan 2 Sistem > Pasang Manager dan Data)");

        if (kedai == null) { laporan.Add("Kedai TIDAK ditemukan"); return laporan; }
        var k = kedai.transform;

        // Tambal lubang pojok dinding, isi rak buku kosong, ganti dekor kotak polos dengan aset kode.
        if ((bagian & Bagian.GantiModel) != 0) GantiModelKosong.Jalankan(k, laporan);

        if (suasana)
        {
            var mesin = Cari(k, "MesinKopi");
            if (mesin != null && Tambah<UapKopi>(mesin.gameObject)) laporan.Add("UapKopi di MesinKopi");

            if (Cari(k, "LuarJendela") == null)
            {
                var luar = Baru("LuarJendela", k, Vector3.zero, 0f);
                luar.AddComponent<KotaLuar>();
                laporan.Add("LuarJendela: kota di luar jendela utara");
            }
        }

        if ((bagian & Bagian.Dekor) != 0) PasangDekor(k, laporan);
        if ((bagian & Bagian.ObjekCerita) != 0) PasangObjekCerita(k, laporan);
        return laporan;
    }

    static void PasangDekor(Transform k, List<string> laporan)
    {
        // ---------- Dekor dari kode, muncul bertahap per hari ----------
        // Alasan posisi dan hari tiap benda: Assets/JalurB/DEKOR_TATA_LETAK.md
        var dekor = Cari(k, "DekorKode");
        if (dekor == null) dekor = Baru("DekorKode", k, Vector3.zero, 0f).transform;      // selalu ada
        var bertahap = Cari(k, "DekorBertahap");
        if (bertahap == null) bertahap = Baru("DekorBertahap", k, Vector3.zero, 0f).transform;
        Tambah<DekorBertahap>(bertahap.gameObject);
        Transform H(int n)
        {
            var t = bertahap.Find("Hari" + n);
            return t != null ? t : Baru("Hari" + n, bertahap, Vector3.zero, 0f).transform;
        }

        Ukur(Cari(k, "Counter"), out var bc);
        bool adaCounter = bc.size.x > 0f;

        // Selalu: kusen pintu menutup celah di sekitar daun pintu.
        Perabot<KusenPintu>(k, dekor, "KusenPintu", new Vector3(-3.5f, 0f, -4.05f), 0f, laporan, "KusenPintu (menutup celah pintu)");

        // Hari 1: yang wajib ada di kedai yang baru buka.
        if (!Pindahkan(k, "PapanKapur", H(1)) && PasangPapanKapur(k, H(1))) laporan.Add("Hari 1: PapanKapur (menu + harga)");
        if (!Pindahkan(k, "JamDinding", H(1)))
        {
            Baru("JamDinding", H(1), new Vector3(-4.995f, 2.3f, 0.25f), 90f).AddComponent<JamDinding>().BangunUlang();
            laporan.Add("Hari 1: JamDinding");
        }

        // Hari 2 (pelanggan tetap): lampu dinding di meja yang belum punya lampu gantung.
        Perabot<LampuDinding>(k, H(2), "LampuDinding_Meja1", new Vector3(-4.995f, 1.72f, 2.52f), 90f, laporan, "Hari 2: LampuDinding di atas Meja1");

        // Hari 3 (hujan deras, variasi menu): etalase kue, rak bahan baku, tempat payung.
        Vector3 posEtalase = adaCounter ? new Vector3(bc.min.x + 0.2f, bc.max.y, bc.max.z - 0.3f) : new Vector3(3.1f, 1f, 1.8f);
        Perabot<EtalaseKue>(k, H(3), "EtalaseKue", posEtalase, -90f, laporan, "Hari 3: EtalaseKue di ujung counter dekat titik pesan");
        Perabot<RakDinding>(k, H(3), "RakDinding", new Vector3(4.995f, 1.3f, -1.6f), -90f, laporan, "Hari 3: RakDinding toples kopi di belakang counter");
        Perabot<TempatPayung>(k, H(3), "TempatPayung", new Vector3(-2.55f, 0f, -3.72f), 0f, laporan, "Hari 3: TempatPayung di samping pintu");

        // Hari 4 (tahap 2, hangat dan penuh): papan neon dan bangku jendela.
        if (!Pindahkan(k, "PapanNeon", H(4)))
        {
            var neon = Baru("PapanNeon", H(4), new Vector3(3.15f, 2.47f, 3.99f), 180f);
            var p = neon.AddComponent<PanelGambar>();
            p.lebar = 1.4f; p.tinggi = 0.4375f;        // rasio gambar 1024 x 320
            p.namaTekstur = "T_Neon"; p.namaTeksturCahaya = "T_Neon_Cahaya";
            p.BangunUlang();
            laporan.Add("Hari 4: PapanNeon di atas rak buku");
        }
        Perabot<BangkuJendela>(k, H(4), "BangkuJendela", new Vector3(1.0f, 0f, 3.77f), 180f, laporan, "Hari 4: BangkuJendela di bawah jendela");

        // Hari 5 (Ibu datang): vas bunga di meja Ibu.
        Vector3 posVas = Ukur(Cari(k, "Meja1"), out var bm)
            ? new Vector3(bm.center.x - 0.25f, bm.max.y, bm.center.z + 0.2f) : new Vector3(-3.85f, 0.76f, 2.75f);
        Perabot<VasBunga>(k, H(5), "VasBunga", posVas, 0f, laporan, "Hari 5: VasBunga di Meja1 (meja Ibu)");
    }

    static void PasangObjekCerita(Transform k, List<string> laporan)
    {
        var grup = Cari(k, "ObjekCerita");
        if (grup == null) grup = Baru("ObjekCerita", k, Vector3.zero, 0f).transform;

        if (Cari(k, "FotoWisuda") == null)
        {
            // Dinding selatan bagian dalam (z = -4), di atas meja laptop. Panah biru menghadap ke dalam ruangan.
            var foto = Baru("FotoWisuda", grup, new Vector3(3.6f, 1.55f, -3.995f), 0f);
            foto.AddComponent<BingkaiFoto>();
            foto.AddComponent<BoxCollider>();
            IsiFoto(foto.AddComponent<ObjekCerita>());
            foto.GetComponent<BingkaiFoto>().BangunUlang();
            laporan.Add("FotoWisuda di dinding selatan");
        }

        if (Cari(k, "CatatanIbu") == null)
        {
            var catatan = Baru("CatatanIbu", grup, PosisiCatatan(k), 160f);
            catatan.AddComponent<KertasCatatan>();
            catatan.AddComponent<BoxCollider>();
            IsiCatatan(catatan.AddComponent<ObjekCerita>());
            catatan.GetComponent<KertasCatatan>().BangunUlang();
            laporan.Add("CatatanIbu di meja laptop");
        }
    }

    // Papan kapur menempel di sisi PapanMenu yang menghadap ke tengah ruangan, seukuran papannya.
    static bool PasangPapanKapur(Transform kedai, Transform dekor)
    {
        if (!Ukur(Cari(kedai, "PapanMenu"), out var b)) return false;

        // sumbu paling tipis = arah hadap papan; arahkan ke tengah ruangan
        Vector3 s = b.size, ke = Vector3.zero - b.center;
        Vector3 normal = s.x <= s.z ? new Vector3(Mathf.Sign(ke.x), 0f, 0f) : new Vector3(0f, 0f, Mathf.Sign(ke.z));
        float lebar = s.x <= s.z ? s.z : s.x;
        Vector3 muka = b.center + Vector3.Scale(normal, b.extents) + normal * 0.006f;

        var go = Baru("PapanKapur", dekor, muka, 0f);
        go.transform.rotation = Quaternion.LookRotation(normal);
        var p = go.AddComponent<PanelGambar>();
        p.lebar = lebar * 0.98f; p.tinggi = s.y * 0.98f;
        p.namaTekstur = "T_PapanKapur"; p.namaTeksturCahaya = ""; p.halus = 0.05f;
        p.BangunUlang();
        go.AddComponent<PapanKapur>();
        return true;
    }

    // ---------- Teks awal (draf, silakan ditulis ulang di Inspector) ----------

    static void IsiFoto(ObjekCerita o)
    {
        o.petunjuk = "[E] Lihat foto wisuda";
        o.namaBenda = "Foto wisuda";
        o.keteranganBenda = "di dinding";
        o.namaGambarPotret = "T_FotoWisuda";
        o.isiObjek = new[] { "Aku memakai toga. Ibu di sebelahku memakai baju batik terbaiknya." };
        o.monolog = new[]
        {
            "Waktu foto ini diambil, semua orang mengira aku akan langsung kerja kantoran.",
            "Ibu tersenyum paling lebar di sini. Semoga senyum itu masih sama sekarang.",
        };
        o.hariBerubah = 4;
        o.monologSetelah = new[]
        {
            "Tawaran itu persis yang dibayangkan orang-orang di hari wisuda.",
            "Tapi yang paling kuingat dari hari itu justru kata Ibu: \"yang penting kamu bahagia.\"",
        };
    }

    static void IsiCatatan(ObjekCerita o)
    {
        o.petunjuk = "[E] Baca catatan Ibu";
        o.namaBenda = "Catatan Ibu";
        o.keteranganBenda = "tulisan tangan";
        o.namaGambarPotret = "T_CatatanIbu";
        o.isiObjek = new[]
        {
            "Nak, jangan lupa sarapan ya.",
            "Kopi buatanmu enak, Ibu yang pertama coba. Apa pun pilihanmu nanti, Ibu tetap bangga.",
        };
        o.monolog = new[]
        {
            "Ibu tidak pernah bertanya kapan aku kerja kantoran.",
            "Mungkin karena itu aku berani membuka kedai ini.",
        };
        o.hariBerubah = 4;
        o.monologSetelah = new[] { "\"Apa pun pilihanmu nanti.\" Hari ini kalimat itu terasa jauh lebih berat." };
        o.ketenanganPertamaKali = 5;   // SDG 3: dukungan keluarga menenangkan
    }

    // ---------- Pembantu ----------

    static void Perabot<T>(Transform kedai, Transform grup, string nama, Vector3 pos, float rotY, List<string> laporan, string ket)
        where T : MeshDariKode
    {
        if (Pindahkan(kedai, nama, grup)) return;
        Baru(nama, grup, pos, rotY).AddComponent<T>().BangunUlang();
        laporan.Add(ket);
    }

    // Kalau benda sudah ada (mungkin di grup lama), pindahkan ke grup yang benar tanpa mengubah posisinya.
    static bool Pindahkan(Transform kedai, string nama, Transform grup)
    {
        var t = Cari(kedai, nama);
        if (t == null) return false;
        if (t.parent != grup) t.SetParent(grup, true);
        return true;
    }

    // Di atas meja laptop, di sisi yang tidak ditempati laptop.
    static Vector3 PosisiCatatan(Transform kedai)
    {
        var meja = Cari(kedai, "MejaSudut");
        if (!Ukur(meja, out var b)) return new Vector3(3.55f, 0.81f, -3.4f);

        float x = b.min.x + 0.16f;
        if (Ukur(Cari(kedai, "Laptop"), out var lap) && lap.center.x < b.center.x) x = b.max.x - 0.16f;
        return new Vector3(x, b.max.y + 0.002f, b.center.z + 0.05f);
    }
}
