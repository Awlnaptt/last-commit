using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// UjiJalurB.cs — INTERACTOR SEMENTARA untuk menguji Jalur B sebelum
// Interactor dan rhythm milik Rizki masuk. Hapus begitu punya Rizki sudah jadi.
//
// Pasang di objek kamera pemain (PemainSementara/Kamera), atau di mana saja:
// kalau tidak di kamera, dipakai Camera.main.
//
//   E         : interaksi objek yang dilihat (pintu, laptop, NPC)
//   E di MesinKopi : mulai racik pesanan terdepan (pengganti rhythm)
//   1 / 2 / 3 : hasil racik Perfect / Good / Miss
//   K         : akhiri shift sekarang (lompat ke rekap, untuk cepat sampai hari NPC)
public class UjiJalurB : MonoBehaviour
{
    public float jarak = 3f;

    bool meracik;

    void Update()
    {
        if (UIStatus.AdaLayarTerbuka && !PapanHarga.Terbuka)
        {
            HUDUI.SetPetunjuk("");
            return;
        }
        if (Tekan(4)) DayManager.Instance.AkhiriShiftSekarang();

        if (DialogueManager.Instance.IsActive || MenuUpgradeUI.Terbuka)
        {
            HUDUI.SetPetunjuk("");
            return;
        }

        if (meracik)
        {
            HUDUI.SetPetunjuk("[1] Perfect   [2] Good   [3] Miss   (uji)");
            if (Tekan(1)) Selesai(HasilRacik.Perfect);
            else if (Tekan(2)) Selesai(HasilRacik.Good);
            else if (Tekan(3)) Selesai(HasilRacik.Miss);
            if (DayManager.Instance.Fase != FaseHari.Shift) meracik = false;
            return;
        }

        Camera cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        string petunjuk = "";
        var ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, jarak, ~0, QueryTriggerInteraction.Ignore))
        {
            var target = hit.collider.GetComponentInParent<IInteraktif>();
            if (target != null && target.BisaDipakai)
            {
                petunjuk = target.Petunjuk;
                if (Tekan(0)) target.Interaksi();
            }
            else if (DiMesinKopi(hit.transform) && DayManager.Instance.Fase == FaseHari.Shift)
            {
                bool ada = OrderManager.Instance.PesananAktif != null;
                petunjuk = ada ? "[E] Seduh " + OrderManager.Instance.PesananAktif.menu.namaTampil : "Belum ada pesanan";
                if (ada && Tekan(0)) meracik = OrderManager.Instance.MulaiRacik() != null;
            }
        }
        HUDUI.SetPetunjuk(petunjuk);
    }

    void Selesai(HasilRacik hasil)
    {
        meracik = false;
        OrderManager.Instance.SelesaiRacik(hasil);
    }

    static bool DiMesinKopi(Transform t)
    {
        for (; t != null; t = t.parent)
            if (t.name == "MesinKopi") return true;
        return false;
    }

    // 0 = E, 1..3 = angka, 4 = K
    static bool Tekan(int tombol)
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return false;
        switch (tombol)
        {
            case 0: return k.eKey.wasPressedThisFrame;
            case 1: return k.digit1Key.wasPressedThisFrame;
            case 2: return k.digit2Key.wasPressedThisFrame;
            case 3: return k.digit3Key.wasPressedThisFrame;
            default: return k.kKey.wasPressedThisFrame;
        }
#else
        KeyCode[] kode = { KeyCode.E, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.K };
        return Input.GetKeyDown(kode[tombol]);
#endif
    }
}
