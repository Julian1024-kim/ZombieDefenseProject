// 플레이어 한 명의 복제되는 지갑, 인벤토리, 매치 통계(처치·헤드샷·부활).
// 상태만 갖고, 바꾸는 시스템은 PlayerNetwork와 StatsSystem이 돌린다.

using Fusion;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Players;
using ZD.Gameplay.Stats;
using ZD.Gameplay.Weapons;

namespace ZD.Network.Players
{
    [UnityEngine.DisallowMultipleComponent]
    public sealed class PlayerLoadoutNetwork : NetworkBehaviour, IInventoryModel, IWalletModel, IPlayerStatsModel
    {
        private NetworkWeaponModel[] _weaponModels;

        [Networked] public int Points { get; set; }
        [Networked] public int PointsEarned { get; set; }
        [Networked] public int Kills { get; set; }
        [Networked] public int Headshots { get; set; }
        [Networked] public int Revives { get; set; }
        [Networked] public WeaponSlot SelectedSlot { get; set; }
        [Networked] public WeaponSlot PreviousSlot { get; set; }
        [Networked] public int ActivePrimary { get; set; }
        [Networked] public int HealKits { get; set; }
        [Networked] public int HealEndTick { get; set; }
        [Networked, Capacity(InventorySlots.WEAPON_COUNT)] private NetworkArray<byte> WeaponIds => default;
        [Networked, Capacity(InventorySlots.WEAPON_COUNT)] private NetworkArray<WeaponSlotState> WeaponStates => default;

        private void Awake()
        {
            _weaponModels = new NetworkWeaponModel[InventorySlots.WEAPON_COUNT];
            for (int weaponIndex = 0; weaponIndex < InventorySlots.WEAPON_COUNT; weaponIndex++)
            {
                _weaponModels[weaponIndex] = new NetworkWeaponModel(this, weaponIndex);
            }
        }

        public byte GetWeaponId(int weaponIndex) => WeaponIds[weaponIndex];

        public void SetWeaponId(int weaponIndex, byte weaponId) => WeaponIds.Set(weaponIndex, weaponId);

        public IWeaponModel GetWeaponState(int weaponIndex) => _weaponModels[weaponIndex];

        internal WeaponSlotState GetSlotState(int weaponIndex) => WeaponStates[weaponIndex];

        internal void SetSlotState(int weaponIndex, WeaponSlotState state) => WeaponStates.Set(weaponIndex, state);
    }
}
