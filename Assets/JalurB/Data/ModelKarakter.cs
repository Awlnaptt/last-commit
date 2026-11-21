using UnityEngine;

// Satu aset = satu model karakter beserta animasinya.
// Dibuat otomatis lewat Last Commit > Jalur B > 2. Buat Data Karakter.
[CreateAssetMenu(fileName = "Karakter_", menuName = "Last Commit/Karakter")]
public class ModelKarakter : ScriptableObject
{
    public GameObject model;

    [Tooltip("Tinggi karakter di game (meter). Isi 0 untuk memakai ukuran asli FBX. " +
             "Bisa beda per karakter (mis. Ibu sedikit lebih pendek).")]
    public float tinggi = 1.7f;

    [Tooltip("Boleh muncul sebagai pelanggan umum acak. Matikan untuk skin yang tidak cocok " +
             "dengan kafe (robot, zombie, ninja, dst.) atau yang dipakai NPC tetap.")]
    public bool pelangganUmum = true;

    [Header("Potret di kotak dialog")]
    [Tooltip("Isi manual kalau mau gambar sendiri. Kosong = wajah diambil otomatis dari tekstur model.")]
    public Sprite potret;

    [Tooltip("Letak wajah di tekstur (pecahan 0-1, dihitung dari kiri-atas). Bawaan cocok untuk Kenney Blocky.")]
    public Rect wajah = new Rect(0.125f, 0.125f, 0.125f, 0.125f);

    [Header("Animasi (diambil dari FBX model ini sendiri)")]
    public AnimationClip diam;    // idle
    public AnimationClip jalan;   // walk
    public AnimationClip duduk;   // sit
    public AnimationClip senang;  // emote-yes
    public AnimationClip kecewa;  // emote-no
}
