

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PasangModelWindow : EditorWindow
{
    enum ModeUkuran { IsiPenuhKotak = 0, JagaProporsi = 1, UkuranAsli = 2 }

    GameObject mMeja, mKursi, mRakBuku, mTanaman, mMejaSudut;
    GameObject mCounter, mMesinKopi, mPintu, mSpeaker, mPapanMenu, mLaptop;
    GameObject mKursiBar, mPoster, mKeset, mGelas, mBuku, mToples, mLampu;

    ModeUkuran mode = ModeUkuran.IsiPenuhKotak;
    Vector2 scroll;

    [MenuItem("Last Commit/Pasang Model ke Blockout")]
    public static void Buka()
    {
        var w = GetWindow<PasangModelWindow>("Pasang Model");
        w.minSize = new Vector2(430, 680);
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.HelpBox(
            "Seret model dari jendela Project ke kotak di bawah, lalu klik tombol hijau.\n\n" +
            "Kotak yang dibiarkan kosong akan dilewati.", MessageType.Info);

        if (GUILayout.Button("Coba tebak dari nama file"))
            Tebak();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Perabot biasa", EditorStyles.boldLabel);
        mMeja      = Slot("Meja  (dipakai 3x)", mMeja);
        mKursi     = Slot("Kursi  (dipakai 8x)", mKursi);
        mRakBuku   = Slot("Rak buku", mRakBuku);
        mMejaSudut = Slot("Meja sudut  (alas laptop)", mMejaSudut);
        mTanaman   = Slot("Tanaman  (Tahap2)", mTanaman);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Objek yang namanya dipakai Tugas A", EditorStyles.boldLabel);
        mCounter   = Slot("Counter", mCounter);
        mMesinKopi = Slot("Mesin kopi", mMesinKopi);
        mPintu     = Slot("Pintu", mPintu);
        mSpeaker   = Slot("Speaker", mSpeaker);
        mPapanMenu = Slot("Papan menu", mPapanMenu);
        mLaptop    = Slot("Laptop", mLaptop);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Hiasan Tahap 2", EditorStyles.boldLabel);
        mKursiBar = Slot("Kursi bar  (dipakai 2x)", mKursiBar);
        mPoster   = Slot("Poster  (dipakai 2x)", mPoster);
        mKeset    = Slot("Keset depan pintu", mKeset);
        mGelas    = Slot("Gelas di meja  (dipakai 3x)", mGelas);
        mBuku     = Slot("Tumpukan buku  (di rak)", mBuku);
        mToples   = Slot("Toples biji  (di counter)", mToples);
        mLampu    = Slot("Kap lampu gantung  (dipakai 5x)", mLampu);

        EditorGUILayout.LabelField(
            "Tanaman, kursi bar, dan lampu gantung tersusun dari beberapa kotak. " +
            "Setelah diganti model, posisinya mungkin perlu digeser sedikit: tahan V lalu snap.",
            EditorStyles.wordWrappedMiniLabel);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Ukuran model", EditorStyles.boldLabel);
        mode = (ModeUkuran)EditorGUILayout.EnumPopup("Mode", mode);

        string ket;
        switch (mode)
        {
            case ModeUkuran.IsiPenuhKotak:
                ket = "Model diregangkan mengisi kotak blockout. Counter tetap panjang, " +
                      "pintu tetap sebesar lubangnya. Bentuknya bisa agak gepeng atau memanjang, " +
                      "tapi tata letaknya dijamin rapi.";
                break;
            case ModeUkuran.JagaProporsi:
                ket = "Model diperbesar atau diperkecil seragam sampai setinggi kotak. " +
                      "Bentuknya tetap wajar, tapi counter akan jadi pendek.";
                break;
            default:
                ket = "Model dipakai apa adanya, tanpa diskalakan sama sekali.";
                break;
        }
        EditorGUILayout.LabelField(ket, EditorStyles.wordWrappedMiniLabel);

        EditorGUILayout.Space(12);
        var warnaLama = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.55f, 0.85f, 0.55f);
        if (GUILayout.Button("Pasang semua yang terisi", GUILayout.Height(36)))
            PasangSemua();
        GUI.backgroundColor = warnaLama;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(
            "Kalau hasilnya berantakan: Ctrl+Z, atau pulihkan lewat Git. " +
            "Tombol bangun ulang sudah dihapus karena terlalu mudah tertekan " +
            "dan menghapus seluruh isi scene.", EditorStyles.wordWrappedMiniLabel);

        EditorGUILayout.EndScrollView();
    }

    static GameObject Slot(string label, GameObject nilai)
    {
        return (GameObject)EditorGUILayout.ObjectField(label, nilai, typeof(GameObject), false);
    }

    // ---------- Tebak otomatis ----------

    void Tebak()
    {
        if (mMeja == null)      mMeja      = Cari("tableround", "table", "desk", "meja");
        if (mKursi == null)     mKursi     = Cari("chair", "stool", "kursi");
        if (mRakBuku == null)   mRakBuku   = Cari("bookcase", "shelf", "book", "rak");
        if (mMejaSudut == null) mMejaSudut = Cari("sidetable", "nightstand", "cabinet");
        if (mTanaman == null)   mTanaman   = Cari("plant", "pot", "flower", "tanaman");
        if (mCounter == null)   mCounter   = Cari("kitchencabinet", "counter", "cabinet", "kitchen");
        if (mMesinKopi == null) mMesinKopi = Cari("coffee", "espresso", "kettle", "microwave");
        if (mPintu == null)     mPintu     = Cari("door", "pintu");
        if (mSpeaker == null)   mSpeaker   = Cari("speaker", "radio", "stereo");
        if (mPapanMenu == null) mPapanMenu = Cari("painting", "board", "sign", "frame");
        if (mLaptop == null)    mLaptop    = Cari("laptop", "computer", "monitor");
        if (mKursiBar == null)  mKursiBar  = Cari("stool", "barstool", "chairstool");
        if (mPoster == null)    mPoster    = Cari("poster", "painting", "frame", "picture");
        if (mKeset == null)     mKeset     = Cari("rug", "carpet", "mat", "keset");
        if (mGelas == null)     mGelas     = Cari("cup", "mug", "glass", "coffee");
        if (mBuku == null)      mBuku      = Cari("books", "bookstack", "book");
        if (mToples == null)    mToples    = Cari("jar", "canister", "pot", "container");
        if (mLampu == null)     mLampu     = Cari("lamp", "ceilinglamp", "light", "lantern");

        Repaint();
        Debug.Log("Tebakan selesai. Periksa tiap kotak, yang salah tinggal diganti manual.");
    }

    static GameObject Cari(params string[] kataKunci)
    {
        string folder = AssetDatabase.IsValidFolder("Assets/Art") ? "Assets/Art" : "Assets";
        var guids = AssetDatabase.FindAssets("t:GameObject", new[] { folder });

        foreach (var kata in kataKunci)
        {
            string k = kata.ToLowerInvariant();
            foreach (var g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                string nama = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                if (nama.Contains(k))
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        return null;
    }

    // ---------- Pemasangan ----------

    void PasangSemua()
    {
        var akar = GameObject.Find("Kedai");
        if (akar == null)
        {
            EditorUtility.DisplayDialog("Objek 'Kedai' tidak ada",
                "Jalankan dulu menu Last Commit > Bangun Blockout Kedai, lalu coba lagi.", "OK");
            return;
        }

        int n = 0;

        n += Pasang(akar, "Meja1", mMeja);
        n += Pasang(akar, "Meja2", mMeja);
        n += Pasang(akar, "Meja3", mMeja);

        n += Pasang(akar, "Kursi1a", mKursi);
        n += Pasang(akar, "Kursi1b", mKursi);
        n += Pasang(akar, "Kursi2a", mKursi);
        n += Pasang(akar, "Kursi2b", mKursi);
        n += Pasang(akar, "Kursi3a", mKursi);
        n += Pasang(akar, "Kursi3b", mKursi);
        n += Pasang(akar, "KursiTambahan1", mKursi);
        n += Pasang(akar, "KursiTambahan2", mKursi);

        n += Pasang(akar, "RakBuku", mRakBuku);
        n += Pasang(akar, "MejaSudut", mMejaSudut);
        n += Pasang(akar, "Tanaman_Pot1", mTanaman);
        n += Pasang(akar, "Tanaman_Pot2", mTanaman);

        n += Pasang(akar, "KursiBar1", mKursiBar);
        n += Pasang(akar, "KursiBar2", mKursiBar);
        n += Pasang(akar, "Poster_Timur", mPoster);
        n += Pasang(akar, "Poster_Selatan", mPoster);
        n += Pasang(akar, "Keset", mKeset);
        n += Pasang(akar, "Gelas_Meja1", mGelas);
        n += Pasang(akar, "Gelas_Meja2", mGelas);
        n += Pasang(akar, "Gelas_Meja3", mGelas);
        n += Pasang(akar, "TumpukanBuku", mBuku);
        n += Pasang(akar, "ToplesBiji", mToples);

        n += Pasang(akar, "LampuGantung_Counter", mLampu);
        n += Pasang(akar, "LampuGantung_Seberang", mLampu);
        n += Pasang(akar, "LampuGantung_Meja1", mLampu);
        n += Pasang(akar, "LampuGantung_Meja2", mLampu);
        n += Pasang(akar, "LampuGantung_MejaSudut", mLampu);

        n += Pasang(akar, "Counter", mCounter);
        n += Pasang(akar, "MesinKopi", mMesinKopi);
        n += Pasang(akar, "Pintu", mPintu);
        n += Pasang(akar, "Speaker", mSpeaker);
        n += Pasang(akar, "PapanMenu", mPapanMenu);
        n += Pasang(akar, "Laptop", mLaptop);

        // Benda yang harus menumpang di atas benda lain, dirapikan terakhir
        DudukkanDiAtas(akar, "MesinKopi", "Counter");
        DudukkanDiAtas(akar, "Laptop", "MejaSudut");
        DudukkanDiAtas(akar, "Gelas_Meja1", "Meja1");
        DudukkanDiAtas(akar, "Gelas_Meja2", "Meja2");
        DudukkanDiAtas(akar, "Gelas_Meja3", "Meja3");
        DudukkanDiAtas(akar, "TumpukanBuku", "RakBuku");
        DudukkanDiAtas(akar, "ToplesBiji", "Counter");

        EditorSceneManager.MarkAllScenesDirty();
        EditorUtility.DisplayDialog("Selesai",
            n + " objek sudah diganti modelnya.\n\nJangan lupa simpan scene: Ctrl+S.", "OK");
    }

    int Pasang(GameObject akar, string nama, GameObject model)
    {
        if (model == null) return 0;

        Transform target = CariAnak(akar.transform, nama);
        if (target == null) return 0;

        Undo.RegisterFullObjectHierarchyUndo(target.gameObject, "Pasang model " + nama);

        Vector3 skalaLama = target.localScale;

        var mr = target.GetComponent<MeshRenderer>();
        bool dulunyaKotak = (mr != null);
        if (mr != null) Undo.DestroyObjectImmediate(mr);

        var mf = target.GetComponent<MeshFilter>();
        if (mf != null) Undo.DestroyObjectImmediate(mf);

        for (int i = target.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(target.GetChild(i).gameObject);

        target.localScale = Vector3.one;

        GameObject inst = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (inst == null) inst = Instantiate(model);
        inst.name = "Model";
        Undo.RegisterCreatedObjectUndo(inst, "Pasang model " + nama);

        inst.transform.SetParent(target, false);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        if (dulunyaKotak)
        {
            Bounds b0;
            if (mode != ModeUkuran.UkuranAsli && HitungBounds(inst, out b0))
            {
                Vector3 s = Vector3.one;

                if (mode == ModeUkuran.IsiPenuhKotak)
                {
                    // Satu sumbu tidak boleh meledak sendirian. Benda pipih seperti
                    // poster dan keset punya ketebalan model hampir nol, dan
                    // pembagian terhadap angka itu menghasilkan skala ratusan kali.
                    // Semua sumbu dikunci di sekitar faktor tinggi.
                    float fAcuan = Aman(skalaLama.y, b0.size.y);
                    float bawah = fAcuan * 0.2f;
                    float atas  = fAcuan * 5f;

                    s.x = Mathf.Clamp(Aman(skalaLama.x, b0.size.x), bawah, atas);
                    s.y = fAcuan;
                    s.z = Mathf.Clamp(Aman(skalaLama.z, b0.size.z), bawah, atas);
                }
                else // JagaProporsi
                {
                    float f = Aman(skalaLama.y, b0.size.y);
                    s = new Vector3(f, f, f);
                }

                inst.transform.localScale = s;
            }

            // Dudukkan alas model di alas kotak lama, lalu pusatkan kiri-kanan dan depan-belakang
            Bounds b;
            if (HitungBounds(inst, out b))
            {
                Vector3 pusat = target.position;
                float alasKotak = pusat.y - skalaLama.y * 0.5f;

                inst.transform.position += new Vector3(
                    pusat.x - b.center.x,
                    alasKotak - b.min.y,
                    pusat.z - b.center.z);
            }
        }

        PasangCollider(target.gameObject, skalaLama);
        return 1;
    }

    static float Aman(float target, float ukuran)
    {
        if (ukuran < 0.0001f) return 1f;
        float f = target / ukuran;
        if (f < 0.0001f || f > 500f) return 1f;
        return f;
    }

    // Letakkan satu objek tepat di atas permukaan objek lain
    static void DudukkanDiAtas(GameObject akar, string namaAtas, string namaBawah)
    {
        Transform atas = CariAnak(akar.transform, namaAtas);
        Transform bawah = CariAnak(akar.transform, namaBawah);
        if (atas == null || bawah == null) return;

        Bounds bAtas, bBawah;
        if (!HitungBounds(atas.gameObject, out bAtas)) return;
        if (!HitungBounds(bawah.gameObject, out bBawah)) return;

        Undo.RecordObject(atas, "Dudukkan " + namaAtas);
        atas.position += new Vector3(
            bBawah.center.x - bAtas.center.x,
            bBawah.max.y - bAtas.min.y,
            bBawah.center.z - bAtas.center.z);

        PasangCollider(atas.gameObject, atas.localScale);
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

    static Transform CariAnak(Transform akar, string nama)
    {
        foreach (var t in akar.GetComponentsInChildren<Transform>(true))
            if (t.name == nama) return t;
        return null;
    }

    static void PasangCollider(GameObject target, Vector3 skalaCadangan)
    {
        var bc = target.GetComponent<BoxCollider>();
        if (bc == null) bc = Undo.AddComponent<BoxCollider>(target);

        Bounds b;
        if (HitungBounds(target, out b) && b.size.sqrMagnitude > 0.000001f)
        {
            bc.center = target.transform.InverseTransformPoint(b.center);
            bc.size = b.size;
        }
        else
        {
            bc.center = Vector3.zero;
            bc.size = skalaCadangan;
        }
    }
}
