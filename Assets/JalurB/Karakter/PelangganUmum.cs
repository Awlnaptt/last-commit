using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ReaksiPelanggan { Puas, Biasa, Kecewa, Pergi, Netral }

// Satu pelanggan umum yang terlihat: masuk dari pintu, antre di depan counter,
// menyebut pesanannya di kotak dialog (tanpa menghentikan permainan),
// bereaksi setelah dilayani, lalu keluar.
// Dibuat dan diatur oleh AntreanPelanggan.
public class PelangganUmum : MonoBehaviour
{
    public float kecepatan = 1.6f;
    public float kecepatanPutar = 540f;
    [Range(0f, 1f)] public float ambangBosan = 0.35f; // sisa sabar di bawah ini -> mengeluh

    struct Langkah { public Vector3 pos; public bool lorong; }

    KarakterAnimasi anim;
    AntreanPelanggan antrean;
    Pesanan pesanan;
    KalimatPelanggan kalimat;
    InfoPembicara siapa;

    readonly List<Langkah> jalur = new List<Langkah>();
    Quaternion hadapAkhir;
    bool adaHadap;
    bool sudahMasuk;   // sudah melewati titik lorong
    bool sudahPesan;
    bool sudahBosan;
    bool pulang;
    bool hapusSaatTiba;
    Vector3 tujuanAntre;
    bool punyaTujuan;

    public void Siapkan(KarakterAnimasi a, AntreanPelanggan q, Pesanan p, KalimatPelanggan k)
    {
        anim = a;
        antrean = q;
        pesanan = p;
        kalimat = k;
        siapa = new InfoPembicara
        {
            nama = "Pelanggan",
            peran = p != null && p.menu != null ? "pesan " + p.menu.namaTampil : "",
            potret = PotretKarakter.Ambil(a.Karakter)
        };
        anim.Main(anim.Karakter.diam, true);
    }

    public void MenujuAntre(Vector3 posisi, Quaternion hadap)
    {
        if (pulang) return;
        if (punyaTujuan && (posisi - tujuanAntre).sqrMagnitude < 0.0001f) return;

        tujuanAntre = posisi;
        punyaTujuan = true;
        jalur.Clear();
        if (!sudahMasuk) jalur.Add(new Langkah { pos = antrean.titikLorong.position, lorong = true });
        jalur.Add(new Langkah { pos = posisi });
        hadapAkhir = hadap;
        adaHadap = true;
    }

    public void MulaiPulang(ReaksiPelanggan reaksi)
    {
        if (pulang) return;
        pulang = true;
        jalur.Clear();
        StartCoroutine(RutinPulang(reaksi));
    }

    IEnumerator RutinPulang(ReaksiPelanggan reaksi)
    {
        string isi = "";
        AnimationClip klip = null;
        if (kalimat != null)
        {
            switch (reaksi)
            {
                case ReaksiPelanggan.Puas:   isi = KalimatPelanggan.Acak(kalimat.puas); break;
                case ReaksiPelanggan.Biasa:  isi = KalimatPelanggan.Acak(kalimat.biasa); break;
                case ReaksiPelanggan.Kecewa: isi = KalimatPelanggan.Acak(kalimat.kecewa); break;
                case ReaksiPelanggan.Pergi:  isi = KalimatPelanggan.Acak(kalimat.pergi); break;
            }
        }
        if (reaksi == ReaksiPelanggan.Puas || reaksi == ReaksiPelanggan.Biasa) klip = anim.Karakter.senang;
        if (reaksi == ReaksiPelanggan.Kecewa || reaksi == ReaksiPelanggan.Pergi) klip = anim.Karakter.kecewa;

        Bicara(isi);

        if (klip != null)
        {
            anim.Main(klip, false);
            yield return new WaitForSeconds(Mathf.Min(klip.length, 2f));
        }

        if (sudahMasuk) jalur.Add(new Langkah { pos = antrean.titikLorong.position });
        jalur.Add(new Langkah { pos = antrean.titikMasuk.position });
        adaHadap = false;
        hapusSaatTiba = true;
    }

    void Update()
    {
        CekBosan();

        if (jalur.Count == 0)
        {
            if (adaHadap)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, hadapAkhir, kecepatanPutar * Time.deltaTime);
            return;
        }

        Vector3 target = jalur[0].pos;
        target.y = transform.position.y;
        Vector3 arah = target - transform.position;

        if (arah.sqrMagnitude < 0.0025f)
        {
            transform.position = target;
            if (jalur[0].lorong) sudahMasuk = true;
            jalur.RemoveAt(0);

            if (jalur.Count == 0)
            {
                if (hapusSaatTiba) { Destroy(gameObject); return; }
                anim.Main(anim.Karakter.diam, true);
                SebutPesanan();
            }
            return;
        }

        anim.Main(anim.Karakter.jalan, true);
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(arah.normalized, Vector3.up), kecepatanPutar * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, target, kecepatan * Time.deltaTime);
    }

    // Pertama kali sampai di antrean: sebut pesanan.
    void SebutPesanan()
    {
        if (sudahPesan || pulang || kalimat == null || pesanan == null) return;
        sudahPesan = true;
        bool mahal = EconomyManager.Instance != null && EconomyManager.Instance.TerlaluMahal(pesanan.menu);
        string[] pilihan = mahal && kalimat.mahal != null && kalimat.mahal.Length > 0 ? kalimat.mahal : kalimat.pesan;
        Bicara(KalimatPelanggan.Acak(pilihan).Replace("{menu}", pesanan.menu.namaTampil));
    }

    void CekBosan()
    {
        if (sudahBosan || pulang || !sudahPesan || pesanan == null || kalimat == null) return;
        if (pesanan.sabar > pesanan.sabarMaks * ambangBosan) return;
        sudahBosan = true;
        Bicara(KalimatPelanggan.Acak(kalimat.mulaiBosan));
    }

    void Bicara(string isi)
    {
        if (!string.IsNullOrEmpty(isi) && DialogueManager.Instance != null)
            DialogueManager.Instance.Ucapkan(siapa, isi);
    }

    void OnDestroy()
    {
        if (antrean != null) antrean.Lepas(this);
    }
}
