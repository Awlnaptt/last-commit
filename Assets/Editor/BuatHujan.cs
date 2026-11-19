

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BuatHujan
{
    // Dinding utara ada di z = 4.05, jendela membentang x -4.0 sampai 1.2
    static readonly Vector3 POSISI = new Vector3(-1.4f, 6.5f, 5.6f);
    static readonly Vector3 KOTAK  = new Vector3(11f, 0.2f, 4.5f);

    [MenuItem("Last Commit/Buat Hujan di Luar Jendela")]
    public static void Buat()
    {
        var akar = GameObject.Find("Kedai");
        if (akar == null)
        {
            EditorUtility.DisplayDialog("Objek 'Kedai' tidak ada",
                "Buka dulu scene Kedai.", "OK");
            return;
        }

        var cuaca = Cari(akar, "Cuaca");
        if (cuaca == null)
        {
            var g = new GameObject("Cuaca");
            Undo.RegisterCreatedObjectUndo(g, "Buat Cuaca");
            g.transform.SetParent(akar.transform, false);
            cuaca = g.transform;
        }

        for (int i = cuaca.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(cuaca.GetChild(i).gameObject);

        var go = new GameObject("Hujan");
        Undo.RegisterCreatedObjectUndo(go, "Buat Hujan");
        go.transform.SetParent(cuaca, false);
        go.transform.position = POSISI;

        var ps = go.AddComponent<ParticleSystem>();
        var r = go.GetComponent<ParticleSystemRenderer>();

        // --- Pengaturan utama ---
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = 1.3f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(9f, 14f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.03f);
        main.startColor = new Color(0.78f, 0.85f, 0.95f, 0.45f);
        main.gravityModifier = 0.5f;
        main.maxParticles = 1200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startRotation3D = false;
        main.playOnAwake = true;

        // Jatuh lurus ke bawah
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 450f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = KOTAK;
        shape.randomDirectionAmount = 0f;

        // Tetesan mati sendiri sebelum menumpuk di tanah
        var collision = ps.collision;
        collision.enabled = false;   // dimatikan: tabrakan partikel berat di Iris Xe

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.12f;      // sedikit goyangan supaya tidak terlalu kaku
        noise.frequency = 0.35f;
        noise.scrollSpeed = 1.2f;

        // Modul yang tidak dipakai, dimatikan supaya ringan
        var limit = ps.limitVelocityOverLifetime; limit.enabled = false;
        var sub = ps.subEmitters; sub.enabled = false;
        var trails = ps.trails; trails.enabled = false;
        var lights = ps.lights; lights.enabled = false;

        // --- Tampilan tetesan ---
        r.renderMode = ParticleSystemRenderMode.Stretch;
        r.lengthScale = 5f;
        r.velocityScale = 0.06f;
        r.cameraVelocityScale = 0f;
        r.alignment = ParticleSystemRenderSpace.View;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        r.sortingOrder = 0;

        var sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (sh == null) sh = Shader.Find("Sprites/Default");
        var m = new Material(sh);
        m.name = "M_Hujan";
        if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);   // Transparent
        if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);       // Alpha
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
        m.renderQueue = 3000;
        r.sharedMaterial = m;

        // Kabut tipis supaya hujan terbaca punya kedalaman
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.018f;
        RenderSettings.fogColor = new Color(0.22f, 0.24f, 0.28f);

        Selection.activeGameObject = go;
        EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog("Hujan dibuat",
            "Grup Cuaca > Hujan sudah ada di dalam Kedai.\n\n" +
            "Penyetelan lewat Inspector:\n" +
            "  Emission > Rate over Time : deras atau tidaknya\n" +
            "  Main > Start Speed        : kecepatan jatuh\n" +
            "  Main > Start Color (alpha): pekat atau samarnya\n" +
            "  Renderer > Length Scale   : panjang garis tetesan\n\n" +
            "Jangan lupa Ctrl+S.", "OK");
    }

    static Transform Cari(GameObject akar, string nama)
    {
        foreach (var t in akar.GetComponentsInChildren<Transform>(true))
            if (t.name == nama) return t;
        return null;
    }
}
