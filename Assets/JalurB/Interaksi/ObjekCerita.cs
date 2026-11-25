using UnityEngine;

// Objek cerita yang bisa ditekan E (GDD slide 17: "foto wisuda di dinding, catatan dari ibu di meja").
//
// Isi dialognya ditulis langsung di Inspector, tidak perlu membuat aset DialogueData:
//   isiObjek : yang "dikatakan" benda itu (misalnya tulisan di catatan). Boleh kosong.
//   monolog  : suara hati tokoh setelah melihatnya.
//   Mulai hari 'hariBerubah' (default hari tawaran), monologSetelah dipakai kalau diisi.
//
// Potret di kotak dialog memakai gambar benda itu sendiri (foto / catatan).
public class ObjekCerita : MonoBehaviour, IInteraktif
{
    public string petunjuk = "[E] Lihat";

    [Header("Benda (nama tampil di kotak dialog)")]
    public string namaBenda = "Foto";
    public string keteranganBenda = "di dinding";
    public string namaGambarPotret = "";      // nama tekstur di Resources/JalurB, boleh kosong
    [TextArea(2, 4)] public string[] isiObjek;

    [Header("Monolog tokoh")]
    [TextArea(2, 4)] public string[] monolog;
    public int hariBerubah = 4;
    [TextArea(2, 4)] public string[] monologSetelah;

    [Header("Efek")]
    [Tooltip("Tambahan Ketenangan, hanya sekali sepanjang permainan. 0 = tidak ada.")]
    public int ketenanganPertamaKali = 0;

    bool sudahDibaca;
    Sprite potret;
    static readonly InfoPembicara Batin = new InfoPembicara { nama = "Aku", peran = "dalam hati" };

    public string Petunjuk => petunjuk;

    public bool BisaDipakai =>
        DayManager.Instance != null &&
        DayManager.Instance.Fase != FaseHari.Selesai &&
        DialogueManager.Instance != null &&
        !DialogueManager.Instance.IsActive &&
        !UIStatus.MenghalangiPemain;

    public void Interaksi()
    {
        if (!BisaDipakai) return;

        if (potret == null && !string.IsNullOrEmpty(namaGambarPotret))
            potret = BahanKode.Gambar(namaGambarPotret);

        bool setelah = DayManager.Instance.HariIni >= hariBerubah && monologSetelah != null && monologSetelah.Length > 0;
        string[] suaraHati = setelah ? monologSetelah : monolog;

        int nIsi = isiObjek != null ? isiObjek.Length : 0;
        int nBatin = suaraHati != null ? suaraHati.Length : 0;
        var baris = new BarisDialog[nIsi + nBatin];
        for (int i = 0; i < nIsi; i++)
            baris[i] = new BarisDialog { pembicara = namaBenda, potret = potret, isi = isiObjek[i] };
        for (int i = 0; i < nBatin; i++)
            baris[nIsi + i] = new BarisDialog { pembicara = Batin.nama, isi = suaraHati[i] };
        if (baris.Length == 0) return;

        var data = ScriptableObject.CreateInstance<DialogueData>();
        data.baris = baris;
        data.ketenanganSaatSelesai = sudahDibaca ? 0 : ketenanganPertamaKali;

        var info = new InfoPembicara { nama = namaBenda, peran = keteranganBenda, potret = potret };
        DialogueManager.Instance.Mulai(data, _ =>
        {
            sudahDibaca = true;
            Destroy(data);
        }, info);
    }
}
