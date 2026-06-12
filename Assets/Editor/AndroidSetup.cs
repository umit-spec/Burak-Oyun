using UnityEditor;
using UnityEngine;

namespace BurakOyun.Editor
{
    public static class AndroidSetup
    {
        [MenuItem("BurakOyun/Android — Player Ayarlarını Kur", priority = 20)]
        public static void SetupAndroid()
        {
            // Kimlik
            PlayerSettings.companyName = "BurakOyun";
            PlayerSettings.productName = "BURAK Harf Oyunu";
            PlayerSettings.applicationIdentifier = "com.burakoyun.harfoyunu";

            // API seviyeleri: min Android 7.0, hedef otomatik (en son)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // 3 şeritli runner için yatay ekran
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // IL2CPP + ARM64 (Google Play 64-bit zorunluluğu)
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // İnternet ve gereksiz izinler kapalı
            PlayerSettings.Android.forceInternetPermission = false;
            PlayerSettings.Android.forceSDCardPermission = false;

            // Giriş sistemi: sadece New Input System
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard);

            AssetDatabase.SaveAssets();

            Debug.Log("[BurakOyun] Android player ayarları uygulandı.");
            EditorUtility.DisplayDialog(
                "Android Player Ayarları Kuruldu",
                "• Paket: com.burakoyun.harfoyunu\n" +
                "• Min API: 24 (Android 7.0)\n" +
                "• Hedef API: Otomatik\n" +
                "• Yönelim: Yatay (Landscape)\n" +
                "• Scripting: IL2CPP, ARM64\n" +
                "• İnternet izni: Kapalı\n\n" +
                "Sonraki adım:\n" +
                "File → Build Settings → Android → Switch Platform → Build",
                "Tamam");
        }

        [MenuItem("BurakOyun/Android — Player Ayarlarını Kur", true)]
        static bool ValidateSetupAndroid() => BuildPipeline.IsBuildTargetSupported(
            BuildTargetGroup.Android, BuildTarget.Android);
    }
}
