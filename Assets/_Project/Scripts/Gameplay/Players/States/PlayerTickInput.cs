// 플레이어 한 틱의 입력(네트워크 계층이 입력 구조체에서 풀어 만든다): 이동, 시선, 사격·재장전, 부활 키, 이번 틱의 슬롯 요청, 조준 레이.
// 슬롯 요청은 누를 때마다 한 번만 들어오도록 네트워크 계층이 걸러서 HasSlotRequest로 알린다.

using UnityEngine;

namespace ZD.Gameplay.Players
{
    public readonly struct PlayerTickInput
    {
        public readonly Vector2 Move;
        public readonly float Yaw;
        public readonly float Pitch;
        public readonly bool IsFireHeld;
        public readonly bool IsFirePressed;
        public readonly bool IsReloadPressed;
        public readonly bool IsReviveHeld;
        public readonly bool HasSlotRequest;
        public readonly WeaponSlot RequestedSlot;
        public readonly Vector3 AimOrigin;
        public readonly Vector3 AimDirection;

        public PlayerTickInput(
            Vector2 move,
            float yaw,
            float pitch,
            bool isFireHeld,
            bool isFirePressed,
            bool isReloadPressed,
            bool isReviveHeld,
            bool hasSlotRequest,
            WeaponSlot requestedSlot,
            Vector3 aimOrigin,
            Vector3 aimDirection)
        {
            Move = move;
            Yaw = yaw;
            Pitch = pitch;
            IsFireHeld = isFireHeld;
            IsFirePressed = isFirePressed;
            IsReloadPressed = isReloadPressed;
            IsReviveHeld = isReviveHeld;
            HasSlotRequest = hasSlotRequest;
            RequestedSlot = requestedSlot;
            AimOrigin = aimOrigin;
            AimDirection = aimDirection;
        }
    }
}
