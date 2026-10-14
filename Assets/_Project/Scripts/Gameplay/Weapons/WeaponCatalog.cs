// 게임의 모든 무기 목록.
// 네트워크가 에셋 참조 대신 1바이트 ID를 보내게 해준다.
// ID 0은 "무기 없음"이고 ID = 배열 순서 + 1이므로 목록 뒤에만 추가해야 한다.

using UnityEngine;

namespace ZD.Gameplay.Weapons
{
    [CreateAssetMenu(menuName = "ZD/Weapon Catalog")]
    public sealed class WeaponCatalog : ScriptableObject
    {
        public const byte NO_WEAPON = 0;

        [SerializeField] private WeaponDefinition[] _weapons;

        public WeaponDefinition Get(byte id)
        {
            if (id == NO_WEAPON || id > _weapons.Length)
            {
                return null;
            }

            return _weapons[id - 1];
        }

        public byte GetId(WeaponDefinition definition)
        {
            for (int weaponIndex = 0; weaponIndex < _weapons.Length; weaponIndex++)
            {
                if (_weapons[weaponIndex] == definition)
                {
                    return (byte)(weaponIndex + 1);
                }
            }

            return NO_WEAPON;
        }
    }
}
