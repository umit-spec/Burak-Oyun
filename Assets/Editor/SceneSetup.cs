using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

namespace BurakOyun.Editor
{
    public static class SceneSetup
    {
        [MenuItem("BurakOyun/1 — Asset'leri Oluştur (Config + WordData)", priority = 1)]
        public static void CreateAssets()
        {
            EnsureFolder("Assets/Data");

            // GameConfig
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Data/GameConfig.asset") == null)
            {
                var gc = ScriptableObject.CreateInstance<Data.GameConfig>();
                AssetDatabase.CreateAsset(gc, "Assets/Data/GameConfig.asset");
                Debug.Log("[BurakOyun] GameConfig oluşturuldu.");
            }

            // WordData
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Data/WordData_BURAK.asset") == null)
            {
                var wd = ScriptableObject.CreateInstance<Data.WordData>();
                wd.word = "BURAK";
                wd.letters = new Data.LetterAudioEntry[]
                {
                    new() { letter = "B" },
                    new() { letter = "U" },
                    new() { letter = "R" },
                    new() { letter = "A" },
                    new() { letter = "K" },
                };
                AssetDatabase.CreateAsset(wd, "Assets/Data/WordData_BURAK.asset");
                Debug.Log("[BurakOyun] WordData_BURAK oluşturuldu.");
            }

            // Çoklu kelime assetleri
            string[] extraWords = { "ANNE", "BABA", "KEDI", "ELMA", "OKUL", "ARABA", "BALIK", "KALEM", "KITAP" };
            foreach (var w in extraWords)
            {
                string wpath = $"Assets/Data/WordData_{w}.asset";
                if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(wpath) == null)
                {
                    var wd = ScriptableObject.CreateInstance<Data.WordData>();
                    wd.word = w;
                    wd.letters = new Data.LetterAudioEntry[w.Length];
                    for (int i = 0; i < w.Length; i++)
                        wd.letters[i] = new Data.LetterAudioEntry { letter = w[i].ToString() };
                    AssetDatabase.CreateAsset(wd, wpath);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[BurakOyun] ✓ Tüm kelime assetleri hazır (BURAK + 9 kelime).");
        }

        [MenuItem("BurakOyun/2 — Letter Prefab Oluştur", priority = 2)]
        public static void CreateLetterPrefab()
        {
            EnsureFolder("Assets/Prefabs");
            string path = "Assets/Prefabs/Letter.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("[BurakOyun] Letter prefab zaten var.");
                return;
            }

            var go = new GameObject("Letter");

            // 3D arka plan küp (harf tahtası)
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "Board";
            board.transform.SetParent(go.transform);
            board.transform.localScale = new Vector3(1.5f, 1.5f, 0.3f);
            board.transform.localPosition = Vector3.zero;
            var boardMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            boardMat.color = new Color(0.3f, 0.7f, 1f);
            board.GetComponent<Renderer>().sharedMaterial = boardMat;
            board.AddComponent<Gameplay.LetterGlow>();
            AssetDatabase.CreateAsset(boardMat, "Assets/Prefabs/LetterBoardMat.mat");

            // Collider: board'un collider'ını trigger yap
            board.GetComponent<BoxCollider>().isTrigger = true;
            // Ana objeye collider taşı
            Object.DestroyImmediate(board.GetComponent<BoxCollider>());
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1.8f, 1.8f, 0.8f);

            // TextMeshPro 3D label
            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform);
            textGo.transform.localPosition = new Vector3(0f, 0f, -0.2f);
            textGo.transform.localScale = Vector3.one;
            var tmp = textGo.AddComponent<TextMeshPro>();
            tmp.text = "A";
            tmp.fontSize = 8;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableAutoSizing = false;
            tmp.rectTransform.sizeDelta = new Vector2(2f, 2f);

            // LetterCollectible script
            var lc = go.AddComponent<Gameplay.LetterCollectible>();
            // label field'ını bağla (SerializeField reflection ile)
            var so = new SerializedObject(lc);
            so.FindProperty("label").objectReferenceValue = tmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log("[BurakOyun] ✓ Letter prefab oluşturuldu.");
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
            Debug.Log("[BurakOyun] ✓ URP pipeline oluşturuldu ve Graphics/Quality'ye atandı. Magenta materyaller artık düzgün render olur.");
            return urp;
        }

        static void AssignPipeline(RenderPipelineAsset urp)
        {
            GraphicsSettings.defaultRenderPipeline = urp;
            QualitySettings.renderPipeline = urp;
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
            // TMP Essential Resources kontrolü — font asset yoksa label'lar bozuk görünür
            string tmpFontDir = System.IO.Path.Combine(Application.dataPath,
                "TextMesh Pro", "Resources", "Fonts & Materials");
            bool tmpReady = System.IO.Directory.Exists(tmpFontDir)
                && System.IO.Directory.GetFiles(tmpFontDir, "*.asset").Length > 0;
            if (!tmpReady && !EditorUtility.DisplayDialog(
                "TMP Essential Resources Eksik",
                "TextMeshPro Essential Resources henüz import edilmemiş.\n\n" +
                "Harf label'ları bozuk görünebilir.\n\n" +
                "Önce: Window → TextMeshPro → Import TMP Essential Resources\n\n" +
                "Yine de devam etmek istiyor musunuz?",
                "Devam Et", "İptal"))
                return;

            // Önce render pipeline'ı garanti et (yoksa her şey magenta)
            SetupURP();
            // Asset'lerin var olduğundan emin ol
            CreateAssets();
            CreateLetterPrefab();

            var config = AssetDatabase.LoadAssetAtPath<Data.GameConfig>("Assets/Data/GameConfig.asset");
            var wordData = AssetDatabase.LoadAssetAtPath<Data.WordData>("Assets/Data/WordData_BURAK.asset");
            var letterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Letter.prefab");

            // ── Zemin Tile'ları ──
            var trackParent = new GameObject("Track");
            var tileMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            tileMat.color = new Color(0.45f, 0.78f, 0.45f); // çimen yeşili
            EnsureFolder("Assets/Prefabs");
            AssetDatabase.CreateAsset(tileMat, "Assets/Prefabs/GroundMat.mat");

            Transform[] tiles = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
                tile.name = $"GroundTile_{i}";
                tile.transform.SetParent(trackParent.transform);
                tile.transform.localScale = new Vector3(1.5f, 1f, 3f); // ~30m uzunluk
                tile.transform.localPosition = new Vector3(0f, 0f, i * 30f);
                tile.GetComponent<Renderer>().sharedMaterial = tileMat;
                tile.layer = 0;
                tiles[i] = tile.transform;
            }

            // ── Şerit çizgileri (görsel yardım) ──
            var lineMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            lineMat.color = new Color(1f, 1f, 1f, 0.4f);
            AssetDatabase.CreateAsset(lineMat, "Assets/Prefabs/LineMat.mat");
            for (int lane = -1; lane <= 1; lane += 2)
            {
                float x = lane * config.laneWidth * 0.5f;
                for (int t = 0; t < 3; t++)
                {
                    var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    line.name = $"LaneLine_{lane}_{t}";
                    line.transform.SetParent(tiles[t]);
                    line.transform.localScale = new Vector3(0.015f, 0.002f, 1f);
                    line.transform.localPosition = new Vector3(x / tiles[t].lossyScale.x, 0.001f, 0f);
                    line.GetComponent<Renderer>().sharedMaterial = lineMat;
                    Object.DestroyImmediate(line.GetComponent<BoxCollider>());
                }
            }

            // ── Yılan (Snake) ──
            var snakeGo = new GameObject("Snake");
            snakeGo.tag = "Player"; // LetterCollectible bunu bekler

            // Gövde: kapsül
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(snakeGo.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var snakeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            snakeMat.color = new Color(0.2f, 0.85f, 0.3f); // sevimli yeşil
            body.GetComponent<Renderer>().sharedMaterial = snakeMat;
            AssetDatabase.CreateAsset(snakeMat, "Assets/Prefabs/SnakeMat.mat");
            Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());
            // Sevimli bob animasyonu görsel gövdede — kök hareketiyle (SnakeController) çakışmaz
            body.AddComponent<Gameplay.SnakeBob>();
            body.AddComponent<Gameplay.SnakeTrail>();

            // Gözler
            for (int side = -1; side <= 1; side += 2)
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = side < 0 ? "EyeL" : "EyeR";
                eye.transform.SetParent(snakeGo.transform);
                eye.transform.localScale = Vector3.one * 0.25f;
                eye.transform.localPosition = new Vector3(side * 0.2f, 0.2f, 0.35f);
                var eyeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                eyeMat.color = Color.white;
                eye.GetComponent<Renderer>().sharedMaterial = eyeMat;
                Object.DestroyImmediate(eye.GetComponent<SphereCollider>());
                if (side < 0) AssetDatabase.CreateAsset(eyeMat, "Assets/Prefabs/EyeMat.mat");
                else eye.GetComponent<Renderer>().sharedMaterial =
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/Prefabs/EyeMat.mat");

                var pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pupil.name = "Pupil";
                pupil.transform.SetParent(eye.transform);
                pupil.transform.localScale = Vector3.one * 0.5f;
                pupil.transform.localPosition = new Vector3(0f, 0f, -0.4f);
                var pupilMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                pupilMat.color = Color.black;
                pupil.GetComponent<Renderer>().sharedMaterial = pupilMat;
                Object.DestroyImmediate(pupil.GetComponent<SphereCollider>());
                if (side < 0) AssetDatabase.CreateAsset(pupilMat, "Assets/Prefabs/PupilMat.mat");
                else pupil.GetComponent<Renderer>().sharedMaterial =
                    AssetDatabase.LoadAssetAtPath<Material>("Assets/Prefabs/PupilMat.mat");
            }

            // Snake collider (trigger toplama için)
            var snakeCol = snakeGo.AddComponent<SphereCollider>();
            snakeCol.radius = 0.6f;
            snakeCol.isTrigger = false;

            // Rigidbody (kinematic — fizik tepkisi istemiyoruz ama trigger algılama için gerekli)
            var rb = snakeGo.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            snakeGo.transform.position = new Vector3(0f, 0.5f, 5f);

            // Snake scriptleri
            var laneInput = snakeGo.AddComponent<Gameplay.LaneInput>();
            var snakeCtrl = snakeGo.AddComponent<Gameplay.SnakeController>();
            SetField(snakeCtrl, "config", config);

            // ── Kamera ──
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }
            var camFollow = cam.gameObject.AddComponent<Gameplay.CameraFollow>();
            SetField(camFollow, "target", snakeGo.transform);
            cam.transform.position = new Vector3(0f, 6f, -3f);

            // ── WordManager ──
            var managers = new GameObject("GameManagers");

            var wordMgr = managers.AddComponent<Gameplay.WordManager>();
            SetField(wordMgr, "wordData", wordData);

            // Kelime listesini yükle ve ata
            var allWordNames = new[] { "BURAK","ANNE","BABA","KEDI","ELMA","OKUL","ARABA","BALIK","KALEM","KITAP" };
            var allWords = new System.Collections.Generic.List<Data.WordData>();
            foreach (var name in allWordNames)
            {
                var wd = AssetDatabase.LoadAssetAtPath<Data.WordData>($"Assets/Data/WordData_{name}.asset");
                if (wd != null) allWords.Add(wd);
            }
            SetWordList(wordMgr, allWords.ToArray());

            // ── LetterSpawner ──
            var spawnRoot = new GameObject("LetterSpawnRoot");
            var spawner = spawnRoot.AddComponent<Gameplay.LetterSpawner>();
            SetField(spawner, "config", config);
            SetField(spawner, "wordManager", wordMgr);
            SetField(spawner, "snake", snakeCtrl);
            var letterPrefabComp = letterPrefab.GetComponent<Gameplay.LetterCollectible>();
            SetField(spawner, "letterPrefab", letterPrefabComp);

            // ── TrackRecycler ──
            var recycler = trackParent.AddComponent<Gameplay.TrackRecycler>();
            SetField(recycler, "snake", snakeGo.transform);
            SetField(recycler, "tiles", tiles);

            // ── RewardManager ──
            var rewardMgr = managers.AddComponent<Core.RewardManager>();
            SetField(rewardMgr, "wordManager", wordMgr);
            SetField(rewardMgr, "snake", snakeGo.transform);

            // ── Konfeti VFX ──
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
            confettiGo.transform.position = new Vector3(0f, 3f, 5f);
            var confettiMat = CreateUrpParticleMaterial();
            AssetDatabase.CreateAsset(confettiMat, "Assets/Prefabs/ParticleMat.mat");
            confettiGo.GetComponent<ParticleSystemRenderer>().sharedMaterial = confettiMat;
            SetField(rewardMgr, "confetti", confettiPS);

            // Collect sparkle
            var sparkleGo = new GameObject("CollectSparkle");
            var sparklePS = sparkleGo.AddComponent<ParticleSystem>();
            var sparkMain = sparklePS.main;
            sparkMain.startLifetime = 0.6f;
            sparkMain.startSpeed = 3f;
            sparkMain.startSize = 0.15f;
            sparkMain.maxParticles = 20;
            sparkMain.startColor = new Color(1f, 0.95f, 0.4f);
            sparkMain.playOnAwake = false;
            var sparkEmission = sparklePS.emission;
            sparkEmission.rateOverTime = 0;
            sparkEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15) });
            sparkleGo.GetComponent<ParticleSystemRenderer>().sharedMaterial =
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Prefabs/ParticleMat.mat");
            SetField(rewardMgr, "collectSparkle", sparklePS);

            // ── AudioManager ──
            var audioMgr = managers.AddComponent<Audio.AudioManager>();
            SetField(audioMgr, "wordManager", wordMgr);
            var sfxSrc = managers.AddComponent<AudioSource>();
            sfxSrc.playOnAwake = false;
            var voiceSrc = managers.gameObject.AddComponent<AudioSource>();
            voiceSrc.playOnAwake = false;
            var musicSrc = managers.gameObject.AddComponent<AudioSource>();
            musicSrc.playOnAwake = false;
            musicSrc.loop = true;
            musicSrc.volume = 0.3f;
            SetField(audioMgr, "sfxSource", sfxSrc);
            SetField(audioMgr, "voiceSource", voiceSrc);
            SetField(audioMgr, "musicSource", musicSrc);

            // ── UI Canvas ──
            var canvas = CreateUICanvas(wordMgr, rewardMgr, out var startPanel, out var completePanel,
                out var progressTxt, out var starsTxt, out var feedbackTxt);

            // ── UIManager ──
            var uiMgr = canvas.gameObject.AddComponent<UI.UIManager>();
            SetField(uiMgr, "wordManager", wordMgr);
            SetField(uiMgr, "rewardManager", rewardMgr);
            SetField(uiMgr, "progressText", progressTxt);
            SetField(uiMgr, "starsText", starsTxt);
            SetField(uiMgr, "feedbackText", feedbackTxt);
            SetField(uiMgr, "startPanel", startPanel);
            SetField(uiMgr, "completePanel", completePanel);

            // ── GameManager ──
            var gameMgr = managers.AddComponent<Core.GameManager>();
            SetField(gameMgr, "snake", snakeCtrl);
            SetField(gameMgr, "spawner", spawner);
            SetField(gameMgr, "wordManager", wordMgr);
            SetField(gameMgr, "rewardManager", rewardMgr);
            SetField(gameMgr, "ui", uiMgr);

            // Butonları bağla
            var startBtn = startPanel.GetComponentInChildren<Button>();
            var replayBtn = completePanel.GetComponentInChildren<Button>();
            if (startBtn != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(
                    startBtn.onClick, gameMgr.StartGame);
            }
            if (replayBtn != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(
                    replayBtn.onClick, gameMgr.Replay);
            }

            // ── Işık ──
            if (Object.FindFirstObjectByType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.color = new Color(1f, 0.97f, 0.9f);
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // ── Skybox / Arka plan rengi ──
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.98f); // açık gökyüzü mavisi

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(managers);

            // Sahneyi diske kaydet + Build Settings'e ekle (yoksa yeniden açılışta kaybolur)
            EnsureFolder("Assets/Scenes");
            const string scenePath = "Assets/Scenes/Game.unity";
            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(active, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };

            Debug.Log("[BurakOyun] ✓ Sahne kuruldu ve 'Assets/Scenes/Game.unity' olarak kaydedildi (Build Settings'e eklendi). Play'e bas, OYNA'ya tıkla, ok tuşlarıyla oyna.");
        }

        static GameObject CreateUICanvas(Gameplay.WordManager wordMgr,
            Core.RewardManager rewardMgr,
            out GameObject startPanel, out GameObject completePanel,
            out TMP_Text progressTxt, out TMP_Text starsTxt, out TMP_Text feedbackTxt)
        {
            // Canvas
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

            // Progress text (üst orta)
            progressTxt = CreateTMPText(canvasGo.transform, "ProgressText",
                "B U R A K", 72, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f),
                new Vector2(800f, 100f));

            // Stars text (sol üst)
            starsTxt = CreateTMPText(canvasGo.transform, "StarsText",
                "★ 0", 48, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f),
                new Vector2(200f, 80f));
            starsTxt.color = new Color(1f, 0.85f, 0.1f);

            // Feedback text (orta)
            feedbackTxt = CreateTMPText(canvasGo.transform, "FeedbackText",
                "", 64, TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 50f),
                new Vector2(600f, 100f));
            feedbackTxt.color = new Color(1f, 0.5f, 0.1f);

            // ── Start Panel ──
            startPanel = CreatePanel(canvasGo.transform, "StartPanel",
                new Color(0f, 0f, 0f, 0.5f));
            CreateTMPText(startPanel.transform, "Title", "BURAK\nHarf Oyunu", 80,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), Vector2.zero,
                new Vector2(800f, 250f));
            CreateButton(startPanel.transform, "BtnPlay", "OYNA",
                new Vector2(0.5f, 0.35f), new Vector2(350f, 120f),
                new Color(0.2f, 0.8f, 0.3f));

            // ── Complete Panel ──
            completePanel = CreatePanel(canvasGo.transform, "CompletePanel",
                new Color(0f, 0f, 0f, 0.5f));
            CreateTMPText(completePanel.transform, "CompleteTitle", "HARİKA!\nBURAK", 80,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), Vector2.zero,
                new Vector2(800f, 250f));
            CreateButton(completePanel.transform, "BtnReplay", "TEKRAR OYNA",
                new Vector2(0.5f, 0.35f), new Vector2(400f, 120f),
                new Color(0.3f, 0.6f, 1f));
            completePanel.SetActive(false);

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

            // Buton üstüne yazı
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

        static void SetWordList(Gameplay.WordManager wm, Data.WordData[] words)
        {
            var so = new SerializedObject(wm);
            var prop = so.FindProperty("wordList");
            if (prop == null) { Debug.LogWarning("[BurakOyun] wordList alanı bulunamadı."); return; }
            prop.arraySize = words.Length;
            for (int i = 0; i < words.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = words[i];
            so.ApplyModifiedPropertiesWithoutUndo();
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
            else if (value is Transform[] transforms)
            {
                prop.arraySize = transforms.Length;
                for (int i = 0; i < transforms.Length; i++)
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = transforms[i];
            }
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
