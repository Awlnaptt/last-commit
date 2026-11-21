// Satu tempat untuk mengecek apakah ada layar yang sedang menutupi permainan.
// Dipakai supaya tombol (E, Tab, angka) tidak bekerja di belakang layar yang terbuka.
//
// UNTUK JALUR A: PlayerController sebaiknya berhenti bergerak saat
// UIStatus.MenghalangiPemain bernilai true.
public static class UIStatus
{
    public static bool AdaLayarTerbuka =>
        MenuUpgradeUI.Terbuka || PapanHarga.Terbuka || MenuJedaUI.Terbuka;

    public static bool MenghalangiPemain =>
        AdaLayarTerbuka || (DialogueManager.Instance != null && DialogueManager.Instance.IsActive);
}
