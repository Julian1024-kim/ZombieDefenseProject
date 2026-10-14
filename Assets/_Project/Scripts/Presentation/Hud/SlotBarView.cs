// 1~5번 키와 각 슬롯에 든 것을 보여주고, 선택된 슬롯을 강조한다.

using R3;
using TMPro;
using UnityEngine;
using VContainer;
using ZD.Gameplay.Hud;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Players;
using ZD.Gameplay.Weapons;

namespace ZD.Presentation.Hud
{
    public sealed class SlotBarView : MonoBehaviour
    {
        private const string EMPTY = "-";

        private static readonly Color SelectedColor = new(1f, 0.85f, 0.35f);
        private static readonly Color IdleColor = new(1f, 1f, 1f, 0.6f);

        [SerializeField] private TMP_Text _primaryText;
        [SerializeField] private TMP_Text _secondaryText;
        [SerializeField] private TMP_Text _meleeText;
        [SerializeField] private TMP_Text _healText;
        [SerializeField] private TMP_Text _throwableText;

        private readonly CompositeDisposable _disposables = new();
        private LocalPlayerHudModel _model;
        private WeaponCatalog _catalog;

        [Inject]
        public void Construct(LocalPlayerHudModel model, WeaponCatalog catalog)
        {
            _model = model;
            _catalog = catalog;
        }

        private void Start()
        {
            _throwableText.SetText("5  -");
            _throwableText.color = IdleColor;

            Observable.Merge(
                    _model.SelectedSlot.AsUnitObservable(),
                    _model.ActivePrimary.AsUnitObservable(),
                    _model.PrimaryAId.AsUnitObservable(),
                    _model.PrimaryBId.AsUnitObservable(),
                    _model.SecondaryId.AsUnitObservable(),
                    _model.MeleeId.AsUnitObservable(),
                    _model.HealKits.AsUnitObservable())
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private void Refresh()
        {
            byte activePrimaryId = _model.ActivePrimary.Value == InventorySlots.PRIMARY_A ? _model.PrimaryAId.Value : _model.PrimaryBId.Value;
            byte otherPrimaryId = _model.ActivePrimary.Value == InventorySlots.PRIMARY_A ? _model.PrimaryBId.Value : _model.PrimaryAId.Value;
            string otherPrimary = otherPrimaryId == WeaponCatalog.NO_WEAPON ? string.Empty : $" / {NameOf(otherPrimaryId)}";
            _primaryText.SetText($"1  {NameOf(activePrimaryId)}{otherPrimary}");
            _secondaryText.SetText($"2  {NameOf(_model.SecondaryId.Value)}");
            _meleeText.SetText($"3  {NameOf(_model.MeleeId.Value)}");
            _healText.SetText("4  HEAL x{0}", _model.HealKits.Value);

            WeaponSlot selected = _model.SelectedSlot.Value;
            _primaryText.color = selected == WeaponSlot.Primary ? SelectedColor : IdleColor;
            _secondaryText.color = selected == WeaponSlot.Secondary ? SelectedColor : IdleColor;
            _meleeText.color = selected == WeaponSlot.Melee ? SelectedColor : IdleColor;
            _healText.color = selected == WeaponSlot.Healing ? SelectedColor : IdleColor;
        }

        private string NameOf(byte weaponId)
        {
            WeaponDefinition weapon = _catalog.Get(weaponId);
            return weapon == null ? EMPTY : weapon.DisplayName;
        }
    }
}
