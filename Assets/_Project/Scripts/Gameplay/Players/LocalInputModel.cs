// 네트워크 틱 사이에 모인 로컬 플레이어 입력.
// InputView가 LocalInputSystem을 통해 쓰고, 네트워크 입력 제공자가 틱마다 한 번 읽는다.
// 한 번 누르는 요청은 몇 틱 동안 유지되어(호스트는 눌린 순간에만 반응), 늦게 도착한 입력 패킷 하나에 짧은 키 입력이 사라지지 않는다.

using UnityEngine;

namespace ZD.Gameplay.Players
{
    public sealed class LocalInputModel
    {
        public Vector2 Move { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public bool FireHeld { get; set; }
        public bool ReviveHeld { get; set; }
        public int FireTicksLeft { get; set; }
        public int ReloadTicksLeft { get; set; }
        public int RestartTicksLeft { get; set; }
        public WeaponSlot RequestedSlot { get; set; } = WeaponSlot.None;

        public byte SlotRequestSequence { get; set; }
    }
}
