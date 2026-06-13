using System.IO;
using UnityEditor;
using UnityEngine;

namespace BurakOyun.Editor
{
    public static class AndroidSetup
    {
        const string NDK_VERSION   = "25.1.8937393";   // r25c
        const string GRADLE_PROPS  = "Assets/Plugins/Android/gradleTemplate.properties";
        const string PACKAGE_NAME  = "com.burakoyun.harfoyunu";

        // ── Ana kurulum menüsü ───────────────────────────────────────────────
        [MenuItem("BurakOyun/Android — Player Ayarlarını Kur", priority = 20)]
        public static void SetupAndroid()
        {
            // Kimlik
            PlayerSettings.companyName          = "BurakOyun";
            PlayerSettings.productName          = "BURAK Harf Oyunu";
            PlayerSettings.applicationIdentifier = PACKAGE_NAME;

            // API seviyeleri
            PlayerSettings.Android.minSdkVersion    = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // 3 şeritli runner → yatay ekran
            PlayerSettings.defaultInterfaceOrientation          = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait           = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft      = true;
            PlayerSettings.allowedAutorotateToLandscapeRight     = true;

            // IL2CPP + ARM64 (Google Play 64-bit zorunluluğu)
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // İzinler kapalı
            PlayerSettings.Android.forceInternetPermission = false;
            PlayerSettings.Android.forceSDCardPermission   = false;

            // API uyumluluğu
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard);

            // Custom Gradle Properties Template etkinleştir (CXX1429 fix)
            EnableGradlePropertiesTemplate();

            AssetDatabase.SaveAssets();
            Debug.Log("[BurakOyun] Android player ayarları uygulandı.");

            // NDK uyarısını kontrol et ve kullanıcıya göster
            string ndkWarning = GetNdkWarning();
            string dialog = BuildDialogMessage(ndkWarning);

            EditorUtility.DisplayDialog("Android Player Ayarları Kuruldu", dialog, "Tamam");
        }

        // ── NDK / CXX1429 hata giderme menüsü ──────────────────────────────
        [MenuItem("BurakOyun/Android — CXX1429 Hata Giderme", priority = 21)]
        public static void FixCxx1429()
        {
            bool fixed1 = EnableGradlePropertiesTemplate();
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "CXX1429 — IL2CPP CMake Build Hatası",
                "[CXX1429] hatası NDK sürümü uyumsuzluğundan kaynaklanır.\n\n" +
                "── Kod tarafı fix ──\n" +
                (fixed1
                    ? "✅ gradleTemplate.properties aktif edildi\n"
                    : "✅ gradleTemplate.properties zaten aktifti\n") +
                "   (android.ndkVersion=25.1.8937393, prefabVersion=2.0.0)\n\n" +
                "── Sıradaki Manuel Adım ──\n" +
                "NDK sürümünü doğrulamak için aşağıdaki IKISINDEN BİRİNİ yap:\n\n" +
                "[ Yol A — Önerilir ]\n" +
                "Edit → Preferences → External Tools → Android\n" +
                "→ 'Android NDK installed with Unity (recommended)' seç\n\n" +
                "[ Yol B — Özel NDK ]\n" +
                "Android SDK Manager'dan NDK r25c (25.1.8937393) indir\n" +
                "ve Preferences → Android NDK yolunu göster\n\n" +
                "── Eğer hâlâ hata alıyorsan ──\n" +
                "Android SDK Manager → SDK Tools → CMake 3.22.1 yüklü mü kontrol et.",
                "Tamam");
        }

        // ── Validation ──────────────────────────────────────────────────────
        [MenuItem("BurakOyun/Android — Player Ayarlarını Kur", true)]
        [MenuItem("BurakOyun/Android — CXX1429 Hata Giderme", true)]
        static bool ValidateAndroid() => BuildPipeline.IsBuildTargetSupported(
            BuildTargetGroup.Android, BuildTarget.Android);

        // ── Yardımcılar ─────────────────────────────────────────────────────

        // gradleTemplate.properties'in PlayerSettings'te aktif olup olmadığını
        // kontrol eder ve gerekliyse aktif eder. true = yeni aktif edildi.
        static bool EnableGradlePropertiesTemplate()
        {
            // Unity; Assets/Plugins/Android/gradleTemplate.properties dosyası varsa
            // Publishing Settings → Custom Gradle Properties Template seçeneğini
            // SerializedObject üzerinden etkinleştir.
            var so = new SerializedObject(
                AssetDatabase.LoadMainAssetAtPath("ProjectSettings/ProjectSettings.asset"));

            // Unity 2021+ dahili property adı
            var prop = so.FindProperty("androidUseCustomBuildGradlePropertiesTemplate");
            if (prop != null && !prop.boolValue)
            {
                prop.boolValue = true;
                so.ApplyModifiedProperties();
                Debug.Log("[BurakOyun] Custom Gradle Properties Template etkinleştirildi.");
                return true;
            }
            return false;
        }

        static string GetNdkWarning()
        {
            string ndkRoot = EditorPrefs.GetString("AndroidNdkRoot");
            if (string.IsNullOrEmpty(ndkRoot))
                return null; // bundled NDK kullanıyor → sorun yok

            // Özel NDK yolu ayarlı — sürümü kontrol et
            if (!ndkRoot.Contains(NDK_VERSION))
                return $"⚠️  NDK sürümü uyumsuz olabilir!\n" +
                       $"Mevcut: {ndkRoot}\n" +
                       $"Beklenen sürüm: r25c ({NDK_VERSION})\n\n" +
                       "Düzeltmek için:\n" +
                       "Edit → Preferences → External Tools → Android NDK\n" +
                       "→ 'Android NDK installed with Unity' seç";
            return null;
        }

        static string BuildDialogMessage(string ndkWarning)
        {
            string msg =
                "• Paket   : " + PACKAGE_NAME + "\n" +
                "• Min API : 24 (Android 7.0)\n" +
                "• Hedef   : Otomatik\n" +
                "• Yönelim : Landscape\n" +
                "• Backend : IL2CPP + ARM64\n" +
                "• İnternet: Kapalı\n" +
                "• Gradle Properties Template: Aktif\n";

            if (ndkWarning != null)
                msg += "\n" + ndkWarning;
            else
                msg += "\n✅ NDK: Bundled (önerilir)\n";

            msg += "\nSonraki adım:\n" +
                   "File → Build Settings → Android → Switch Platform → Build";
            return msg;
        }
    }
}
