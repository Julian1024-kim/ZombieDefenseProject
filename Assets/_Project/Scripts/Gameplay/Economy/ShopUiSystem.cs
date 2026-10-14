// 로컬 상점 창.
// 보급품 상자 옆에서만 열리고, 멀어지거나 쓰러지면 닫힌다.
// 여기의 거리 검사는 안내 표시용이고, 구매 시 호스트가 다시 검사한다.

using VContainer.Unity;
using ZD.Gameplay.Cameras;
using ZD.Gameplay.Hud;

namespace ZD.Gameplay.Economy
{
    public sealed class ShopUiSystem : ITickable
    {
        private readonly ShopUiModel _model;
        private readonly ShopSystem _shopSystem;
        private readonly CameraModel _cameraModel;
        private readonly LocalPlayerHudModel _localPlayer;

        public ShopUiSystem(ShopUiModel model, ShopSystem shopSystem, CameraModel cameraModel, LocalPlayerHudModel localPlayer)
        {
            _model = model;
            _shopSystem = shopSystem;
            _cameraModel = cameraModel;
            _localPlayer = localPlayer;
        }

        public void Tick()
        {
            bool canShop = _cameraModel.HasTarget && _localPlayer.Health.Value > 0 && _shopSystem.IsNearSupplyBox(_cameraModel.TargetPosition);
            _model.IsNearSupplyBox.Value = canShop;
            if (!canShop && _model.IsOpen.Value)
            {
                _model.IsOpen.Value = false;
            }
        }

        public void Toggle()
        {
            _model.IsOpen.Value = !_model.IsOpen.Value && _model.IsNearSupplyBox.Value;
        }

        public void Close()
        {
            _model.IsOpen.Value = false;
        }

        public void Purchase(int itemIndex)
        {
            if (_model.IsOpen.Value)
            {
                _model.RequestSender?.SendPurchaseRequest(itemIndex);
            }
        }
    }
}
