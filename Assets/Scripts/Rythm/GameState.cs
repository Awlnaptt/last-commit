using UnityEngine;
using TMPro;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    [Header("Nilai game")]
    public int money;
    [Range(0f, 100f)] public float ketenangan = 50f;

    [Header("Harga dan tip")]
    public int coffeePrice = 20000;
    public int tipAmount = 5000;

    [Header("Batas skor (akurasi %)")]
    public float perfectThreshold = 85f;
    public float normalThreshold = 50f;

    [Header("Perubahan Ketenangan")]
    public float gainPerfect = 5f;
    public float gainNormal = 1f;
    public float lossBad = 5f;

    [Header("Tampilan")]
    public TMP_Text moneyText;
    public TMP_Text ketenanganText;

    const float MinKetenangan = 20f;

    void Awake() { Instance = this; }
    void Start() { Refresh(); }

    // Efek Ketenangan ke rhythm
    public float PerfectMult =>
        ketenangan < 40f ? Mathf.Lerp(0.6f, 1f, (ketenangan - MinKetenangan) / (40f - MinKetenangan)) : 1f;
    public float GoodMult => ketenangan >= 70f ? 1.2f : 1f;
    public float TipMult => ketenangan >= 70f ? 1.1f : 1f;

    // Dipanggil setelah satu kopi selesai dibuat
    public string ApplyCoffee(float accuracy)
    {
        money += coffeePrice;   // harga normal, selalu didapat

        string text;
        float change;

        if (accuracy >= perfectThreshold)
        {
            int tip = Mathf.RoundToInt(tipAmount * TipMult);
            money += tip;
            change = gainPerfect;
            text = "Kopi sempurna!\nHarga: Rp " + coffeePrice.ToString("N0") +
                   "\nTip: Rp " + tip.ToString("N0");
        }
        else if (accuracy >= normalThreshold)
        {
            change = gainNormal;
            text = "Kopi biasa, pelanggan puas.\nHarga: Rp " + coffeePrice.ToString("N0");
        }
        else
        {
            change = -lossBad;
            text = "Pelanggan komplain.\nHarga: Rp " + coffeePrice.ToString("N0");
        }

        ketenangan = Mathf.Clamp(ketenangan + change, MinKetenangan, 100f);
        Refresh();

        return text + "\nKetenangan: " + (change >= 0 ? "+" : "") + change;
    }

    public void Refresh()
    {
        if (moneyText != null) moneyText.text = "Rp " + money.ToString("N0");
        if (ketenanganText != null) ketenanganText.text = "Ketenangan: " + Mathf.RoundToInt(ketenangan);
    }
}