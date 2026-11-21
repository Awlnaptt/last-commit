// Kontrak interaksi E (GDD slide 16, 19).
//
// UNTUK JALUR A (Interactor): raycast dari kamera, lalu
//   var target = hit.collider.GetComponentInParent<IInteraktif>();
//   if (target != null && target.BisaDipakai) { HUDUI.SetPetunjuk(target.Petunjuk); if (E ditekan) target.Interaksi(); }
// MesinKopi milik Jalur A, boleh ikut memakai interface ini.
public interface IInteraktif
{
    string Petunjuk { get; }
    bool BisaDipakai { get; }
    void Interaksi();
}
