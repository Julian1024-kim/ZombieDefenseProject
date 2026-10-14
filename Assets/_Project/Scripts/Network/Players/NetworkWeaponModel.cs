// 플레이어의 네트워크 무기 배열 중 한 칸을 IWeaponModel로 보여주는 어댑터.

using ZD.Gameplay.Weapons;

namespace ZD.Network.Players
{
    public sealed class NetworkWeaponModel : IWeaponModel
    {
        private readonly PlayerLoadoutNetwork _owner;
        private readonly int _slotIndex;

        public NetworkWeaponModel(PlayerLoadoutNetwork owner, int slotIndex)
        {
            _owner = owner;
            _slotIndex = slotIndex;
        }

        public int AmmoInMagazine
        {
            get => _owner.GetSlotState(_slotIndex).AmmoInMagazine;
            set
            {
                WeaponSlotState state = _owner.GetSlotState(_slotIndex);
                state.AmmoInMagazine = value;
                _owner.SetSlotState(_slotIndex, state);
            }
        }

        public int ReserveAmmo
        {
            get => _owner.GetSlotState(_slotIndex).ReserveAmmo;
            set
            {
                WeaponSlotState state = _owner.GetSlotState(_slotIndex);
                state.ReserveAmmo = value;
                _owner.SetSlotState(_slotIndex, state);
            }
        }

        public int NextFireTick
        {
            get => _owner.GetSlotState(_slotIndex).NextFireTick;
            set
            {
                WeaponSlotState state = _owner.GetSlotState(_slotIndex);
                state.NextFireTick = value;
                _owner.SetSlotState(_slotIndex, state);
            }
        }

        public int ReloadEndTick
        {
            get => _owner.GetSlotState(_slotIndex).ReloadEndTick;
            set
            {
                WeaponSlotState state = _owner.GetSlotState(_slotIndex);
                state.ReloadEndTick = value;
                _owner.SetSlotState(_slotIndex, state);
            }
        }
    }
}
