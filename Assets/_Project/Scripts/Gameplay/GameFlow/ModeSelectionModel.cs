// 시작 화면에서 고른 모드.
// 앱 전체(Root)에 하나 있고, Game 씬이 이 모드의 세션에 접속한다.
// Game 씬을 바로 플레이하면 Game 씬이 명령줄이나 기본값으로 따로 만든다.

namespace ZD.Gameplay.GameFlow
{
    public sealed class ModeSelectionModel
    {
        public GameModeId Mode { get; set; } = GameModeId.Survival;
    }
}
