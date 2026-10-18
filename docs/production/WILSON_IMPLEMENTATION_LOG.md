# Wilson 5 Bölüm — Uygulama Günlüğü

## FAZ 0 — Adli Envanter / Baseline
- Tarih / branch / HEAD: 2026-10-18 / `feature/wilson-missions-five-levels` / `c7639d6` (yedek etiketi `backup/pre-wilson-5level`)
- Amaç: Repo, dal, sahne, asset ve MCP durumunu belgelemek.
- Dokunulan dosyalar: `docs/production/PHASE_00_AUDIT.md`, bu dosya.
- Oluşturulan prefab / sahne / ScriptableObject: yok.
- Tamamlanan işler: 5 dal karşılaştırıldı; sahne/build listesi, Level1 kod ve asset envanteri çıkarıldı; teknik borç listelendi.
- Unity açıldı mı / versiyon: Evet, 6000.3.26f1, Level1 açık. MCP (`UnityMCP`) bağlı, `execute_code` / `read_console` / `refresh_unity` çalışıyor.
- Compile: PASS — `read_console` error=0, warning=0.
- EditMode / PlayMode testleri: NOT_RUN — projede test assembly'si yok.
- Manuel Play testi: NOT_RUN — `runInBackground=0`, Editor arka plandayken Play frame 2'de donuyor (gözlemlendi).
- Sahne/prefab missing reference: NOT_RUN.
- Görsel doğrulama: NOT_RUN.
- Git status / commit hash: bu fazın commit'i aşağıda.
- Açık risk ve çözümü: Sahiplik (Level2–4), Level5/build listesi, `runInBackground` ve test paketi kullanıcı/takım onayı bekliyor (bkz. PHASE_00_AUDIT §7).
- Kapı kararı: **PASS (denetim)** / ilerleyen fazlar için kısmi **BLOCKED**.
- Sonraki faz: FAZ 1 — engeller cevaplanınca.

## FAZ 1 — Mimari göç, branch ve kararlar
- Tarih / branch / HEAD: 2026-10-18 / `feature/wilson-missions-five-levels` / `e710c0a`
- Amaç: Sahiplik ve mimari kararlarını kayda geçirmek.
- Kullanıcı (Lead) kararları: Level2–5 sahneleri bu feature dalında Level1 şablonundan kurulacak (Kişi 2/3/4 dallarına dokunulmaz, birleştirme PR ile). `runInBackground` açıldı. Level5'in build listesine eklenmesi ve Test Framework kurulumu **onaylanmadı** → Level5 sahnesi oluşturulacak ama build listesine eklenmeyecek; testler MCP `execute_code` ile yapılacak.
- Mimari: `L1_ProjectGameController` beş sahnenin ortak oynanış motoru olarak kalır (her sahnede bölüm numarası farklı). Görev/veri/kayıt/puan yeni ortak katmanda: `Assets/Scripts/Missions` (`Jam.Missions`). Metinler `Assets/Data/{Levels,Missions,Wilson}` ScriptableObject'lerinde.
- Dokunulan dosyalar: `ProjectSettings/ProjectSettings.asset` (yalnız `runInBackground: 1`).
- Compile: PASS. Kapı kararı: PASS.

## FAZ 2 — Ortak Mission + Level çekirdeği
- Dokunulan dosyalar: `Assets/Scripts/Missions/{MissionTypes,TaskDefinition,WilsonDialogueSet,LevelDefinition,MissionManager,MissionScoring,LevelProgressSave}.cs`, `Assets/Editor/WilsonContentBuilder.cs`.
- Oluşturulan ScriptableObject: 5 `LevelDefinition` (`Assets/Data/Levels/L1_Deprem…L5_Yagmur`), 19 `TaskDefinition` (`Assets/Data/Missions/L1_T01…L5_T04`), 5 `WilsonDialogueSet` (`Assets/Data/Wilson/L1…L5_Dialogue`). Üretici menü: `Jam > Wilson > Bölüm verilerini oluştur`.
- Veri doğrulaması (MCP): 5 bölüm; süreler 90/100/120/110/140 – 12/12/30/25/25; kat 2/2/3/3/3; görev 4+4+4+3+4 = **19**; tekrar eden ID yok.
- Compile: PASS — Console error 0.
- Mantık testleri (MCP `execute_code`, EditMode eşdeğeri): **21/21 PASS** — bilinmeyen ID reddi, idempotent tamamlama (tek olay), hazırlık puanı 35 normalize, kritik görev kontrolü, koruma anahtarı, afette Missed, kilit sonrası tamamlanmama, reset, ön koşul, yıldız eşikleri (0/1/2@70/3@90+kritik), denge puanı, toplam clamp.
- PlayMode testi: NOT_RUN (henüz sahneye bağlanmadı).
- Kapı kararı: PASS. Sonraki faz: FAZ 3.
