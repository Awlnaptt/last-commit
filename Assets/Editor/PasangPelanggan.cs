// PasangPelanggan.cs — menaruh pelanggan di kedai
//
// Taruh di: Assets/Editor/PasangPelanggan.cs
// Buka lewat menu: Last Commit > 3 Karakter dan NPC > Pasang Pelanggan (versi lama)
//
// Alurnya dua tahap, dan sengaja dipisah:
//   1. Buat titik posisi  — bikin objek kosong penanda tempat berdiri pelanggan
//   2. Pasang model       — menaruh model di titik-titik itu
//
// Titiknya boleh kamu geser sendiri di Scene view. Tombol "Pasang model"
// selalu mengikuti posisi titik yang sekarang, bukan angka di dalam script,
// jadi pergeseranmu tidak akan tertimpa.
//
// Titik ini juga yang nanti dipakai OrderManager untuk memunculkan pelanggan,
// jadi namanya jangan diubah.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PasangPelangganWindow : EditorWindow
{
    struct Titik
    {
        public string nama;
        public Vector3 posisi;   // dipakai kalau acuan kosong atau tidak ketemu
        public Vector3 hadap;
        public string acuan;        // perabot patokan posisi, boleh kosong
        public string acuanHadap;   // perabot yang dilihat, boleh kosong
        public Vector3 arah;     // dari titik tengah perabot ke tempat berdiri
        public string catatan;
    }

    // Jarak berdiri dari tepi perabot
    const float RENGGANG = 0.45f;

    // Kedai: x -5..5, z -4..4. Counter di x 2.9..3.9, meja di sisi barat.
    // Posisi dihitung dari isi scene per 19 November: kursi bar di (2.40, 0.30)
    // dan (2.40, -0.40), Meja1 (-4.30, 1.70), Meja2 (-1.70, 1.80), Meja3 (-4.30, -1.20),
    // tanaman di (1.70, -3.40), jendela membentang x -4.2 sampai 1.8.
    static readonly Titik[] TITIK = {
        // Antrean digeser ke sisi utara counter, menjauh dari kursi bar.
        // Jarak terdekat ke kursi bar sekitar 0,6 m.
        // Dua orang di counter dengan jarak 0,95 m, satu lagi menunggu agak jauh.
        // Depan counter cuma menyisakan 1,5 m bebas karena ada dua kursi bar,
        // jadi tiga orang berjajar pasti bersenggolan.
        // acuanHadap = Counter supaya mereka menghadap counter yang TERLIHAT,
        // bukan koordinat Transform-nya yang meleset sekitar satu meter.
        new Titik { nama = "TitikAntre1", acuanHadap = "Counter", posisi = new Vector3( 2.15f, 0f, 2.00f), hadap = new Vector3( 3.40f, 0f, 2.00f), catatan = "sedang dilayani" },
        new Titik { nama = "TitikAntre2", acuanHadap = "Counter", posisi = new Vector3( 2.15f, 0f, 1.05f), hadap = new Vector3( 3.40f, 0f, 1.05f), catatan = "antrean kedua" },
        new Titik { nama = "TitikAntre3", acuanHadap = "Counter", posisi = new Vector3( 1.25f, 0f, 1.55f), hadap = new Vector3( 3.40f, 0f, 1.55f), catatan = "menunggu agak jauh" },

        // Berdiri di sisi meja yang kosong, menjauh dari kursi yang sudah ada
        // Titik meja TIDAK memakai koordinat tetap. Pivot model Kenney meleset
        // sekitar 1 meter dari bendanya yang terlihat, jadi posisinya dihitung
        // dari titik tengah meja yang sebenarnya, lalu digeser ke tepinya.
        new Titik { nama = "TitikMeja1", acuan = "Meja1", arah = new Vector3( 1f, 0f,  0f), posisi = new Vector3(-3.45f, 0f,  1.70f), hadap = new Vector3(-4.30f, 0f,  1.70f), catatan = "sisi timur Meja1" },
        new Titik { nama = "TitikMeja2", acuan = "Meja2", arah = new Vector3( 0f, 0f, -1f), posisi = new Vector3(-1.70f, 0f,  0.95f), hadap = new Vector3(-1.70f, 0f,  1.80f), catatan = "sisi selatan Meja2" },
        new Titik { nama = "TitikMeja3", acuan = "Meja3", arah = new Vector3( 1f, 0f,  0f), posisi = new Vector3(-3.45f, 0f, -1.20f), hadap = new Vector3(-4.30f, 0f, -1.20f), catatan = "sisi timur Meja3" },

        new Titik { nama = "TitikSantai",  posisi = new Vector3( 0.60f, 0f, -2.70f), hadap = new Vector3( 0.60f, 0f, -1.00f), catatan = "pengisi suasana" },
    };

    GameObject mNpc1, mNpc2, mNpc3, mUmumA, mUmumB;
    bool pakaiCollider = true;
    Vector2 scroll;

    [MenuItem(MenuLastCommit.KARAKTER + "Pasang Pelanggan (versi lama, sudah diganti nomor 3)", false, 20)]
    public static void Buka()
    {
        var w = GetWindow<PasangPelangganWindow>("Pasang Pelanggan");
        w.minSize = new Vector2(430, 560);
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.HelpBox(
            "Tahap 1 bikin titik penanda. Tahap 2 menaruh model di titik itu.\n\n" +
            "Titiknya boleh kamu geser sendiri — tombol pasang selalu mengikuti " +
            "posisi titik yang sekarang.", MessageType.Info);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("1. Titik posisi", EditorStyles.boldLabel);

        if (GUILayout.Button("Buat titik yang belum ada", GUILayout.Height(28)))
            BuatTitik(false);

        if (GUILayout.Button("Kembalikan semua titik ke posisi awal"))
        {
            if (EditorUtility.DisplayDialog("Kembalikan posisi titik?",
                "Semua titik dikembalikan ke posisi bawaan. Pergeseran yang kamu buat hilang.\n\n" +
                "Model yang sudah terpasang ikut berpindah.", "Ya", "Batal"))
                BuatTitik(true);
        }

        EditorGUILayout.Space(14);
        EditorGUILayout.LabelField("2. Model pelanggan", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Seret prefab atau FBX dari Project. Yang kosong dilewati.",
            EditorStyles.wordWrappedMiniLabel);

        mNpc1  = Slot("NPC 1 — kakak tingkat", mNpc1);
        mNpc2  = Slot("NPC 2 — teman kuliah", mNpc2);
        mNpc3  = Slot("NPC 3 — ibu", mNpc3);
        mUmumA = Slot("Pelanggan umum A", mUmumA);
        mUmumB = Slot("Pelanggan umum B", mUmumB);

        EditorGUILayout.Space(8);
        pakaiCollider = EditorGUILayout.Toggle(
            new GUIContent("Beri Capsule Collider",
                "Supaya pemain tidak bisa menembus pelanggan."), pakaiCollider);

        EditorGUILayout.Space(10);
        var warnaLama = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.55f, 0.85f, 0.55f);
        if (GUILayout.Button("Pasang model di titik", GUILayout.Height(34)))
            PasangSemua();
        GUI.backgroundColor = warnaLama;

        if (GUILayout.Button("Kosongkan semua titik (model dihapus)"))
            KosongkanModel();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Pembagian titik", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Antre1 = NPC 1 · Antre2 = NPC 2 · Antre3 = NPC 3\n" +
            "Meja1 = umum A · Meja2, Meja3, Santai = umum B",
            EditorStyles.wordWrappedMiniLabel);

        EditorGUILayout.EndScrollView();
    }

    static GameObject Slot(string label, GameObject nilai)
    {
        return (GameObject)EditorGUILayout.ObjectField(label, nilai, typeof(GameObject), false);
    }

    // ---------- Tahap 1 ----------

    void BuatTitik(bool paksaPosisi)
    {
        var akar = Akar(); if (akar == null) return;
        var grup = Grup(akar, "Pelanggan");

        dipakai.Clear();
        int baru = 0, dipindah = 0;

        foreach (var t in TITIK)
        {
            var ada = CariAnak(grup, t.nama);
            if (ada == null)
            {
                var go = new GameObject(t.nama);
                Undo.RegisterCreatedObjectUndo(go, "Buat " + t.nama);
                go.transform.SetParent(grup, false);
                Pasangkan(akar, go.transform, t);
                BeriIkon(go);
                baru++;
            }
            else if (paksaPosisi)
            {
                Undo.RecordObject(ada, "Kembalikan " + t.nama);
                Pasangkan(akar, ada, t);
                dipindah++;
            }
        }

        EditorSceneManager.MarkAllScenesDirty();
        EditorUtility.DisplayDialog("Titik siap",
            baru + " titik baru dibuat" +
            (dipindah > 0 ? ", " + dipindah + " dikembalikan ke posisi awal" : "") +
            ".\n\nTitiknya ada di Kedai > Pelanggan. Geser sesukamu, lalu pasang model.\n\n" +
            "Jangan lupa Ctrl+S.", "OK");
    }

    static void Pasangkan(GameObject akar, Transform t, Titik data)
    {
        Vector3 berdiri = data.posisi;
        Vector3 lihat = data.hadap;

        if (!string.IsNullOrEmpty(data.acuan) && data.arah.sqrMagnitude > 0.0001f)
        {
            var perabot = CariAnak(akar.transform, data.acuan);
            Bounds b;
            if (perabot != null && HitungBounds(perabot.gameObject, out b))
            {
                Vector3 tengah = new Vector3(b.center.x, 0f, b.center.z);
                lihat = tengah;
                berdiri = TempatKosongSekitarPerabot(akar, perabot, b, data.arah, data.nama);
            }
            else
            {
                Debug.LogWarning("Acuan '" + data.acuan + "' tidak ketemu. " +
                                 data.nama + " memakai koordinat cadangan.");
                berdiri = TempatKosongSekitarTitik(akar, data.posisi, lihat, data.nama);
            }
        }
        else
        {
            berdiri = TempatKosongSekitarTitik(akar, data.posisi, lihat, data.nama);
        }

        // Arah hadap boleh ditentukan oleh perabot yang terlihat
        if (!string.IsNullOrEmpty(data.acuanHadap))
        {
            var sasaran = CariAnak(akar.transform, data.acuanHadap);
            Bounds bh;
            if (sasaran != null && HitungBounds(sasaran.gameObject, out bh))
                lihat = new Vector3(bh.center.x, 0f, bh.center.z);
        }

        dipakai.Add(berdiri);
        t.position = berdiri;

        Vector3 hadapArah = lihat - berdiri;
        hadapArah.y = 0f;
        if (hadapArah.sqrMagnitude > 0.0001f)
            t.rotation = Quaternion.LookRotation(hadapArah.normalized, Vector3.up);
    }

    // Ruang yang dibutuhkan satu orang, diukur dari pusat badannya
    const float RADIUS_ORANG = 0.38f;

    // Jarak minimum antar dua pelanggan
    const float JARAK_ORANG = 0.68f;

    // Posisi yang sudah ditetapkan dalam satu kali jalan. Perlu dicatat terpisah
    // karena titik yang belum sempat dipindah masih berada di tempat lamanya.
    static readonly List<Vector3> dipakai = new List<Vector3>();

    // Coba empat sisi perabot, dari sisi yang diminta dulu, lalu makin menjauh
    // sampai ketemu tempat yang tidak ditempati kursi atau perabot lain.
    static Vector3 TempatKosongSekitarPerabot(GameObject akar, Transform perabot,
                                              Bounds b, Vector3 arahUtama, string namaTitik)
    {
        Vector3 tengah = new Vector3(b.center.x, 0f, b.center.z);
        Vector3 u0 = new Vector3(arahUtama.x, 0f, arahUtama.z).normalized;
        Vector3 tegak = new Vector3(-u0.z, 0f, u0.x);

        Vector3[] arah = { u0, tegak, -tegak, -u0 };
        float[] renggang = { RENGGANG, RENGGANG + 0.35f, RENGGANG + 0.75f };

        foreach (float r in renggang)
        {
            foreach (var u in arah)
            {
                float tepi = Mathf.Abs(u.x) * b.extents.x + Mathf.Abs(u.z) * b.extents.z;
                Vector3 calon = tengah + u * (tepi + r);
                if (DiDalamRuangan(calon) && Bebas(akar, calon, perabot, namaTitik))
                    return calon;
            }
        }

        Debug.LogWarning(namaTitik + ": tidak ada sisi meja yang kosong. " +
                         "Dipakai sisi yang diminta, geser manual kalau menembus kursi.");
        float tepi0 = Mathf.Abs(u0.x) * b.extents.x + Mathf.Abs(u0.z) * b.extents.z;
        return tengah + u0 * (tepi0 + RENGGANG);
    }

    // Untuk titik berkoordinat tetap: kalau tertimpa perabot, cari di sekelilingnya
    static Vector3 TempatKosongSekitarTitik(GameObject akar, Vector3 asal, Vector3 lihat, string namaTitik)
    {
        if (Bebas(akar, asal, null, namaTitik)) return asal;

        // Calon yang membuat pelanggan membelakangi tujuannya ditolak, supaya
        // tidak melompat ke seberang counter dan memunggungi pemain.
        Vector3 pandangAsal = new Vector3(lihat.x - asal.x, 0f, lihat.z - asal.z);

        for (float r = 0.4f; r <= 1.6f; r += 0.4f)
        {
            for (int d = 0; d < 8; d++)
            {
                float sudut = d * 45f * Mathf.Deg2Rad;
                Vector3 calon = asal + new Vector3(Mathf.Cos(sudut), 0f, Mathf.Sin(sudut)) * r;

                Vector3 pandangBaru = new Vector3(lihat.x - calon.x, 0f, lihat.z - calon.z);
                if (Vector3.Dot(pandangBaru, pandangAsal) <= 0f) continue;

                if (DiDalamRuangan(calon) && Bebas(akar, calon, null, namaTitik))
                {
                    Debug.Log(namaTitik + " digeser " + r.ToString("0.0") +
                              " m karena tempat aslinya tertimpa perabot.");
                    return calon;
                }
            }
        }

        Debug.LogWarning(namaTitik + ": tidak ketemu tempat kosong di sekitarnya. Geser manual.");
        return asal;
    }

    // Ruangan 10 x 8 m, beri jarak dari dinding
    static bool DiDalamRuangan(Vector3 p)
    {
        return p.x > -4.5f && p.x < 4.5f && p.z > -3.5f && p.z < 3.5f;
    }

    static bool Bebas(GameObject akar, Vector3 titik, Transform kecuali, string namaSendiri)
    {
        foreach (var t in akar.GetComponentsInChildren<Transform>(true))
        {
            if (t == null || !Rintangan(t.name)) continue;
            if (namaSendiri != null && t.name == namaSendiri) continue;
            if (kecuali != null && t.IsChildOf(kecuali)) continue;

            Bounds b;
            if (!HitungBounds(t.gameObject, out b)) continue;

            float dx = Mathf.Max(0f, Mathf.Abs(titik.x - b.center.x) - b.extents.x);
            float dz = Mathf.Max(0f, Mathf.Abs(titik.z - b.center.z) - b.extents.z);
            if (dx * dx + dz * dz < RADIUS_ORANG * RADIUS_ORANG) return false;
        }

        foreach (var lain in dipakai)
        {
            float jx = titik.x - lain.x, jz = titik.z - lain.z;
            if (jx * jx + jz * jz < JARAK_ORANG * JARAK_ORANG) return false;
        }

        return true;
    }

    // Benda yang tidak boleh ditembus pelanggan
    static bool Rintangan(string n)
    {
        return n.StartsWith("Titik")      // pelanggan lain yang sudah ditempatkan
            || n.StartsWith("Kursi")
            || n.StartsWith("Meja")
            || n.StartsWith("Counter")
            || n.StartsWith("RakBuku")
            || n.StartsWith("Tanaman")
            || n.StartsWith("Speaker")
            || n.StartsWith("MesinKopi");
    }

    // Supaya titiknya kelihatan di Scene view, bukan objek kosong tak kasat mata
    static void BeriIkon(GameObject go)
    {
        var ikon = EditorGUIUtility.IconContent("sv_label_5");
        if (ikon != null && ikon.image is Texture2D tex)
            EditorGUIUtility.SetIconForObject(go, tex);
    }

    // ---------- Tahap 2 ----------

    void PasangSemua()
    {
        var akar = Akar(); if (akar == null) return;
        var grup = Grup(akar, "Pelanggan");

        if (grup.childCount == 0)
        {
            EditorUtility.DisplayDialog("Belum ada titik",
                "Klik dulu tombol \"Buat titik yang belum ada\" di tahap 1.", "OK");
            return;
        }

        var peta = new Dictionary<string, GameObject>
        {
            { "TitikAntre1",  mNpc1  },
            { "TitikAntre2",  mNpc2  },
            { "TitikAntre3",  mNpc3  },
            { "TitikMeja1",   mUmumA },
            { "TitikMeja2",   mUmumB },
            { "TitikMeja3",   mUmumB },
            { "TitikSantai",  mUmumB },
        };

        int n = 0, kosong = 0;
        foreach (var pasangan in peta)
        {
            if (pasangan.Value == null) { kosong++; continue; }

            var titik = CariAnak(grup, pasangan.Key);
            if (titik == null) continue;

            if (Taruh(titik, pasangan.Value)) n++;
        }

        EditorSceneManager.MarkAllScenesDirty();
        EditorUtility.DisplayDialog("Selesai",
            n + " pelanggan dipasang" +
            (kosong > 0 ? ", " + kosong + " titik dilewati karena slotnya kosong" : "") +
            ".\n\nKalau ada yang menembus kursi atau meja, geser titiknya di Scene " +
            "view lalu klik tombol ini lagi.\n\nJangan lupa Ctrl+S.", "OK");
    }

    bool Taruh(Transform titik, GameObject model)
    {
        // buang model lama di titik ini
        var lama = CariAnak(titik, "Model");
        if (lama != null) Undo.DestroyObjectImmediate(lama.gameObject);

        var inst = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (inst == null) inst = Instantiate(model);
        if (inst == null) return false;

        inst.name = "Model";
        Undo.RegisterCreatedObjectUndo(inst, "Pasang pelanggan");

        inst.transform.SetParent(titik, false);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        // dudukkan telapak kaki di lantai, berapa pun tinggi modelnya
        Bounds b;
        if (HitungBounds(inst, out b))
            inst.transform.position += new Vector3(0f, titik.position.y - b.min.y, 0f);

        if (pakaiCollider) BeriCollider(titik.gameObject, inst);
        return true;
    }

    static void BeriCollider(GameObject titik, GameObject model)
    {
        Bounds b;
        if (!HitungBounds(model, out b)) return;

        var cc = titik.GetComponent<CapsuleCollider>();
        if (cc == null) cc = Undo.AddComponent<CapsuleCollider>(titik);

        float tinggi = Mathf.Max(0.5f, b.size.y);
        cc.height = tinggi;
        cc.radius = Mathf.Clamp(Mathf.Max(b.size.x, b.size.z) * 0.5f, 0.15f, 0.45f);
        cc.center = new Vector3(0f, tinggi * 0.5f, 0f);
        cc.direction = 1;   // sumbu Y
    }

    void KosongkanModel()
    {
        var akar = Akar(); if (akar == null) return;
        var grup = Grup(akar, "Pelanggan");

        int n = 0;
        foreach (var t in grup.GetComponentsInChildren<Transform>(true))
        {
            if (t == null || t.name != "Model") continue;
            Undo.DestroyObjectImmediate(t.gameObject);
            n++;
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log(n + " model pelanggan dihapus. Titiknya tetap ada.");
    }

    // ---------- Pembantu ----------

    GameObject Akar()
    {
        var a = GameObject.Find("Kedai");
        if (a == null)
            EditorUtility.DisplayDialog("Objek 'Kedai' tidak ada",
                "Buka dulu scene Kedai.", "OK");
        return a;
    }

    static Transform Grup(GameObject akar, string nama)
    {
        var t = CariAnak(akar.transform, nama);
        if (t != null) return t;

        var go = new GameObject(nama);
        Undo.RegisterCreatedObjectUndo(go, "Buat " + nama);
        go.transform.SetParent(akar.transform, false);
        return go.transform;
    }

    static Transform CariAnak(Transform akar, string nama)
    {
        foreach (var t in akar.GetComponentsInChildren<Transform>(true))
            if (t.name == nama) return t;
        return null;
    }

    static bool HitungBounds(GameObject go, out Bounds hasil)
    {
        hasil = new Bounds(go.transform.position, Vector3.zero);
        var rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return false;

        hasil = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) hasil.Encapsulate(rends[i].bounds);
        return true;
    }
}
