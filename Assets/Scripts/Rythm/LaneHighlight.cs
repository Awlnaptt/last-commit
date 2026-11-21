using UnityEngine;

public class LaneHighlight : MonoBehaviour
{
    public KeyCode key;
    public Color normalColor = new Color(0.2f, 0.2f, 0.25f);
    public Color pressedColor = new Color(0.9f, 0.7f, 0.3f);

    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = normalColor;
    }

    void Update()
    {
        rend.material.color = Input.GetKey(key) ? pressedColor : normalColor;
    }
}