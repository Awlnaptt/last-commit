using UnityEngine;

// Kipas langit-langit dari kode, bagian yang DIAM: dudukan plafon, batang, dan rumah motor.
// Baling-baling dibuat terpisah (BalingKipas, anak "Baling") dan hanya itu yang berputar.
// (Kipas Kenney satu mesh utuh, jadi dulu batangnya ikut berputar dan bergeser.)
// Titik asal: titik dudukan di plafon, bentuk menggantung ke bawah.
public class KipasLangit : MeshDariKode
{
    public float panjangBatang = 0.22f;
    public Color warnaLogam = new Color(0.16f, 0.15f, 0.14f);
    public Color warnaRumah = new Color(0.78f, 0.74f, 0.66f);

    // Ketinggian baling-baling relatif titik asal (dipakai saat memasang anak "Baling").
    public float YBaling => -(0.05f + panjangBatang + 0.06f);

    protected override Mesh Bangun()
    {
        var b = new PembangunMesh(2);   // 0 logam, 1 rumah motor
        b.Silinder(0, new Vector3(0f, -0.05f, 0f), 0.05f, 0.08f, 0.05f, 16, false, true);              // dudukan plafon
        b.Silinder(0, new Vector3(0f, -0.05f - panjangBatang, 0f), 0.014f, 0.014f, panjangBatang, 8, false, false);
        float yRumah = -0.05f - panjangBatang - 0.10f;
        b.Silinder(1, new Vector3(0f, yRumah, 0f), 0.11f, 0.13f, 0.10f, 18, true, true);                // rumah motor
        b.Silinder(1, new Vector3(0f, yRumah - 0.035f, 0f), 0.05f, 0.10f, 0.035f, 18, false, true);    // tutup bawah
        return b.Jadi("Mesh_KipasLangit");
    }

    protected override Material[] Bahan() => new[]
    {
        BahanKode.Lit("Logam_Kipas", warnaLogam, null, 0.5f),
        BahanKode.Lit("Rumah_Kipas", warnaRumah, null, 0.45f),
    };
}
