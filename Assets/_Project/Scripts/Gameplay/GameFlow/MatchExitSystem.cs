// 세션을 나가 시작 화면(로비)으로 돌아간다.
// 게임 오버 화면에서 Esc를 누르면 바로, 참가에 실패하면(방 없음, 꽉 참, 비밀번호 틀림) 그 이유를 로비 안내로 들고 바로, 호스트가 나가 세션이 끊기면(호스트 이전 없음) 안내를 잠시 보여 준 뒤 돌아간다.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ZD.Core;
using ZD.Gameplay.Hud;

namespace ZD.Gameplay.GameFlow
{
    public sealed class MatchExitSystem : IDisposable
    {
        // 호스트가 나갔다는 안내를 읽을 시간.
        private static readonly TimeSpan AutoReturnDelay = TimeSpan.FromSeconds(3);

        private readonly ISessionService _session;
        private readonly ISceneNavigator _navigator;
        private readonly MatchHudModel _match;
        private readonly SessionStatusModel _status;
        private readonly CancellationTokenSource _cts = new();
        private readonly IDisposable _statusSubscription;
        private bool _isLeaving;

        public MatchExitSystem(ISessionService session, ISceneNavigator navigator, MatchHudModel match, SessionStatusModel status)
        {
            _session = session;
            _navigator = navigator;
            _match = match;
            _status = status;
            _statusSubscription = status.Status.Subscribe(OnStatusChanged);
        }

        public bool TryLeaveAfterGameOver()
        {
            if (_match.Phase.Value != GamePhase.GameOver)
            {
                return false;
            }

            LeaveAsync(MenuReturnReason.Left).Forget();
            return true;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _statusSubscription.Dispose();
        }

        private void OnStatusChanged(SessionStatus status)
        {
            if (status == SessionStatus.Failed)
            {
                LeaveAsync(ToReturnReason(_status.FailureReason)).Forget();
            }
            else if (status == SessionStatus.Disconnected)
            {
                ReturnAfterDelay().Forget();
            }
        }

        private async UniTaskVoid ReturnAfterDelay()
        {
            bool isCancelled = await UniTask.Delay(AutoReturnDelay, cancellationToken: _cts.Token).SuppressCancellationThrow();
            if (!isCancelled)
            {
                LeaveAsync(MenuReturnReason.HostLeft).Forget();
            }
        }

        private static MenuReturnReason ToReturnReason(SessionStartResult result)
        {
            return result switch
            {
                SessionStartResult.RoomNotFound => MenuReturnReason.RoomNotFound,
                SessionStartResult.RoomFull => MenuReturnReason.RoomFull,
                SessionStartResult.WrongPassword => MenuReturnReason.WrongPassword,
                _ => MenuReturnReason.ConnectFailed,
            };
        }

        private async UniTaskVoid LeaveAsync(MenuReturnReason reason)
        {
            if (_isLeaving)
            {
                return;
            }

            _isLeaving = true;
            await _session.ShutdownAsync();
            await _navigator.ReturnToMenuAsync(reason);
        }
    }
}
