using UnityEngine;
using UnityEditor;
using BurakOyun.Gameplay;

namespace BurakOyun.Editor
{
    public static class VisualUpgrade
    {
        [MenuItem("BurakOyun/4 — Görsel & Ses Paketi Kur", priority = 4)]
        public static void ApplyVisualPackage()
        {
            AddSnakeTrail();
            AddDecorations();
            EnhanceParticles();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("✓ Görsel paket uygulandı.");
        }

        private static void AddSnakeTrail()
        {
            var snakeBody = GameObject.Find("Snake/Body");
            if (snakeBody == null)
            {
                Debug.LogWarning("[VisualUpgrade] 'Snake/Body' sahnede bulunamadı, SnakeTrail atlandı.");
                return;
            }

            if (snakeBody.GetComponent<SnakeTrail>() != null)
            {
                Debug.Log("[VisualUpgrade] SnakeTrail zaten mevcut, atlandı.");
                return;
            }

            snakeBody.AddComponent<SnakeTrail>();
            EditorUtility.SetDirty(snakeBody);
        }

        private static void AddDecorations()
        {
            var decorations = new GameObject("Decorations");

            // ── Güneş ──
            var sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sun.name = "Sun";
            sun.transform.SetParent(decorations.transform);
            sun.transform.position = new Vector3(20f, 15f, 40f);
            sun.transform.localScale = new Vector3(4f, 4f, 4f);

            var sunCollider = sun.GetComponent<SphereCollider>();
            if (sunCollider != null) Object.DestroyImmediate(sunCollider);

            var sunMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            sunMat.color = new Color(1f, 0.95f, 0.5f);
            AssetDatabase.CreateAsset(sunMat, "Assets/Prefabs/SunMat.mat");
            sun.GetComponent<Renderer>().sharedMaterial = sunMat;

            // ── Bulutlar ──
            var cloudMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prefabs/CloudMat.mat");
            if (cloudMat == null)
            {
                cloudMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                cloudMat.color = Color.white;
                AssetDatabase.CreateAsset(cloudMat, "Assets/Prefabs/CloudMat.mat");
            }

            Vector3[] cloudPositions = new Vector3[]
            {
                new Vector3(-12f, 8f, 15f),
                new Vector3(12f,  8f, 40f),
                new Vector3(-10f, 9f, 65f),
            };

            Vector3[] sphereOffsets = new Vector3[]
            {
                new Vector3(0f,     0f,    0f),
                new Vector3(-1.2f,  0.3f,  0f),
                new Vector3( 1.2f,  0.2f,  0f),
            };
            float[] sphereScales = new float[] { 2f, 1.5f, 1.7f };

            for (int c = 0; c < cloudPositions.Length; c++)
            {
                var cloud = new GameObject($"Cloud_{c}");
                cloud.transform.SetParent(decorations.transform);
                cloud.transform.position = cloudPositions[c];

                for (int s = 0; s < sphereOffsets.Length; s++)
                {
                    var part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    part.name = $"CloudPart_{s}";
                    part.transform.SetParent(cloud.transform);
                    part.transform.localPosition = sphereOffsets[s];
                    float sc = sphereScales[s];
                    part.transform.localScale = new Vector3(sc, sc, sc);
                    part.GetComponent<Renderer>().sharedMaterial = cloudMat;

                    var col = part.GetComponent<SphereCollider>();
                    if (col != null) Object.DestroyImmediate(col);
                }
            }

            EditorUtility.SetDirty(decorations);
        }

        private static void EnhanceParticles()
        {
            // ── Confetti ──
            var confettiGo = GameObject.Find("Confetti");
            if (confettiGo == null)
            {
                Debug.LogWarning("[VisualUpgrade] 'Confetti' sahnede bulunamadı, atlandı.");
            }
            else
            {
                var ps = confettiGo.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.maxParticles = 150;
                    main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
                    main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 12f);

                    var emission = ps.emission;
                    emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 100) });

                    // Rainbow gradient startColor
                    var gradient = new Gradient();
                    gradient.SetKeys(
                        new GradientColorKey[]
                        {
                            new GradientColorKey(Color.red,     0f),
                            new GradientColorKey(Color.yellow,  0.25f),
                            new GradientColorKey(Color.green,   0.5f),
                            new GradientColorKey(Color.cyan,    0.75f),
                            new GradientColorKey(Color.magenta, 1f),
                        },
                        new GradientAlphaKey[]
                        {
                            new GradientAlphaKey(1f, 0f),
                            new GradientAlphaKey(0f, 1f),
                        }
                    );
                    main.startColor = new ParticleSystem.MinMaxGradient(gradient);
                }
                EditorUtility.SetDirty(confettiGo);
            }

            // ── CollectSparkle ──
            var sparkleGo = GameObject.Find("CollectSparkle");
            if (sparkleGo == null)
            {
                Debug.LogWarning("[VisualUpgrade] 'CollectSparkle' sahnede bulunamadı, atlandı.");
            }
            else
            {
                var ps = sparkleGo.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.maxParticles = 25;
                    main.startColor = new ParticleSystem.MinMaxGradient(
                        new Color(1f, 0.95f, 0.3f),
                        new Color(1f, 0.5f,  0.1f));
                    main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);

                    var emission = ps.emission;
                    emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });
                }
                EditorUtility.SetDirty(sparkleGo);
            }
        }
    }
}
