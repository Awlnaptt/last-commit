# Tata letak dekor kedai: di mana dan kenapa

Denah: lantai x -5..5, z -4..4. Pintu di selatan (x -3,5), jendela hujan di utara, counter di timur (x 2,9..3,9, z -1,1..2,1).

## Tiga aturan penempatan

1. **Jalur pelanggan harus kosong.** Pelanggan berjalan dari pintu (-3,5; -3,3) ke lorong (0,3; -1,2), lalu ke titik antre di depan counter (x 1..2,4; z 1..2,3). Lantai di jalur ini tidak boleh diisi barang.
2. **Area barista harus kosong.** Lantai di belakang counter (x 3,9..5) adalah tempat pemain bekerja. Barang hanya boleh menempel di dinding atau ditaruh di atas counter.
3. **Benda ditaruh di tempat orang memakainya.** Contohnya jaket dan payung di dekat pintu, lampu di atas meja, bahan baku di dekat barista, dan kue di dekat titik pesan.

## Muncul hari ke berapa, dan kenapa

Tahap 1 (hari 1–3) di GDD "sepi dan dingin", tahap 2 (hari 4–5) "hangat dan penuh". Jadi kedai mulai dari perlengkapan wajib saja, lalu bertambah mengikuti cerita tiap hari.

**Semua tambahan setelah hari 1 dibeli dengan uang kedai.** Tiap pagi, setelah monolog pagi, paket hari itu dibeli otomatis dan tokoh menjelaskannya di monolog ("Tadi pagi aku belanja ... Habis Rp ... Sisa uang kedai Rp ..."). Kalau uang tidak cukup, paket ditunda dan dicoba lagi besok pagi. Tahap 2 juga begitu: baru terjadi kalau renovasi hari 4 terbayar.

| Paket | Hari | Biaya awal | Isi |
|---|---|---|---|
| Sudut nyaman | 2 | Rp 60.000 | lampu dinding, gantungan jaket, tanaman counter |
| Menu baru dan musim hujan | 3 | Rp 90.000 | etalase kue, rak toples, karung biji kopi, tempat payung |
| Renovasi tahap 2 | 4 | Rp 250.000 | poster, tanaman besar, kursi bar, lampu gantung meja, papan neon, bangku jendela |
| Bunga untuk Ibu | 5 | Rp 25.000 | vas bunga |

Biaya bisa diubah di Inspector `Kedai > DekorBertahap`. Angkanya masih perkiraan sampai pendapatan per hari bisa diuji bersama rhythm.

| Hari | Benda | Posisi | Alasan |
|---|---|---|---|
| Selalu | Kusen pintu (kode) | Lubang pintu selatan | Menutup celah di tepi daun pintu. Teras gelap di luarnya membuat celah yang tersisa memperlihatkan luar ruangan, bukan ruang kosong |
| 1 | Papan kapur menu (kode) | Menutupi PapanMenu di dinding timur | Kedai baru buka paling butuh daftar menu. Hanya menu yang sudah tersedia yang ditulis; menu hari 3 muncul sendiri saat harinya tiba |
| 1 | Jam dinding (kode) | Dinding barat, z 0,25 | Terlihat dari area duduk dan dari counter. Jarumnya menunjukkan jam permainan |
| 1 | Kipas langit-langit (Kenney) | Tengah area duduk (-2,3; 1,2) | Mendinginkan tempat pelanggan duduk, bukan lorong. Tidak bertabrakan dengan lampu gantung |
| 1 | Tempat sampah (Kenney) | Ujung selatan counter, sisi lorong | Terlewati pelanggan yang berjalan keluar setelah minum |
| 1 | Dua cangkir (Kenney) | Counter sisi barista, di samping mesin | Gelas siap pakai harus dekat mesin |
| 1 | Foto wisuda, catatan Ibu | Pojok laptop | Pojok cerita "dunia IT", sudah ada sejak awal |
| 2 | Lampu dinding (kode) | Dinding barat di atas Meja1 | Pelanggan tetap mulai datang. Meja1 belum punya lampu gantung sampai tahap 2 |
| 2 | Gantungan jaket (Kenney) | Pojok kiri pintu | Hujan: jaket basah langsung digantung begitu masuk |
| 2 | Tanaman kecil (Kenney) | Ujung utara counter, sisi barista | Sentuhan pertama yang membuat counter terasa dirawat |
| 3 | Etalase kue (kode) | Ujung utara counter, sisi pelanggan | Hari 3 = variasi menu. Etalase diletakkan di titik pesan supaya terlihat saat antre, seperti di kafe sungguhan |
| 3 | Rak toples kopi (kode) | Dinding timur di belakang counter (z -1,6) | Bahan baku menu baru, dalam jangkauan barista, tidak menutupi papan menu |
| 3 | Karung biji kopi (Kenney) | Lantai di bawah rak, menempel dinding | Stok tambahan untuk menu baru, tidak menghalangi barista |
| 3 | Tempat payung (kode) | Kanan pintu | Hari 3 = hujan deras |
| 4 | Papan neon "Last Commit" (kode) | Dinding utara di atas rak buku | Tahap 2: kedai makin percaya diri, punya identitas |
| 4 | Bangku jendela (kode) | Di bawah jendela, x 0,2..1,8 | Tahap 2 "kursi tambahan" (GDD slide 19), tempat duduk menghadap hujan |
| 4 | Poster, tanaman pot, kursi bar, lampu gantung meja | (objek Tahap2 yang sudah ada) | Isi tahap visual 2 di GDD |
| 5 | Vas bunga (kode) | Meja1, meja Ibu | Kedai dirapikan karena Ibu datang hari ini |

Dekor dari **upgrade** (VisualUpgrade) tidak ikut jadwal ini: dekor itu muncul saat dibeli.

## Cara kerja di Unity

- Semua dekor bertahap ada di `Kedai > DekorBertahap > Hari1..Hari5`. Komponen `DekorBertahap` menyalakan grup HariN setelah paketnya terbayar (Hari1 selalu menyala).
- Di Edit Mode semua grup terlihat sekaligus supaya mudah ditata. Geser bendanya bebas; menu 8 dan 9 tidak memindahkan benda yang sudah ada.
- Benda bertanda "(kode)" dibangun dari kode (`Assets/JalurB/Visual/Perabot`). Ukuran dan warnanya bisa diubah di Inspector dan bentuknya langsung ikut berubah.
