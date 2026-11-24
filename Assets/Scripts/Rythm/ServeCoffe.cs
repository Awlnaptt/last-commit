using UnityEngine;
using TMPro;

public class ServeCoffee : MonoBehaviour
{
    public static float LastServeTime;

    public ScoreManager score;
    public RhythmController controller;
    public TMP_Text customerText;
    public KeyCode serveKey = KeyCode.E;

    void Start()
    {
        if (customerText != null) customerText.text = "";
    }

    void Update()
    {
        if (controller != null && controller.IsRunning) return;
        if (score.coffeeReady && Input.GetKeyDown(serveKey))
            Serve();
    }

    public void Serve()
    {
        if (!score.coffeeReady) return;
        score.coffeeReady = false;
        LastServeTime = Time.time;

        string result = GameState.Instance != null
            ? GameState.Instance.ApplyCoffee(score.lastAccuracy)
            : "";

        customerText.text = result;
    }
}