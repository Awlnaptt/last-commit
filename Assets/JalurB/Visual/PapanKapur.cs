using System.Text;
using TMPro;
using UnityEngine;

// Daftar menu dan harga di papan kapur, ditulis langsung dari data permainan:
//   - menu yang sudah tersedia hari ini + harga yang sedang dipasang (EconomyManager.HargaMenu)
//   - harga di atas batas wajar ditulis oranye (pengingat sistem harga mahal)
//   - hanya menu yang SUDAH tersedia hari ini. Menu baru (misalnya hari 3) muncul sendiri di papan
//     saat harinya tiba, tanpa bocoran sebelumnya.
// Jadi kalau harga diubah di papan harga (E), tulisan di papan kapur ikut berubah.
// Dipasang bersama PanelGambar (gambar papan kapur) oleh PasangAtmosfer.
[RequireComponent(typeof(PanelGambar))]
public class PapanKapur : MonoBehaviour
{
    public Color warnaKapur = new Color(0.93f, 0.91f, 0.86f);
    public string warnaMahal = "#F2B36B";

    [Tooltip("Ukuran huruf dibanding tinggi papan. Tetap (tidak otomatis) supaya nama dan harga tidak bertumpuk.")]
    public float ukuranHuruf = 0.95f;

    // Dua kolom terpisah: nama rata kiri, harga rata kanan, di kotak yang sama.
    // Barisnya sejajar karena huruf dan jarak barisnya sama.
    TextMeshPro kolomNama, kolomHarga;
    float jeda;
    string terakhir;
    static TMP_FontAsset hurufKapur;

    void Start()
    {
        var panel = GetComponent<PanelGambar>();
        kolomNama = BuatKolom("KolomNama", panel, TextAlignmentOptions.Left);
        kolomHarga = BuatKolom("KolomHarga", panel, TextAlignmentOptions.Right);
    }

    TextMeshPro BuatKolom(string nama, PanelGambar panel, TextAlignmentOptions rata)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, -panel.tinggi * 0.12f, 0.004f);
        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // teks TMP terbaca dari -z, papan menghadap +z

        var t = go.AddComponent<TextMeshPro>();
        var huruf = HurufKapur();
        if (huruf != null) t.font = huruf;
        t.rectTransform.sizeDelta = new Vector2(panel.lebar * 0.74f, panel.tinggi * 0.6f);
        t.enableAutoSizing = false;
        t.fontSize = panel.tinggi * ukuranHuruf;   // TMP 3D: fontSize 1 kira-kira setinggi 0,1 m
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.color = warnaKapur;
        t.alignment = rata == TextAlignmentOptions.Left ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.TopRight;
        t.lineSpacing = 18f;
        return t;
    }

    // Huruf seperti tulisan kapur (Comic Relief, lisensi OFL), sama dengan judul "MENU KOPI" di gambar papan.
    static TMP_FontAsset HurufKapur()
    {
        if (hurufKapur != null) return hurufKapur;
        var ttf = Resources.Load<Font>("JalurB/ComicRelief-Bold");
        if (ttf != null) hurufKapur = TMP_FontAsset.CreateFontAsset(ttf);
        return hurufKapur;
    }

    void Update()
    {
        if (kolomNama == null) return;
        jeda -= Time.deltaTime;
        if (jeda > 0f) return;
        jeda = 0.5f;

        Tulis(out string nama, out string harga);
        if (nama + harga == terakhir) return;
        terakhir = nama + harga;
        kolomNama.text = nama;
        kolomHarga.text = harga;
    }

    void Tulis(out string kolomKiri, out string kolomKanan)
    {
        kolomKiri = kolomKanan = "";
        var order = OrderManager.Instance;
        var eco = EconomyManager.Instance;
        var hari = DayManager.Instance;
        if (order == null || eco == null || order.semuaMenu == null) return;

        int hariIni = hari != null ? Mathf.Max(1, hari.HariIni) : 1;
        var kiri = new StringBuilder();
        var kanan = new StringBuilder();
        foreach (var m in order.semuaMenu)
        {
            if (m == null) continue;
            if (m.hariTersedia > hariIni) continue;   // belum tersedia: tidak ditulis
            string harga = EconomyManager.Rupiah(eco.HargaMenu(m));
            if (eco.TerlaluMahal(m)) harga = $"<color={warnaMahal}>{harga}</color>";
            kiri.Append(m.namaTampil).Append('\n');
            kanan.Append(harga).Append('\n');
        }
        kolomKiri = kiri.ToString();
        kolomKanan = kanan.ToString();
    }
}
