// 보급품 상자가 파는 모든 품목과 가격.
// 밸런스를 바꿀 때 코드 수정이 필요 없다.

using UnityEngine;

namespace ZD.Gameplay.Economy
{
    [CreateAssetMenu(menuName = "ZD/Shop Catalog")]
    public sealed class ShopCatalog : ScriptableObject
    {
        [SerializeField] private ShopItem[] _items;
        [Tooltip("How close (m) a player must stand to a supply box to buy.")]
        [SerializeField] private float _interactRange = 2.5f;

        public int Count => _items.Length;
        public float InteractRange => _interactRange;

        public ShopItem Get(int index) => _items[index];
    }
}
