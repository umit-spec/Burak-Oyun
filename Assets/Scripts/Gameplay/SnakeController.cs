using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BurakOyun.Data;

namespace BurakOyun.Gameplay
{
    /// <summary>
    /// Izgara tabanlı klasik yılan: her tick bir hücre ilerler, yem yiyince büyür,
    /// duvara/kendine çarpınca OnDied yayınlar.
    /// Görsel segmentler havuzlanır — uzun oyunlarda Instantiate/Destroy döngüsü yok.
    /// Görsel prefab'lar boş bırakılırsa salt-mantık modunda çalışır (testler).
    /// </summary>
    [RequireComponent(typeof(DirectionInput))]
    public class SnakeController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private FoodSpawner foodSpawner;

        [Header("Görseller (boş = salt-mantık modu)")]
        [SerializeField] private GameObject headPrefab;
        [SerializeField] private GameObject segmentPrefab;
        [SerializeField] private float visualHeight = 0.5f;

        [Header("Ölüm geri bildirimi (çocuk dostu, kısa)")]
        [SerializeField] private float hurtDuration = 0.35f;
        [SerializeField] private float hurtShake = 0.12f;
        [SerializeField] private Color hurtFlashColor = Color.white;

        [Header("Gövde gradyan renk (baş→kuyruk)")]
        [SerializeField] private Color bodyHeadColor = new Color(0.25f, 0.82f, 0.45f);
        [SerializeField] private Color bodyTailColor = new Color(0.15f, 0.62f, 0.72f);

        public event Action OnAteFood;
        public event Action OnDied;

        public GridMovement Grid { get; private set; }
        public SnakeBody Body { get; private set; }
        public Direction CurrentDirection { get; private set; } = Direction.Right;
        public bool IsMoving { get; set; }
        /// <summary>Yedikçe kısalan tick süresi (yumuşak hızlanma).</summary>
        public float CurrentTickRate { get; private set; }
        public Vector3 HeadWorldPosition => Grid.CellToWorld(Body.HeadPosition, config.cellSize, visualHeight);

        private DirectionInput input;
        private float tickTimer;
        private GameObject headVisual;
        private readonly List<GameObject> bodyVisuals = new();
        private readonly List<Renderer> bodyRenderers = new();

        // Tick'ler arası yumuşatma (Lerp): görsel listesi + kaynak/hedef dünya konumları.
        // Listeler yeniden kullanılır (Clear+Add) → kararlı durumda kare-başı tahsis yok.
        private readonly List<Transform> visualTr = new();
        private readonly List<Vector3> fromPos = new();
        private readonly List<Vector3> toPos = new();
        private int activeBodyCount;

        private MaterialPropertyBlock mpb;
        private Renderer headRenderer;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            input = GetComponent<DirectionInput>();
            Grid = new GridMovement(config.gridWidth, config.gridHeight);
            Body = new SnakeBody();
            ResetSnake();
        }

        /// <summary>Yılanı tahta ortasına, sağa bakar şekilde başlangıç uzunluğunda kurar.</summary>
        public void ResetSnake()
        {
            var center = new Vector2Int(config.gridWidth / 2, config.gridHeight / 2);
            CurrentDirection = Direction.Right;
            // Gövde sola uzanır; tahtadan taşmasın
            int length = Mathf.Clamp(config.initialLength, 1, center.x + 1);
            Body.Reset(center, CurrentDirection, length);
            CurrentTickRate = config.tickRate;
            tickTimer = 0f;
            input.ClearQueue();
            activeBodyCount = 0;
            StopAllCoroutines();          // varsa süren ölüm feedback'ini durdur
            RefreshVisualTargets(true);   // baştan kurarken kaymadan hedeflere otur
        }

        private void Update()
        {
            if (!IsMoving) return;        // pause/ölüm → tick de Lerp de durur
            tickTimer += Time.deltaTime;
            if (tickTimer >= CurrentTickRate)
            {
                tickTimer -= CurrentTickRate;
                Step();
            }
            InterpolateVisuals();
        }

        /// <summary>Bir hücre ilerletir. Update tick'i çağırır; testler doğrudan çağırabilir.</summary>
        public void Step()
        {
            if (input.TryGetNextDirection(CurrentDirection, out Direction next))
                CurrentDirection = next;

            Vector2Int nextPos = Grid.GetNextPosition(Body.HeadPosition, CurrentDirection);
            bool hasFood = foodSpawner != null && foodSpawner.HasFood;
            Vector2Int foodPos = hasFood ? foodSpawner.Position : default;

            switch (CollisionManager.Check(Grid, Body, nextPos, hasFood, foodPos))
            {
                case CollisionManager.CollisionType.Wall:
                case CollisionManager.CollisionType.Self:
                    IsMoving = false;
                    OnDied?.Invoke();
                    return;

                case CollisionManager.CollisionType.Food:
                    Body.AdvanceHead(nextPos); // kuyruk silinmez → büyüme
                    CurrentTickRate = Mathf.Max(config.minTickRate, CurrentTickRate - config.speedUpPerFood);
                    OnAteFood?.Invoke();
                    break;

                default:
                    Body.AdvanceHead(nextPos);
                    Body.RemoveTail();
                    break;
            }

            RefreshVisualTargets(false); // yeni tick: kaynak = mevcut konum, hedef = yeni hücre
        }

        // ── Görseller (havuzlu + Lerp) ──
        // Izgara modeli (Body) tamamen ayrık kalır; burada yalnız görsel temsil
        // tick'ler arası hedef hücreye akıtılır. Salt-mantık modunda (prefab yok) atlanır.
        private void RefreshVisualTargets(bool snap)
        {
            if (segmentPrefab == null) return; // salt-mantık modu (testler)

            if (headVisual == null && headPrefab != null)
                headVisual = Instantiate(headPrefab, transform);

            int needed = Body.Length - 1; // baş hariç gövde segmenti sayısı
            while (bodyVisuals.Count < needed)
            {
                var seg = Instantiate(segmentPrefab, transform);
                bodyVisuals.Add(seg);
                bodyRenderers.Add(seg.GetComponentInChildren<Renderer>());
            }

            visualTr.Clear(); fromPos.Clear(); toPos.Clear();

            // Baş
            if (headVisual != null)
            {
                Vector3 target = Grid.CellToWorld(Body.HeadPosition, config.cellSize, visualHeight);
                Vector3 source = snap ? target : headVisual.transform.position;
                Push(headVisual.transform, source, target, snap);
                Vector2Int f = GridMovement.ToVector(CurrentDirection);
                headVisual.transform.rotation = Quaternion.LookRotation(new Vector3(f.x, 0f, f.y));
            }

            // Gövde
            for (int i = 0; i < bodyVisuals.Count; i++)
            {
                bool active = i < needed;
                bodyVisuals[i].SetActive(active);
                if (!active) continue;
                Vector3 target = Grid.CellToWorld(Body.Segments[i + 1], config.cellSize, visualHeight);
                // Bu tick yeni aktifleşen segment (büyüme) kaymadan hedefe otursun
                bool isNew = i >= activeBodyCount;
                Vector3 source = (snap || isNew) ? target : bodyVisuals[i].transform.position;
                Push(bodyVisuals[i].transform, source, target, snap || isNew);
                ApplyBodyGradient(i, needed);
            }

            activeBodyCount = needed;
        }

        // Segment indeksine göre baş→kuyruk gradyan rengi (MaterialPropertyBlock — paylaşılan
        // materyali kirletmez, tahsis yapmaz). Salt-mantık modunda hiç çağrılmaz.
        private void ApplyBodyGradient(int index, int count)
        {
            Renderer r = bodyRenderers[index];
            if (r == null) return;
            if (mpb == null) mpb = new MaterialPropertyBlock();
            float g = count > 1 ? (float)index / (count - 1) : 0f;
            r.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, Color.Lerp(bodyHeadColor, bodyTailColor, g));
            r.SetPropertyBlock(mpb);
        }

        private void Push(Transform tr, Vector3 source, Vector3 target, bool placeNow)
        {
            visualTr.Add(tr); fromPos.Add(source); toPos.Add(target);
            if (placeNow) tr.position = target;
        }

        private void InterpolateVisuals()
        {
            int n = visualTr.Count;
            if (n == 0) return;
            float t = CurrentTickRate > 0f ? Mathf.Clamp01(tickTimer / CurrentTickRate) : 1f;
            for (int k = 0; k < n; k++)
                visualTr[k].position = Vector3.Lerp(fromPos[k], toPos[k], t);
        }

        /// <summary>
        /// Ölümde kısa, çocuk dostu geri bildirim: baş hafifçe sarsılır + beyaza flaşlar.
        /// Korkutucu değil; salt-mantık modunda (baş görseli yok) sessizce hiçbir şey yapmaz.
        /// </summary>
        public void PlayHurtFeedback()
        {
            if (headVisual == null) return;
            StopCoroutine(nameof(HurtRoutine));
            StartCoroutine(nameof(HurtRoutine));
        }

        private IEnumerator HurtRoutine()
        {
            if (headRenderer == null)
            {
                var cube = headVisual.transform.Find("Cube");
                headRenderer = cube != null ? cube.GetComponent<Renderer>()
                                            : headVisual.GetComponentInChildren<Renderer>();
            }
            if (mpb == null) mpb = new MaterialPropertyBlock();

            Vector3 basePos = headVisual.transform.position;
            Color baseColor = headRenderer != null ? headRenderer.sharedMaterial.color : Color.white;

            float t = 0f;
            while (t < hurtDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(1f - t / hurtDuration); // sönümlenen genlik
                Vector3 jitter = new Vector3(
                    Mathf.PerlinNoise(t * 40f, 0f) - 0.5f,
                    0f,
                    Mathf.PerlinNoise(0f, t * 40f) - 0.5f) * (hurtShake * k);
                headVisual.transform.position = basePos + jitter;
                if (headRenderer != null)
                {
                    headRenderer.GetPropertyBlock(mpb);
                    mpb.SetColor(BaseColorId, Color.Lerp(baseColor, hurtFlashColor, k));
                    headRenderer.SetPropertyBlock(mpb);
                }
                yield return null;
            }

            headVisual.transform.position = basePos;
            if (headRenderer != null)
            {
                headRenderer.GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorId, baseColor);
                headRenderer.SetPropertyBlock(mpb);
            }
        }
    }
}
