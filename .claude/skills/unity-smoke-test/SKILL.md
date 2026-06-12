---
name: unity-smoke-test
description: BURAK Unity oyunu için iki katmanlı smoke test çalıştırır — önce Unity gerektirmeyen statik yapı/derleme-riski kontrolleri, sonra (Unity kuruluysa) batchmode EditMode + PlayMode test koşumu. Oyunu çalışmaktan alıkoyan eksikleri önem sırasına göre raporlar. "smoke test", "oyunu test et", "eksikleri kontrol et", "duman testi" denince kullan.
---

# Unity Smoke Test — BURAK Oyunu

Amaç: `C:\Burak Oyun` projesinin **gerçekten çalışabilir** olduğunu hızlıca doğrulamak.
İki katman var; Katman 1 her zaman çalışır, Katman 2 Unity kuruluysa çalışır.

## Katman 1 — Statik Kontroller (Unity gerekmez)

Sırayla kontrol et, her madde için ✅/❌ ve kısa not üret:

1. **Proje import edilmiş mi?**
   - `ProjectSettings/` klasörü var mı? `Library/` var mı? `Assets/` altında en az bir `.meta` var mı?
   - Hiçbiri yoksa: **KRİTİK** — Unity bu klasörü henüz import etmemiş. Kullanıcıdan Unity Hub'da `C:\Burak Oyun`'u Add/Open ile açmasını iste. Diğer kontrollerin çoğu bundan sonra anlamlı.
2. **Render pipeline (URP) kurulu mu?**
   - `Assets/Settings/URP-Pipeline.asset` var mı?
   - `ProjectSettings/GraphicsSettings.asset` içinde bir `m_CustomRenderPipeline` / SRP referansı var mı (boş değil mi)?
   - Yoksa: materyaller magenta render olur → `BurakOyun/0 — URP Pipeline Kur` menüsünü çalıştır.
3. **Sahne kaydedilmiş mi?**
   - `Assets/Scenes/Game.unity` var mı? `EditorBuildSettings`'te (`ProjectSettings/EditorBuildSettings.asset`) listede mi?
   - Yoksa: `BurakOyun/3 — Sahneyi Kur` menüsü sahneyi kaydeder.
4. **TMP font asset hazır mı?**
   - `Assets/TextMesh Pro/Resources/Fonts & Materials/` altında `.asset` font var mı?
   - Yoksa: Window → TextMeshPro → Import TMP Essential Resources.
5. **asmdef tutarlılığı**
   - `Assets/Scripts/BurakOyun.Runtime.asmdef` → `Unity.InputSystem`, `Unity.TextMeshPro` referansları duruyor mu?
   - Test asmdef'leri (`EditMode`, `PlayMode`) `BurakOyun.Runtime`'a referans veriyor mu?
6. **C# derleme-riski taraması** (Grep ile):
   - Her `Scripts/**/*.cs` dosyasındaki `namespace BurakOyun.X` ↔ `using BurakOyun.X` çağrıları eşleşiyor mu?
   - `SceneSetup.cs` içindeki `SetField(obj, "alanAdı", ...)` çağrılarındaki her "alanAdı", ilgili script'te `[SerializeField] ... alanAdı;` olarak var mı? (En sık kırılma noktası.)
   - `Shader.Find("Universal Render Pipeline/...")` çağrıları → URP paketi `Packages/manifest.json`'da mı?

Çıktı: numaralı ✅/❌ tablo + "Önce şunu yap" tek satırlık aksiyon listesi.

## Katman 2 — Unity Batchmode (Unity kuruluysa)

1. **Unity.exe bul:**
   - `C:\Program Files\Unity\Hub\Editor\<sürüm>\Editor\Unity.exe` (Glob ile en yeni 6000.x sürümü).
   - Bulunamazsa Katman 2'yi atla, "Unity kurulu değil/bulunamadı" de.
2. **EditMode testleri:**
   ```
   & "<Unity.exe>" -batchmode -projectPath "C:\Burak Oyun" `
     -runTests -testPlatform EditMode `
     -testResults "C:\Burak Oyun\edit-results.xml" `
     -logFile "C:\Burak Oyun\edit.log" -quit
   ```
3. **PlayMode testleri:** aynı komut, `-testPlatform PlayMode`, ayrı sonuç/log dosyası.
4. **Sonuç ayrıştır:**
   - `edit.log` / `play.log` içinde `error CS`, `Compilation failed`, `Exception` ara → derleme/runtime hatası.
   - `*-results.xml` içinde `<test-run ... result="..." total= passed= failed=>` özetini çıkar; başarısız testlerin adını ve mesajını raporla.
5. Batchmode aynı anda iki kez çalıştırılamaz (proje kilidi) — komutları sıralı çalıştır.

## Rapor Formatı

```
## BURAK Smoke Test Raporu
### Katman 1 — Statik
✅/❌ madde madde...
### Katman 2 — Batchmode (çalıştıysa)
EditMode: X passed / Y failed   PlayMode: ...
Derleme: temiz / N hata
### Öncelikli Eksikler
1. [KRİTİK] ...
2. [MANTIK] ...
3. [POLISH] ...
### Sıradaki Tek Aksiyon
→ ...
```

Kurallar: sadece salt-okunur + test koşumu yap; kaynak dosyaları DEĞİŞTİRME (düzeltmeyi kullanıcı onayıyla ayrı adımda yap). Bulunamayan Unity'yi hata sayma, sadece not düş.
