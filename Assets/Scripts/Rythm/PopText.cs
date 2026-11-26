using UnityEngine;
using TMPro;

public class PopText : MonoBehaviour
{
    public TMP_Text text;
    public Color perfectColor = new Color(1f, 0.85f, 0.4f);
    public Color goodColor = new Color(0.6f, 0.85f, 0.7f);
    public Color missColor = new Color(0.9f, 0.45f, 0.45f);
    public float popScale = 1.5f;
    public float duration = 0.6f;

    float t = 999f;

    void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }

    public void Show(string result)
    {
        if (text == null) return;
        text.text = result;
        text.color = result == "Perfect" ? perfectColor : result == "Good" ? goodColor : missColor;
        t = 0f;
    }

    void Update()
    {
        if (text == null) return;
        if (t > duration)
        {
            if (text.text != "") text.text = "";
            transform.localScale = Vector3.one;
            return;
        }
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / 0.2f);
        transform.localScale = Vector3.one * Mathf.Lerp(popScale, 1f, k);
        Color c = text.color;
        c.a = t > duration * 0.6f ? Mathf.InverseLerp(duration, duration * 0.6f, t) : 1f;
        text.color = c;
    }
}