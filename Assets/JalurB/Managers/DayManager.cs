using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum FaseHari { Pagi, Shift, Malam, Selesai }

// Ringkasan satu hari, untuk statistik "kemarin" di menu Tab.
public class RekapHarian
{
    public int hari, dilayani, perfect, kecewa, pemasukan, ketenangan;
}

// Alur lima hari (GDD slide 7, 8, 15):
//   Pagi   : monolog pagi, Tab untuk upgrade, E di pintu untuk buka kedai
//   Shift  : pesanan berjalan, NPC bisa diajak bicara, timer berjalan
//   Malam  : rekap (pendapatan, tip, Ketenangan) lalu monolog batin
//   Hari 5 : perbandingan proyeksi pendapatan vs gaji tawaran -> 1 dari 4 ending
//
// UNTUK JALUR A (DisturbanceManager): baca HariIni, Fase, ProgresShift (0..1),
// dan dengarkan OnShiftMulai / OnShiftSelesai.
public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Hari")]
    public int totalHari = 5;
    public float durasiShift = 240f;
    [Tooltip("Objek tahap visual 2. Menyala setelah renovasi dibeli (lihat DekorBertahap), bukan otomatis per hari.")]
    public string namaObjekTahap2 = "Tahap2";

    [Header("Monolog (index 0 = hari 1, boleh kosong)")]
    public DialogueData[] monologPagi;
    public DialogueData[] monologMalam;

    [Header("Ending (GDD slide 15)")]
    public int ambangKetenangan = 60;
    public string judulBerkembangTenang = "KEDAI BERKEMBANG";
    [TextArea] public string isiBerkembangTenang = "Kedai ramai dan kamu tenang. Pilihanmu terbukti benar.";
    public string judulBerkembangLelah = "KEDAI BERKEMBANG";
    [TextArea] public string isiBerkembangLelah = "Kedai ramai, tapi kamu kelelahan dan masih ragu.";
    public string judulITTenang = "KEMBALI KE IT";
    [TextArea] public string isiITTenang = "Kedai kamu titipkan, tapi kamu tidak menyesal pernah mencoba.";
    public string judulITLelah = "KEMBALI KE IT";
    [TextArea] public string isiITLelah = "Kedai ditutup dan kamu kembali tanpa menemukan jawabannya.";

    [Header("UI (kosongkan untuk cari otomatis di Kanvas_UI)")]
    public GameObject layarRekap;
    public GameObject layarEndingBerkembang; // prefab EndingTenang
    public GameObject layarEndingIT;         // prefab EndingBurnout

    public int HariIni { get; private set; }
    public FaseHari Fase { get; private set; }
    public float SisaWaktu { get; private set; }
    public float ProgresShift => durasiShift <= 0f ? 0f : 1f - SisaWaktu / durasiShift;
    public int KetenanganAwalHari { get; private set; }
    public RekapHarian Kemarin { get; private set; }
    public bool BisaUlangiHari => Fase == FaseHari.Shift;

    // Monolog tokoh utama: nama "Aku" diberi keterangan supaya jelas itu suara hati.
    static readonly InfoPembicara Batin = new InfoPembicara { nama = "Aku", peran = "dalam hati" };

    public event Action<FaseHari> OnFaseBerubah;
    public event Action<int> OnShiftMulai;
    public event Action<int> OnShiftSelesai;
    public event Action<int> OnHariDiulang;
    // Monolog pagi selesai (dipakai DekorBertahap untuk belanja dekor dan menjelaskannya).
    public event Action<int> OnMonologPagiSelesai;

    // Tahap visual 2 (GDD slide 19) tidak lagi gratis: menyala setelah renovasi dibeli (DekorBertahap).
    public bool Tahap2Aktif { get; private set; }
    public void AktifkanTahap2()
    {
        Tahap2Aktif = true;
        if (tahap2 != null) tahap2.SetActive(true);
    }

    // Keadaan tepat saat kedai dibuka, untuk "Ulangi Hari".
    int uangSaatBuka, ketenanganSaatBuka;

    GameObject tahap2;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Ikat();
    }

    void Ikat()
    {
        var kanvas = GameObject.Find("Kanvas_UI");
        if (kanvas != null)
        {
            var k = kanvas.transform;
            if (layarRekap == null) layarRekap = k.Find("RekapMalam")?.gameObject;
            if (layarEndingBerkembang == null) layarEndingBerkembang = k.Find("EndingTenang")?.gameObject;
            if (layarEndingIT == null) layarEndingIT = k.Find("EndingBurnout")?.gameObject;
        }

        var kedai = GameObject.Find("Kedai");
        if (kedai != null)
            foreach (var t in kedai.GetComponentsInChildren<Transform>(true))
                if (t.name == namaObjekTahap2) { tahap2 = t.gameObject; break; }
    }

    void Start()
    {
        Tampilkan(layarRekap, false);
        Tampilkan(layarEndingBerkembang, false);
        Tampilkan(layarEndingIT, false);

        PasangTombol(layarRekap, "Panel/TombolLanjut", Lanjut);
        PasangTombol(layarEndingBerkembang, "TombolMenu", MainLagi);
        PasangTombol(layarEndingIT, "TombolMenu", MainLagi);

        MulaiHari(1);
    }

    void MulaiHari(int hari)
    {
        HariIni = hari;
        KetenanganAwalHari = KetenanganTracker.Instance.Nilai;
        EconomyManager.Instance.ResetHarian();
        if (tahap2 != null) tahap2.SetActive(Tahap2Aktif);

        KunciKursor(true);
        GantiFase(FaseHari.Pagi);
        DialogueManager.Instance.Mulai(Ambil(monologPagi, hari), _ => OnMonologPagiSelesai?.Invoke(hari), Batin);
    }

    // Dipanggil pintu (E) saat pagi.
    public bool BisaBukaKedai =>
        Fase == FaseHari.Pagi && !UIStatus.MenghalangiPemain;

    public void BukaKedai()
    {
        if (!BisaBukaKedai) return;

        // Harga belum dicek / terlalu mahal: papan harga menampilkan dialog dulu.
        if (PapanHarga.Periksa(LanjutBukaKedai)) return;
        LanjutBukaKedai();
    }

    void LanjutBukaKedai()
    {
        if (Fase != FaseHari.Pagi) return;

        uangSaatBuka = EconomyManager.Instance.Uang;
        ketenanganSaatBuka = KetenanganTracker.Instance.Nilai;

        SisaWaktu = durasiShift;
        GantiFase(FaseHari.Shift);
        OrderManager.Instance.MulaiTerima();
        OnShiftMulai?.Invoke(HariIni);
    }

    // Menu jeda: batalkan shift yang sedang berjalan dan kembali ke pagi hari yang sama.
    // Uang dan Ketenangan kembali seperti saat pintu dibuka; upgrade yang dibeli pagi tadi tetap ada.
    public void UlangiHari()
    {
        if (!BisaUlangiHari) return;

        var order = OrderManager.Instance;
        order.BerhentiTerima();
        order.KosongkanAntrean();

        EconomyManager.Instance.AturUang(uangSaatBuka);
        EconomyManager.Instance.ResetHarian();
        KetenanganTracker.Instance.Atur(ketenanganSaatBuka, "ulangi hari");

        SisaWaktu = 0f;
        GantiFase(FaseHari.Pagi);
        OnHariDiulang?.Invoke(HariIni);
    }

    // Untuk uji coba: langsung tutup shift supaya cepat sampai ke hari NPC.
    public void AkhiriShiftSekarang()
    {
        if (Fase == FaseHari.Shift) SisaWaktu = 0.01f;
    }

    void Update()
    {
        if (Fase != FaseHari.Shift || DialogueManager.Instance.IsActive) return;

        SisaWaktu -= Time.deltaTime;
        if (SisaWaktu <= 0f) TutupKedai();
    }

    void TutupKedai()
    {
        SisaWaktu = 0f;
        var order = OrderManager.Instance;
        var eco = EconomyManager.Instance;
        var ket = KetenanganTracker.Instance;

        order.BerhentiTerima();
        order.KosongkanAntrean();

        if (eco.KecewaHariIni == 0) ket.Ubah(+5, "shift tanpa pelanggan kecewa");
        if (UpgradeManager.Instance.AdaBonusKetenanganHarian) ket.Ubah(+5, "lampu hangat / playlist akustik");

        eco.TutupHari();
        Kemarin = new RekapHarian
        {
            hari = HariIni,
            dilayani = eco.DilayaniHariIni,
            perfect = eco.PerfectHariIni,
            kecewa = eco.KecewaHariIni,
            pemasukan = eco.PemasukanHariIni,
            ketenangan = ket.Nilai
        };
        GantiFase(FaseHari.Malam);
        OnShiftSelesai?.Invoke(HariIni);
        TampilkanRekap();
    }

    void TampilkanRekap()
    {
        if (layarRekap == null) { Lanjut(); return; }

        var eco = EconomyManager.Instance;
        var ket = KetenanganTracker.Instance;
        var panel = layarRekap.transform.Find("Panel");
        int selisih = ket.Nilai - KetenanganAwalHari;

        Isi(panel, "Subjudul", "Hari " + HariIni);
        Isi(panel, "Nilai_Pelanggandilayani", eco.DilayaniHariIni + (eco.KecewaHariIni > 0 ? $"  ({eco.KecewaHariIni} kecewa)" : ""));
        Isi(panel, "Nilai_PesananPerfect", eco.PerfectHariIni.ToString());
        Isi(panel, "Nilai_Pendapatan", EconomyManager.Rupiah(eco.PendapatanHariIni) + "  + tip " + EconomyManager.Rupiah(eco.TipHariIni));
        Isi(panel, "Nilai_Ketenanganakhir", $"{ket.Nilai} / 100  ({(selisih >= 0 ? "+" : "")}{selisih})");
        Isi(panel, "TombolLanjut/Label", HariIni < totalHari ? "Lanjut ke Hari " + (HariIni + 1) : "Lihat Akhir Cerita");

        KunciKursor(false);
        layarRekap.SetActive(true);
    }

    void Lanjut()
    {
        Tampilkan(layarRekap, false);
        KunciKursor(true);

        int hari = HariIni;
        DialogueManager.Instance.Mulai(Ambil(monologMalam, hari), _ =>
        {
            if (hari < totalHari) MulaiHari(hari + 1);
            else TampilkanEnding();
        }, Batin);
    }

    void TampilkanEnding()
    {
        GantiFase(FaseHari.Selesai);

        var eco = EconomyManager.Instance;
        bool berkembang = eco.KedaiMengalahkanGaji;
        bool tenang = KetenanganTracker.Instance.Nilai >= ambangKetenangan;

        string judul, isi;
        if (berkembang) { judul = tenang ? judulBerkembangTenang : judulBerkembangLelah; isi = tenang ? isiBerkembangTenang : isiBerkembangLelah; }
        else            { judul = tenang ? judulITTenang : judulITLelah;                 isi = tenang ? isiITTenang : isiITLelah; }

        isi += "\n\nProyeksi kedai: " + EconomyManager.Rupiah(eco.ProyeksiBulanan) + " / bulan" +
               "\nGaji tawaran: " + EconomyManager.Rupiah(eco.gajiTawaran) + " / bulan";

        GameObject layar = berkembang ? layarEndingBerkembang : layarEndingIT;
        if (layar == null) { Debug.Log($"[Ending] {judul}\n{isi}"); return; }

        Isi(layar.transform, "Judul", judul);
        Isi(layar.transform, "Isi", isi);
        Isi(layar.transform, "TombolMenu/Label", "Main Lagi");

        KunciKursor(false);
        layar.SetActive(true);
    }

    void MainLagi()
    {
        Time.timeScale = 1f;
        // Scene Kedai harus ada di File > Build Profiles > Scene List.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ---------- Pembantu ----------

    void GantiFase(FaseHari f)
    {
        Fase = f;
        OnFaseBerubah?.Invoke(f);
    }

    static DialogueData Ambil(DialogueData[] daftar, int hari) =>
        daftar != null && hari - 1 >= 0 && hari - 1 < daftar.Length ? daftar[hari - 1] : null;

    static void Tampilkan(GameObject go, bool aktif)
    {
        if (go != null) go.SetActive(aktif);
    }

    static void Isi(Transform akar, string jalur, string teks)
    {
        if (akar == null) return;
        var t = akar.Find(jalur);
        var tmp = t != null ? t.GetComponent<TMP_Text>() : null;
        if (tmp != null) tmp.text = teks;
        else Debug.LogWarning($"[DayManager] Teks '{jalur}' tidak ditemukan di {akar.name}.");
    }

    static void PasangTombol(GameObject akar, string jalur, UnityEngine.Events.UnityAction aksi)
    {
        if (akar == null) return;
        var t = akar.transform.Find(jalur);
        var b = t != null ? t.GetComponent<Button>() : null;
        if (b != null) b.onClick.AddListener(aksi);
    }

    public static void KunciKursor(bool kunci)
    {
        Cursor.lockState = kunci ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !kunci;
    }
}
