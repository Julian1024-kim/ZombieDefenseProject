// 시작 화면과 게임 씬 사이의 전환.
// Bootstrap이 씬 불러오기로 구현하고, Game 씬만 바로 플레이할 때는 돌아갈 메뉴가 없는 대체 구현을 쓴다.

using Cysharp.Threading.Tasks;

namespace ZD.Core
{
    public interface ISceneNavigator
    {
        UniTask EnterGameAsync();

        UniTask ReturnToMenuAsync(MenuReturnReason reason);
    }
}
