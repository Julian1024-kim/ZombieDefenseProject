// 상점 품목 하나(이름, 종류, 무기, 가격, 수량).

using System;
using UnityEngine;
using ZD.Gameplay.Weapons;

namespace ZD.Gameplay.Economy
{
    [Serializable]
    public struct ShopItem
    {
        [SerializeField] private string _displayName;
        [SerializeField] private ShopItemKind _kind;
        [Tooltip("The weapon sold, or the weapon the ammo is for.")]
        [SerializeField] private WeaponDefinition _weapon;
        [SerializeField] private int _price;
        [Tooltip("Rounds for ammo. Ignored otherwise.")]
        [SerializeField] private int _amount;

        public string DisplayName => _displayName;
        public ShopItemKind Kind => _kind;
        public WeaponDefinition Weapon => _weapon;
        public int Price => _price;
        public int Amount => _amount;
    }
}
