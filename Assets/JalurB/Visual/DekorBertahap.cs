using System;
using System.Collections.Generic;
using UnityEngine;

// Dekor yang bertambah mengikuti hari, dan SETIAP tambahan setelah hari 1 dibeli dengan uang kedai.
//
// Anak objek ini bernama "Hari1".."Hari5". Hari1 = perlengkapan dasar (gratis, selalu ada).
// Tiap pagi, setelah monolog pagi selesai:
//   1. Paket yang harinya sudah tiba dan belum terbeli dicoba dibeli (urut dari hari terkecil).
//   2. Uang cukup  -> uang berkurang, grup HariN menyala. Paket renovasi (hari tahap 2) juga
//                     menyalakan objek Tahap2 (poster, tanaman besar, kursi bar, lampu gantung meja).
//      Uang kurang -> paket ditunda, dicoba lagi besok pagi.
//   3. Tokoh menjelaskan belanjanya lewat monolog "Aku (dalam hati)": apa yang dibeli, habis berapa, sisa berapa.
//
// Harga dan isi tiap paket bisa diubah di Inspector. Alasan posisi tiap benda: Assets/JalurB/DEKOR_TATA_LETAK.md
public class DekorBertahap : MonoBehaviour
{
    [Serializable]
    public class Paket
    {
        public int hari;
        public int biaya;
        [Tooltip("Daftar barang, ditulis seperti kalimat. Dipakai di monolog.")]
        public string barang;
        [Tooltip("Paket renovasi: ikut menyalakan objek Tahap2")]
        public bool renovasiTahap2;
    }

    public Paket[] paket =
    {
        new Paket { hari = 2, biaya = 60000,  barang = "lampu dinding, gantungan jaket, dan tanaman kecil untuk counter" },
        new Paket { hari = 3, biaya = 90000,  barang = "etalase kue, rak toples kopi, sekarung biji kopi, dan tempat payung" },
        new Paket { hari = 4, biaya = 250000, barang = "poster, tanaman besar, kursi bar, lampu gantung di tiap meja, papan neon, dan bangku jendela", renovasiTahap2 = true },
        new Paket { hari = 5, biaya = 25000,  barang = "vas bunga untuk meja yang nanti diduduki Ibu" },
    };

    static readonly InfoPembicara Batin = new InfoPembicara { nama = "Aku", peran = "dalam hati" };
    readonly HashSet<int> terbeli = new HashSet<int>();
    bool sudahDaftar;

    void Start()
    {
        Terapkan();
        Daftar();
    }

    void OnEnable() => Daftar();

    void OnDisable()
    {
        if (DayManager.Instance != null) DayManager.Instance.OnMonologPagiSelesai -= Belanja;
        sudahDaftar = false;
    }

    void Daftar()
    {
        if (sudahDaftar || DayManager.Instance == null) return;
        DayManager.Instance.OnMonologPagiSelesai += Belanja;
        sudahDaftar = true;
    }

    // Dipanggil DayManager tiap pagi setelah monolog pagi selesai.
    void Belanja(int hari)
    {
        var eco = EconomyManager.Instance;
        var beli = new List<Paket>();
        var tunda = new List<Paket>();
        int total = 0;

        foreach (var p in paket)
        {
            if (p.hari > hari || terbeli.Contains(p.hari)) continue;
            if (eco.CobaBelanja(p.biaya)) { terbeli.Add(p.hari); beli.Add(p); total += p.biaya; }
            else tunda.Add(p);
        }
        if (beli.Count == 0 && tunda.Count == 0) return;

        Terapkan();
        var monolog = BuatMonolog(beli, tunda, total, eco.Uang);
        DialogueManager.Instance.Mulai(monolog, _ => Destroy(monolog), Batin);
    }

    // Nyalakan grup yang sudah dibeli; Hari1 selalu menyala.
    void Terapkan()
    {
        foreach (Transform anak in transform)
        {
            if (!anak.name.StartsWith("Hari")) continue;
            if (int.TryParse(anak.name.Substring(4), out int n)) anak.gameObject.SetActive(n <= 1 || terbeli.Contains(n));
        }
        foreach (var p in paket)
            if (p.renovasiTahap2 && terbeli.Contains(p.hari) && DayManager.Instance != null)
                DayManager.Instance.AktifkanTahap2();
    }

    DialogueData BuatMonolog(List<Paket> beli, List<Paket> tunda, int total, int sisa)
    {
        var baris = new List<BarisDialog>();
        void Kata(string isi) => baris.Add(new BarisDialog { pembicara = Batin.nama, isi = isi });

        foreach (var p in beli)
        {
            if (p.renovasiTahap2) Kata($"Hari ini kedai kurenovasi: {p.barang}. Biayanya {EconomyManager.Rupiah(p.biaya)}.");
            else Kata($"Tadi pagi aku belanja {p.barang}. Habis {EconomyManager.Rupiah(p.biaya)}.");
        }
        if (beli.Count > 0)
            Kata(beli.Count > 1
                ? $"Total {EconomyManager.Rupiah(total)}. Sisa uang kedai {EconomyManager.Rupiah(sisa)}."
                : $"Sisa uang kedai {EconomyManager.Rupiah(sisa)}.");

        foreach (var p in tunda)
            Kata($"Aku ingin menambah {p.barang}, tapi uangnya belum cukup. Kurang {EconomyManager.Rupiah(p.biaya - sisa)}. Nanti saja kalau sudah ada.");

        var data = ScriptableObject.CreateInstance<DialogueData>();
        data.baris = baris.ToArray();
        return data;
    }
}
