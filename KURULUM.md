# BURAK Oyunu — Unity Kurulum Rehberi

Tüm C# kodu hazır (`Assets/Scripts`). Bu rehber, Unity editöründe **bir kez elle yapılacak** sahne kurulumunu anlatır. Süre: ~30-45 dk.

## 0. Projeyi Aç
1. Unity Hub → **Add** → `C:\Burak Oyun` klasörünü seç → Unity **6 LTS** ile aç.
2. İlk açılışta Unity paketleri (`Packages/manifest.json`) otomatik indirir ve kodu derler. Hata olmamalı.
3. **Edit → Project Settings → Player → Active Input Handling** = `Input System Package (New)`.
4. İlk TMP kullanımında çıkan pencerede **Import TMP Essentials**'a tıkla.

## 1. Asset'leri Oluştur (Project panelinde sağ tık → Create → BurakOyun)
- `Assets/Data/GameConfig` (**Game Config**) — varsayılan değerler hazır.
- `Assets/Data/BurakWord` (**Word Data**) — `word = BURAK`. Ses klipleri şimdilik boş kalabilir (oyun sessiz çalışır); sonra harf kayıtlarını `letters` dizisine bağla (B, U, R, A, K) + `wordAudio`.

## 2. Sahne Kurulumu (`Assets/Scenes/Game.unity` olarak kaydet)
### Zemin
- 3 adet **Cube**: scale `(10, 0.5, 30)`, pozisyonlar Z = `0, 30, 60`, Y = `-0.25`. Hepsini boş bir `Track` objesi altına koy.
- `Track`'e **TrackRecycler** ekle: `snake` = Snake, `tiles` = 3 küp, `tileLength = 30`.
- Küplere sevimli pastel yeşil material ver.

### Yılan
- **Capsule** oluştur, adı `Snake`, pozisyon `(0, 0.5, 0)`, rotation X=90 (yatay dursun).
- Tag'ini **Player** yap. **Rigidbody** ekle: `Is Kinematic = ✓`, `Use Gravity = ✗`. Collider'ı kalsın.
- Script ekle: **SnakeController** (config = GameConfig) + **LaneInput** (otomatik gelir).
- Sevimli gözler: iki küçük Sphere'i kapsülün önüne child yap. (Model sonra güzelleşir.)

### Kamera
- Main Camera'ya **CameraFollow** ekle: `target = Snake`. Offset varsayılan iyi.

### Harf Prefab'ı
1. Boş GameObject `Letter` → **Box Collider** ekle: `Is Trigger = ✓`, size `(1.5, 2, 1)`.
2. Child olarak **3D Object → Text - TextMeshPro** ekle: font size ~12, kalın, parlak renk (turuncu), ortalanmış. Karaktere bakacak şekilde rotation Y=180.
3. `Letter` köküne **LetterCollectible** ekle, `label` alanına TMP text'i sürükle.
4. `Assets/Prefabs/Letter.prefab` olarak kaydet, sahnedekini sil.

### Manager'lar
Boş GameObject `Managers` oluştur, şu scriptleri ekle ve alanları bağla:
- **WordManager**: `wordData = BurakWord`
- **LetterSpawner**: config, wordManager, snake, `letterPrefab = Letter.prefab`
- **RewardManager**: wordManager, snake; `confetti` ve `collectSparkle` için iki **Particle System** oluştur (aşağıda), sürükle.
- **AudioManager**: wordManager + 3 **AudioSource** (sfx, voice, music — `Play On Awake = ✗`). SFX klipleri sonra.
- **GameManager**: snake, spawner, wordManager, rewardManager, ui (aşağıdaki UIManager).

### Partiküller (çocuk dostu, yumuşak)
- `Confetti`: Particle System — Shape: Cone, Start Color: rastgele pastel (Random Between Two Colors), Burst 100, gravity 0.5, `Play On Awake = ✗`, Looping = ✗.
- `CollectSparkle`: küçük sarı yıldız patlaması, Burst 20, `Play On Awake = ✗`.

### UI (Canvas)
Canvas (Scale With Screen Size, 1920x1080) altında:
- `ProgressText` (TMP): üst-orta, font ≥ 90, kalın. — "B U R A K"
- `StarsText` (TMP): sağ-üst, font ≥ 72. — "★ 0"
- `FeedbackText` (TMP): ekran ortası üstü, font ≥ 100.
- `StartPanel`: yarı saydam panel + dev **OYNA** butonu (≥ 300px, yuvarlak köşeli). Butonun OnClick → `Managers.GameManager.StartGame`.
- `CompletePanel`: "HARİKA BURAK! 🎉" yazısı + dev **TEKRAR OYNA** butonu → `GameManager.Replay`. Başlangıçta **kapalı** (inactive).
- Canvas'a **UIManager** ekle, tüm alanları bağla (wordManager, rewardManager, text'ler, paneller).

## 3. Test
- **Play** bas → OYNA → ok tuşları/A-D ile şerit değiştir, doğru harfleri sırayla topla.
- **Window → General → Test Runner → EditMode → Run All** → 6 test yeşil olmalı (WordProgressTests).

## 4. Sesler (5. gün)
Telefonla kaydet veya ücretsiz kaynak kullan: `Bee!`, `U!`, `Re!`, `A!`, `Ke!` (veya harf adları), `BURAK!`, yumuşak "ding", komik "boing", alkış. `.wav` olarak `Assets/Audio`'ya at, WordData ve AudioManager'a bağla.

## Güvenlik Hatırlatması
- Unity **Services** panelinden hiçbir servisi (Analytics, Ads, Cloud) açma.
- Android build alırken Player Settings → internet erişimi `Auto` yerine kullanılmadığından manifest'te `INTERNET` izni oluşmamalı; build sonrası APK manifest'ini kontrol et.

## Ayar İpuçları (ev testi sonrası — kod değişmez, GameConfig'ten)
- Çok hızlıysa: `forwardSpeed` 4 → 3.
- Harfler çok seyrek/sıksa: `spawnInterval` 9 → 7 veya 11.
- Çok zorsa: `decoyCount` 2 → 1.
