# Kontrak Jalur A ↔ Jalur B — Last Commit

Semua script Jalur B ada di `Assets/JalurB/`. `Assets/Scripts/` tetap wilayah Jalur A.

## Rhythm (RhythmConductor / NoteSpawner)
| Kapan | Panggil |
|---|---|
| E di MesinKopi | `Pesanan p = OrderManager.Instance.MulaiRacik();` (null = antrean kosong) |
| Pola yang dimainkan | `p.menu.polaRitme` (isi aset pola milikmu di aset Menu) |
| Pola selesai | `OrderManager.Instance.SelesaiRacik(HasilRacik.Perfect / Good / Miss);` |
| Racik dibatalkan | `OrderManager.Instance.BatalRacik();` |

`HasilRacik` didefinisikan di `OrderManager.cs`. Jangan bikin enum dengan nama sama.
Ambang "Miss banyak" diputuskan Jalur A, Jalur B hanya menerima hasil akhirnya.

## Efek ke rhythm
- `KetenanganTracker.Instance.PengaliJendelaPerfect` → 1 normal, minimal 0,6 saat gelisah
- `KetenanganTracker.Instance.PengaliJendelaGood` → 1,2 saat tenang
- `KetenanganTracker.Instance.Tingkat` → Gelisah / Normal / Tenang (untuk vignette, getar, musik teredam)
- `UpgradeManager.Instance.PilihanCabang("Mesin")` → "A" / "B" / null
- `UpgradeManager.Instance.PilihanCabang("Musik")` → "A" / "B" / null

## Meter Ketenangan di HUD
- Hilang otomatis selama `OrderManager.Instance.SedangMeracik` (antara `MulaiRacik()` dan `SelesaiRacik()`), tidak perlu kode tambahan

## DisturbanceManager
- `DayManager.Instance.HariIni`, `.Fase`, `.ProgresShift` (0..1)
- Event `DayManager.Instance.OnShiftMulai` / `OnShiftSelesai` (int hari)

## PlayerController dan Interactor
- Berhenti bergerak (dan kamera berhenti menoleh) saat `UIStatus.MenghalangiPemain` bernilai true
  (percakapan, menu upgrade, papan harga, menu jeda)
- Interactor: `hit.collider.GetComponentInParent<IInteraktif>()`, cek `BisaDipakai`,
  tampilkan `Petunjuk` lewat `HUDUI.SetPetunjuk(...)`, panggil `Interaksi()` saat E
- Kalau Interactor-mu sudah jadi, hapus `UjiJalurB` dari kamera dan folder `Assets/JalurB/_Sementara`
