// 세션당 하나, 호스트가 스폰한다.
// 복제되는 매치 상태(모드, 단계, 웨이브)를 갖고, 호스트 전용 매치·좀비·방어막 시스템을 틱마다 돌린다.
// 모드는 호스트가 고른 값으로 정하고(모드마다 세션이 따로라 참가자도 같은 모드), 좀비 목표 선정은 모드가 정한다.
// 게임 오버 때 확정한 결과(팀 점수, 플레이어별 줄)도 여기 [Networked]로 두어 모든 피어가 같은 결과 화면을 본다.

using Fusion;
using UnityEngine;
using VContainer;
using ZD.Gameplay.Building;
using ZD.Gameplay.Defense;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Hud;
using ZD.Gameplay.Lobby;
using ZD.Gameplay.Players;
using ZD.Gameplay.Stats;
using ZD.Gameplay.Zombies;

namespace ZD.Network.GameFlow
{
    public sealed class GameDirectorNetwork : NetworkBehaviour, IGameFlowModel, IMatchResultModel
    {
        private GameFlowSystem _gameFlow;
        private GameModeSystem _gameModes;
        private BaseSystem _baseSystem;
        private ModeSelectionModel _modeSelection;
        private ZombieAISystem _zombieAI;
        private BarricadeSystem _barricades;
        private ZombieRosterModel _zombies;
        private MatchHudModel _hud;
        private ResultScreenSystem _resultScreen;
        private MatchStateModel _match;
        private SessionRequestModel _request;
        private PlayerRosterModel _players;

        [Networked] public GameModeId Mode { get; set; }
        [Networked] public GamePhase Phase { get; set; }
        [Networked] public int PhaseEndTick { get; set; }
        [Networked] public int WaveNumber { get; set; }
        [Networked] private int ZombiesAlive { get; set; }
        [Networked] public int Version { get; set; }
        [Networked] public int TeamScore { get; set; }
        [Networked] public int WavesCleared { get; set; }
        [Networked] public int PlayerCount { get; set; }
        [Networked] private NetworkBool BaseDestroyedFlag { get; set; }
        [Networked, Capacity(MatchResultSystem.MAX_PLAYERS)] private NetworkArray<PlayerResultData> ResultRows => default;

        public IMatchResultModel Result => this;

        public bool IsBaseDestroyed
        {
            get => BaseDestroyedFlag;
            set => BaseDestroyedFlag = value;
        }

        [Inject]
        public void Construct(
            GameFlowSystem gameFlow,
            GameModeSystem gameModes,
            BaseSystem baseSystem,
            ModeSelectionModel modeSelection,
            ZombieAISystem zombieAI,
            BarricadeSystem barricades,
            ZombieRosterModel zombies,
            MatchHudModel hud,
            ResultScreenSystem resultScreen,
            MatchStateModel match,
            SessionRequestModel request,
            PlayerRosterModel players)
        {
            _match = match;
            _request = request;
            _players = players;
            _gameFlow = gameFlow;
            _gameModes = gameModes;
            _baseSystem = baseSystem;
            _modeSelection = modeSelection;
            _zombieAI = zombieAI;
            _barricades = barricades;
            _zombies = zombies;
            _hud = hud;
            _resultScreen = resultScreen;
        }

        public override void Spawned()
        {
            _match.Flow = this;
            if (HasStateAuthority)
            {
                Mode = _modeSelection.Mode;
                _baseSystem.Setup(Mode);
                _gameFlow.Begin(this, waitForStart: _request.Kind != SessionRequestKind.DevAuto);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_match.Flow == (IGameFlowModel)this)
            {
                _match.Flow = null;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
            {
                return;
            }

            _gameFlow.Tick(this);
            if (Phase == GamePhase.GameOver)
            {
                _zombieAI.StopAll();
            }
            else
            {
                _zombieAI.Tick(_gameModes.GetTargetSelector(Mode));
                _barricades.Tick();
            }

            ZombiesAlive = _zombies.AliveCount;
        }

        public override void Render()
        {
            _hud.Mode.Value = Mode;
            _hud.Phase.Value = Phase;
            _hud.WaveNumber.Value = WaveNumber;
            _hud.ZombiesAlive.Value = ZombiesAlive;
            _hud.PlayerCount.Value = _players.Count;

            int ticksLeft = PhaseEndTick > 0 ? PhaseEndTick - Runner.Tick.Raw : 0;
            _hud.SecondsRemaining.Value = Mathf.Max(0, Mathf.CeilToInt(ticksLeft * Runner.DeltaTime));
            if (Phase == GamePhase.GameOver)
            {
                _resultScreen.Show(this, Mode);
            }
        }

        public PlayerResult GetPlayer(int index) => ResultRows[index].ToResult();

        public void SetPlayer(int index, in PlayerResult result) => ResultRows.Set(index, PlayerResultData.From(result));
    }
}
