using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

namespace BurakOyun.Editor
{
    /// <summary>
    /// Yılan oyunu sahnesini sıfırdan programatik kurar (elle sahne kurulumu yok).
    /// Menü sırası: 0 URP → 1 Asset → 2 Prefab → 3 Sahne. "3" hepsini otomatik çağırır.
    /// </summary>
    public static class SceneSetup
    {
        [MenuItem("BurakOyun/1 — Asset'leri Oluştur (GameConfig)", priority = 1)]
        public static void CreateAssets()
        {
            EnsureFolder("Assets/Data");

            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Data/GameConfig.asset") == null)
            {
                var gc = ScriptableObject.CreateInstance<Data.GameConfig>();
                AssetDatabase.CreateAsset(gc, "Assets/Data/GameConfig.asset");
                Debug.Log("[BurakOyun] GameConfig oluşturuldu.");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[BurakOyun] ✓ Asset'ler hazır.");
        }

        [MenuItem("BurakOyun/2 — Prefab'ları Oluştur (Yılan + Yem)", priority = 2)]
        public static void CreatePrefabs()
        {
            EnsureFolder("Assets/Prefabs");
            CreateHeadPrefab();
            CreateSegmentPrefab();
            CreateFoodPrefab();
            AssetDatabase.SaveAssets();
            Debug.Log("[BurakOyun] ✓ Prefab'lar hazır (SnakeHead, SnakeSegment, Food).");
        }

        static void CreateHeadPrefab()
        {
            const string path = "Assets/Prefabs/SnakeHead.prefab";
            var go = new GameObject("SnakeHead");

            // Yuvarlak sevimli baş (küre). İsim "Cube" kalır — ölüm flaşı bu çocuğu hedefler.
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Cube";
            body.transform.SetParent(go.transform);
            body.transform.localScale = new Vector3(1.0f, 0.92f, 1.08f); // hafif öne uzun
            body.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateMaterial("Assets/Prefabs/SnakeHeadMat.mat", new Color(0.22f, 0.80f, 0.40f), 0.25f);
            Object.DestroyImmediate(body.GetComponent<SphereCollider>());

            // Büyük sevimli gözler — baş, SnakeController tarafından gidiş yönüne (+Z) döndürülür
            var eyeMat = GetOrCreateMaterial("Assets/Prefabs/EyeMat.mat", Color.white, 0.35f);
            var pupilMat = GetOrCreateMaterial("Assets/Prefabs/PupilMat.mat", new Color(0.10f, 0.10f, 0.13f), 0.1f);
            for (int side = -1; side <= 1; side += 2)
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = side < 0 ? "EyeL" : "EyeR";
                eye.transform.SetParent(go.transform);
                eye.transform.localScale = Vector3.one * 0.34f;
                eye.transform.localPosition = new Vector3(side * 0.24f, 0.28f, 0.40f);
                eye.GetComponent<Renderer>().sharedMaterial = eyeMat;
                Object.DestroyImmediate(eye.GetComponent<SphereCollider>());

                var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pupil.name = "Pupil";
                pupil.transform.SetParent(eye.transform);
                pupil.transform.localScale = Vector3.one * 0.55f;
                pupil.transform.localPosition = new Vector3(0f, -0.05f, 0.42f);
                pupil.GetComponent<Renderer>().sharedMaterial = pupilMat;
                Object.DestroyImmediate(pupil.GetComponent<SphereCollider>());
            }

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        static void CreateSegmentPrefab()
        {
            const string path = "Assets/Prefabs/SnakeSegment.prefab";
            var go = new GameObject("SnakeSegment");
            // Yuvarlak gövde boncuğu (küre). Renk SnakeController'da segment indeksine göre
            // gradyan ile değiştirilir (MaterialPropertyBlock); buradaki renk yedek.
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Cube";
            body.transform.SetParent(go.transform);
            body.transform.localScale = Vector3.one * 0.82f;
            body.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateMaterial("Assets/Prefabs/SnakeMat.mat", new Color(0.20f, 0.80f, 0.45f), 0.25f);
            Object.DestroyImmediate(body.GetComponent<SphereCollider>());

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        static void CreateFoodPrefab()
        {
            const string path = "Assets/Prefabs/Food.prefab";
            var go = new GameObject("Food");

            // Elma gövdesi (parlak, hafif basık)
            var apple = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            apple.name = "Apple";
            apple.transform.SetParent(go.transform);
            apple.transform.localScale = new Vector3(0.72f, 0.66f, 0.72f);
            apple.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateMaterial("Assets/Prefabs/FoodMat.mat", new Color(0.93f, 0.26f, 0.28f), 0.6f);
            Object.DestroyImmediate(apple.GetComponent<SphereCollider>());

            // Kahverengi sap (silindir, hafif eğik)
            var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(go.transform);
            stem.transform.localScale = new Vector3(0.05f, 0.16f, 0.05f);
            stem.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            stem.transform.localRotation = Quaternion.Euler(8f, 0f, -10f);
            stem.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateMaterial("Assets/Prefabs/StemMat.mat", new Color(0.45f, 0.30f, 0.16f), 0.2f);
            Object.DestroyImmediate(stem.GetComponent<CapsuleCollider>());

            // Yeşil yaprak (basık küre)
            var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaf.name = "Leaf";
            leaf.transform.SetParent(go.transform);
            leaf.transform.localScale = new Vector3(0.28f, 0.05f, 0.16f);
            leaf.transform.localPosition = new Vector3(0.16f, 0.46f, 0f);
            leaf.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
            leaf.GetComponent<Renderer>().sharedMaterial =
                GetOrCreateMaterial("Assets/Prefabs/LeafMat.mat", new Color(0.40f, 0.78f, 0.32f), 0.3f);
            Object.DestroyImmediate(leaf.GetComponent<SphereCollider>());

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        [MenuItem("BurakOyun/0 — URP Pipeline Kur (önce bunu)", priority = 0)]
        public static UniversalRenderPipelineAsset SetupURP()
        {
            EnsureFolder("Assets/Settings");
            const string urpPath = "Assets/Settings/URP-Pipeline.asset";

            var existing = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(urpPath);
            if (existing != null)
            {
                AssignPipeline(existing);
                Debug.Log("[BurakOyun] URP pipeline zaten var, atandı.");
                return existing;
            }

            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, "Assets/Settings/UniversalRenderer.asset");

            var urp = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(urp, urpPath);
            AssetDatabase.SaveAssets();

            AssignPipeline(urp);
            Debug.Log("[BurakOyun] ✓ URP pipeline oluşturuldu ve Graphics/Quality'ye atandı.");
            return urp;
        }

        static void AssignPipeline(RenderPipelineAsset urp)
        {
            GraphicsSettings.defaultRenderPipeline = urp;
            QualitySettings.renderPipeline = urp;
        }

        // Materyali oluşturur YA DA varsa renk/parlaklığını günceller (yeniden kurulumda yeni palet uygulanır).
        static Material GetOrCreateMaterial(string path, Color color, float smoothness = 0.2f)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static Material CreateUrpParticleMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default"); // güvenli yedek
            return new Material(shader);
        }

        [MenuItem("BurakOyun/3 — Sahneyi Kur (Tüm Objeler)", priority = 3)]
        public static void SetupScene()
        {
            // TMP Essential Resources kontrolü — font asset yoksa UI yazıları bozuk görünür
            string tmpFontDir = System.IO.Path.Combine(Application.dataPath,
                "TextMesh Pro", "Resources", "Fonts & Materials");
            bool tmpReady = System.IO.Directory.Exists(tmpFontDir)
                && System.IO.Directory.GetFiles(tmpFontDir, "*.asset").Length > 0;
            if (!tmpReady && !EditorUtility.DisplayDialog(
                "TMP Essential Resources Eksik",
                "TextMeshPro Essential Resources henüz import edilmemiş.\n\n" +
                "UI yazıları bozuk görünebilir.\n\n" +
                "Önce: Window → TextMeshPro → Import TMP Essential Resources\n\n" +
                "Yine de devam etmek istiyor musunuz?",
                "Devam Et", "İptal"))
                return;

            // Önce render pipeline + asset + prefab garanti
            SetupURP();
            CreateAssets();
            CreatePrefabs();

            // Temiz sahne (eski harf oyunu objeleri karışmasın)
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var config = AssetDatabase.LoadAssetAtPath<Data.GameConfig>("Assets/Data/GameConfig.asset");
            var headPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SnakeHead.prefab");
            var segmentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SnakeSegment.prefab");
            var foodPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Food.prefab");

            // Hücre→dünya dönüşümü oyunla birebir aynı olsun diye aynı çekirdek kullanılır
            var grid = new Gameplay.GridMovement(config.gridWidth, config.gridHeight);
            float cell = config.cellSize;
            float boardW = config.gridWidth * cell;
            float boardH = config.gridHeight * cell;

            // ── Dama desenli zemin (çocuk hücreleri görsün) ──
            var board = new GameObject("Board");
            var groundA = GetOrCreateMaterial("Assets/Prefabs/GroundMat.mat", new Color(0.64f, 0.86f, 0.66f), 0.1f);
            var groundB = GetOrCreateMaterial("Assets/Prefabs/GroundMatAlt.mat", new Color(0.74f, 0.92f, 0.74f), 0.1f);
            for (int x = 0; x < config.gridWidth; x++)
                for (int y = 0; y < config.gridHeight; y++)
                {
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = $"Cell_{x}_{y}";
                    tile.transform.SetParent(board.transform);
                    tile.transform.localScale = new Vector3(cell, 0.1f, cell);
                    tile.transform.position = grid.CellToWorld(new Vector2Int(x, y), cell, -0.05f);
                    tile.GetComponent<Renderer>().sharedMaterial = (x + y) % 2 == 0 ? groundA : groundB;
                    Object.DestroyImmediate(tile.GetComponent<BoxCollider>());
                }

            // ── Duvarlar (görsel sınır — çarpışma mantığı ızgarada) ──
            var wallMat = GetOrCreateMaterial("Assets/Prefabs/WallMat.mat", new Color(0.45f, 0.78f, 0.74f), 0.2f); // pastel deniz mavisi
            var walls = new GameObject("Walls");
            void MakeWall(string name, Vector3 pos, Vector3 scale)
            {
                var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                w.name = name;
                w.transform.SetParent(walls.transform);
                w.transform.position = pos;
                w.transform.localScale = scale;
                w.GetComponent<Renderer>().sharedMaterial = wallMat;
                Object.DestroyImmediate(w.GetComponent<BoxCollider>());
            }
            const float t = 0.5f; // duvar kalınlığı
            MakeWall("WallTop", new Vector3(0f, 0.25f, boardH / 2f + t / 2f), new Vector3(boardW + 2 * t, 0.6f, t));
            MakeWall("WallBottom", new Vector3(0f, 0.25f, -boardH / 2f - t / 2f), new Vector3(boardW + 2 * t, 0.6f, t));
            MakeWall("WallLeft", new Vector3(-boardW / 2f - t / 2f, 0.25f, 0f), new Vector3(t, 0.6f, boardH));
            MakeWall("WallRight", new Vector3(boardW / 2f + t / 2f, 0.25f, 0f), new Vector3(t, 0.6f, boardH));

            // Yuvarlak köşe direkleri (şirin çerçeve, sıcak sarı)
            var postMat = GetOrCreateMaterial("Assets/Prefabs/PostMat.mat", new Color(0.98f, 0.78f, 0.42f), 0.3f);
            float px = boardW / 2f + t / 2f, pz = boardH / 2f + t / 2f;
            foreach (var corner in new[]
            {
                new Vector3(-px, 0.35f, -pz), new Vector3(px, 0.35f, -pz),
                new Vector3(-px, 0.35f, pz), new Vector3(px, 0.35f, pz)
            })
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                post.name = "CornerPost";
                post.transform.SetParent(walls.transform);
                post.transform.position = corner;
                post.transform.localScale = Vector3.one * (t * 1.7f);
                post.GetComponent<Renderer>().sharedMaterial = postMat;
                Object.DestroyImmediate(post.GetComponent<SphereCollider>());
            }

            // ── Yılan ──
            var snakeGo = new GameObject("Snake");
            snakeGo.AddComponent<Gameplay.DirectionInput>();
            var snakeCtrl = snakeGo.AddComponent<Gameplay.SnakeController>();
            SetField(snakeCtrl, "config", config);
            SetField(snakeCtrl, "headPrefab", headPrefab);
            SetField(snakeCtrl, "segmentPrefab", segmentPrefab);

            // ── Yem ──
            var foodGo = new GameObject("FoodSpawner");
            var foodSpawner = foodGo.AddComponent<Gameplay.FoodSpawner>();
            SetField(foodSpawner, "config", config);
            SetField(foodSpawner, "snake", snakeCtrl);
            SetField(foodSpawner, "foodPrefab", foodPrefab);
            SetField(snakeCtrl, "foodSpawner", foodSpawner);

            // ── Kamera (sabit — tüm tahtayı görür, hafif eğik sevimli açı) ──
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            cam.transform.position = new Vector3(0f, boardH * 1.15f, -boardH * 0.85f);
            cam.transform.LookAt(new Vector3(0f, 0f, -boardH * 0.05f));

            // ── Gradient gökyüzü (procedural skybox) ──
            const string skyPath = "Assets/Settings/SkyMat.mat";
            var skyMat = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
            if (skyMat == null)
            {
                skyMat = new Material(Shader.Find("Skybox/Procedural"));
                AssetDatabase.CreateAsset(skyMat, skyPath);
            }
            skyMat.SetColor("_SkyTint", new Color(0.55f, 0.78f, 0.98f));
            skyMat.SetColor("_GroundColor", new Color(0.82f, 0.92f, 0.86f));
            skyMat.SetFloat("_AtmosphereThickness", 0.9f);
            skyMat.SetFloat("_Exposure", 1.35f);
            EditorUtility.SetDirty(skyMat);
            RenderSettings.skybox = skyMat;
            cam.clearFlags = CameraClearFlags.Skybox;

            // ── Işık: sıcak ana + serin dolgu + yumuşak pastel ortam ──
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.05f;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

            var fillGo = new GameObject("Fill Light");
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.4f;
            fill.color = new Color(0.70f, 0.82f, 1f);
            fill.shadows = LightShadows.None;
            fillGo.transform.rotation = Quaternion.Euler(35f, 150f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.70f, 0.75f);

            // ── Manager'lar ──
            var managers = new GameObject("GameManagers");

            var gameMgr = managers.AddComponent<Core.GameManager>();
            var rewardMgr = managers.AddComponent<Core.RewardManager>();
            var audioMgr = managers.AddComponent<Audio.AudioManager>();

            // Ses kaynakları
            var sfxSrc = managers.AddComponent<AudioSource>();
            sfxSrc.playOnAwake = false;
            var musicSrc = managers.AddComponent<AudioSource>();
            musicSrc.playOnAwake = false;
            musicSrc.loop = true;
            musicSrc.volume = 0.12f; // çok düşük, rahatsız etmesin (AudioManager da ayarlar)
            SetField(audioMgr, "gameManager", gameMgr);
            SetField(audioMgr, "sfxSource", sfxSrc);
            SetField(audioMgr, "musicSource", musicSrc);

            // ── Partiküller ──
            var particleMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prefabs/ParticleMat.mat");
            if (particleMat == null)
            {
                particleMat = CreateUrpParticleMaterial();
                AssetDatabase.CreateAsset(particleMat, "Assets/Prefabs/ParticleMat.mat");
            }

            var confettiGo = new GameObject("Confetti");
            var confettiPS = confettiGo.AddComponent<ParticleSystem>();
            var main = confettiPS.main;
            main.startLifetime = 2f;
            main.startSpeed = 8f;
            main.startSize = 0.3f;
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.8f, 0.2f), new Color(0.3f, 0.8f, 1f));
            main.playOnAwake = false;
            var emission = confettiPS.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });
            var shape = confettiPS.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 45f;
            confettiGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = particleMat;

            var sparkleGo = new GameObject("CollectSparkle");
            var sparklePS = sparkleGo.AddComponent<ParticleSystem>();
            var sparkMain = sparklePS.main;
            sparkMain.startLifetime = 0.6f;
            sparkMain.startSpeed = 3f;
            sparkMain.startSize = 0.15f;
            sparkMain.maxParticles = 20;
            sparkMain.simulationSpace = ParticleSystemSimulationSpace.World;
            sparkMain.startColor = new Color(1f, 0.95f, 0.4f);
            sparkMain.playOnAwake = false;
            var sparkEmission = sparklePS.emission;
            sparkEmission.rateOverTime = 0;
            sparkEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15) });
            sparkleGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = particleMat;

            SetField(rewardMgr, "gameManager", gameMgr);
            SetField(rewardMgr, "snake", snakeCtrl);
            SetField(rewardMgr, "confetti", confettiPS);
            SetField(rewardMgr, "collectSparkle", sparklePS);

            // ── UI ──
            var canvas = CreateUICanvas(out var startPanel, out var gameOverPanel,
                out var scoreTxt, out var bestTxt, out var feedbackTxt, out var gameOverScoreTxt,
                out var pausePanel, out var pauseBtn);

            var uiMgr = canvas.AddComponent<UI.UIManager>();
            SetField(uiMgr, "gameManager", gameMgr);
            SetField(uiMgr, "scoreText", scoreTxt);
            SetField(uiMgr, "bestText", bestTxt);
            SetField(uiMgr, "feedbackText", feedbackTxt);
            SetField(uiMgr, "startPanel", startPanel);
            SetField(uiMgr, "gameOverPanel", gameOverPanel);
            SetField(uiMgr, "gameOverScoreText", gameOverScoreTxt);
            SetField(uiMgr, "pausePanel", pausePanel);

            // GameManager bağlantıları
            SetField(gameMgr, "snake", snakeCtrl);
            SetField(gameMgr, "foodSpawner", foodSpawner);
            SetField(gameMgr, "ui", uiMgr);

            // Butonları bağla
            var startBtn = startPanel.GetComponentInChildren<Button>();
            var replayBtn = gameOverPanel.GetComponentInChildren<Button>(true);
            var resumeBtn = pausePanel.GetComponentInChildren<Button>(true);
            if (startBtn != null)
                UnityEditor.Events.UnityEventTools.AddPersistentListener(startBtn.onClick, gameMgr.StartGame);
            if (replayBtn != null)
                UnityEditor.Events.UnityEventTools.AddPersistentListener(replayBtn.onClick, gameMgr.Replay);
            if (pauseBtn != null)
                UnityEditor.Events.UnityEventTools.AddPersistentListener(pauseBtn.onClick, gameMgr.Pause);
            if (resumeBtn != null)
                UnityEditor.Events.UnityEventTools.AddPersistentListener(resumeBtn.onClick, gameMgr.Resume);

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(managers);

            // Sahneyi kaydet + Build Settings'e ekle
            EnsureFolder("Assets/Scenes");
            const string scenePath = "Assets/Scenes/Game.unity";
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(active, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };

            Debug.Log("[BurakOyun] ✓ Yılan oyunu sahnesi kuruldu ve 'Assets/Scenes/Game.unity' olarak kaydedildi. " +
                      "Play'e bas, OYNA'ya tıkla, WASD/ok tuşları veya parmakla kaydırarak oyna; P/Escape veya II butonuyla duraklat.");
        }

        static GameObject CreateUICanvas(
            out GameObject startPanel, out GameObject gameOverPanel,
            out TMP_Text scoreTxt, out TMP_Text bestTxt, out TMP_Text feedbackTxt,
            out TMP_Text gameOverScoreTxt, out GameObject pausePanel, out Button pauseButton)
        {
            var canvasGo = new GameObject("UI Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // Skor (sol üst)
            scoreTxt = CreateTMPText(canvasGo.transform, "ScoreText",
                "Skor: 0", 56, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f),
                new Vector2(400f, 80f));
            scoreTxt.color = new Color(1f, 0.85f, 0.1f);

            // En iyi skor (sağ üst)
            bestTxt = CreateTMPText(canvasGo.transform, "BestText",
                "En İyi: 0", 48, TextAlignmentOptions.TopRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -40f),
                new Vector2(400f, 80f));
            bestTxt.color = new Color(1f, 1f, 1f, 0.9f);

            // Feedback (orta üst)
            feedbackTxt = CreateTMPText(canvasGo.transform, "FeedbackText",
                "", 64, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f),
                new Vector2(600f, 100f));
            feedbackTxt.color = new Color(1f, 0.5f, 0.1f);

            // ── Start Panel ──
            startPanel = CreatePanel(canvasGo.transform, "StartPanel", new Color(0f, 0f, 0f, 0.5f));
            CreateTMPText(startPanel.transform, "Title", "BURAK\nYılan Oyunu", 80,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), Vector2.zero,
                new Vector2(800f, 250f));
            CreateButton(startPanel.transform, "BtnPlay", "OYNA",
                new Vector2(0.5f, 0.42f), new Vector2(350f, 120f),
                new Color(0.2f, 0.8f, 0.3f));
            // Çocuk dostu kontrol yönergesi (6 yaş tablette ilk açılışta anlasın)
            var hint = CreateTMPText(startPanel.transform, "ControlHint",
                "Parmağınla KAYDIR!\nYukarı  •  Aşağı  •  Sol  •  Sağ\n(veya ok tuşları)", 46,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero,
                new Vector2(1000f, 220f));
            hint.color = new Color(1f, 0.95f, 0.7f);

            // ── HUD Duraklat butonu (üst orta, her zaman görünür) ──
            pauseButton = CreateButton(canvasGo.transform, "BtnPause", "II",
                new Vector2(0.5f, 1f), new Vector2(120f, 90f),
                new Color(0.2f, 0.5f, 0.9f));
            pauseButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -60f);

            // ── Game Over Panel ──
            gameOverPanel = CreatePanel(canvasGo.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.5f));
            CreateTMPText(gameOverPanel.transform, "GameOverTitle", "Oyun Bitti! :)", 80,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero,
                new Vector2(800f, 150f));
            gameOverScoreTxt = CreateTMPText(gameOverPanel.transform, "GameOverScore", "Skor: 0", 64,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero,
                new Vector2(800f, 180f));
            gameOverScoreTxt.color = new Color(1f, 0.85f, 0.1f);
            CreateButton(gameOverPanel.transform, "BtnReplay", "TEKRAR OYNA",
                new Vector2(0.5f, 0.3f), new Vector2(400f, 120f),
                new Color(0.3f, 0.6f, 1f));
            gameOverPanel.SetActive(false);

            // ── Pause Panel ──
            pausePanel = CreatePanel(canvasGo.transform, "PausePanel", new Color(0f, 0f, 0f, 0.6f));
            CreateTMPText(pausePanel.transform, "PauseTitle", "DURAKLATILDI", 80,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero,
                new Vector2(800f, 150f));
            CreateButton(pausePanel.transform, "BtnResume", "DEVAM ET",
                new Vector2(0.5f, 0.38f), new Vector2(400f, 120f),
                new Color(0.2f, 0.8f, 0.3f));
            pausePanel.SetActive(false);

            return canvasGo;
        }

        static TMP_Text CreateTMPText(Transform parent, string name, string text,
            float fontSize, TextAlignmentOptions align,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.enableAutoSizing = false;
            var rt = tmp.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return tmp;
        }

        static GameObject CreatePanel(Transform parent, string name, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            return go;
        }

        static Button CreateButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 48;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            var txtRt = tmp.rectTransform;
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            return btn;
        }

        static void SetField(object target, string fieldName, object value)
        {
            var so = new SerializedObject((Object)target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[BurakOyun] '{fieldName}' alanı bulunamadı: {target.GetType().Name}");
                return;
            }
            if (value is Object obj)
                prop.objectReferenceValue = obj;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
