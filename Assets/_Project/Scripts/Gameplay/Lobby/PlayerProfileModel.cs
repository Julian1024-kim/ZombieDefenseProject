// 이 컴퓨터 플레이어의 프로필(닉네임).
// 앱 전체(Root)에 하나 있고, 설정 파일에서 불러와 로비와 게임이 쓴다.

using R3;

namespace ZD.Gameplay.Lobby
{
    public sealed class PlayerProfileModel
    {
        public ReactiveProperty<string> Nickname { get; } = new(TextRules.DEFAULT_NICKNAME);
    }
}
