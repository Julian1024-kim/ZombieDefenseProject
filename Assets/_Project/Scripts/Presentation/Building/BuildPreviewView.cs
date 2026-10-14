// 설치 모드의 반투명 미리보기.
// 고른 종류의 고스트 하나만 보이고, 설치 가능하면 초록, 아니면 빨강 머티리얼(공유)로 바꾼다.
// 고스트 순서는 설치 카탈로그 순서와 같다.

using R3;
using UnityEngine;
using VContainer;
using ZD.Gameplay.Building;

namespace ZD.Presentation.Building
{
    public sealed class BuildPreviewView : MonoBehaviour
    {
        [SerializeField] private GameObject[] _ghosts;
        [SerializeField] private Material _validMaterial;
        [SerializeField] private Material _invalidMaterial;

        private readonly CompositeDisposable _disposables = new();
        private BuildModeModel _model;
        private Renderer[] _renderers;
        private Material[][] _validSlots;
        private Material[][] _invalidSlots;

        [Inject]
        public void Construct(BuildModeModel model)
        {
            _model = model;
        }

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _validSlots = new Material[_renderers.Length][];
            _invalidSlots = new Material[_renderers.Length][];
            for (int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                int slotCount = _renderers[rendererIndex].sharedMaterials.Length;
                _validSlots[rendererIndex] = Fill(slotCount, _validMaterial);
                _invalidSlots[rendererIndex] = Fill(slotCount, _invalidMaterial);
            }

            for (int ghostIndex = 0; ghostIndex < _ghosts.Length; ghostIndex++)
            {
                _ghosts[ghostIndex].SetActive(false);
            }
        }

        private void Start()
        {
            Observable.Merge(_model.IsActive.AsUnitObservable(), _model.SelectedIndex.AsUnitObservable())
                .Subscribe(_ => RefreshVisibility())
                .AddTo(_disposables);

            _model.PreviewResult
                .Subscribe(result => ApplyMaterials(result == PlacementResult.Valid ? _validSlots : _invalidSlots))
                .AddTo(_disposables);

            _model.PreviewPose
                .Subscribe(pose => transform.SetPositionAndRotation(pose.position, pose.rotation))
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private void RefreshVisibility()
        {
            bool isActive = _model.IsActive.Value;
            int selected = _model.SelectedIndex.Value;
            for (int ghostIndex = 0; ghostIndex < _ghosts.Length; ghostIndex++)
            {
                _ghosts[ghostIndex].SetActive(isActive && ghostIndex == selected);
            }
        }

        private void ApplyMaterials(Material[][] slots)
        {
            for (int rendererIndex = 0; rendererIndex < _renderers.Length; rendererIndex++)
            {
                _renderers[rendererIndex].sharedMaterials = slots[rendererIndex];
            }
        }

        private static Material[] Fill(int count, Material material)
        {
            var slots = new Material[count];
            for (int slotIndex = 0; slotIndex < count; slotIndex++)
            {
                slots[slotIndex] = material;
            }

            return slots;
        }
    }
}
