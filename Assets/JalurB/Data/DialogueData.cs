using UnityEngine;

[System.Serializable]
public class BarisDialog
{
    public string pembicara;
    public Sprite potret; // boleh kosong
    [TextArea(2, 4)] public string isi;
}

[System.Serializable]
public class PilihanDialog
{
    public string teks;
    public int ubahKetenangan;
    [TextArea(2, 4)] public string balasan; // diucapkan pembicara terakhir setelah dipilih, boleh kosong
}

// Satu aset = satu percakapan atau monolog. Create > Last Commit > Dialog
// Token yang otomatis diganti: {gaji} {proyeksi} {hari}
[CreateAssetMenu(fileName = "Dialog_", menuName = "Last Commit/Dialog")]
public class DialogueData : ScriptableObject
{
    public BarisDialog[] baris;

    [Tooltip("Maksimal 2 pilihan (GDD slide 17). Muncul setelah baris terakhir.")]
    public PilihanDialog[] pilihan;

    [Tooltip("Pelanggan tetap: +10 saat percakapan selesai (GDD slide 13). Monolog: 0.")]
    public int ketenanganSaatSelesai = 0;
}
