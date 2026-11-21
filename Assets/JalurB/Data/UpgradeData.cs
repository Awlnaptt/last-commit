using UnityEngine;

// Satu aset = satu upgrade. Create > Last Commit > Upgrade
// Upgrade bercabang: isi grupCabang (mis. "Mesin") dan labelCabang ("A"/"B").
// Dua upgrade dengan grupCabang sama saling mengunci: beli satu, yang lain hilang.
[CreateAssetMenu(fileName = "Upgrade_", menuName = "Last Commit/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string namaTampil = "Kursi dan Meja";
    [TextArea] public string deskripsi;
    public int harga = 150000;
    public int hariTersedia = 1;

    [Header("Cabang (kosongkan untuk upgrade linear)")]
    public string grupCabang = "";
    public string labelCabang = "";

    [Header("Efek ekonomi")]
    public float tambahPengaliHarga = 0f; // 0.2 = harga jual +20%
    public float tambahPersenTip = 0f;    // 0.05 = tip +5 poin persen
    public int tambahKapasitas = 0;       // pelanggan yang boleh antre

    [Tooltip("0.15 = pelanggan datang 15% lebih sering (jeda antarpelanggan lebih pendek)")]
    [Range(0f, 0.5f)] public float kurangiJedaPelanggan = 0f;

    [Header("Efek lampu")]
    [Tooltip("Lampu gantung di kedai berubah dari putih redup ke kuning hangat")]
    public bool hangatkanLampu = false;

    [Header("Efek Ketenangan")]
    [Tooltip("Lampu hangat / playlist akustik: +5 Ketenangan tiap tutup shift (GDD slide 13)")]
    public bool ketenanganHarian = false;

    [Header("Efek visual (nama objek di bawah 'Kedai', boleh kosong)")]
    public string nyalakanObjek = "";
    public string matikanObjek = "";
}
