using UnityEngine;

// Satu aset = satu menu kopi. Create > Last Commit > Menu
[CreateAssetMenu(fileName = "Menu_", menuName = "Last Commit/Menu")]
public class MenuItemData : ScriptableObject
{
    public string namaTampil = "Espresso";
    [Tooltip("Harga awal sekaligus harga REKOMENDASI yang ditampilkan di papan harga")]
    public int hargaDasar = 15000;

    [Tooltip("Batas harga wajar. Di atas angka ini: Ketenangan -5 saat dinaikkan, pelanggan jauh lebih jarang datang, dan mereka komplain")]
    public int hargaWajarMaks = 20000;

    [Tooltip("Menu mulai dipesan sejak hari ke berapa. GDD: variasi menu masuk hari 3")]
    public int hariTersedia = 1;

    [Tooltip("Untuk Rizki: aset pola note menu ini (ScriptableObject buatan Jalur A)")]
    public ScriptableObject polaRitme;
}
