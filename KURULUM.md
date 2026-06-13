# BURAK Yılan Oyunu — Unity Kurulum Rehberi

Tüm C# kodu hazır (`Assets/Scripts`). Klasik yılan oyunu: WASD/ok tuşlarıyla 4 yönde hareket, yem ye → büyü, duvara/kendine çarpınca skor ekranı + TEKRAR OYNA. Sahne tamamen menüden otomatik kurulur — elle obje dizmek gerekmez.

---

## İlk Kurulum / Smoke Test

### Neden `ProjectSettings/`, `Library/` ve `.meta` dosyaları yok?

Bu dosyalar Unity'nin projeyi **ilk kez açtığında otomatik oluşturduğu** çıktı dosyalarıdır.
Git deposunda kasıtlı olarak bulunmazlar — `.gitignore` ile dışlanmışlardır.
Projeyi klonladıktan sonra Unity Hub'dan açana kadar bu klasörler oluşmaz; bu bir hata değil, beklenen durumdur.

### Kurulum Sırası (Bu Sırada Yapılmalı)

```
1. Unity Hub → Add → repo klasörünü seç → Unity 6 LTS ile aç
   (İlk açılışta Packages/manifest.json'daki paketler indirilir ve
    tüm kod derlenir — 2-5 dk sürebilir, konsolda hata olmamalı.)

2. Window → TextMeshPro → Import TMP Essential Resources
   (UI yazıları için zorunlu. Bir kez yapılır.)

3. Menü çubuğunda:  BurakOyun → 3 — Sahneyi Kur
   (URP pipeline + GameConfig + prefab'lar + tüm sahne nesneleri
    otomatik kurulur, sahne Assets/Scenes/Game.unity olarak kaydedilir
    ve Build Settings'e eklenir.)

4. Editor araç çubuğunda ▶ Play → "OYNA" → WASD veya ok tuşları ile oyna.
```

> Daha önce harf-toplama sürümünü kurduysanız: "3 — Sahneyi Kur" yeni boş bir
> sahne açıp yılan oyununu kurar ve `Game.unity` üzerine kaydeder. Eski sahneden
> hiçbir obje kalmaz.

### Kurulum Sağlaması — Otomatik Kontrol

```
BurakOyun → Smoke Test — Editor Check
```

| Kontrol | Beklenen |
|---------|---------|
| URP Pipeline asset | ✅ `Assets/Settings/URP-Pipeline.asset` var |
| Sahne dosyası | ✅ `Assets/Scenes/Game.unity` var |
| TMP Essential Resources | ✅ Font asset'leri import edilmiş |
| Runtime asmdef referansları | ✅ `Unity.InputSystem` + `Unity.TextMeshPro` |
| Sahne bileşenleri | ✅ GameManager, SnakeController, FoodSpawner, UIManager mevcut |

Tüm kontroller yeşilse **▶ Play basabilirsiniz.**

---

## Oynanış

- **Hareket:** WASD veya ok tuşları (4 yön). Ters yöne basmak güvenle yutulur — yılan asla anında kendine dönmez.
- **Amaç:** Kırmızı elmaları ye → yılan büyür, skor artar.
- **Bitiş:** Duvara veya kendine çarpınca skor ekranı gelir ("KAYBETTİN" yazısı yoktur — skor + TEKRAR OYNA).
- **Rekor:** En iyi skor cihazda lokal saklanır (PlayerPrefs); yeni rekorda konfeti + alkış.

## Mimari (kısa)

| Katman | Dosya | Görev |
|--------|-------|-------|
| Saf C# çekirdek (test edilebilir) | `GridMovement`, `SnakeBody`, `CollisionManager` | Izgara, gövde, çarpışma — Unity sahnesiz |
| Gameplay | `SnakeController`, `FoodSpawner`, `DirectionInput` | Tick döngüsü, görseller (havuzlu), yem, input kuyruğu |
| Core | `GameManager`, `GameStateManager`, `SaveManager`, `RewardManager` | Durum makinesi, skor, kayıt, efektler |
| UI / Audio | `UIManager`, `AudioManager`, `SfxGenerator` | Skor/paneller, programatik yumuşak SFX |

## Test

- **Window → General → Test Runner → EditMode → Run All** → ızgara/gövde/çarpışma/input testleri yeşil olmalı.
- **PlayMode → Run All** → çekirdek döngü smoke testleri (ye-büyü-skor, duvar, kendine çarpma, tekrar oyna).

## Güvenlik Hatırlatması

- Unity **Services** panelinden hiçbir servisi (Analytics, Ads, Cloud) açma.
- Hiçbir veri cihaz dışına çıkmaz; kayıt yalnızca lokal PlayerPrefs.

## Ayar İpuçları (ev testi sonrası — kod değişmez, `Assets/Data/GameConfig`'ten)

- Çok hızlıysa: `tickRate` 0.3 → 0.35 (hücre başına saniye; büyük = yavaş).
- Çok kolaysa: `tickRate` 0.3 → 0.25 veya `speedUpPerFood` artır.
- Hızlanma hiç olmasın: `speedUpPerFood` = 0.
- Tahta küçük/büyük gelirse: `gridWidth` / `gridHeight` (sonra **BurakOyun → 3 — Sahneyi Kur** ile sahneyi yeniden kur — zemin ve kamera ızgara boyutuna göre kurulur).
- Başlangıç uzunluğu: `initialLength` (varsayılan 3).
