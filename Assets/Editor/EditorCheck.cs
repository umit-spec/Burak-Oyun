using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using BurakOyun.Core;
using BurakOyun.Gameplay;
using BurakOyun.UI;

namespace BurakOyun.Editor
{
    public static class EditorCheck
    {
        [MenuItem("BurakOyun/Smoke Test — Editor Check", priority = 10)]
        public static void RunCheck()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== BurakOyun Smoke Test — Editor Check ===\n");

            int pass = 0, fail = 0;

            Check(sb, ref pass, ref fail,
                "URP Pipeline asset (Assets/Settings/URP-Pipeline.asset)",
                AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/URP-Pipeline.asset") != null,
                "BurakOyun/0 — URP Pipeline Kur");

            Check(sb, ref pass, ref fail,
                "Assets/Scenes/Game.unity",
                AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Game.unity") != null,
                "BurakOyun/3 — Sahneyi Kur");

            string tmpFontDir = Path.Combine(Application.dataPath, "TextMesh Pro", "Resources", "Fonts & Materials");
            Check(sb, ref pass, ref fail,
                "TMP Essential Resources (font asset'leri)",
                Directory.Exists(tmpFontDir) && Directory.GetFiles(tmpFontDir, "*.asset").Length > 0,
                "Window → TextMeshPro → Import TMP Essential Resources");

            string asmdefPath = Path.Combine(Application.dataPath, "Scripts", "BurakOyun.Runtime.asmdef");
            bool asmdefOk = false;
            if (File.Exists(asmdefPath))
            {
                string c = File.ReadAllText(asmdefPath);
                asmdefOk = c.Contains("Unity.InputSystem") && c.Contains("Unity.TextMeshPro");
            }
            Check(sb, ref pass, ref fail,
                "Runtime asmdef referansları (InputSystem + TMP)",
                asmdefOk,
                "Assets/Scripts/BurakOyun.Runtime.asmdef dosyasını kontrol et");

            sb.AppendLine();
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.isLoaded && activeScene.rootCount > 0)
            {
                CheckSceneComponent<GameManager>(sb, ref pass, ref fail, "GameManager");
                CheckSceneComponent<SnakeController>(sb, ref pass, ref fail, "SnakeController");
                CheckSceneComponent<FoodSpawner>(sb, ref pass, ref fail, "FoodSpawner");
                CheckSceneComponent<UIManager>(sb, ref pass, ref fail, "UIManager");
            }
            else
            {
                sb.AppendLine("[ ] Sahne bileşenleri — Game.unity açık değil, kontrol atlandı");
            }

            sb.AppendLine($"\nSonuç: {pass} PASS / {fail} FAIL");
            Debug.Log(sb.ToString());

            if (fail == 0)
                EditorUtility.DisplayDialog(
                    "Smoke Test — PASS",
                    $"Tüm {pass} kontrol geçti.\n► Play basabilirsiniz.",
                    "Tamam");
            else
                EditorUtility.DisplayDialog(
                    $"Smoke Test — {fail} FAIL",
                    sb.ToString(),
                    "Tamam");
        }

        static void Check(StringBuilder sb, ref int pass, ref int fail,
            string label, bool ok, string fix)
        {
            if (ok) { sb.AppendLine($"✅ {label}"); pass++; }
            else    { sb.AppendLine($"❌ {label}\n   → {fix}"); fail++; }
        }

        static void CheckSceneComponent<T>(StringBuilder sb, ref int pass, ref int fail, string label)
            where T : Component
        {
            Check(sb, ref pass, ref fail,
                $"{label} sahnede mevcut",
                Object.FindAnyObjectByType<T>(FindObjectsInactive.Include) != null,
                "BurakOyun/3 — Sahneyi Kur ile sahneyi yeniden kur");
        }
    }
}
