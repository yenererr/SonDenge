# Son Denge — Menü Yeniden Tasarımı Uygulama Günlüğü

Şartname: `docs/design/SON_DENGE_MENU_CLAUDE_CODE_URETIM_PROMPTU.md`
Referans görseller (yalnız tasarım panosu, runtime'a girmez): `docs/design/menu_references/01..07*.png`
Ekran görüntüleri: `docs/production/screenshots/menu/`

---

### FAZ 0 — Repo denetimi
- Tarih: 18.10.2026 · branch: `feature/menu-redesign` (kaynak: `feature/wilson-missions-five-levels` @ `73b5c7a`)
- Unity: 6000.3.26f1, URP 3D, yeni Input System. Unity MCP: `mcp-for-unity-server 3.4.9` @ `http://localhost:8080/mcp`
  (yerel `.mcp.json` girişi kaldırılmış olduğundan bu oturumda HTTP JSON-RPC ile doğrudan çağrıldı; `.mcp.json`'a dokunulmadı).
- Uzak dallar: `main`, `kisi1-level1`, `kisi2-level2`, `kisi3-level3`, `kisi4-level4`, `feature/wilson-missions-five-levels`. Hiçbiri merge edilmedi.
- Kirli çalışma ağacı (başkasına/öncesine ait): `.mcp.json`, CloudSea/L1 materyalleri, `Assets/Settings/*`, `ProjectSettings.asset`,
  Yughues `.meta` silinmeleri, Medieval Room Kit yeni dosyaları. **Bunlar bu çalışmada stage edilmez/commit edilmez.**
- Sahiplik: Kullanıcı Lead (Kişi 1). `Menu.unity`, `Scripts/UI/`, `Scripts/Core/` (Contracts.cs hariç), `Assets/Prefabs/`, `Levels/Level1/` Lead'e ait → yetkili alan.

**Mevcut sistem haritası**
| Alan | Dosya | Not |
|---|---|---|
| Eski menü | `Scripts/UI/MainMenuUI.cs` (kodla kurulan legacy `Text` UI) | Yerine yeni `MenuFlowController` sistemi gelecek, eski sınıf sahneden kaldırılacak (dosya korunur) |
| UI yardımcı | `Scripts/UI/UiKit.cs`, `UiHoverScale` | Level HUD'ları kullanıyor → değiştirilmez |
| Ayarlar | `Scripts/UI/GameSettings.cs` (PlayerPrefs, ses + ReduceShake) | Genişletilecek (anahtar adları korunur) |
| İlerleme | `Scripts/Missions/LevelProgressSave.cs` (`sondenge.v1.*`, yıldız/kilit/brifing) | Tek ilerleme kaynağı olarak yeniden kullanılacak |
| Bölüm verisi | `Data/Levels/L1..L5*.asset` (`LevelDefinition`: sahne, afet süresi, görevler) + `Data/Missions/*` | Kart süresi ve görev listesi buradan okunur |
| Sahne geçişi | `Scripts/Core/SceneLoader.cs` (fade + LoadSceneAsync) | Gerçek ilerleme API'si eklenecek |
| Wilson | `Levels/Level1/Art/Characters/Wilson/low_poly_chicken.obj/.png` (Pjager, CC BY 4.0), `L1_WilsonChicken.BuildPortraitModel`, `Scripts/UI/WilsonPortrait.cs` (RenderTexture, MainCamera/AudioListener yok) | Menüde tek portre kamerası, tüm sayfalar aynı RT'yi paylaşır |
| Oyun içi duraklatma | `L1_ProjectGameController` ESC → `ToggleMenu`, UI: `L1_Hud.Panels.BuildPause` | FAZ 9'da aynı tasarım diliyle yenilenecek, tek ESC sahibi korunacak |
| Görseller | `Levels/Level1/Art/L1_MenuBackdrop.png` (oyunun ev+gün batımı render'ı, yazısız), `L1_MistyMountainValley.png`, `L1_DuskSky.png`, `L1_CozyPreview.png` (evin iç kesiti) | Yazısız "background-only" katmanlar olarak kullanılacak |
| Font | TMP `LiberationSans SDF` (Türkçe glif destekli) | Lisanslı ek font yok; yeni font indirilmedi |
| UI ses | Yok | Kısa ahşap tık sesleri editörde sentezlenecek (3. parti indirme yok) |

**Riskler / kararlar**
- Level2-5 sahneleri aynı `L1_ProjectGameController`'ı kullanır; menü ve duraklatma değişiklikleri tek koddan beş bölüme yansır.
- Referans PNG'ler arayüzü baked içerir → runtime'da kullanılmaz; tüm metin/buton gerçek TMP/Button.
- Parlaklık ayarı için projede gerçek bir uygulama yok → sahte slider eklenmeyecek.
- Durum: **PASS** (denetim) · Sonraki: FAZ 1
