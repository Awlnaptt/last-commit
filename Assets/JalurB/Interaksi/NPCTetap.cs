using UnityEngine;

// Satu pelanggan tetap di scene. Pasang di titiknya (Kedai > NPC > Titik_<nama>).
// Semua isi NPC ada di aset NPCData: model, hari muncul, dan dialog.
// Model hanya muncul saat shift di hari miliknya; percakapan sekali per permainan.
// Saat pemain mendekat, NPC menyapa di kotak dialog (tanpa menghentikan permainan).
public class NPCTetap : MonoBehaviour, IInteraktif
{
    public NPCData data;

    [Tooltip("Jarak pemain (meter) agar NPC menyapa")]
    public float jarakSapa = 4f;

    KarakterAnimasi model;
    CapsuleCollider kapsul;
    InfoPembicara siapa;
    bool sudahBicara;
    bool tadiDekat;

    public string Petunjuk => "[E] Ngobrol dengan " + (data != null ? data.namaTampil : "?");

    public bool BisaDipakai => !sudahBicara && Hadir && !DialogueManager.Instance.IsActive;

    bool Hadir =>
        data != null && model != null &&
        DayManager.Instance != null &&
        DayManager.Instance.HariIni == data.hariMuncul &&
        DayManager.Instance.Fase == FaseHari.Shift;

    void Start()
    {
        if (data == null || data.karakter == null || data.karakter.model == null)
        {
            Debug.LogWarning($"[NPC] {name}: NPCData atau modelnya belum diisi.");
            enabled = false;
            return;
        }

        model = KarakterAnimasi.Buat(data.karakter, transform, transform.position, transform.rotation);
        model.Main(data.karakter.diam, true);
        BuatCollider();
        siapa = new InfoPembicara
        {
            nama = data.namaTampil,
            peran = data.peran,
            potret = PotretKarakter.Ambil(data.karakter)
        };

        DayManager.Instance.OnFaseBerubah += Segarkan;
        DayManager.Instance.OnHariDiulang += HariDiulang;
        Segarkan(DayManager.Instance.Fase);
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnFaseBerubah -= Segarkan;
            DayManager.Instance.OnHariDiulang -= HariDiulang;
        }
    }

    // Ketenangan dikembalikan saat hari diulang, jadi percakapan hari ini juga boleh diulang.
    void HariDiulang(int hari)
    {
        if (data != null && hari == data.hariMuncul) sudahBicara = false;
    }

    void BuatCollider()
    {
        kapsul = GetComponent<CapsuleCollider>();
        if (kapsul == null) kapsul = gameObject.AddComponent<CapsuleCollider>();

        float tinggi = 1.2f;
        var rs = model.GetComponentsInChildren<Renderer>();
        if (rs.Length > 0)
        {
            Bounds b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            tinggi = b.size.y;
        }
        kapsul.height = tinggi;
        kapsul.radius = 0.3f;
        kapsul.center = new Vector3(0f, tinggi * 0.5f, 0f);
    }

    void Segarkan(FaseHari _)
    {
        bool tampil = Hadir;
        model.gameObject.SetActive(tampil);
        kapsul.enabled = tampil;
        tadiDekat = false;
    }

    // Menyapa sekali setiap kali pemain mendekat, sebelum diajak bicara.
    void Update()
    {
        if (siapa == null) return;

        bool dekat = Hadir && !sudahBicara && PemainDekat();
        if (dekat && !tadiDekat && !DialogueManager.Instance.IsActive)
            DialogueManager.Instance.Ucapkan(siapa, KalimatPelanggan.Acak(data.sapaan), 3f);
        tadiDekat = dekat;
    }

    bool PemainDekat()
    {
        var cam = Camera.main;
        if (cam == null) return false;
        Vector3 d = cam.transform.position - transform.position;
        d.y = 0f;
        return d.sqrMagnitude <= jarakSapa * jarakSapa;
    }

    public void Interaksi()
    {
        if (!BisaDipakai) return;
        DialogueManager.Instance.Mulai(data.dialog, _ => sudahBicara = true, siapa);
    }

    // Penanda di Scene view supaya posisi NPC kelihatan tanpa Play.
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.9f, 0.64f, 0.24f);
        Vector3 p = transform.position;
        Gizmos.DrawWireSphere(p + Vector3.up * 0.6f, 0.3f);
        Gizmos.DrawLine(p + Vector3.up * 0.6f, p + Vector3.up * 0.6f + transform.forward * 0.6f);
    }
}
