using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BurakOyun.Editor
{
    /// <summary>
    /// Tek komutla Android APK üretir (sideload). Player Settings'i build anında programatik
    /// ayarlar — ProjectSettings.asset elle düzenlenmeden build kendi kendini yapılandırır.
    /// Headless: Unity.exe -batchmode -executeMethod BurakOyun.Editor.BuildScript.PerformAndroidBuild -quit
    /// </summary>
    public static class BuildScript
    {
        // Burak için sabit kimlik/ayarlar. İstenirse buradan değişir.
        private const string PackageId   = "com.umit.burakoyun"; // BR-1
        private const string CompanyName = "Umit";               // BR-2
        private const string ProductName = "Burak Oyun";         // BR-2
        private const string OutputDir   = "Build";
        private const string ApkName     = "Burak.apk";

        [MenuItem("BurakOyun/9 — Android APK Build", priority = 9)]
        public static void PerformAndroidBuild()
        {
            RedirectToWritableSdk();
            ConfigureAndroidPlayerSettings();

            // Build'e girecek sahneler: Build Settings'te etkin olanlar (SceneSetup "3" bunu doldurur).
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                const string fallback = "Assets/Scenes/Game.unity";
                if (File.Exists(fallback))
                {
                    scenes = new[] { fallback };
                }
                else
                {
                    Fail("Build Settings'te etkin sahne yok ve 'Assets/Scenes/Game.unity' bulunamadı. " +
                         "Önce 'BurakOyun/3 — Sahneyi Kur' menüsünü çalıştır.");
                    return;
                }
            }

            // Aktif hedefi Android'e geçir (Android Build Support modülü gerekir — BR-7).
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Fail("Android'e geçilemedi. Unity Hub → bu sürüme 'Android Build Support' modülünü ekle (BR-7).");
                return;
            }

            string outDir = Path.Combine(Directory.GetCurrentDirectory(), OutputDir);
            Directory.CreateDirectory(outDir);
            string apkPath = Path.Combine(outDir, ApkName);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BurakOyun] ✓ APK BAŞARILI: {apkPath}  ({summary.totalSize / (1024 * 1024)} MB, " +
                          $"{summary.totalTime.TotalSeconds:0} sn). Telefona/tablete yükleyip oynat.");
            }
            else
            {
                Fail($"APK build sonucu: {summary.result} ({summary.totalErrors} hata).");
            }
        }

        // BR-1..BR-4: paket adı, şirket/ürün, landscape kilidi, Mono + ARMv7 (sideload APK).
        private static void ConfigureAndroidPlayerSettings()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;

            // BR-3: yalnızca landscape (her iki yön) — portre kapalı. Çocuk tableti yatay tutar.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // BR-4: Mono backend yalnızca ARMv7 destekler; sideload APK için yeterli ve hızlı.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        }

        // Unity'nin paketlediği SDK Program Files altında salt-okunur olabilir; Unity bir bileşeni
        // güncellemek isteyince yazamayıp "Failed to update Android SDK" verir. Kullanıcının yazılabilir
        // Android Studio SDK'sı (%LOCALAPPDATA%\Android\Sdk) varsa Unity'yi ona yönlendir.
        private static void RedirectToWritableSdk()
        {
            string localAppData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            string userSdk = Path.Combine(localAppData, "Android", "Sdk");
            if (Directory.Exists(Path.Combine(userSdk, "platform-tools")))
            {
                EditorPrefs.SetString("AndroidSdkRoot", userSdk);
                Debug.Log($"[BurakOyun] Android SDK (yazılabilir) kullanılacak: {userSdk}");
            }
        }

        private static void Fail(string message)
        {
            Debug.LogError("[BurakOyun] " + message);
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }
}
