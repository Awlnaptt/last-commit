// PerbaikiKaca.cs — membuat kaca jendela tembus pandang.
//
// Menu: Last Commit > Perbaiki Kaca Jendela
//
// Material M_Kaca dari BuatKedai dibuat dengan URP Lit mode Opaque dan alpha 1,
// jadi tampil sebagai balok biru. Script ini membuat material kaca baru yang
// Transparent (alpha blending), licin (memantulkan cahaya), tanpa bayangan,
// lalu memasangnya ke objek JendelaHujan. Aman dijalankan berulang kali.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class PerbaikiKaca
{
    const string FOLDER = "Assets/Art/Materials";
    const string PATH = FOLDER + "/M_KacaTransparan.mat";

    [MenuItem("Last Commit/Perbaiki Kaca Jendela")]
    public static void Perbaiki()
    {
        var kedai = AlatEditor.Kedai();
        if (kedai == null) return;

        var kaca = AlatEditor.Cari(kedai.transform, "JendelaHujan");
        if (kaca == null)
        {
            EditorUtility.DisplayDialog("Kaca", "Objek JendelaHujan tidak ditemukan di bawah Kedai.", "OK");
            return;
        }

        var mat = BuatMaterial();
        foreach (var r in kaca.GetComponentsInChildren<Renderer>(true))
        {
            Undo.RecordObject(r, "Pasang kaca transparan");
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;   // cahaya dan bayangan hujan tetap masuk
        }

        EditorSceneManager.MarkSceneDirty(kedai.scene);
        Selection.activeObject = mat;
        EditorUtility.DisplayDialog("Kaca",
            "Kaca jendela sekarang tembus pandang (" + PATH + ").\n\n" +
            "Atur tingkat bening lewat Base Map > warna > Alpha di material itu.\nJangan lupa Ctrl+S.", "OK");
    }

    static Material BuatMaterial()
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(PATH);
        if (m == null)
        {
            AlatEditor.Folder(FOLDER);
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            m = new Material(sh) { name = "M_KacaTransparan" };
            AssetDatabase.CreateAsset(m, PATH);
        }

        // Biru muda, alpha 0,25: masih terlihat sebagai kaca, tapi luar jendela kelihatan.
        var warna = new Color(0.70f, 0.85f, 0.95f, 0.25f);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", warna);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.92f);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);

        // Mode Transparent ala URP Lit. Mengubah _Surface saja tidak cukup:
        // blend, ZWrite, keyword, tag, dan render queue juga harus diubah.
        m.SetFloat("_Surface", 1f);   // Transparent
        m.SetFloat("_Blend", 0f);     // Alpha
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        if (m.HasProperty("_SrcBlendAlpha")) m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        if (m.HasProperty("_DstBlendAlpha")) m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.DisableKeyword("_ALPHATEST_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)RenderQueue.Transparent;
        m.SetShaderPassEnabled("DepthOnly", false);
        m.SetShaderPassEnabled("ShadowCaster", false);

        EditorUtility.SetDirty(m);
        AssetDatabase.SaveAssets();
        return m;
    }
}
