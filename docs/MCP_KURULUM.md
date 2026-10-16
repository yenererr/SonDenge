# Unity MCP Kurulumu (MCP for Unity — CoplayDev)

Claude Code'un Unity Editor'ü doğrudan kontrol etmesi (nesne oluşturma, bileşen ekleme, sahne kaydetme, Console okuma) için.
Kaynak: https://github.com/CoplayDev/unity-mcp

**Repoda hazır olanlar** (sen bir şey eklemezsin):
- Unity paketi: `Packages/manifest.json` → `com.coplaydev.unity-mcp` **v10.0.0** (herkeste aynı sürüm)
- Claude Code bağlantısı: repo kökündeki `.mcp.json` → `unityMCP` = `http://localhost:8080/mcp`

## Bir kerelik hazırlık (her bilgisayarda)

1. **Python 3.10+** kurulu olsun: PowerShell'de `python --version`
2. **uv** kurulu olsun: `uv --version`. Yoksa:
   ```powershell
   powershell -ExecutionPolicy ByPass -c "irm https://astral.sh/uv/install.ps1 | iex"
   ```
   Kurduktan sonra terminali kapatıp aç.
3. **Claude Code** kurulu olsun: `claude --version`

## Her çalışma oturumunda

1. Projeyi Unity'de aç (ilk açılışta paket kendiliğinden iner).
2. Unity menüsü: **Window → MCP for Unity**. Pencerede sunucunun çalıştığını (yeşil / *Running*) gör; çalışmıyorsa **Start Server**.
3. İlk seferde aynı pencerede **Configure All Detected Clients** (veya Claude Code satırında *Configure*) butonuna bas.
4. Claude Code'u **proje klasöründe** başlat. İlk açılışta `.mcp.json` için "bu MCP sunucusunu onaylıyor musun" sorusuna **evet** de.
5. Test: Claude Code'a yaz → `Unity MCP bağlı mı? Aktif sahnedeki nesneleri listele.`

## Kurallar

- **Bir Unity'ye tek AI.** Aynı Unity Editor'e iki Claude Code oturumu bağlanmaz.
- Aynı bilgisayarda iki proje açıksa MCP'yi yalnızca çalıştığın projede başlat (port 8080 tek).
- MCP ile sahnede değişiklik yaptıktan sonra **sahneyi kaydet** ve commit et (ZORUNLU_KURALLAR §7).
- MCP sadece kendi sahnende ve kendi klasörlerinde iş yapsın; başkasının sahnesini açtırma.
- Paket sürümünü (`Packages/manifest.json`) kendi başına güncelleme — takım onayı gerekir.

## Sorun çıkarsa

| Belirti | Çözüm |
|---|---|
| Claude Code'da `unityMCP` bağlı değil | Unity açık mı? *Window → MCP for Unity*'de sunucu çalışıyor mu? Claude Code'u yeniden başlat. |
| Port 8080 kullanımda | Başka bir Unity projesi MCP sunucusu açık olabilir; onu kapat. |
| `uv` bulunamadı | uv'yi kur, terminali ve Unity Hub'ı kapatıp yeniden aç. |
| Transport değiştirdim, çalışmıyor | HTTP ↔ stdio değişikliği sonrası Claude Code'u yeniden başlat. |
| Hâlâ olmuyor | `claude mcp list` çıktısını ve Unity Console'daki hatayı Lead'e gönder. |
