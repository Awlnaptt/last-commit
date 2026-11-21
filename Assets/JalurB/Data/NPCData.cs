using UnityEngine;

// Satu aset = satu pelanggan tetap. Kalau satu NPC perlu diperbaiki
// (model, hari, dialog), cukup ubah aset ini. NPC lain tidak tersentuh.
// Posisinya diatur dari titiknya di scene: Kedai > NPC > Titik_<nama>.
[CreateAssetMenu(fileName = "NPC_", menuName = "Last Commit/NPC")]
public class NPCData : ScriptableObject
{
    public string namaTampil = "Kak Raka";

    [Tooltip("Keterangan kecil di samping nama pada kotak dialog, mis. 'Kakak tingkat'")]
    public string peran = "Pelanggan tetap";
    public ModelKarakter karakter;
    public int hariMuncul = 2;
    public DialogueData dialog;

    [Tooltip("Sapaan di kotak dialog saat pemain mendekat, sebelum diajak bicara. Dipilih acak.")]
    public string[] sapaan = { "Eh, sini sebentar." };
}
