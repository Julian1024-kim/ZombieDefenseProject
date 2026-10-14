// 보급품 상자 창.
// 숨겨둔 템플릿으로 시작 시 한 번만 품목마다 버튼을 만든다.
// 살 수 없는 품목은 버튼이 흐려지지만, 모든 구매는 여전히 호스트가 검증한다.

using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Hud;

namespace ZD.Presentation.Shop
{
    public sealed class ShopView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private Button _itemTemplate;
        [SerializeField] private TMP_Text _pointsText;
        [SerializeField] private TMP_Text _messageText;

        private readonly CompositeDisposable _disposables = new();
        private ShopCatalog _catalog;
        private ShopUiModel _shopUi;
        private ShopUiSystem _shopUiSystem;
        private LocalPlayerHudModel _localPlayer;
        private Button[] _buttons;

        [Inject]
        public void Construct(ShopCatalog catalog, ShopUiModel shopUi, ShopUiSystem shopUiSystem, LocalPlayerHudModel localPlayer)
        {
            _catalog = catalog;
            _shopUi = shopUi;
            _shopUiSystem = shopUiSystem;
            _localPlayer = localPlayer;
        }

        private void Start()
        {
            BuildButtons();
            SetVisible(false);

            _shopUi.IsOpen
                .Subscribe(isOpen =>
                {
                    _messageText.SetText(string.Empty);
                    SetVisible(isOpen);
                })
                .AddTo(_disposables);

            _localPlayer.Points
                .Subscribe(RefreshAffordability)
                .AddTo(_disposables);

            _shopUi.PurchaseResults
                .Subscribe(result => _messageText.SetText(Describe(result)))
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private void BuildButtons()
        {
            _buttons = new Button[_catalog.Count];
            for (int itemIndex = 0; itemIndex < _catalog.Count; itemIndex++)
            {
                ShopItem item = _catalog.Get(itemIndex);
                Button button = Instantiate(_itemTemplate, _itemTemplate.transform.parent);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<TMP_Text>().SetText($"{item.DisplayName}   {item.Price} P");
                int capturedIndex = itemIndex;
                button.onClick.AddListener(() => _shopUiSystem.Purchase(capturedIndex));
                _buttons[itemIndex] = button;
            }

            _itemTemplate.gameObject.SetActive(false);
        }

        private void RefreshAffordability(int points)
        {
            _pointsText.SetText("{0} P", points);
            for (int itemIndex = 0; itemIndex < _buttons.Length; itemIndex++)
            {
                _buttons[itemIndex].interactable = points >= _catalog.Get(itemIndex).Price;
            }
        }

        private void SetVisible(bool isVisible)
        {
            _panel.alpha = isVisible ? 1f : 0f;
            _panel.blocksRaycasts = isVisible;
            _panel.interactable = isVisible;
        }

        private static string Describe(PurchaseResult result)
        {
            return result switch
            {
                PurchaseResult.Success => "Purchased",
                PurchaseResult.NotEnoughPoints => "Not enough Points",
                PurchaseResult.AlreadyOwned => "Already owned",
                PurchaseResult.WeaponNotOwned => "Buy the weapon first",
                PurchaseResult.AtCapacity => "Carrying the maximum",
                PurchaseResult.TooFarFromSupplyBox => "Too far from the supply box",
                PurchaseResult.Downed => "Cannot buy while downed",
                PurchaseResult.MatchOver => "The match is over",
                _ => "Purchase failed",
            };
        }
    }
}
