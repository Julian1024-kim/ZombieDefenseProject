// 씬 빌더가 만든 에셋을 한데 묶어 전달하는 구조체.

using ZD.Gameplay.Building;
using ZD.Gameplay.Cameras;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Inventory;
using ZD.Gameplay.Players;
using ZD.Gameplay.Waves;
using ZD.Gameplay.Weapons;
using ZD.Network.Combat;
using ZD.Network.Session;

namespace ZD.Editor
{
    internal readonly struct MvpAssets
    {
        public readonly PlayerDefinition PlayerDefinition;
        public readonly WaveProfile WaveProfile;
        public readonly CameraSettings CameraSettings;
        public readonly NetworkPrefabCatalog PrefabCatalog;
        public readonly HitQuerySettings HitQuerySettings;
        public readonly WeaponCatalog WeaponCatalog;
        public readonly InventorySettings InventorySettings;
        public readonly ShopCatalog ShopCatalog;
        public readonly BuildCatalog BuildCatalog;

        public MvpAssets(
            PlayerDefinition playerDefinition,
            WaveProfile waveProfile,
            CameraSettings cameraSettings,
            NetworkPrefabCatalog prefabCatalog,
            HitQuerySettings hitQuerySettings,
            WeaponCatalog weaponCatalog,
            InventorySettings inventorySettings,
            ShopCatalog shopCatalog,
            BuildCatalog buildCatalog)
        {
            PlayerDefinition = playerDefinition;
            WaveProfile = waveProfile;
            CameraSettings = cameraSettings;
            PrefabCatalog = prefabCatalog;
            HitQuerySettings = hitQuerySettings;
            WeaponCatalog = weaponCatalog;
            InventorySettings = inventorySettings;
            ShopCatalog = shopCatalog;
            BuildCatalog = buildCatalog;
        }
    }
}
