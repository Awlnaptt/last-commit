using System.Collections.Generic;
using UnityEngine;
using static AlatKedai;

// Area barista = sisi counter yang menghadap dinding (bukan sisi pelanggan).
// Counter di sisi timur (x 2,9..3,9, z -1,1..2,1): barista berdiri di x > 3,9, menghadap pelanggan di barat.
//
//   DiBalikCounter(posisi) : true kalau posisi ada di sisi barista. Dipakai mesin kopi supaya
//                            kopi hanya bisa diseduh dari balik counter, bukan dari antrean pelanggan.
//   Tata(kedai, laporan)   : memutar MesinKopi supaya bagian depannya (tempat cangkir) menghadap barista.
//
// UNTUK JALUR A: di MesinKopiInteraksi.BisaDipakai tambahkan
//   && AreaBarista.DiBalikCounter(Camera.main.transform.position)
public static class AreaBarista
{
    static Transform counter;
    static Bounds batas;
    static Vector3 sisiBarista;   // arah dari tengah counter ke area barista (sumbu pendek counter)
    static bool siap;

    static bool Siapkan()
    {
        if (siap && counter != null) return true;
        var kedai = GameObject.Find("Kedai");
        counter = kedai != null ? Cari(kedai.transform, "Counter") : null;
        if (counter == null || !Ukur(counter, out batas)) return siap = false;

        // Sumbu pendek counter = arah barista-pelanggan. Barista di sisi yang lebih jauh dari tengah ruangan.
        Vector3 c = batas.center;
        sisiBarista = batas.size.x <= batas.size.z ? new Vector3(Mathf.Sign(c.x), 0f, 0f) : new Vector3(0f, 0f, Mathf.Sign(c.z));
        return siap = true;
    }

    public static bool DiBalikCounter(Vector3 posisi)
    {
        if (!Siapkan()) return true;   // counter tidak ditemukan: jangan menghalangi permainan
        float tepi = Vector3.Dot(batas.center, sisiBarista) + Vector3.Dot(batas.extents, new Vector3(Mathf.Abs(sisiBarista.x), 0f, Mathf.Abs(sisiBarista.z)));
        return Vector3.Dot(posisi, sisiBarista) > tepi - 0.05f;
    }

    // Arah hadap barista saat melayani (ke pelanggan).
    public static Vector3 ArahKePelanggan => Siapkan() ? -sisiBarista : Vector3.left;

    public static void Tata(Transform kedai, List<string> laporan)
    {
        siap = false;
        if (!Siapkan()) return;
        var mesin = Cari(kedai, "MesinKopi");
        if (mesin == null || !Ukur(mesin, out var bm)) return;

        Vector3 depan = Vector3.ProjectOnPlane(mesin.forward, Vector3.up).normalized;
        if (Vector3.Dot(depan, sisiBarista) > 0.7f) return;   // sudah menghadap barista
        float sudut = Vector3.SignedAngle(depan, sisiBarista, Vector3.up);
        mesin.RotateAround(bm.center, Vector3.up, sudut);
        laporan.Add("MesinKopi diputar menghadap area barista (balik counter)");
    }
}
