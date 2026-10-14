// 장치 입력을 로컬 플레이어의 의도로 바꾼다.
// 입력 장치를 몰라서 InputView, 봇, 리플레이 무엇이든 조종할 수 있다.

using UnityEngine;

namespace ZD.Gameplay.Players
{
    public sealed class LocalInputSystem
    {
        private const float MIN_PITCH = -70f;
        private const float MAX_PITCH = 70f;

        // 요청은 이 틱 수(64Hz 기준 약 60ms) 동안 눌린 상태로 유지된다. 호스트는 눌린 순간에만 반응하므로
        // 오래 유지해도 동작이 반복되지 않고, 늦은 틱 하나로 입력을 잃지 않는다.
        private const int ONE_SHOT_HOLD_TICKS = 4;

        private readonly LocalInputModel _model;
        private readonly PlayerDefinition _definition;

        public LocalInputSystem(LocalInputModel model, PlayerDefinition definition)
        {
            _model = model;
            _definition = definition;
        }

        public void SetMoveInput(Vector2 move)
        {
            _model.Move = Vector2.ClampMagnitude(move, 1f);
        }

        public void AddLookInput(Vector2 lookDelta)
        {
            float sensitivity = _definition.LookSensitivity;
            _model.Yaw = Mathf.Repeat(_model.Yaw + lookDelta.x * sensitivity, 360f);
            _model.Pitch = Mathf.Clamp(_model.Pitch - lookDelta.y * sensitivity, MIN_PITCH, MAX_PITCH);
        }

        public void ResetLook(float yaw, float pitch)
        {
            _model.Yaw = Mathf.Repeat(yaw, 360f);
            _model.Pitch = Mathf.Clamp(pitch, MIN_PITCH, MAX_PITCH);
        }

        public void SetFireHeld(bool isHeld)
        {
            _model.FireHeld = isHeld;
            if (isHeld)
            {
                _model.FireTicksLeft = ONE_SHOT_HOLD_TICKS;
            }
        }

        public void SetReviveHeld(bool isHeld)
        {
            _model.ReviveHeld = isHeld;
        }

        public bool IsReviveHeld() => _model.ReviveHeld;

        public void RequestReload()
        {
            _model.ReloadTicksLeft = ONE_SHOT_HOLD_TICKS;
        }

        public void RequestRestart()
        {
            _model.RestartTicksLeft = ONE_SHOT_HOLD_TICKS;
        }

        public void RequestSlot(WeaponSlot slot)
        {
            _model.RequestedSlot = slot;
            _model.SlotRequestSequence++;
        }

        public bool IsFireDown() => _model.FireHeld || _model.FireTicksLeft > 0;

        public bool IsReloadDown() => _model.ReloadTicksLeft > 0;

        public bool IsRestartDown() => _model.RestartTicksLeft > 0;

        public WeaponSlot RequestedSlot => _model.RequestedSlot;

        public byte SlotRequestSequence => _model.SlotRequestSequence;

        public void AdvanceTick()
        {
            _model.FireTicksLeft = Mathf.Max(0, _model.FireTicksLeft - 1);
            _model.ReloadTicksLeft = Mathf.Max(0, _model.ReloadTicksLeft - 1);
            _model.RestartTicksLeft = Mathf.Max(0, _model.RestartTicksLeft - 1);
        }
    }
}
