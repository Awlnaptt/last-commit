// PemeriksaRuangan.cs — controller first-person SEMENTARA
//
// Gunanya cuma satu: supaya kamu bisa berjalan di dalam kedai dan menilai
// apakah jarak antar meja enak. Ini BUKAN controller final game.
// Hapus folder _Sementara begitu PlayerController milik Rizki sudah jadi.
//
// Taruh di: Assets/_Sementara/PemeriksaRuangan.cs
//           (bikin folder _Sementara dulu, JANGAN di Assets/Editor,
//            dan JANGAN di Assets/Scripts karena itu wilayah Rizki)
//
// Pasang lewat menu: Last Commit > Pasang Pemain Sementara
//
// Kontrol saat Play:  W A S D jalan  ·  Mouse lihat  ·  Shift lari  ·  Esc lepas mouse

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(CharacterController))]
public class PemeriksaRuangan : MonoBehaviour
{
    [Header("Kecepatan")]
    public float kecepatanJalan = 2.8f;
    public float kecepatanLari = 5.5f;

    [Header("Kamera")]
    public float sensitivitas = 0.14f;
    public float tinggiMata = 1.6f;
    public float batasPitch = 70f;

    [Header("Fisika")]
    public float gravitasi = -18f;

    CharacterController cc;
    Transform kamera;
    float pitch;
    float kecepatanY;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        var cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            kamera = cam.transform;
            kamera.localPosition = new Vector3(0f, tinggiMata, 0f);
        }
    }

    void Start()
    {
        KunciKursor(true);
    }

    void Update()
    {
        Vector2 gerak, lihat;
        bool lari, lepas;
        BacaInput(out gerak, out lihat, out lari, out lepas);

        if (lepas) KunciKursor(false);

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            transform.Rotate(0f, lihat.x * sensitivitas, 0f);
            pitch = Mathf.Clamp(pitch - lihat.y * sensitivitas, -batasPitch, batasPitch);
            if (kamera != null) kamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        Vector3 arah = transform.right * gerak.x + transform.forward * gerak.y;
        if (arah.sqrMagnitude > 1f) arah.Normalize();

        if (cc.isGrounded && kecepatanY < 0f) kecepatanY = -2f;
        kecepatanY += gravitasi * Time.deltaTime;

        float v = lari ? kecepatanLari : kecepatanJalan;
        cc.Move((arah * v + Vector3.up * kecepatanY) * Time.deltaTime);
    }

    static void KunciKursor(bool kunci)
    {
        Cursor.lockState = kunci ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !kunci;
    }

    // Bisa jalan di sistem input lama maupun baru, jadi tidak perlu ganti setting
    void BacaInput(out Vector2 gerak, out Vector2 lihat, out bool lari, out bool lepas)
    {
#if ENABLE_INPUT_SYSTEM
        gerak = Vector2.zero;
        lihat = Vector2.zero;
        lari = false;
        lepas = false;

        var k = Keyboard.current;
        if (k != null)
        {
            if (k.wKey.isPressed) gerak.y += 1f;
            if (k.sKey.isPressed) gerak.y -= 1f;
            if (k.dKey.isPressed) gerak.x += 1f;
            if (k.aKey.isPressed) gerak.x -= 1f;
            lari = k.leftShiftKey.isPressed;
            lepas = k.escapeKey.wasPressedThisFrame;
        }

        var m = Mouse.current;
        if (m != null) lihat = m.delta.ReadValue();
#else
        gerak = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        lihat = new Vector2(Input.GetAxisRaw("Mouse X") * 12f, Input.GetAxisRaw("Mouse Y") * 12f);
        lari = Input.GetKey(KeyCode.LeftShift);
        lepas = Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}

#if UNITY_EDITOR
public static class PemasangPemainSementara
{
    [MenuItem("Last Commit/Pasang Pemain Sementara")]
    public static void Pasang()
    {
        var lama = GameObject.Find("PemainSementara");
        if (lama != null) Undo.DestroyObjectImmediate(lama);

        // Cari titik awal dari blockout, kalau tidak ada pakai posisi cadangan
        Vector3 posisi = new Vector3(-3.5f, 0f, -3f);
        var kedai = GameObject.Find("Kedai");
        if (kedai != null)
        {
            foreach (var t in kedai.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "TitikAwalPemain") { posisi = t.position; break; }
            }
        }

        var pemain = new GameObject("PemainSementara");
        Undo.RegisterCreatedObjectUndo(pemain, "Pasang pemain sementara");
        pemain.transform.position = posisi + new Vector3(0f, 0.15f, 0f);

        var cc = pemain.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;

        var kameraGO = new GameObject("Kamera");
        kameraGO.transform.SetParent(pemain.transform, false);
        kameraGO.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        var cam = kameraGO.AddComponent<Camera>();
        cam.fieldOfView = 70f;
        cam.nearClipPlane = 0.05f;
        kameraGO.AddComponent<AudioListener>();
        kameraGO.tag = "MainCamera";

        // Matikan kamera dan audio listener lain supaya tidak bentrok
        foreach (var lainnya in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (lainnya != cam) lainnya.gameObject.SetActive(false);

        foreach (var al in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            if (al.gameObject != kameraGO) al.enabled = false;

        pemain.AddComponent<PemeriksaRuangan>();

        Selection.activeGameObject = pemain;
        EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog("Pemain terpasang",
            "Tekan tombol Play di atas untuk mulai jalan.\n\n" +
            "W A S D  : jalan\n" +
            "Mouse    : lihat sekeliling\n" +
            "Shift    : lari\n" +
            "Esc      : lepas mouse\n\n" +
            "Jangan lupa Ctrl+S.", "OK");
    }
}
#endif
