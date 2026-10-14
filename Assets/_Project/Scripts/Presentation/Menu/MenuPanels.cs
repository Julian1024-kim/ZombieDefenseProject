// 시작 화면 패널을 보이거나 숨긴다(SetActive 대신 CanvasGroup: 다시 켤 때 캔버스를 새로 짓지 않음).

using UnityEngine;

namespace ZD.Presentation.Menu
{
    internal static class MenuPanels
    {
        public static void SetVisible(CanvasGroup panel, bool isVisible)
        {
            panel.alpha = isVisible ? 1f : 0f;
            panel.interactable = isVisible;
            panel.blocksRaycasts = isVisible;
        }
    }
}
