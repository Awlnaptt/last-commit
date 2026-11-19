

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class IsiTahap2
{
    static readonly Color KAYU   = new Color(0.42f, 0.29f, 0.20f);
    static readonly Color DAUN   = new Color(0.33f, 0.52f, 0.30f);
    static readonly Color POT    = new Color(0.55f, 0.35f, 0.26f);
    static readonly Color KAIN   = new Color(0.46f, 0.33f, 0.31f);
    static readonly Color KERTAS = new Color(0.86f, 0.80f, 0.70f);
    static readonly Color KACA   = new Color(0.80f, 0.82f, 0.80f);
    static readonly Color LOGAM  = new Color(0.25f, 0.23f, 0.21f);
    static readonly Color HANGAT = new Color(1f, 0.74f, 0.44f);

    [MenuItem("Last Commit/Isi Tahap 2")]
    public static void Isi()
    {
        var akar = GameObject.Find("Kedai");
        if (akar == null)
        {
            EditorUtility.DisplayDialog("Objek 'Kedai' tidak ada",
                "Buka dulu scene Kedai yang sudah ada blockout-nya.", "OK");
            return;
        }

        // Dua grup terpisah, dan pemisahan ini disengaja:
        //   Pencahayaan = wajib, ruangan gelap gulita tanpanya
        //   Tahap2      = hiasan, boleh dimatikan kapan saja
        var pencahayaan = Grup(akar, "Pencahayaan");
        var tahap2 = Grup(akar, "Tahap2");

        int jumlah = 0;

        // --- Tanaman di dua sudut yang masih kosong ---
        jumlah += Tanaman(tahap2, "Tanaman_Pot1", new Vector3(-4.5f, 0f, 3.5f), 1.1f);
        jumlah += Tanaman(tahap2, "Tanaman_Pot2", new Vector3( 1.7f, 0f, -3.4f), 0.9f);

        int dibuang = BersihkanLampuLiar(akar, pencahayaan);

        // --- Tahap 1: dua lampu saja, counter dan seberangnya ---
        // Jangkauannya dilebihkan supaya ruangan tetap terbaca tanpa Tahap2.
        jumlah += LampuGantung(pencahayaan, "LampuGantung_Counter",  new Vector3( 3.4f, 0f,  0.5f), 9f);
        jumlah += LampuGantung(pencahayaan, "LampuGantung_Seberang", new Vector3(-3.5f, 0f, -0.4f), 9f);

        // --- Tahap 2: lampu tambahan di atas meja-meja ---
        jumlah += LampuGantung(tahap2, "LampuGantung_Meja1",     new Vector3(-3.5f, 0f,  2.6f));
        jumlah += LampuGantung(tahap2, "LampuGantung_Meja2",     new Vector3(-1.0f, 0f,  2.6f));
        jumlah += LampuGantung(tahap2, "LampuGantung_MejaSudut", new Vector3( 3.9f, 0f, -3.2f));

        // --- Poster di dinding timur dan selatan ---
        jumlah += Kotak(tahap2, "Poster_Timur",   new Vector3( 4.92f, 1.8f, -1.0f), new Vector3(0.04f, 0.9f, 0.65f), KERTAS);
        jumlah += Kotak(tahap2, "Poster_Selatan", new Vector3( 0.8f,  1.8f, -3.94f), new Vector3(0.75f, 0.95f, 0.04f), KERTAS);

        // --- Keset di depan pintu ---
        jumlah += Kotak(tahap2, "Keset", new Vector3(-3.5f, 0.015f, -3.5f), new Vector3(1.3f, 0.03f, 0.85f), KAIN);

        // --- Dua kursi bar menghadap counter ---
        jumlah += KursiBar(tahap2, "KursiBar1", new Vector3(2.4f, 0f,  1.5f));
        jumlah += KursiBar(tahap2, "KursiBar2", new Vector3(2.4f, 0f, -0.4f));

        // --- Benda kecil di atas perabot yang sudah ada ---
        jumlah += DiAtas(akar, tahap2, "Gelas_Meja1", "Meja1", new Vector3(0.09f, 0.13f, 0.09f), KACA);
        jumlah += DiAtas(akar, tahap2, "Gelas_Meja2", "Meja2", new Vector3(0.09f, 0.13f, 0.09f), KACA);
        jumlah += DiAtas(akar, tahap2, "Gelas_Meja3", "Meja3", new Vector3(0.09f, 0.13f, 0.09f), KACA);
        jumlah += DiAtas(akar, tahap2, "TumpukanBuku", "RakBuku", new Vector3(0.45f, 0.22f, 0.28f), KAYU);
        jumlah += DiAtas(akar, tahap2, "ToplesBiji", "Counter", new Vector3(0.22f, 0.3f, 0.22f), KAYU);

        // Cahaya dasar sedikit dinaikkan supaya sudut gelap tetap terbaca
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.17f, 0.16f, 0.18f);

        pencahayaan.gameObject.SetActive(true);
        tahap2.gameObject.SetActive(true);

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("Tahap2 diisi " + jumlah + " objek, " + dibuang + " lampu lama dibuang.");

        EditorUtility.DisplayDialog("Selesai",
            jumlah + " benda tambahan dibuat di dalam grup Tahap2, dan grupnya dinyalakan.\n" +
            dibuang + " point light lama dibuang.\n\n" +
            "Tahap 1 (grup Pencahayaan): 2 lampu, di atas counter dan seberangnya.\n" +
            "Tahap 2: 3 lampu tambahan di atas meja-meja.\n\n" +
            "Kalau ruangan jadi terlalu terang atau ada lampu yang mati sendiri, buka " +
            "URP Asset > Lighting > Additional Lights dan naikkan Per Object Limit.\n\n" +
            "Jangan lupa Ctrl+S.", "OK");
    }

    // ---------- Pembuat benda ----------

    static int Tanaman(Transform induk, string nama, Vector3 posisi, float tinggi)
    {
        var akar = new GameObject(nama);
        Undo.RegisterCreatedObjectUndo(akar, "Buat " + nama);
        akar.transform.SetParent(induk, false);
        akar.transform.position = posisi;

        float tinggiPot = tinggi * 0.33f;
        Kotak(akar.transform, "Pot", posisi + new Vector3(0f, tinggiPot * 0.5f, 0f),
              new Vector3(0.36f, tinggiPot, 0.36f), POT);
        Kotak(akar.transform, "Daun", posisi + new Vector3(0f, tinggiPot + (tinggi - tinggiPot) * 0.5f, 0f),
              new Vector3(0.6f, tinggi - tinggiPot, 0.6f), DAUN);
        return 1;
    }

    static int LampuGantung(Transform induk, string nama, Vector3 posisiLantai, float jangkauan = 6.5f)
    {
        var akar = new GameObject(nama);
        Undo.RegisterCreatedObjectUndo(akar, "Buat " + nama);
        akar.transform.SetParent(induk, false);
        akar.transform.position = posisiLantai;

        // kabel dari langit-langit (y = 3) turun ke kap lampu
        Kotak(akar.transform, "Kabel", new Vector3(posisiLantai.x, 2.72f, posisiLantai.z),
              new Vector3(0.03f, 0.56f, 0.03f), LOGAM, false);
        Kotak(akar.transform, "Kap", new Vector3(posisiLantai.x, 2.34f, posisiLantai.z),
              new Vector3(0.38f, 0.22f, 0.38f), LOGAM, false);

        var l = new GameObject("Cahaya");
        l.transform.SetParent(akar.transform, false);
        l.transform.position = new Vector3(posisiLantai.x, 2.15f, posisiLantai.z);
        var lampu = l.AddComponent<Light>();
        lampu.type = LightType.Point;
        lampu.color = HANGAT;
        lampu.intensity = 1.35f;
        lampu.range = jangkauan;
        lampu.shadows = LightShadows.None;   // bayangan point light mahal di Iris Xe
        return 1;
    }

    static int KursiBar(Transform induk, string nama, Vector3 posisi)
    {
        var akar = new GameObject(nama);
        Undo.RegisterCreatedObjectUndo(akar, "Buat " + nama);
        akar.transform.SetParent(induk, false);
        akar.transform.position = posisi;

        Kotak(akar.transform, "Tiang", posisi + new Vector3(0f, 0.32f, 0f),
              new Vector3(0.08f, 0.64f, 0.08f), LOGAM);
        Kotak(akar.transform, "Dudukan", posisi + new Vector3(0f, 0.69f, 0f),
              new Vector3(0.36f, 0.1f, 0.36f), KAYU);
        return 1;
    }

    // Taruh benda kecil persis di permukaan atas objek yang sudah ada
    static int DiAtas(GameObject akarKedai, Transform induk, string nama,
                      string namaAlas, Vector3 ukuran, Color warna)
    {
        var alas = Cari(akarKedai, namaAlas);
        if (alas == null)
        {
            Debug.LogWarning("'" + namaAlas + "' tidak ketemu, " + nama + " dilewati.");
            return 0;
        }

        Bounds b;
        if (!HitungBounds(alas.gameObject, out b))
        {
            Debug.LogWarning("'" + namaAlas + "' tidak punya Renderer, " + nama + " dilewati.");
            return 0;
        }

        var posisi = new Vector3(b.center.x, b.max.y + ukuran.y * 0.5f, b.center.z);
        Kotak(induk, nama, posisi, ukuran, warna);
        return 1;
    }

    // Cari grup di dalam Kedai, buat kalau belum ada, lalu kosongkan isinya
    static Transform Grup(GameObject akar, string nama)
    {
        var t = Cari(akar, nama);
        if (t == null)
        {
            var go = new GameObject(nama);
            Undo.RegisterCreatedObjectUndo(go, "Buat " + nama);
            go.transform.SetParent(akar.transform, false);
            t = go.transform;
        }

        for (int i = t.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(t.GetChild(i).gameObject);

        return t;
    }

    // Buang point light yang tidak menempel di lampu gantung, supaya tidak dobel
    static int BersihkanLampuLiar(GameObject akar, Transform tahap2)
    {
        int n = 0;
        foreach (var l in akar.GetComponentsInChildren<Light>(true))
        {
            if (l == null || l.type != LightType.Point) continue;
            if (l.transform.IsChildOf(tahap2)) continue;

            // kalau objeknya cuma pembawa lampu, buang objeknya; kalau tidak, buang komponennya saja
            if (l.GetComponent<Renderer>() == null && l.transform.childCount == 0)
                Undo.DestroyObjectImmediate(l.gameObject);
            else
                Undo.DestroyObjectImmediate(l);
            n++;
        }
        return n;
    }

    // ---------- Pembantu ----------

    static int Kotak(Transform induk, string nama, Vector3 posisi, Vector3 skala, Color warna,
                     bool daftarUndo = true)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if (daftarUndo) Undo.RegisterCreatedObjectUndo(go, "Buat " + nama);
        go.name = nama;
        go.transform.SetParent(induk, false);
        go.transform.position = posisi;
        go.transform.localScale = skala;

        var r = go.GetComponent<Renderer>();
        var sh = Shader.Find("Universal Render Pipeline/Lit");
        var m = new Material(sh != null ? sh : Shader.Find("Standard"));
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", warna);
        m.color = warna;
        r.sharedMaterial = m;

        return 1;
    }

    static Transform Cari(GameObject akar, string nama)
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
