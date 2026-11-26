using System.Collections.Generic;
using UnityEngine;

// Tampilan 3D antrean pelanggan umum. Terpisah dari OrderManager:
// OrderManager mengurus data pesanan, script ini hanya menggambarkannya.
// Kalau tampilannya salah, cukup perbaiki di sini (atau geser titiknya di scene).
//
// Titik diatur di Kedai > AlurPelanggan: TitikMasuk, TitikLorong, TitikAntre1..3.
// Arah biru (sumbu Z) titik antre = arah pelanggan menghadap.
public class AntreanPelanggan : MonoBehaviour
{
    [Tooltip("Model yang boleh muncul sebagai pelanggan umum. Dipilih acak, sebisa mungkin tidak kembar.")]
    public List<ModelKarakter> pilihanModel = new List<ModelKarakter>();

    [Tooltip("Kalimat pelanggan di kotak dialog (aset Kalimat_Pelanggan)")]
    public KalimatPelanggan kalimat;

    public Transform titikMasuk;
    public Transform titikLorong;
    public Transform[] titikAntre;

    readonly Dictionary<Pesanan, PelangganUmum> peta = new Dictionary<Pesanan, PelangganUmum>();
    readonly Dictionary<PelangganUmum, ModelKarakter> dipakai = new Dictionary<PelangganUmum, ModelKarakter>();

    void Start()
    {
        if (titikMasuk == null || titikLorong == null || titikAntre == null || titikAntre.Length == 0)
        {
            Debug.LogError("[AntreanPelanggan] Titik belum lengkap. Jalankan Last Commit > 3 Karakter dan NPC > 3. Pasang Antrean Pelanggan.");
            enabled = false;
            return;
        }

        var o = OrderManager.Instance;
        o.OnPesananMasuk += Masuk;
        o.OnPesananSelesai += Selesai;
        o.OnPelangganPergi += Pergi;
        o.OnAntreanBerubah += Rapikan;
    }

    void OnDestroy()
    {
        var o = OrderManager.Instance;
        if (o == null) return;
        o.OnPesananMasuk -= Masuk;
        o.OnPesananSelesai -= Selesai;
        o.OnPelangganPergi -= Pergi;
        o.OnAntreanBerubah -= Rapikan;
    }

    void Masuk(Pesanan p)
    {
        ModelKarakter k = PilihModel();
        if (k == null || k.model == null) return;

        var anim = KarakterAnimasi.Buat(k, transform, titikMasuk.position, titikMasuk.rotation);
        var pu = anim.gameObject.AddComponent<PelangganUmum>();
        pu.Siapkan(anim, this, p, kalimat);
        peta[p] = pu;
        dipakai[pu] = k;
        // Tujuan antre diatur di Rapikan, yang dipanggil OrderManager setelah event ini.
    }

    void Selesai(Pesanan p, HasilRacik hasil, int uang) =>
        Lepaskan(p, hasil == HasilRacik.Perfect ? ReaksiPelanggan.Puas :
                    hasil == HasilRacik.Good ? ReaksiPelanggan.Biasa : ReaksiPelanggan.Kecewa);

    void Pergi(Pesanan p) => Lepaskan(p, ReaksiPelanggan.Pergi);

    void Lepaskan(Pesanan p, ReaksiPelanggan reaksi)
    {
        if (!peta.TryGetValue(p, out var pu)) return;
        peta.Remove(p);
        if (pu != null) pu.MulaiPulang(reaksi);
    }

    void Rapikan()
    {
        var antrean = OrderManager.Instance.Antrean;

        // Pesanan yang hilang tanpa event (kedai tutup): pulang tanpa reaksi.
        var hilang = new List<Pesanan>();
        foreach (var kv in peta)
        {
            bool ada = false;
            for (int i = 0; i < antrean.Count; i++) if (antrean[i] == kv.Key) { ada = true; break; }
            if (!ada) hilang.Add(kv.Key);
        }
        foreach (var p in hilang) Lepaskan(p, ReaksiPelanggan.Netral);

        for (int i = 0; i < antrean.Count; i++)
            if (peta.TryGetValue(antrean[i], out var pu) && pu != null)
                pu.MenujuAntre(Posisi(i), Rotasi(i));
    }

    // Dipanggil PelangganUmum saat dihapus.
    public void Lepas(PelangganUmum pu) => dipakai.Remove(pu);

    Vector3 Posisi(int i)
    {
        int n = titikAntre.Length;
        if (i < n) return titikAntre[i].position;

        // Antrean lebih panjang dari jumlah titik: lanjutkan ke belakang titik terakhir.
        Vector3 akhir = titikAntre[n - 1].position;
        Vector3 sebelum = n > 1 ? titikAntre[n - 2].position : akhir - titikAntre[n - 1].forward;
        return akhir + (akhir - sebelum) * (i - n + 1);
    }

    Quaternion Rotasi(int i) => titikAntre[Mathf.Min(i, titikAntre.Length - 1)].rotation;

    // Model tanpa klip jalan/diam akan meluncur kaku seperti patung, jadi tidak dipakai.
    static bool BisaJalan(ModelKarakter m) => m != null && m.model != null && m.jalan != null && m.diam != null;

    ModelKarakter PilihModel()
    {
        var bebas = new List<ModelKarakter>();
        foreach (var m in pilihanModel)
            if (BisaJalan(m) && !dipakai.ContainsValue(m)) bebas.Add(m);

        var sumber = bebas;
        if (sumber.Count == 0)
            foreach (var m in pilihanModel) if (BisaJalan(m)) sumber.Add(m);
        if (sumber.Count == 0) return null;
        return sumber[Random.Range(0, sumber.Count)];
    }

    // Penanda jalur di Scene view: masuk -> lorong -> antre.
    void OnDrawGizmos()
    {
        if (titikMasuk == null || titikLorong == null || titikAntre == null) return;
        Gizmos.color = new Color(0.42f, 0.59f, 0.72f);
        Gizmos.DrawLine(titikMasuk.position, titikLorong.position);
        foreach (var t in titikAntre)
        {
            if (t == null) continue;
            Gizmos.DrawLine(titikLorong.position, t.position);
            Gizmos.DrawWireSphere(t.position + Vector3.up * 0.6f, 0.3f);
            Gizmos.DrawLine(t.position + Vector3.up * 0.6f, t.position + Vector3.up * 0.6f + t.forward * 0.6f);
        }
    }
}
