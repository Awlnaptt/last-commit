using UnityEngine;

// Laptop lama di sudut (GDD slide 17, 19).
// Sebelum hari tawaran: monolog tentang dunia IT. Mulai hari tawaran: email berisi gaji ({gaji}).
public class LaptopLama : MonoBehaviour, IInteraktif
{
    public int hariTawaran = 4;
    public DialogueData monologSebelum;
    public DialogueData emailTawaran;

    public bool EmailSudahDibaca { get; private set; }

    static readonly InfoPembicara Email = new InfoPembicara { nama = "Email", peran = "kotak masuk" };
    static readonly InfoPembicara Batin = new InfoPembicara { nama = "Aku", peran = "dalam hati" };

    public string Petunjuk =>
        DayManager.Instance.HariIni >= hariTawaran && !EmailSudahDibaca ? "[E] Ada email baru" : "[E] Laptop lama";

    public bool BisaDipakai =>
        DayManager.Instance != null &&
        DayManager.Instance.Fase != FaseHari.Selesai &&
        !DialogueManager.Instance.IsActive;

    public void Interaksi()
    {
        if (!BisaDipakai) return;

        if (DayManager.Instance.HariIni >= hariTawaran)
            DialogueManager.Instance.Mulai(emailTawaran, _ => EmailSudahDibaca = true, Email);
        else
            DialogueManager.Instance.Mulai(monologSebelum, null, Batin);
    }
}
