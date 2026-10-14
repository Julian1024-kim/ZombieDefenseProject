// 멀티플레이 테스트용 개발 전용 입력원.
// 개발 빌드를 -zd-bot 옵션으로 실행하면 LocalInputSystem을 조종한다 (원을 그리며 걷기, 회전, 사격, 재시작).
// 사람이 없어도 두 번째 피어를 테스트할 수 있다.
// 릴리스 빌드나 옵션이 없으면 아무것도 하지 않는다.

using System;
using UnityEngine;
using VContainer;
using ZD.Gameplay.Players;

namespace ZD.Presentation.Input
{
    [DisallowMultipleComponent]
    public sealed class DevInputBot : MonoBehaviour
    {
        private const string COMMAND_LINE_FLAG = "-zd-bot";
        private const float LOOK_SPEED = 3f;
        private const float FIRE_PERIOD_SECONDS = 1f;
        private const float RESTART_PERIOD_SECONDS = 2f;

        private LocalInputSystem _inputSystem;
        private float _elapsed;
        private float _nextFireTime;
        private float _nextRestartTime;

        private void Awake()
        {
            bool isRequested = Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(), COMMAND_LINE_FLAG) >= 0;
            enabled = isRequested;
        }

        [Inject]
        public void Construct(LocalInputSystem inputSystem)
        {
            _inputSystem = inputSystem;
        }

        // LateUpdate에서 실행해, 이번 프레임에 InputView가 읽은 (가만히 있는) 장치 입력을 봇의 이동 입력으로 덮어쓴다.
        private void LateUpdate()
        {
            _elapsed += Time.deltaTime;
            _inputSystem.SetMoveInput(new Vector2(Mathf.Sin(_elapsed), Mathf.Cos(_elapsed)));
            _inputSystem.AddLookInput(new Vector2(LOOK_SPEED, 0f));

            if (_elapsed >= _nextFireTime)
            {
                _nextFireTime = _elapsed + FIRE_PERIOD_SECONDS;
                _inputSystem.SetFireHeld(true);
                _inputSystem.SetFireHeld(false);
            }

            if (_elapsed >= _nextRestartTime)
            {
                _nextRestartTime = _elapsed + RESTART_PERIOD_SECONDS;
                _inputSystem.RequestRestart();
            }
        }
    }
}
