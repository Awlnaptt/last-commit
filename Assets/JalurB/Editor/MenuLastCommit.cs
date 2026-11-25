// Susunan menu editor "Last Commit", dikelompokkan per kategori dan diberi nomor urut pemasangan.
// Semua skrip editor memakai konstanta di sini, jadi kalau mau mengganti nama kategori cukup di satu tempat.
//
//   Last Commit
//   ├─ Panduan Urutan Pemasangan
//   ├─ 1 Ruangan             : blockout, model, tata letak, kaca
//   ├─ 2 Sistem dan UI       : layar UI, manager, papan harga, font, build
//   ├─ 3 Karakter dan NPC    : data karakter, NPC tetap, antrean pelanggan
//   ├─ 4 Dekorasi            : tahap 2, ganti dekor polos, dekor per hari, objek cerita, visual upgrade
//   ├─ 5 Suasana             : hujan, cahaya, cuaca, post-processing
//   └─ 6 Alat Bantu          : pengecekan ukuran model

using UnityEditor;

public static class MenuLastCommit
{
    public const string AKAR = "Last Commit/";
    public const string RUANGAN = AKAR + "1 Ruangan/";
    public const string SISTEM = AKAR + "2 Sistem dan UI/";
    public const string KARAKTER = AKAR + "3 Karakter dan NPC/";
    public const string DEKORASI = AKAR + "4 Dekorasi/";
    public const string SUASANA = AKAR + "5 Suasana/";
    public const string ALAT = AKAR + "6 Alat Bantu/";

    [MenuItem(AKAR + "Panduan Urutan Pemasangan", false, 0)]
    static void Panduan()
    {
        EditorUtility.DisplayDialog("Urutan pemasangan",
            "Jalankan dari atas ke bawah. Semua langkah aman diulang (yang sudah ada tidak ditimpa).\n\n" +
            "1 Ruangan: 1 Blockout > 2 Model > 3 Tata Letak > 4 Kaca\n" +
            "2 Sistem dan UI: 1 Layar UI > 2 Manager dan Data > 3 Papan Harga dan Menu Jeda\n" +
            "3 Karakter dan NPC: 1 Data Karakter > 2 NPC Tetap > 3 Antrean Pelanggan\n" +
            "4 Dekorasi: 1 Isi Tahap 2 > 2 Ganti Dekor Polos > 3 Dekor Kode > 4 Dekor Kenney > 5 Objek Cerita\n" +
            "      lalu Visual Upgrade (pratinjau untuk mengecek posisinya)\n" +
            "5 Suasana: 1 Hujan > 2 Cahaya, Cuaca, Post-processing\n" +
            "Terakhir: 2 Sistem dan UI > 4 Font dan Warna, 5 Kedai ke Build\n\n" +
            "Jangan lupa Ctrl+S setelah selesai.", "OK");
    }
}
