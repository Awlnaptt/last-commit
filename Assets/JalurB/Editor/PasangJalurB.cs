// PasangJalurB.cs — menu 2 Sistem dan UI > 2: manager, data ekonomi, dialog, pintu, laptop.
//
// Karakter, NPC, dan antrean ada di file terpisah (menu 3 Karakter dan NPC) supaya bisa diperbaiki sendiri-sendiri:
//   PasangKarakter.cs, PasangNPC.cs, PasangAntrean.cs
//
// Aset yang SUDAH ADA tidak ditimpa, jadi angka dan teks hasil editanmu aman.
// Aman dijalankan berulang kali.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static AlatEditor;

public static class PasangJalurB
{
    [MenuItem(MenuLastCommit.SISTEM + "2. Pasang Manager dan Data", false, 2)]
    public static void Pasang()
    {
        var kedai = GameObject.Find("Kedai");
        if (kedai == null || GameObject.Find("Kanvas_UI") == null)
        {
            EditorUtility.DisplayDialog("Scene belum siap",
                "Buka scene Kedai yang sudah punya objek 'Kedai' dan 'Kanvas_UI' " +
                "(Last Commit > Bangun Blockout Kedai dan Buat Layar UI).", "OK");
            return;
        }

        Folder(DATA); Folder(DATA + "/Menu"); Folder(DATA + "/Upgrade"); Folder(DATA + "/Dialog");

        // ---------- 1. Data ----------
        var espresso = Menu("Espresso", 15000, 20000, 1);
        var latte    = Menu("Latte",    22000, 28000, 1);
        var arenKopi = Menu("Kopi Gula Aren", 28000, 35000, 3);

        var upgrade = new List<UpgradeData>
        {
            Upgrade("KursiMeja", "Kursi dan Meja", "Kapasitas pelanggan naik.", 150000, 1, "", "", 0f, 0f, 1, false),
            Upgrade("LampuHangat", "Lampu Hangat", "Tip naik. Ketenangan +5 tiap tutup shift.", 200000, 1, "", "", 0f, 0.05f, 0, true),
            Upgrade("Dekorasi", "Dekorasi dan Rak Buku", "Ruangan lebih hidup.", 180000, 1, "", "", 0f, 0f, 0, false),
            Upgrade("MesinCepat", "Mesin Cepat", "Pola pendek dan mudah. Uang per pesanan tinggi, tip rendah.", 250000, 2, "Mesin", "A", 0.25f, -0.05f, 0, false),
            Upgrade("MesinManual", "Mesin Manual", "Pola panjang dan sulit. Uang standar, tip besar.", 250000, 2, "Mesin", "B", 0f, 0.15f, 0, false),
            Upgrade("MusikCepat", "Musik Cepat", "Rhythm lebih menantang, uang naik.", 200000, 4, "Musik", "A", 0.15f, 0f, 0, false),
            Upgrade("MusikAkustik", "Musik Akustik", "Lebih santai. Tip naik, Ketenangan +5 tiap tutup shift.", 200000, 4, "Musik", "B", 0f, 0.05f, 0, true),
        };

        const string AKU = "Aku";
        var pagi = new[]
        {
            Dialog("Pagi1", 0, null,
                (AKU, "Seminggu lalu aku wisuda. Pagi ini aku membuka kedai sendiri."),
                (AKU, "Ponsel bergetar. Pesan dari Ibu: \"Jangan lupa makan, Nak.\""),
                (AKU, "Tab untuk melihat upgrade. Kalau sudah siap, buka pintu dengan E.")),
            Dialog("Pagi2", 0, null, (AKU, "Hari kedua. Semoga ada yang kembali lagi hari ini.")),
            Dialog("Pagi3", 0, null, (AKU, "Hujan deras dari subuh. Orang-orang butuh tempat berteduh.")),
            Dialog("Pagi4", 0, null, (AKU, "Ada notifikasi di laptop lama. Sudah lama tidak kubuka.")),
            Dialog("Pagi5", 0, null, (AKU, "Hari terakhir sebelum aku harus menjawab email itu.")),
        };
        var malam = new[]
        {
            Dialog("Malam1", 0, null, (AKU, "Sepi. Tapi untuk pertama kalinya, capeknya terasa milikku sendiri.")),
            Dialog("Malam2", 0, null, (AKU, "Kalau kakak tingkat saja menyesal, apa aku akan menyesal juga?")),
            Dialog("Malam3", 0, null, (AKU, "Dia burnout di pekerjaan yang dulu aku tolak.")),
            Dialog("Malam4", 0, null, (AKU, "{gaji} sebulan. Angka itu terus muncul di kepala.")),
            Dialog("Malam5", 0, null, (AKU, "Sudah lima hari. Saatnya menghitung.")),
        };

        var npc1 = Dialog("NPC1_KakakTingkat", 10, null,
            ("Kak Raka", "Kopimu enak. Dulu aku juga pengin buka kedai."),
            ("Kak Raka", "Tapi aku ambil kerja kantoran. Gajinya besar, tapi aku tidak pernah benar-benar pulang."),
            ("Kak Raka", "Jangan berhenti cuma karena takut, ya."));
        var npc2 = Dialog("NPC2_TemanKuliah", 10, null,
            ("Dimas", "Aku tiga hari tidak tidur. Deploy terus, revisi terus."),
            ("Dimas", "Di sini tenang. Boleh aku duduk sebentar lagi?"));
        var npc3 = Dialog("NPC3_Ibu", 10, new[]
            {
                new PilihanDialog { teks = "Aku mau bertahan di kedai, Bu.", balasan = "Kalau itu yang bikin kamu tenang, Ibu dukung." },
                new PilihanDialog { teks = "Mungkin aku ambil tawarannya, Bu.", balasan = "Apa pun pilihanmu, kamu tetap anak Ibu." },
            },
            ("Ibu", "Ibu mampir. Kedainya hangat, ya."),
            ("Ibu", "Kamu sudah memutuskan?"));

        var laptopSebelum = Dialog("Laptop_Sebelum", 0, null, (AKU, "Laptop lama. Isinya repositori yang tidak pernah kuselesaikan."));
        var laptopEmail = Dialog("Laptop_Email", 0, null,
            ("Email", "Selamat, kami menawarkan posisi Software Engineer dengan gaji {gaji} per bulan."),
            (AKU, "Kalau dihitung, kedai ini baru menghasilkan sekitar {proyeksi} sebulan."));

        // ---------- 2. Managers ----------
        var lama = GameObject.Find("JalurB_Managers");
        var m = lama != null ? lama : new GameObject("JalurB_Managers");
        if (lama == null) Undo.RegisterCreatedObjectUndo(m, "Pasang Jalur B");

        Komponen<KetenanganTracker>(m);
        Komponen<EconomyManager>(m);
        var order = Komponen<OrderManager>(m);
        var upg = Komponen<UpgradeManager>(m);
        Komponen<DialogueManager>(m);
        var hari = Komponen<DayManager>(m);
        Komponen<HUDUI>(m);
        Komponen<MenuUpgradeUI>(m);

        Undo.RecordObject(order, "Isi menu");
        order.semuaMenu = new List<MenuItemData> { espresso, latte, arenKopi };
        Undo.RecordObject(upg, "Isi upgrade");
        upg.semuaUpgrade = upgrade;
        upg.akarKedai = kedai.transform;
        Undo.RecordObject(hari, "Isi monolog");
        hari.monologPagi = pagi;
        hari.monologMalam = malam;

        // ---------- 3. Objek interaktif ----------
        var laporan = new List<string>();

        var pintu = Cari(kedai.transform, "Pintu");
        if (pintu != null) { Komponen<PintuKedai>(pintu.gameObject); PastikanCollider(pintu.gameObject); laporan.Add("Pintu: PintuKedai"); }
        else laporan.Add("Pintu TIDAK ditemukan");

        var laptop = Cari(kedai.transform, "Laptop");
        if (laptop != null)
        {
            var l = Komponen<LaptopLama>(laptop.gameObject);
            Undo.RecordObject(l, "Isi laptop");
            l.monologSebelum = laptopSebelum;
            l.emailTawaran = laptopEmail;
            PastikanCollider(laptop.gameObject);
            laporan.Add("Laptop: LaptopLama");
        }
        else laporan.Add("Laptop TIDAK ditemukan");

        var mesin = Cari(kedai.transform, "MesinKopi");
        if (mesin != null) PastikanCollider(mesin.gameObject);

        // NPC dipasang terpisah lewat 3 Karakter dan NPC > 2 (PasangNPC.cs). Dialognya dibuat di sini.
        laporan.Add("Dialog NPC dibuat: " + npc1.name + ", " + npc2.name + ", " + npc3.name);

        // ---------- 4. Interactor sementara ----------
        var cam = Camera.main;
        if (cam != null) { Komponen<UjiJalurB>(cam.gameObject); laporan.Add("UjiJalurB di " + cam.name); }
        else laporan.Add("Kamera utama TIDAK ditemukan, pasang UjiJalurB manual");

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(kedai.scene);

        EditorUtility.DisplayDialog("Jalur B terpasang",
            string.Join("\n", laporan) +
            "\n\nData ada di " + DATA + ".\nLanjutkan ke menu 3 Karakter dan NPC (nomor 1, 2, 3). Jangan lupa Ctrl+S.", "OK");
    }

    // ---------- Pembantu data ----------

    static MenuItemData Menu(string nama, int harga, int wajar, int hari) =>
        Aset<MenuItemData>(DATA + "/Menu/Menu_" + nama.Replace(" ", "") + ".asset", d =>
        {
            d.namaTampil = nama; d.hargaDasar = harga; d.hargaWajarMaks = wajar; d.hariTersedia = hari;
        });

    static UpgradeData Upgrade(string file, string nama, string desk, int harga, int hari,
        string grup, string label, float pengali, float tip, int kapasitas, bool ketenangan) =>
        Aset<UpgradeData>(DATA + "/Upgrade/Upgrade_" + file + ".asset", d =>
        {
            d.namaTampil = nama; d.deskripsi = desk; d.harga = harga; d.hariTersedia = hari;
            d.grupCabang = grup; d.labelCabang = label;
            d.tambahPengaliHarga = pengali; d.tambahPersenTip = tip; d.tambahKapasitas = kapasitas;
            d.ketenanganHarian = ketenangan;
        });

    static DialogueData Dialog(string file, int ketenangan, PilihanDialog[] pilihan,
        params (string pembicara, string isi)[] baris) =>
        Aset<DialogueData>(DATA + "/Dialog/Dialog_" + file + ".asset", d =>
        {
            d.baris = new BarisDialog[baris.Length];
            for (int i = 0; i < baris.Length; i++)
                d.baris[i] = new BarisDialog { pembicara = baris[i].pembicara, isi = baris[i].isi };
            d.pilihan = pilihan ?? new PilihanDialog[0];
            d.ketenanganSaatSelesai = ketenangan;
        });
}
