using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD shift (GDD slide 21): uang hari ini, sisa waktu, pesanan yang menunggu.
// Meter Ketenangan (keputusan tim, menggantikan GDD slide 13):
// tampil saat melayani biasa, hilang saat meracik (rhythm) supaya lebih tegang.
// Bisa diganti lewat kolom "Mode Meter Ketenangan" di Inspector.
//
// UNTUK JALUR A (Interactor): tulis petunjuk interaksi lewat HUDUI.SetPetunjuk("[E] Seduh").
public enum ModeMeterKetenangan
{
    Sembunyi,              // sesuai GDD awal: hanya di rekap malam
    SelaluTampil,
    SembunyiSaatMeracik    // tampil, kecuali saat rhythm berjalan
}

public class HUDUI : MonoBehaviour
{
    public static HUDUI Instance { get; private set; }

    [Tooltip("Kosongkan untuk cari otomatis Kanvas_UI/HUD")]
    public GameObject layar;

    [Tooltip("Kapan meter Ketenangan tampil di HUD")]
    public ModeMeterKetenangan modeMeterKetenangan = ModeMeterKetenangan.SembunyiSaatMeracik;
    public Color warnaTenang = new Color(0.45f, 0.72f, 0.45f);
    public Color warnaNormal = new Color(0.90f, 0.70f, 0.30f);
    public Color warnaGelisah = new Color(0.85f, 0.35f, 0.30f);

    [Tooltip("Mulai hari ke berapa proyeksi pendapatan vs gaji tawaran tampil (email masuk hari 4)")]
    public int tampilProyeksiMulaiHari = 4;

    TMP_Text teksHari, teksUang, teksPesanan, teksPetunjuk, teksProyeksi;
    GameObject meter, labelMeter;
    RectTransform isiMeter;
    Image warnaMeter;
    readonly StringBuilder sb = new StringBuilder();

    void Awake()
    {
        Instance = this;

        if (layar == null)
        {
            var kanvas = GameObject.Find("Kanvas_UI");
            var t = kanvas != null ? kanvas.transform.Find("HUD") : null;
            if (t != null) layar = t.gameObject;
        }
        if (layar == null) { Debug.LogError("[HUD] Kanvas_UI/HUD tidak ditemukan."); enabled = false; return; }

        var a = layar.transform;
        teksHari = a.Find("TeksHari")?.GetComponent<TMP_Text>();
        teksUang = a.Find("TeksUang")?.GetComponent<TMP_Text>();
        teksPesanan = a.Find("TeksPesanan")?.GetComponent<TMP_Text>();
        teksPetunjuk = a.Find("TeksPetunjuk")?.GetComponent<TMP_Text>();

        meter = a.Find("MeterKetenangan")?.gameObject;
        labelMeter = a.Find("LabelKetenangan")?.gameObject;
        var isi = a.Find("MeterKetenangan/Isi");
        if (isi != null)
        {
            isiMeter = (RectTransform)isi;
            warnaMeter = isi.GetComponent<Image>();
        }
        SetPetunjuk("");
        BuatTeksProyeksi();
    }

    // Satu baris kecil di bawah uang: proyeksi pendapatan per bulan vs gaji tawaran (sketsa HUD slide 21).
    void BuatTeksProyeksi()
    {
        if (teksUang == null) return;
        var go = Instantiate(teksUang.gameObject, teksUang.transform.parent);
        go.name = "TeksProyeksi";
        teksProyeksi = go.GetComponent<TMP_Text>();
        teksProyeksi.fontSize = teksUang.fontSize * 0.6f;
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = ((RectTransform)teksUang.transform).anchoredPosition + new Vector2(0f, -48f);
        go.SetActive(false);
    }

    public static void SetPetunjuk(string teks)
    {
        if (Instance != null && Instance.teksPetunjuk != null) Instance.teksPetunjuk.text = teks;
    }

    void Update()
    {
        var hari = DayManager.Instance;
        var eco = EconomyManager.Instance;

        bool shift = hari.Fase == FaseHari.Shift;
        layar.SetActive(hari.Fase != FaseHari.Selesai);
        PerbaruiMeter();

        if (teksHari != null)
        {
            string waktu;
            if (shift)
            {
                int t = Mathf.CeilToInt(hari.SisaWaktu);
                waktu = $"{t / 60}:{t % 60:00}";
            }
            else waktu = hari.Fase == FaseHari.Pagi ? "Pagi" : "Malam";
            teksHari.text = $"Hari {hari.HariIni}  ·  {waktu}";
        }

        if (teksUang != null)
            teksUang.text = shift ? EconomyManager.Rupiah(eco.PemasukanHariIni) : "Saldo " + EconomyManager.Rupiah(eco.Uang);

        if (teksProyeksi != null)
        {
            bool tampil = hari.HariIni >= tampilProyeksiMulaiHari && hari.Fase != FaseHari.Selesai;
            if (teksProyeksi.gameObject.activeSelf != tampil) teksProyeksi.gameObject.SetActive(tampil);
            if (tampil)
            {
                bool unggul = eco.KedaiMengalahkanGaji;
                teksProyeksi.text = $"Kedai {EconomyManager.Rupiah(eco.ProyeksiBulanan)}/bln  vs  gaji {EconomyManager.Rupiah(eco.gajiTawaran)}";
                teksProyeksi.color = unggul ? warnaTenang : warnaGelisah;
            }
        }

        if (teksPesanan != null)
        {
            sb.Clear();
            if (shift)
            {
                var antrean = OrderManager.Instance.Antrean;
                if (antrean.Count == 0) sb.Append("Belum ada pesanan");
                else
                {
                    sb.Append("Pesanan:  ");
                    for (int i = 0; i < antrean.Count; i++)
                    {
                        if (i > 0) sb.Append("  ·  ");
                        sb.Append(antrean[i].menu.namaTampil);
                    }
                }
            }
            else if (hari.Fase == FaseHari.Pagi) sb.Append("[Tab] Upgrade  ·  [E] di pintu untuk buka kedai");
            teksPesanan.text = sb.ToString();
        }
    }

    bool MeterTampil()
    {
        switch (modeMeterKetenangan)
        {
            case ModeMeterKetenangan.SelaluTampil: return true;
            case ModeMeterKetenangan.SembunyiSaatMeracik:
                // SedangMeracik bernilai true dari MulaiRacik() sampai SelesaiRacik()/BatalRacik().
                return OrderManager.Instance == null || !OrderManager.Instance.SedangMeracik;
            default: return false;
        }
    }

    void PerbaruiMeter()
    {
        bool tampil = MeterTampil();
        if (meter != null && meter.activeSelf != tampil) meter.SetActive(tampil);
        if (labelMeter != null && labelMeter.activeSelf != tampil) labelMeter.SetActive(tampil);
        if (!tampil || isiMeter == null || KetenanganTracker.Instance == null) return;

        var k = KetenanganTracker.Instance;
        isiMeter.anchorMax = new Vector2(k.Nilai / 100f, 1f);
        if (warnaMeter != null)
            warnaMeter.color = k.Tingkat == TingkatKetenangan.Tenang ? warnaTenang :
                               k.Tingkat == TingkatKetenangan.Normal ? warnaNormal : warnaGelisah;
    }
}
