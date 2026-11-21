using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text comboText;
    public TMP_Text summaryText;

    public int perfect, good, miss, combo, maxCombo;

    public float lastAccuracy;
    public bool coffeeReady;
    void Start()
    {
        summaryText.text = "";
        UpdateCombo();
    }

    public void Register(string result)
    {
        if (result == "Perfect") perfect++;
        else if (result == "Good") good++;
        else miss++;

        if (result == "Miss") combo = 0;
        else
        {
            combo++;
            if (combo > maxCombo) maxCombo = combo;
        }
        UpdateCombo();
    }

    void UpdateCombo()
    {
        comboText.text = combo > 1 ? "Combo x" + combo : "";
    }

    public void ShowSummary()
    {
        int total = perfect + good + miss;
        float acc = total > 0 ? (perfect + good * 0.5f) / total * 100f : 0f;

        lastAccuracy = acc;
        coffeeReady = true;

        summaryText.text =
            "Perfect: " + perfect + "\n" +
            "Good: " + good + "\n" +
            "Miss: " + miss + "\n" +
            "Combo terpanjang: " + maxCombo + "\n" +
            "Akurasi: " + acc.ToString("F0") + "%\n\n" +
            "Kopi selesai! Tekan E untuk mengantar ke pelanggan.";
    }
}