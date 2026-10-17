# Level 1 + Menü — Varlık Atıfları

Bu dosya Level 1 ve Menü sahnesinde kullanılan dış kaynaklı varlıkları listeler (ZORUNLU_KURALLAR §32).
Yeni indirilen paket **yok**; yalnız repoda zaten bulunan paketler kullanıldı.

| Paket | Yol | Level1/Menü'de kullanımı | Lisans |
|---|---|---|---|
| KayKit Adventurers 2.0 (Kay Lousberg) | `Assets/Levels/Level1/Art/Characters/KayKit_Barbarian/` | Sakin karakteri: Barbarian.fbx, barbarian_texture.png, Rig_Medium_General/MovementBasic animasyonları | CC0 (lisans dosyası klasörde) |
| Medieval Room Kit (1Luka2) — [Asset Store](https://assetstore.unity.com/packages/3d/environments/medieval-room-kit-399232) | `Assets/Medieval Room Kit/` (yalnız kullanılan parçalar git'te) | Boş kat prefab'ı `L1_EmptyFloor_Medieval`: Floor_4x4, Wall_4m, Wall_2m_Window, Column_1, SkirtingBoard_2m/4m, CeilingMoldings_2m/4m | Unity Asset Store Standart EULA |
| MIX Furniture Pack (ZNS3D) | `Assets/ZNS3D/` | Kanepe (`MIX — yıpranmış deri kanepe`), kitaplık, sehpa | Unity Asset Store Standart EULA |
| StylArts | `Assets/StylArts/` | Tekli koltuk; (gizli) küvet, lavabo, ocak, buzdolabı, TV | Unity Asset Store Standart EULA |
| FarmHouse | `Assets/FarmHouse/` | Döşeme, taşıyıcı ve payanda parçaları | Unity Asset Store Standart EULA |
| Modular Sewer Props (Ata Khani) | `Assets/Ata Khani/Modular Sewer Props/` | Sarkan lambalar | Unity Asset Store Standart EULA |
| Low Poly Tavern Pack | `Assets/Low Poly Tarvern Pack/` | Raf üstü seramikler (mesh; materyal `L1_3D_Clay` ile değiştirildi) | Unity Asset Store Standart EULA |
| 3Digitalis Low Poly Interior Props | `Assets/3Digitalis/` | Raf, saksı, dekor | Unity Asset Store Standart EULA |
| LowPolyInteriorsFree | `Assets/LowPolyInteriorsFree/` | Pencere camı vb. | Unity Asset Store Standart EULA |
| Yughues Free Wooden Floor Materials | `Assets/YughuesFreeFlooringMaterials/` | Zemin dokusu kaynakları | Unity Asset Store Standart EULA |

## Projede üretilenler (dış kaynak değil)

- `Assets/Levels/Level1/Meshes/L1_RockSpireMesh.asset` — sivri kaya gövdesi, MCP ile kodla üretilmiş low-poly mesh.
- `Assets/Levels/Level1/Art/L1_MenuBackdrop.png` — Level1 sahnesinin Unity içinde alınmış render'ı (menü arka planı).
- `Assets/Levels/Level1/Art/L1_MistyMountainValley.png`, `L1_DuskSky.png`, `L1_RockSpire.png` — ekibin önceki üretimleri;
  kaynak notu `Assets/Levels/Level1/Art/L1_ReferenceHouse_SOURCE.md`.
- Müzikler `Assets/Levels/Level1/Audio/bgm_*.mp3`: `PROJECT_FILES_INTEGRATION.md`'deki proje dosyalarından alındı.
  **Açık konu:** ses dosyalarının asıl kaynağı/lisansı belgelenmeli; belgelenemezse finalde değiştirilmeli.

## Asset Store EULA notu

Asset Store paketleri oyun içine gömülü olarak kullanılabilir; ham `.fbx`/`.png` dosyaları ayrı ürün gibi dağıtılamaz.
