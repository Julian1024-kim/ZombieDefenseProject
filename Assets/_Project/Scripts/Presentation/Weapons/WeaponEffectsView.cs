// 로컬 사격 피드백: 미리 만들어 둔 풀에서 총구 섬광, 탄 궤적, 착탄 불꽃을 꺼내 쓴다.
// 네트워크로 보내는 것도 없고 Awake 이후 새로 생성하는 것도 없다.

using System;
using MessagePipe;
using UnityEngine;
using VContainer;
using ZD.Core.Messages;

namespace ZD.Presentation.Weapons
{
    public sealed class WeaponEffectsView : MonoBehaviour
    {
        private const int TRACER_POOL_SIZE = 16;
        private const int SPARK_POOL_SIZE = 32;
        private const float TRACER_SECONDS = 0.06f;
        private const float SPARK_SECONDS = 0.05f;
        private const float TRACER_WIDTH = 0.025f;
        private const float MUZZLE_FLASH_SIZE = 0.18f;
        private const float IMPACT_SPARK_SIZE = 0.1f;

        [SerializeField] private Material _tracerMaterial;
        [SerializeField] private Material _sparkMaterial;
        [Tooltip("What a tracer stops at visually. The host's lag-compensated hit decides damage, not this.")]
        [SerializeField] private LayerMask _visualHitMask = 1;

        private readonly LineRenderer[] _tracers = new LineRenderer[TRACER_POOL_SIZE];
        private readonly float[] _tracerTimeLeft = new float[TRACER_POOL_SIZE];
        private readonly Transform[] _sparks = new Transform[SPARK_POOL_SIZE];
        private readonly float[] _sparkTimeLeft = new float[SPARK_POOL_SIZE];
        private readonly float[] _sparkSize = new float[SPARK_POOL_SIZE];
        private int _nextTracer;
        private int _nextSpark;
        private ISubscriber<WeaponFiredMessage> _weaponFiredSubscriber;
        private IDisposable _subscription;

        private void Awake()
        {
            for (int tracerIndex = 0; tracerIndex < TRACER_POOL_SIZE; tracerIndex++)
            {
                _tracers[tracerIndex] = CreateTracer(tracerIndex);
            }

            for (int sparkIndex = 0; sparkIndex < SPARK_POOL_SIZE; sparkIndex++)
            {
                _sparks[sparkIndex] = CreateSpark(sparkIndex);
            }
        }

        [Inject]
        public void Construct(ISubscriber<WeaponFiredMessage> weaponFiredSubscriber)
        {
            _weaponFiredSubscriber = weaponFiredSubscriber;
        }

        private void Start()
        {
            _subscription = _weaponFiredSubscriber.Subscribe(OnWeaponFired);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int tracerIndex = 0; tracerIndex < TRACER_POOL_SIZE; tracerIndex++)
            {
                if (_tracerTimeLeft[tracerIndex] <= 0f)
                {
                    continue;
                }

                _tracerTimeLeft[tracerIndex] -= deltaTime;
                if (_tracerTimeLeft[tracerIndex] <= 0f)
                {
                    _tracers[tracerIndex].enabled = false;
                }
            }

            for (int sparkIndex = 0; sparkIndex < SPARK_POOL_SIZE; sparkIndex++)
            {
                if (_sparkTimeLeft[sparkIndex] <= 0f)
                {
                    continue;
                }

                _sparkTimeLeft[sparkIndex] -= deltaTime;
                float scale = Mathf.Max(0f, _sparkTimeLeft[sparkIndex] / SPARK_SECONDS) * _sparkSize[sparkIndex];
                _sparks[sparkIndex].localScale = new Vector3(scale, scale, scale);
                if (_sparkTimeLeft[sparkIndex] <= 0f)
                {
                    _sparks[sparkIndex].gameObject.SetActive(false);
                }
            }
        }

        private void OnDestroy() => _subscription?.Dispose();

        private void OnWeaponFired(WeaponFiredMessage message)
        {
            if (message.IsMelee)
            {
                return;
            }

            Vector3 end = message.AimOrigin + message.AimDirection * message.Range;
            bool hitSomething = Physics.Raycast(message.AimOrigin, message.AimDirection, out RaycastHit hit, message.Range, _visualHitMask, QueryTriggerInteraction.Ignore);
            if (hitSomething)
            {
                end = hit.point;
            }

            ShowTracer(message.MuzzlePosition, end);
            ShowSpark(message.MuzzlePosition, MUZZLE_FLASH_SIZE);
            if (hitSomething)
            {
                ShowSpark(end, IMPACT_SPARK_SIZE);
            }
        }

        private void ShowTracer(Vector3 start, Vector3 end)
        {
            LineRenderer tracer = _tracers[_nextTracer];
            tracer.SetPosition(0, start);
            tracer.SetPosition(1, end);
            tracer.enabled = true;
            _tracerTimeLeft[_nextTracer] = TRACER_SECONDS;
            _nextTracer = (_nextTracer + 1) % TRACER_POOL_SIZE;
        }

        private void ShowSpark(Vector3 position, float size)
        {
            Transform spark = _sparks[_nextSpark];
            spark.position = position;
            spark.localScale = new Vector3(size, size, size);
            spark.gameObject.SetActive(true);
            _sparkTimeLeft[_nextSpark] = SPARK_SECONDS;
            _sparkSize[_nextSpark] = size;
            _nextSpark = (_nextSpark + 1) % SPARK_POOL_SIZE;
        }

        private LineRenderer CreateTracer(int index)
        {
            var tracerObject = new GameObject($"Tracer_{index}");
            tracerObject.transform.SetParent(transform, false);
            LineRenderer line = tracerObject.AddComponent<LineRenderer>();
            line.sharedMaterial = _tracerMaterial;
            line.positionCount = 2;
            line.startWidth = TRACER_WIDTH;
            line.endWidth = TRACER_WIDTH;
            line.useWorldSpace = true;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private Transform CreateSpark(int index)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = $"Spark_{index}";
            Destroy(spark.GetComponent<Collider>());
            spark.transform.SetParent(transform, false);
            MeshRenderer sparkRenderer = spark.GetComponent<MeshRenderer>();
            sparkRenderer.sharedMaterial = _sparkMaterial;
            sparkRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            sparkRenderer.receiveShadows = false;
            spark.SetActive(false);
            return spark.transform;
        }
    }
}
