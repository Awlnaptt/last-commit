using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Menu jeda (Esc). Memakai prefab MenuJeda di Kanvas_UI.
//   Lanjutkan   : tutup menu
//   Ulangi Hari : batalkan shift yang sedang berjalan, kembali ke pagi hari yang sama
//                 (hanya aktif saat shift)
//   Keluar Game : tutup game (di Editor: berhenti Play)
// Waktu game berhenti selama menu terbuka (Time.timeScale = 0).
public class MenuJedaUI : MonoBehaviour
{
    public static bool Terbuka { get; private set; }

    [Tooltip("Kosongkan untuk cari otomatis Kanvas_UI/MenuJeda")]
    public GameObject layar;

    Button tombolUlang;
    bool kursorTerkunciSebelumnya;

    void Start()
    {
        if (layar == null)
        {
            var kanvas = GameObject.Find("Kanvas_UI");
            var t = kanvas != null ? kanvas.transform.Find("MenuJeda") : null;
            if (t != null) layar = t.gameObject;
        }
        if (layar == null) { Debug.LogError("[MenuJeda] Kanvas_UI/MenuJeda tidak ditemukan."); enabled = false; return; }

        var panel = layar.transform.Find("Panel");
        Pasang(panel, "TombolLanjut", Tutup);
        tombolUlang = Pasang(panel, "TombolUlang", UlangiHari);
        Pasang(panel, "TombolKeluar", Keluar);

        layar.SetActive(false);
        Terbuka = false;
    }

    void OnDestroy()
    {
        Terbuka = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (IntroSinematik.Berjalan || !EscDitekan()) return;   // Esc saat intro = lewati intro
        if (Terbuka) Tutup();
        else if (PapanHarga.Terbuka) PapanHarga.TutupYangTerbuka();   // Esc menutup papan dulu
        else if (!MenuUpgradeUI.Terbuka) Buka();
    }

    public void Buka()
    {
        Terbuka = true;
        kursorTerkunciSebelumnya = Cursor.lockState == CursorLockMode.Locked;
        Time.timeScale = 0f;
        layar.SetActive(true);
        DayManager.KunciKursor(false);

        if (tombolUlang != null)
        {
            tombolUlang.interactable = DayManager.Instance.BisaUlangiHari;
            var label = tombolUlang.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (label != null) label.text = DayManager.Instance.BisaUlangiHari ? "Ulangi Hari" : "Ulangi Hari (saat shift)";
        }
    }

    public void Tutup()
    {
        Terbuka = false;
        Time.timeScale = 1f;
        layar.SetActive(false);
        if (kursorTerkunciSebelumnya) DayManager.KunciKursor(true);
    }

    void UlangiHari()
    {
        DayManager.Instance.UlangiHari();
        kursorTerkunciSebelumnya = true;
        Tutup();
    }

    void Keluar()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static Button Pasang(Transform panel, string nama, UnityEngine.Events.UnityAction aksi)
    {
        var b = panel != null ? panel.Find(nama)?.GetComponent<Button>() : null;
        if (b != null) b.onClick.AddListener(aksi);
        return b;
    }

    static bool EscDitekan()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
