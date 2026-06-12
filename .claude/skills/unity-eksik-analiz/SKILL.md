---
name: unity-eksik-analiz
description: BURAK Unity yılan oyununun eksikliklerini (tamamlanmamış özellikler, mantık hataları, oyun hissi, görsel/işitsel cila, 6-yaş erişilebilirliği) token-cimri biçimde analiz eder ve gruplu, öncelikli bir iyileştirme yol haritası üretir. Önce deterministik tarama script'i çalıştırılır (0 model token), sonra yalnızca onun sıkıştırılmış çıktısı yorumlanır. "eksiklik analizi", "oyunu analiz et", "neler eksik", "ne ekleyelim", "iyileştirme planı", "oyunu geliştir" denince kullan. Bu skill "çalışıyor mu" sorusuyla ilgilenmez — o unity-smoke-test'in işidir.
---

# Unity Eksiklik Analizi — BURAK Oyunu

Amaç: Oyunun **derlendiğini/çalıştığını değil**, **ne kadar tam ve cilalı** olduğunu değerlendirmek; eksiklikleri önem sırasına dizmek; **token-optimize** (gruplu, sıralı) bir iyileştirme yol haritası üretmek.

## Token-cimri prosedür (ZORUNLU SIRA — sapma)

1. **Tarama script'ini çalıştır, yalnızca çıktısını oku:**
   ```
   powershell -NoProfile -File .claude/skills/unity-eksik-analiz/scan.ps1
   ```
   Bu script mekanik taramayı modelden çıkarır (envanter, SerializeField çapraz-kontrolü, event kablolaması, özellik VAR/YOK bayrakları, yarım iş izleri, GameConfig kullanımı, test kapsamı, allocation sezgisi). Çıktısı birkaç KB'lık bir "olgu sayfası"dır.
2. **Olgu sayfasını rubriğe göre yorumla.** Kaynak dosyaları toptan OKUMA. Olgu sayfası zaten gerekli olguları içerir.
3. **Yalnızca gerektiğinde hedefli oku:** Olgu sayfası belirsiz bir nokta işaret ediyorsa (ör. bir VAR/YOK bayrağını doğrulamak, bir `dosya:satır`'ın bağlamını görmek) **en fazla 1-2 belirli dosyayı** Read/Grep ile aç. Asla "her ihtimale karşı" okuma yapma.
4. **Unity batchmode ÇALIŞTIRMA.** Test koşumu bu skill'in işi değildir (o `unity-smoke-test`). Ayrımı koru.

Grep heuristikleri kusurludur: bir VAR bayrağı tek bir **yorum** satırına işaret ediyorsa şüphelen ve o satırı doğrula. Emin olmadığını "doğrulanamadı" diye işaretle, uydurma.

## Rubrik (değerlendirme boyutları)

- **Tamlık (klasik yılan):** Klasik yılan oyununun beklenen parçaları var mı? (yön kontrolü, büyüme, çarpışma, skor, yeniden başlat, rekor.) Eksik beklenen parça?
- **Mantık/bug:** Olgu sayfasındaki event kablolaması tutarsızlıkları (abonesi olmayan / hiç tetiklenmeyen event'ler), öksüz GameConfig alanları, SerializeField eşleşmezlikleri.
- **Oyun hissi:** Hız eğrisi 6-yaş için uygun mu? Input tepkiselliği/tamponlama? Hareket cilası (hücre-hücre zıplama vs. yumuşak geçiş)?
- **Cila (görsel/işitsel):** Müzik gerçekten çalıyor mu? Ses efekti çeşitliliği? Yem yeme/ölüm geri bildirimi? Görsel canlılık?
- **Çocuk erişilebilirliği (6-yaş — Burak):** Nasıl oynanacağı anlatılıyor mu? Kayıp durumu korkutucu değil mi? Büyük/net dokunma hedefleri? Dokunmatik (tablet/telefon) desteği?
- **Test kapsamı:** Olgu sayfasındaki KAPSANMAYAN sınıflar — hangisinin testi gerçekten değerli (saf mantık) vs. gereksiz (görsel/MonoBehaviour)?

## Önem kovaları

- **KRİTİK** — oyunu bozar/yanlış oynatır (ör. eşleşmeyen SerializeField, yanlış çarpışma).
- **ÖNEMLİ** — oyun çalışır ama önemli bir parça eksik (ör. dokunmatik yok → tablette oynanamaz, müzik hiç çalmıyor).
- **CİLA** — his/görsel/işitsel iyileştirme (ör. yumuşak hareket, ölüm sarsıntısı).
- **FİKİR** — isteğe bağlı yeni özellik (ör. zorluk seviyeleri, engeller, 2. oyuncu).

## Çıktı formatı (token-optimize yol haritası)

```
## BURAK Eksiklik Raporu

### Özet
2-3 cümle: oyun ne durumda, en büyük 1-2 boşluk ne.

### Bulgular (önem sırasiyla, üst sınır ~12)
[KRİTİK]  <tek satır bulgu> — Düzeltme: <tek satır> (dosya:satır)
[ÖNEMLİ]  ...
[CİLA]    ...
[FİKİR]   ...

### Token-optimize Yol Haritası (gruplu)
GRUP A — <tema> (tek odaklı oturum): bulgu#'ları, neden birlikte (aynı dosyalar).
GRUP B — <tema>: ...
(Ucuz-yüksek-etki önce. Her grup tek seferde, minimum dosyaya dokunarak yapılabilecek boyutta.)

### Sıradaki Tek Aksiyon
-> En yüksek değerli, en ucuz tek adım.
```

**Gruplama kuralı:** Aynı dosyalara dokunan veya aynı temayı paylaşan bulguları tek GRUP yap ki gelecekteki düzeltme oturumu bir grubu tek bağlam yüklemesiyle bitirsin (downstream token tasarrufu). Grupları ucuz-yüksek-etki sırala.

## Kurallar

- **Salt-okunur.** Kaynak dosyaları DEĞİŞTİRME; yalnızca öner.
- Önce script, sonra düşün; toptan okuma yok.
- Bulguları üst sınırla (~12); terse tek-satır.
- Mevcut korunan-ama-kullanılmayan dosyaları (CameraFollow, TrackRecycler) "ölü kod" diye yazmadan önce kullanılıp kullanılmadığını olgu sayfasından teyit et.
