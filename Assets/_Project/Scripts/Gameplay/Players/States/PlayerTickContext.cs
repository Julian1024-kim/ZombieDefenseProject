// 플레이어 상태가 한 틱에 필요한 것: 플레이어 모델, 캐릭터 이동기, 이번 틱 입력, 호스트(결과를 확정하는 쪽)인지.
// 클라이언트는 자기 플레이어를 예측하므로 HasStateAuthority가 false여도 상태가 돈다.

using ZD.Core;

namespace ZD.Gameplay.Players
{
    public readonly struct PlayerTickContext
    {
        public readonly IPlayerModel Player;
        public readonly ICharacterMotor Motor;
        public readonly PlayerTickInput Input;
        public readonly bool HasStateAuthority;

        public PlayerTickContext(IPlayerModel player, ICharacterMotor motor, in PlayerTickInput input, bool hasStateAuthority)
        {
            Player = player;
            Motor = motor;
            Input = input;
            HasStateAuthority = hasStateAuthority;
        }
    }
}
