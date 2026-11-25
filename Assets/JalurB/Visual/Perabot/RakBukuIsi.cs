using UnityEngine;

// Rak buku berisi buku, dibangun dari kode (pengganti rak Kenney yang kosong).
//
// Isi tiap papan rak diacak dengan seed tetap, jadi hasilnya selalu sama tiap dibangun:
//   - buku berdiri: lebar, tinggi, kedalaman, dan warna acak (5 warna sampul)
//   - kadang satu buku miring bersandar ke buku sebelahnya: diputar di sudut kiri-bawahnya,
//     dan digeser sejauh tinggi * sin(sudut) supaya ujung atasnya pas menyentuh buku sebelah
//   - kadang tumpukan buku tidur
//   - sisa ruang di ujung papan dibiarkan kosong supaya tidak terlihat terlalu rapi
// Titik asal: tengah-bawah rak. +z lokal = sisi depan (menghadap ruangan).
public class RakBukuIsi : MeshDariKode
{
    public float lebar = 1.0f, tinggi = 2.0f, dalam = 0.4f;
    [Range(2, 7)] public int jumlahRuang = 5;
    public int seed = 7;
    public Color warnaKayu = new Color(0.42f, 0.28f, 0.17f);
    public Color[] warnaSampul =
    {
        new Color(0.55f, 0.18f, 0.16f), new Color(0.17f, 0.30f, 0.45f), new Color(0.80f, 0.66f, 0.38f),
        new Color(0.22f, 0.40f, 0.30f), new Color(0.90f, 0.87f, 0.80f),
    };

    const float TEBAL = 0.03f;

    protected override Mesh Bangun()
    {
        int nWarna = Mathf.Max(1, warnaSampul.Length);
        var b = new PembangunMesh(1 + nWarna);
        float w = lebar * 0.5f, d = dalam * 0.5f;

        // rangka: dua sisi, atas, bawah, papan belakang
        b.Kotak(0, new Vector3(-w + TEBAL * 0.5f, tinggi * 0.5f, 0f), new Vector3(TEBAL, tinggi, dalam));
        b.Kotak(0, new Vector3( w - TEBAL * 0.5f, tinggi * 0.5f, 0f), new Vector3(TEBAL, tinggi, dalam));
        b.Kotak(0, new Vector3(0f, tinggi - TEBAL * 0.5f, 0f), new Vector3(lebar, TEBAL, dalam));
        b.Kotak(0, new Vector3(0f, 0.04f, 0f), new Vector3(lebar, 0.08f, dalam));                     // alas (plint)
        b.Kotak(0, new Vector3(0f, tinggi * 0.5f, -d + 0.008f), new Vector3(lebar, tinggi, 0.016f));

        var acak = new System.Random(seed);
        float R(float a, float c) => a + (float)acak.NextDouble() * (c - a);

        float bawah = 0.08f, atas = tinggi - TEBAL;
        float ruang = (atas - bawah) / jumlahRuang;
        for (int i = 0; i < jumlahRuang; i++)
        {
            float y = bawah + i * ruang;
            if (i > 0) b.Kotak(0, new Vector3(0f, y, 0f), new Vector3(lebar - 2 * TEBAL, TEBAL * 0.8f, dalam - 0.02f));
            IsiPapan(b, acak, R, y + TEBAL * 0.4f, ruang - TEBAL, nWarna);
        }
        return b.Jadi("Mesh_RakBukuIsi");
    }

    void IsiPapan(PembangunMesh b, System.Random acak, System.Func<float, float, float> R, float y, float ruang, int nWarna)
    {
        float xMin = -lebar * 0.5f + TEBAL + 0.01f, xMax = lebar * 0.5f - TEBAL - 0.01f;
        float batasIsi = xMin + (xMax - xMin) * R(0.7f, 0.95f);
        float x = xMin, zDepan = dalam * 0.5f - 0.015f;
        bool adaSandaran = false;   // buku miring hanya boleh tepat setelah buku berdiri tanpa celah

        while (x < batasIsi)
        {
            int warna = 1 + acak.Next(nWarna);
            double pilih = acak.NextDouble();

            if (pilih < 0.1 && batasIsi - x > 0.2f)          // tumpukan buku tidur
            {
                float yy = y;
                int n = 2 + acak.Next(3);
                for (int k = 0; k < n; k++)
                {
                    float tebal = R(0.025f, 0.045f), panjang = R(0.15f, 0.2f), dlm = R(0.14f, 0.2f);
                    b.Kotak(1 + acak.Next(nWarna), new Vector3(x + panjang * 0.5f + R(-0.01f, 0.01f), yy + tebal * 0.5f, zDepan - dlm * 0.5f),
                            new Vector3(panjang, tebal, dlm));
                    yy += tebal;
                }
                x += 0.21f;
                adaSandaran = false;
                continue;
            }

            float bw = R(0.022f, 0.055f), bh = ruang * R(0.6f, 0.92f), bd = dalam * R(0.55f, 0.85f);
            if (pilih < 0.18 && adaSandaran)                // buku miring bersandar ke buku di kirinya
            {
                float sudut = R(8f, 18f);
                float geser = bh * Mathf.Sin(sudut * Mathf.Deg2Rad);
                b.Atur(new Vector3(x + geser, y, zDepan - bd * 0.5f), new Vector3(0f, 0f, sudut));
                b.Kotak(warna, new Vector3(bw * 0.5f, bh * 0.5f, 0f), new Vector3(bw, bh, bd));
                b.Reset();
                x += geser + bw + R(0.03f, 0.08f);          // sisakan celah setelah buku miring
                adaSandaran = false;
                continue;
            }

            b.Kotak(warna, new Vector3(x + bw * 0.5f, y + bh * 0.5f, zDepan - bd * 0.5f), new Vector3(bw, bh, bd));
            bool celah = acak.NextDouble() < 0.1;
            x += bw + (celah ? R(0.01f, 0.03f) : 0.002f);
            adaSandaran = !celah;
        }
    }

    protected override Material[] Bahan()
    {
        int nWarna = Mathf.Max(1, warnaSampul.Length);
        var m = new Material[1 + nWarna];
        m[0] = BahanKode.Lit("Kayu_RakBuku", warnaKayu, null, 0.25f);
        for (int i = 0; i < nWarna; i++)
            m[1 + i] = BahanKode.Lit("Sampul_" + i, warnaSampul.Length > 0 ? warnaSampul[i] : Color.gray, null, 0.3f);
        return m;
    }
}
