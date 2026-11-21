using UnityEngine;
using TMPro;

public class ServeCoffee : MonoBehaviour
{
    public ScoreManager score;
    public TMP_Text customerText;
    public KeyCode serveKey = KeyCode.E;

    void Start()
    {
        customerText.text = "";
    }

    void Update()
    {
        if (score.coffeeReady && Input.GetKeyDown(serveKey))
            Serve();
    }

    // Nanti bisa dipanggil dari interaksi di meja pelanggan
    public void Serve()
    {
        if (!score.coffeeReady) return;
        score.coffeeReady = false;

        string result = GameState.Instance != null
            ? GameState.Instance.ApplyCoffee(score.lastAccuracy)
            : "";

        score.summaryText.text = "";
        customerText.text = result;
    }
}