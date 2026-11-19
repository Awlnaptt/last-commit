// PasangPelanggan.cs — menaruh pelanggan di kedai
//
// Taruh di: Assets/Editor/PasangPelanggan.cs
// Buka lewat menu: Last Commit > Pasang Pelanggan
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
        public Vector3 posisi;
        public Vector3 hadap;    // titik yang dilihat pelanggan
        public string catatan;
    }

    // Kedai: x -5..5, z -4..4. Counter di x 2.9..3.9, meja di sisi barat.
    static readonly Titik[] TITIK = {
        new Titik { nama = "TitikAntre1",  posisi = new Vector3( 2.3f, 0f,  1.4f), hadap = new Vector3( 3.4f, 0f,  1.4f), catatan = "paling depan di counter" },
        new Titik { nama = "TitikAntre2",  posisi = new Vector3( 2.3f, 0f,  0.5f), hadap = new Vector3( 3.4f, 0f,  0.5f), catatan = "antrean kedua" },
        new Titik { nama = "TitikAntre3",  posisi = new Vector3( 2.3f, 0f, -0.4f), hadap = new Vector3( 3.4f, 0f, -0.4f), catatan = "antrean ketiga" },
        new Titik { nama = "TitikMeja1",   posisi = new Vector3(-3.5f, 0f,  1.75f), hadap = new Vector3(-3.5f, 0f,  2.6f), catatan = "berdiri di sisi Meja1" },
        new Titik { nama = "TitikMeja2",   posisi = new Vector3(-1.0f, 0f,  1.75f), hadap = new Vector3(-1.0f, 0f,  2.6f), catatan = "berdiri di sisi Meja2" },
        new Titik { nama = "TitikMeja3",   posisi = new Vector3(-2.65f, 0f, -0.4f), hadap = new Vector3(-3.5f, 0f, -0.4f), catatan = "berdiri di sisi Meja3" },
        new Titik { nama = "TitikJendela", posisi = new Vector3(-1.4f, 0f,  3.1f), hadap = new Vector3(-1.4f, 0f,  4.0f), catatan = "berdiri melihat hujan" },
        new Titik { nama = "TitikSantai",  posisi = new Vector3( 1.2f, 0f, -2.9f), hadap = new Vector3( 1.2f, 0f, -1.0f), catatan = "pengisi suasana" },
    };

    GameObject mNpc1, mNpc2, mNpc3, mUmumA, mUmumB;
    bool pakaiCollider = true;
    Vector2 scroll;

    [MenuItem("Last Commit/Pasang Pelanggan")]
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
            "Meja1, Jendela = umum A · Meja2, Meja3, Santai = umum B",
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

        int baru = 0, dipindah = 0;

        foreach (var t in TITIK)
        {
            var ada = CariAnak(grup, t.nama);
            if (ada == null)
            {
                var go = new GameObject(t.nama);
                Undo.RegisterCreatedObjectUndo(go, "Buat " + t.nama);
                go.transform.SetParent(grup, false);
                Pasangkan(go.transform, t);
                BeriIkon(go);
                baru++;
            }
            else if (paksaPosisi)
            {
                Undo.RecordObject(ada, "Kembalikan " + t.nama);
                Pasangkan(ada, t);
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

    static void Pasangkan(Transform t, Titik data)
    {
        t.position = data.posisi;

        Vector3 arah = data.hadap - data.posisi;
        arah.y = 0f;
        if (arah.sqrMagnitude > 0.0001f)
            t.rotation = Quaternion.LookRotation(arah.normalized, Vector3.up);
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
            { "TitikJendela", mUmumA },
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
