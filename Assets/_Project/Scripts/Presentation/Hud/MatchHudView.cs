// 접속 상태, 대기 단계(인원, 방 코드, 시작 안내), 웨이브 번호, 단계 타이머, 남은 좀비 수, 게임 오버 창(보이기·숨기기, 진 이유, 호스트면 재시작 안내·아니면 호스트를 기다린다는 안내)을 표시한다.
// 게임 오버 창의 점수와 플레이어별 표는 ResultView가 그린다.

using System.Text;
using R3;
using TMPro;
using UnityEngine;
using VContainer;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Hud;
using ZD.Gameplay.Stats;

namespace ZD.Presentation.Hud
{
    public sealed class MatchHudView : MonoBehaviour
    {
        private const string BASE_DESTROYED_TITLE = "GAME OVER\n<size=50%>BASE DESTROYED</size>";
        private const string ALL_DOWN_TITLE = "GAME OVER\n<size=50%>ALL PLAYERS DOWN</size>";
        private const string HOST_HINT = "ENTER: RESTART    ESC: MENU";
        private const string CLIENT_HINT = "WAITING FOR THE HOST TO RESTART    ESC: MENU";

        [SerializeField] private TMP_Text _waveText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private CanvasGroup _gameOverPanel;
        [SerializeField] private TMP_Text _gameOverText;
        [SerializeField] private TMP_Text _gameOverHintText;

        private readonly CompositeDisposable _disposables = new();
        private readonly StringBuilder _builder = new(96);
        private MatchHudModel _model;
        private SessionStatusModel _session;
        private ResultHudModel _result;

        [Inject]
        public void Construct(MatchHudModel model, SessionStatusModel session, ResultHudModel result)
        {
            _model = model;
            _session = session;
            _result = result;
        }

        private void Start()
        {
            SetGameOverVisible(false);

            Observable.Merge(
                    _model.WaveNumber.AsUnitObservable(),
                    _session.Status.AsUnitObservable(),
                    _model.Phase.AsUnitObservable(),
                    _model.SecondsRemaining.AsUnitObservable(),
                    _model.ZombiesAlive.AsUnitObservable(),
                    _model.PlayerCount.AsUnitObservable(),
                    _result.Version.AsUnitObservable())
                .Subscribe(_ => Refresh())
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables.Dispose();

        private void Refresh()
        {
            switch (_session.Status.Value)
            {
                case SessionStatus.Connecting:
                    ShowStatusOnly("CONNECTING...");
                    return;
                case SessionStatus.Failed:
                    ShowStatusOnly("CONNECTION FAILED - CHECK THE PHOTON APP ID AND NETWORK\nRETURNING TO MENU...");
                    return;
                case SessionStatus.Disconnected:
                    ShowStatusOnly("DISCONNECTED - THE HOST LEFT THE SESSION\nRETURNING TO MENU...");
                    return;
            }

            switch (_model.Phase.Value)
            {
                case GamePhase.Waiting:
                    SetGameOverVisible(false);
                    _waveText.SetText("WAITING FOR PLAYERS  {0} / {1}", _model.PlayerCount.Value, MatchResultSystem.MAX_PLAYERS);
                    ShowWaitingStatus();
                    break;
                case GamePhase.Preparation:
                    SetGameOverVisible(false);
                    _waveText.SetText("WAVE {0}", _model.WaveNumber.Value + 1);
                    _statusText.SetText("NEXT WAVE IN {0}", _model.SecondsRemaining.Value);
                    break;
                case GamePhase.Wave:
                    SetGameOverVisible(false);
                    _waveText.SetText("WAVE {0}", _model.WaveNumber.Value);
                    _statusText.SetText("ZOMBIES {0}", _model.ZombiesAlive.Value);
                    break;
                case GamePhase.GameOver:
                    _statusText.SetText(string.Empty);
                    _gameOverText.SetText(_result.IsBaseDestroyed.Value ? BASE_DESTROYED_TITLE : ALL_DOWN_TITLE);
                    _gameOverHintText.SetText(_session.IsHost ? HOST_HINT : CLIENT_HINT);
                    SetGameOverVisible(true);
                    break;
            }
        }

        private void ShowWaitingStatus()
        {
            _builder.Clear();
            _builder.Append("ROOM CODE  <b>").Append(_session.RoomCode).Append("</b>\n");
            _builder.Append(_session.IsHost ? "PRESS ENTER TO START" : "WAITING FOR THE HOST TO START");
            _statusText.SetText(_builder);
        }

        private void ShowStatusOnly(string message)
        {
            SetGameOverVisible(false);
            _statusText.SetText(message);
        }

        private void SetGameOverVisible(bool isVisible)
        {
            _gameOverPanel.alpha = isVisible ? 1f : 0f;
            _gameOverPanel.blocksRaycasts = isVisible;
        }
    }
}
