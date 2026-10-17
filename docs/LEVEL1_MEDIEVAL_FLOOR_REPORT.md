# Son Denge — Medieval Room Kit Entegrasyon Raporu

- **Branch:** `kisi1-level1`
- **Asset import durumu:** `Medieval Room Kit.unitypackage` (1Luka2, ~254 MB) MCP ile `AssetDatabase.ImportPackage` kullanılarak import edildi.
  Git'e **yalnız prefab'ın kullandığı** dosyalar (+ `.meta`) eklendi; demo sahnesi, mobilya ve dekor dosyaları yerel kopyada duruyor, commit edilmedi.
- **Paket kök dizini:** `Assets/Medieval Room Kit/` (Materials, Mesh, Prefab, Scenes, Textures)

## Seçilen gerçek modeller

| Rol | AssetDatabase yolu | Materyal |
|---|---|---|
| Floor | `Assets/Medieval Room Kit/Prefab/Floor/Floor_4x4.prefab` (×3) | `L1_Medieval_Floor` |
| BackWall | `Prefab/Wall/Wall_4m.prefab` (×2), `Prefab/Wall/Wall_2m_Window.prefab` | `L1_Medieval_Wall` |
| Left/RightFrame | `Prefab/Column/Column_1.prefab` (×4, köşelerde) | `L1_Medieval_Column` |
| TopBeam | `Prefab/CeilingMoldings/CeilingMoldings_4m.prefab` (×2), `_2m` | `L1_Medieval_CeilingMoldings` |
| Süpürgelik + ön ince profil | `Prefab/SkirtingBoard/SkirtingBoard_4m.prefab`, `_2m` | `L1_Medieval_SkirtingBoard` |
| Pencere | `Wall_2m_Window` duvardaki açıklık (ayrı pencere modeli kullanılmadı) | — |

Kullanılmayanlar: Ceiling (taş tavan; Kat 2 döşemesi tavan olarak kalıyor), Door, Window modelleri, tüm mobilya/dekor.
Materyaller `Assets/Levels/Level1/Materials/L1_Medieval_*.mat` yerel kopyalar (URP/Lit); kit orijinalleri değiştirilmedi.
Kullanılan 4096 px dokuların import `Max Size` değeri 2048'e çekildi (görsel kayıp gözlenmedi).

## Prefab ve yerleşim

- `Assets/Levels/Level1/Prefabs/L1_EmptyFloor_Medieval.prefab` — 19 renderer, **0 collider, 0 Rigidbody, mobilya yok**.
  `Geometry` (ölçek 0,66: 10 m kit = 6,6 m oyun odası) + `Markers` (FloorVisualTop, RoomLeft/RightLimit, UpperFloorAttach = +2,25 m).
- Level1'de `houseRoot/Kat 1` altına, yerel y = −1,15 (zemin üstü, mevcut `Yürünebilir zemin` collider'ı ile aynı: dünya y −1,60).
- Eski Kat 1 görselleri **silinmedi**: `Kat 1/LegacyFloorVisuals` altına taşındı ve pasif. Collider'lar (`Yürünebilir zemin`,
  `Yan duvar çarpışması`) ve sarkan lambalar yerinde.
- **Eski görünüme dönüş:** `LegacyFloorVisuals`'ı aktif yap, `L1_EmptyFloor_Medieval`'ı pasif yap (veya bu commit'i geri al).
- `L1_CozyPlayableBuilder` çalıştırılmadı; çalıştırılırsa `[L1] World Visuals`'ı yeniden kurar ve bu değişikliği siler.

## Test sonuçları (Editor, Play modu, MCP)

| Kontrol | Sonuç |
|---|---|
| Paket import, gerçek yollar | PASS |
| Prefab eşyasız, collider/Rigidbody yok | PASS |
| Oda `houseRoot` ile dönüyor (ev 353,54° = oda 353,54°) | PASS |
| Denge değişmedi (kitaplık 2,7 m: 6,46°, önceki ile aynı) | PASS |
| Kameradan 4 mobilyaya raycast engellenmiyor | PASS |
| Karakter yüksekliği sabit (yerel y −0,730) | PASS |
| Sabitleme + deprem + rapor (Kitaplık sabit, 5/5 korundu, 78/100) | PASS |
| Yeniden başlat: Preparation, 750 altın, oda mevcut | PASS |
| Mor/pembe materyal | PASS (yok) |
| Console kırmızı hata | 0 önce / 0 sonra |
| Elle fare sürükleme, zoom uçlarında kadraj | NOT TESTED (elle denenmeli) |
| Menu/Level2–4 sahneleri | Değiştirilmedi |
