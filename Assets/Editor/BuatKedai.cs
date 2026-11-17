// BuatKedai.cs — Blockout kedai "Last Commit"  (versi 2, error CS1503 sudah diperbaiki)
// Taruh file ini di: Assets/Editor/BuatKedai.cs
// Lalu di Unity, menu atas: Last Commit > Bangun Blockout Kedai
//
// Script ini HANYA jalan di Editor, tidak ikut ke build.
// Isinya kotak-kotak untuk menata ruangan. Nanti tinggal diganti
// model asli dari asset pack, posisinya sudah benar.

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BuatKedai
{
    // Ukuran ruangan, dalam meter
    const float LEBAR = 10f;   // sumbu X, kiri ke kanan
    const float PANJANG = 8f;  // sumbu Z, pintu ke jendela
    const float TINGGI = 3f;

    [MenuItem("Last Commit/Bangun Blockout Kedai")]
    public static void Bangun()
    {
        // Hapus hasil bangun sebelumnya supaya bisa dijalankan berulang kali
        var lama = GameObject.Find("Kedai");
        if (lama != null) UnityEngine.Object.DestroyImmediate(lama);

        var akar = new GameObject("Kedai");
        Undo.RegisterCreatedObjectUndo(akar, "Bangun Blockout Kedai");
        var t = akar.transform;

        var mLantai  = Mat("M_Lantai",  new Color(0.34f, 0.25f, 0.19f));
        var mDinding = Mat("M_Dinding", new Color(0.47f, 0.38f, 0.31f));
        var mKayu    = Mat("M_Kayu",    new Color(0.53f, 0.37f, 0.23f));
        var mCounter = Mat("M_Counter", new Color(0.38f, 0.26f, 0.17f));
        var mGelap   = Mat("M_Gelap",   new Color(0.17f, 0.16f, 0.15f));
        var mKaca    = Mat("M_Kaca",    new Color(0.24f, 0.41f, 0.57f));
        var mAksen   = Mat("M_Aksen",   new Color(0.91f, 0.64f, 0.24f));
        var mDaun    = Mat("M_Daun",    new Color(0.35f, 0.52f, 0.30f));

        // ---------- Struktur ruangan ----------
        var str = Induk("Struktur", t);

        Kotak("Lantai", str, new Vector3(0f, -0.05f, 0f), new Vector3(LEBAR, 0.1f, PANJANG), mLantai);
        Kotak("Plafon", str, new Vector3(0f, TINGGI + 0.05f, 0f), new Vector3(LEBAR, 0.1f, PANJANG), mDinding);

        Kotak("Dinding_Barat", str, new Vector3(-5.05f, 1.5f, 0f), new Vector3(0.1f, TINGGI, PANJANG), mDinding);
        Kotak("Dinding_Timur", str, new Vector3( 5.05f, 1.5f, 0f), new Vector3(0.1f, TINGGI, PANJANG), mDinding);

        // Dinding utara (z = +4), ada lubang jendela x -2..+2, y 1..2.4
        Kotak("Dinding_Utara_Kiri",  str, new Vector3(-3.5f, 1.5f, 4.05f), new Vector3(3f, TINGGI, 0.1f), mDinding);
        Kotak("Dinding_Utara_Kanan", str, new Vector3( 3.5f, 1.5f, 4.05f), new Vector3(3f, TINGGI, 0.1f), mDinding);
        Kotak("Dinding_Utara_Bawah", str, new Vector3( 0f, 0.5f, 4.05f),   new Vector3(4f, 1f, 0.1f), mDinding);
        Kotak("Dinding_Utara_Atas",  str, new Vector3( 0f, 2.7f, 4.05f),   new Vector3(4f, 0.6f, 0.1f), mDinding);

        // Dinding selatan (z = -4), ada lubang pintu x -4.2..-2.8, y 0..2.1
        Kotak("Dinding_Selatan_Kiri",      str, new Vector3(-4.6f, 1.5f, -4.05f),  new Vector3(0.8f, TINGGI, 0.1f), mDinding);
        Kotak("Dinding_Selatan_Kanan",     str, new Vector3( 1.1f, 1.5f, -4.05f),  new Vector3(7.8f, TINGGI, 0.1f), mDinding);
        Kotak("Dinding_Selatan_AtasPintu", str, new Vector3(-3.5f, 2.55f, -4.05f), new Vector3(1.4f, 0.9f, 0.1f), mDinding);

        // ---------- Objek yang namanya dipakai Tugas A ----------
        // JANGAN ganti nama objek di bawah ini tanpa bilang ke Rizki.
        var interaktif = Induk("ObjekInteraktif", t);

        Kotak("JendelaHujan", interaktif, new Vector3(0f, 1.7f, 4f), new Vector3(4f, 1.4f, 0.06f), mKaca);
        Kotak("Pintu", interaktif, new Vector3(-3.5f, 1.05f, -4f), new Vector3(1.3f, 2.1f, 0.08f), mKayu);

        // Counter memanjang ke arah Z di sisi timur, tinggi 1 m
        Kotak("Counter", interaktif, new Vector3(3.4f, 0.5f, 0.5f), new Vector3(1f, 1f, 3.2f), mCounter);
        Kotak("MesinKopi", interaktif, new Vector3(3.4f, 1.25f, 1.6f), new Vector3(0.6f, 0.5f, 0.7f), mGelap);
        Kotak("PapanMenu", interaktif, new Vector3(4.9f, 2.1f, 0.5f), new Vector3(0.08f, 0.8f, 2f), mGelap);

        Kotak("Speaker", interaktif, new Vector3(-4.6f, 0.5f, -2.6f), new Vector3(0.5f, 1f, 0.5f), mGelap);

        // Laptop di sudut tenggara, di atas meja kecil
        Kotak("MejaSudut", t, new Vector3(3.9f, 0.4f, -3.2f), new Vector3(1f, 0.8f, 0.9f), mKayu);
        var laptop = Induk("Laptop", interaktif);
        laptop.transform.localPosition = new Vector3(3.9f, 0.82f, -3.2f);
        Kotak("Badan", laptop, new Vector3(0f, 0f, 0f), new Vector3(0.42f, 0.04f, 0.3f), mGelap);
        var layar = Kotak("Layar", laptop, new Vector3(0f, 0.13f, -0.14f), new Vector3(0.42f, 0.28f, 0.03f), mGelap);
        layar.transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);

        // ---------- Perabot ----------
        var perabot = Induk("Perabot", t);

        Meja("Meja1", perabot, new Vector3(-3.5f, 0f, 2.6f), mKayu);
        Meja("Meja2", perabot, new Vector3(-1f, 0f, 2.6f), mKayu);
        Meja("Meja3", perabot, new Vector3(-3.5f, 0f, -0.4f), mKayu);

        Kursi("Kursi1a", perabot, new Vector3(-4.3f, 0f, 2.6f), 90f, mKayu);
        Kursi("Kursi1b", perabot, new Vector3(-2.7f, 0f, 2.6f), -90f, mKayu);
        Kursi("Kursi2a", perabot, new Vector3(-1.8f, 0f, 2.6f), 90f, mKayu);
        Kursi("Kursi2b", perabot, new Vector3(-0.2f, 0f, 2.6f), -90f, mKayu);
        Kursi("Kursi3a", perabot, new Vector3(-4.3f, 0f, -0.4f), 90f, mKayu);
        Kursi("Kursi3b", perabot, new Vector3(-2.7f, 0f, -0.4f), -90f, mKayu);

        Kotak("RakBuku", perabot, new Vector3(1.4f, 1f, 3.7f), new Vector3(2f, 2f, 0.4f), mKayu);

        // ---------- Pencahayaan ----------
        var cahaya = Induk("Cahaya", t);
        LampuHangat("LampuHangat1", cahaya, new Vector3(-1.5f, 2.5f, 1.5f));
        LampuHangat("LampuHangat2", cahaya, new Vector3(2.5f, 2.5f, -1f));

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.15f, 0.22f); // biru gelap, kesan malam di luar

        // ---------- Titik awal pemain ----------
        var titik = new GameObject("TitikAwalPemain");
        titik.transform.SetParent(t, false);
        titik.transform.localPosition = new Vector3(-3.5f, 0f, -3f);
        titik.transform.localRotation = Quaternion.identity; // menghadap ke dalam kedai

        // ---------- Tahap 2, dimatikan dulu ----------
        var tahap2 = Induk("Tahap2", t);

        Kotak("Tanaman_Pot", tahap2, new Vector3(-4.4f, 0.25f, 3.2f), new Vector3(0.45f, 0.5f, 0.45f), mKayu);
        Bola("Tanaman_Daun", tahap2, new Vector3(-4.4f, 0.8f, 3.2f), new Vector3(0.8f, 0.8f, 0.8f), mDaun);

        Kotak("LampuGantung_Badan", tahap2, new Vector3(0f, 2.55f, 0f), new Vector3(0.5f, 0.3f, 0.5f), mAksen);
        var lampuG = Induk("LampuGantung_Cahaya", tahap2);
        lampuG.transform.localPosition = new Vector3(0f, 2.3f, 0f);
        var lg = lampuG.AddComponent<Light>();
        lg.type = LightType.Point;
        lg.color = new Color(1f, 0.76f, 0.46f);
        lg.intensity = 2f;
        lg.range = 7f;

        Kotak("Poster", tahap2, new Vector3(-4.9f, 2f, 1f), new Vector3(0.06f, 0.9f, 1.3f), mAksen);
        Kursi("KursiTambahan1", tahap2, new Vector3(0.6f, 0f, 2.6f), -90f, mKayu);
        Kursi("KursiTambahan2", tahap2, new Vector3(1.4f, 0f, 1.2f), 180f, mKayu);

        tahap2.SetActive(false); // Tugas A yang menyalakannya lewat script saat upgrade dibeli

        // ---------- Tandai statis supaya lighting bisa di-bake ----------
        TandaiStatis(str);
        TandaiStatis(perabot);

        Selection.activeGameObject = akar;
        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("Blockout kedai selesai dibangun. Jangan lupa simpan scene: Ctrl+S.");
    }

    // ===================== Fungsi pembantu =====================

    static Material Mat(string nama, Color warna)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard"); // jaga-jaga kalau project bukan URP
        var m = new Material(sh);
        m.name = nama;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", warna);
        m.color = warna;
        return m;
    }

    // Dua versi Induk supaya bisa dipanggil dengan GameObject maupun Transform
    static GameObject Induk(string nama, Transform parent)
    {
        var go = new GameObject(nama);
        go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject Induk(string nama, GameObject parent)
    {
        return Induk(nama, parent.transform);
    }

    // Dua versi Kotak, alasannya sama
    static GameObject Kotak(string nama, GameObject induk, Vector3 pos, Vector3 ukuran, Material mat)
    {
        return Kotak(nama, induk.transform, pos, ukuran, mat);
    }

    static GameObject Kotak(string nama, Transform induk, Vector3 pos, Vector3 ukuran, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nama;
        go.transform.SetParent(induk, false);
        go.transform.localPosition = pos;
        go.transform.localScale = ukuran;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static GameObject Bola(string nama, GameObject induk, Vector3 pos, Vector3 ukuran, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = nama;
        go.transform.SetParent(induk.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = ukuran;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static void Meja(string nama, GameObject induk, Vector3 pos, Material mat)
    {
        var meja = Induk(nama, induk);
        meja.transform.localPosition = pos;
        Kotak("Papan", meja, new Vector3(0f, 0.75f, 0f), new Vector3(1.2f, 0.08f, 1.2f), mat);

        var kaki = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        kaki.name = "Kaki";
        kaki.transform.SetParent(meja.transform, false);
        kaki.transform.localPosition = new Vector3(0f, 0.36f, 0f);
        kaki.transform.localScale = new Vector3(0.18f, 0.36f, 0.18f);
        kaki.GetComponent<Renderer>().sharedMaterial = mat;
    }

    static void Kursi(string nama, GameObject induk, Vector3 pos, float rotasiY, Material mat)
    {
        var kursi = Induk(nama, induk);
        kursi.transform.localPosition = pos;
        kursi.transform.localRotation = Quaternion.Euler(0f, rotasiY, 0f);
        Kotak("Dudukan", kursi, new Vector3(0f, 0.45f, 0f), new Vector3(0.45f, 0.08f, 0.45f), mat);
        Kotak("Sandaran", kursi, new Vector3(0f, 0.72f, -0.2f), new Vector3(0.45f, 0.5f, 0.07f), mat);
        Kotak("KakiKursi", kursi, new Vector3(0f, 0.22f, 0f), new Vector3(0.1f, 0.45f, 0.1f), mat);
    }

    static void LampuHangat(string nama, GameObject induk, Vector3 pos)
    {
        var go = Induk(nama, induk);
        go.transform.localPosition = pos;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.74f, 0.44f);
        l.intensity = 2.6f;
        l.range = 8f;
        l.shadows = LightShadows.Soft; // kalau berat, ganti ke LightShadows.None
    }

    static void TandaiStatis(GameObject induk)
    {
        foreach (var tr in induk.GetComponentsInChildren<Transform>(true))
            tr.gameObject.isStatic = true;
    }
}
