using UnityEngine;

// Semua kalimat pelanggan umum (tampil di kotak dialog). Ubah teks di aset ini, kodenya tidak perlu disentuh.
// Dibuat otomatis lewat Last Commit > Jalur B > 4. Pasang Antrean Pelanggan.
[CreateAssetMenu(fileName = "Kalimat_Pelanggan", menuName = "Last Commit/Kalimat Pelanggan")]
public class KalimatPelanggan : ScriptableObject
{
    [Tooltip("{menu} diganti nama menu yang dipesan")]
    public string[] pesan = { "{menu}, ya!", "Satu {menu}, Kak.", "{menu} satu, makasih." };
    [Tooltip("Dipakai menggantikan kalimat pesan kalau harga menu di atas batas wajar. {menu} diganti nama menu")]
    public string[] mahal = { "{menu}... kok mahal banget, ya?", "Hmm, {menu} segini? Mahal juga.", "Harganya naik? Ya sudah, {menu} satu." };
    public string[] mulaiBosan = { "Masih lama, ya?", "Hmm...", "Agak buru-buru nih." };
    public string[] puas = { "Enak banget!", "Wah, mantap.", "Besok balik lagi!" };
    public string[] biasa = { "Makasih, Kak.", "Oke, makasih." };
    public string[] kecewa = { "Kok pahit...", "Hmm, beda dari biasanya." };
    public string[] pergi = { "Kelamaan, ah.", "Nggak jadi deh." };

    public static string Acak(string[] daftar) =>
        daftar == null || daftar.Length == 0 ? "" : daftar[Random.Range(0, daftar.Length)];
}
