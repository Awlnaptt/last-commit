using System.Collections.Generic;
using UnityEngine;

// Mengambil potret wajah karakter untuk kotak dialog.
// Kalau aset karakter punya Sprite potret, itu yang dipakai. Kalau tidak, wajah dipotong
// dari tekstur modelnya (posisi wajah diatur di kolom "wajah" pada aset karakter).
public static class PotretKarakter
{
    static readonly Dictionary<ModelKarakter, Sprite> simpanan = new Dictionary<ModelKarakter, Sprite>();

    public static Sprite Ambil(ModelKarakter k)
    {
        if (k == null) return null;
        if (k.potret != null) return k.potret;
        if (simpanan.TryGetValue(k, out var ada)) return ada;

        Sprite hasil = null;
        var r = k.model != null ? k.model.GetComponentInChildren<Renderer>(true) : null;
        var tex = r != null && r.sharedMaterial != null ? r.sharedMaterial.mainTexture as Texture2D : null;
        if (tex != null)
        {
            float w = tex.width, h = tex.height;
            // Rect di aset dihitung dari kiri-atas; Sprite.Create dari kiri-bawah.
            var area = new Rect(k.wajah.x * w, h - (k.wajah.y + k.wajah.height) * h, k.wajah.width * w, k.wajah.height * h);
            hasil = Sprite.Create(tex, area, new Vector2(0.5f, 0.5f), 100f);
            hasil.name = "Potret_" + k.name;
        }
        else Debug.LogWarning($"[Potret] Tekstur {k.name} tidak ditemukan, potret dikosongkan.");

        simpanan[k] = hasil;
        return hasil;
    }
}
