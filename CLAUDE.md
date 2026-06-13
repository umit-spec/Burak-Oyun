# BURAK Oyunu — Çalışma Talimatları

Bu dosya her oturumda otomatik yüklenir. Amacı: bu repodaki analiz/geliştirme işini **token-verimli** yapmak ve işi **doğru model katmanına otomatik delege** etmek. Aşağıdaki kurallar varsayılan davranışı belirler.

> Proje işleyişi (commit kapsamı, gh yokluğu, Unity batchmode tuzakları) ayrıca hafızadaki `burak-oyun-workflow` notunda tutulur — onunla çelişme.

## 1. Model & Token Yönlendirme Katmanları

Kural: **işi her zaman onu yapabilecek en ucuz katmana ver.** Yukarı çıkmadan önce alttaki katmanın yetip yetmediğini sor.

- **Tier 0 — Deterministik script (0 model token).** Mekanik tarama modelden tamamen çıkar. `.claude/skills/unity-eksik-analiz/scan.ps1` envanter, SerializeField çapraz-kontrolü, event kablolaması, GameConfig öksüz alanları ve test kapsamı olgu sayfasını üretir. Kod-yapısı sorularında **önce bu script**; asla "her ihtimale karşı" toptan dosya okuma.
- **Tier 1 — Haiku (ucuz triyaj/teyit).** Olgu sayfasının basit özeti, tek-dosya/tek-satır bayrak teyidi, dispatch. Ana oturumdan `Agent(model: haiku)` ile çağrılır; ajan dosyaları değişmez.
- **Tier 2 — Sonnet (yargı/analiz).** `unity-eksik-analizci` ve `unity-smoke-tester` ajanları — rubriğe göre yargı, gruplu/öncelikli yol haritası. Frontmatter'ları zaten Sonnet; korunur.
- **Tier 3 — Opus (ana oturum / sen).** Yalnızca sentez, mimari & çapraz-kesen kararlar, çelişki çözümü, kullanıcı diyaloğu, plan yazımı. **Mekanik okuma/yorumu Opus'ta yapma** — delege et.

## 2. Yönlendirme Kuralı (niyet → katman)

| Kullanıcı niyeti | Rota |
|---|---|
| "ne eksik", "oyunu analiz et", "ne ekleyelim", "iyileştirme planı", "oyunu geliştir" | **`unity-eksik-analizci`** (Tier 2). Önce Tier 0 script. |
| "çalışıyor mu", "test et", "smoke test", "derleniyor mu", "eksikleri kontrol et" | **`unity-smoke-tester`** (Tier 2). |
| Saf mekanik tarama / "şu alan var mı", "şu event bağlı mı" | **Tier 0 script** ya da **Tier 1 Haiku** nokta teyidi. |
| Mimari karar, çelişki çözümü, planlama, kullanıcıya özet | **Tier 3 Opus** (ana oturum) sentezi. |

**Eskalasyon kuralı:** Opus'a yalnızca yargı belirsizse, bulgular çelişiyorsa veya çapraz-kesen mimari karar gerekiyorsa çık. Aksi halde alt katmanda kal.

## 3. Token Disiplini

- Önce script, sonra düşün; **toptan okuma yok** — olgu sayfasından çalış.
- Hedefli okuma en fazla 1-2 dosya, yalnızca bir bayrağı doğrulamak için.
- Bulguları üst-sınırla (~12), terse tek-satır + `dosya:satır`.
- Ana iş parçacığına **tek derli toplu rapor** dön; ham script çıktısı ya da dosya içeriği yapıştırma.
- Yol haritasını **grupla** (aynı dosyalara dokunan bulgular tek grup) → gelecekteki düzeltme oturumu bir grubu tek bağlam yüklemesiyle bitirir.

## 4. Gelecek İş (henüz yapılmadı)

- `unity-smoke-test`'e Tier 0 `scan.ps1` ekleyerek statik kontrolleri modelden çıkarmak (en büyük kalan token açığı).
- `unity-smoke-test/SKILL.md` batchmode komutundaki `-quit` bayrağını kaldırmak — hafızadaki `-runTests`+`-quit` tuzağıyla çelişiyor (editör testler koşmadan çıkar).
