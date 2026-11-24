using UnityEngine;
using TMPro;

public class Interactor : MonoBehaviour
{
    public Camera cam;
    public Transform mesinKopi;
    public RhythmController rhythm;
    public ScoreManager score;
    public TMP_Text promptText;
    public float distance = 3.5f;
    public float maxAngle = 35f;   // seberapa lurus pemain harus menghadap

    void Start()
    {
        Debug.Log("Interactor aktif");
    }

    Vector3 MachineCenter()
    {
        var r = mesinKopi.GetComponentInChildren<Renderer>();
        return r != null ? r.bounds.center : mesinKopi.position;
    }

    void Update()
    {
        promptText.text = "";

        if (rhythm == null || score == null || mesinKopi == null || cam == null)
        {
            Debug.LogWarning("Interactor: ada kolom yang masih kosong.");
            return;
        }

        if (rhythm.IsRunning || score.coffeeReady) return;
        if (!cam.gameObject.activeInHierarchy) return;

        Vector3 toMachine = MachineCenter() - cam.transform.position;
        float dist = toMachine.magnitude;
        float angle = Vector3.Angle(cam.transform.forward, toMachine);
        bool near = dist <= distance && angle <= maxAngle;

        if (near) promptText.text = "[E] Seduh kopi";

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E ditekan. jarak=" + dist.ToString("F1") +
                      " sudut=" + angle.ToString("F0") + " dekat=" + near);

            if (near && Time.time - ServeCoffee.LastServeTime > 0.3f)
                rhythm.StartRhythm();
        }
    }
}