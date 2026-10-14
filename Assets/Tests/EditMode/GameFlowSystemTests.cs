// GameFlowSystem의 단계 전환, 패배, 재시작 테스트.

using NUnit.Framework;
using ZD.Core.Messages;
using ZD.Gameplay.Defense;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Players;
using ZD.Gameplay.Stats;
using ZD.Gameplay.Waves;
using ZD.Gameplay.Zombies;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class GameFlowSystemTests
    {
        private FakeClock _clock;
        private FakeZombieSpawner _spawner;
        private FakeDefeatCondition _defeat;
        private CapturingPublisher<WaveStartedMessage> _waveStarted;
        private CapturingPublisher<WaveClearedMessage> _waveCleared;
        private CapturingPublisher<GameOverMessage> _gameOver;
        private CapturingPublisher<MatchRestartedMessage> _restarted;
        private FakeGameFlow _model;
        private GameFlowSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock();
            var zombies = new ZombieRosterModel();
            _spawner = new FakeZombieSpawner(zombies);
            WaveProfile profile = TestAssets.Waves(baseBudget: 2, perWave: 1, TestAssets.Zombie(cost: 1));
            var waveSystem = new WaveSystem(_clock, profile, new WaveBudgetGenerator(new FixedRandomSource()), _spawner, zombies);
            _defeat = new FakeDefeatCondition();
            _waveStarted = new CapturingPublisher<WaveStartedMessage>();
            _waveCleared = new CapturingPublisher<WaveClearedMessage>();
            _gameOver = new CapturingPublisher<GameOverMessage>();
            _restarted = new CapturingPublisher<MatchRestartedMessage>();
            _model = new FakeGameFlow();
            _sut = new GameFlowSystem(_clock, profile, waveSystem, new FakeModeRules(_defeat), new MatchResultSystem(new PlayerRosterModel(), new BaseRosterModel()), _waveStarted, _waveCleared, _gameOver, _restarted);
        }

        [Test]
        public void Begin_StartsPreparationAndWaitsForTimer()
        {
            _sut.Begin(_model);
            _sut.Tick(_model);

            Assert.That(_model.Phase, Is.EqualTo(GamePhase.Preparation));
            Assert.That(_model.WaveNumber, Is.EqualTo(0));
        }

        [Test]
        public void Tick_AfterPreparation_StartsWaveOneAndSpawnsItsZombies()
        {
            _sut.Begin(_model);
            AdvancePastPreparation();

            _sut.Tick(_model);
            _sut.Tick(_model);
            _clock.Advance(1);
            _sut.Tick(_model);

            Assert.That(_model.Phase, Is.EqualTo(GamePhase.Wave));
            Assert.That(_model.WaveNumber, Is.EqualTo(1));
            Assert.That(_waveStarted.Messages[0].ZombieCount, Is.EqualTo(2));
            Assert.That(_spawner.Spawned.Count, Is.EqualTo(2));
        }

        [Test]
        public void Tick_AllZombiesDead_ClearsWaveAndReturnsToPreparation()
        {
            _sut.Begin(_model);
            AdvancePastPreparation();
            _sut.Tick(_model);
            SpawnWholeWave();

            _spawner.KillAll();
            _sut.Tick(_model);

            Assert.That(_model.Phase, Is.EqualTo(GamePhase.Preparation));
            Assert.That(_waveCleared.Messages[0].WaveNumber, Is.EqualTo(1));
        }

        [Test]
        public void Tick_DefeatConditionMet_EndsGameReportingClearedWaves()
        {
            _sut.Begin(_model);
            AdvancePastPreparation();
            _sut.Tick(_model);

            _defeat.Met = true;
            _sut.Tick(_model);
            _sut.Tick(_model);

            Assert.That(_model.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(_gameOver.Messages.Count, Is.EqualTo(1));
            Assert.That(_gameOver.Messages[0].WavesCleared, Is.EqualTo(0));
        }

        [Test]
        public void Begin_WaitForStart_WaitsUntilStartRequested()
        {
            _sut.Begin(_model, waitForStart: true);
            AdvancePastPreparation();
            _sut.Tick(_model);
            Assert.That(_model.Phase, Is.EqualTo(GamePhase.Waiting));
            Assert.That(_spawner.Spawned, Is.Empty);

            _sut.RequestStart();
            _sut.Tick(_model);

            Assert.That(_model.Phase, Is.EqualTo(GamePhase.Preparation));
            Assert.That(_model.WaveNumber, Is.EqualTo(0));
        }

        [Test]
        public void RequestStart_OutsideWaiting_IsDiscarded()
        {
            _sut.Begin(_model);
            _sut.RequestStart();
            _sut.Tick(_model);

            _sut.Begin(_model, waitForStart: true);
            _sut.Tick(_model);

            Assert.That(_model.Phase, Is.EqualTo(GamePhase.Waiting));
        }

        [Test]
        public void Tick_DefeatedDuringPreparation_CountsLastWaveAsCleared()
        {
            _sut.Begin(_model);
            AdvancePastPreparation();
            _sut.Tick(_model);
            SpawnWholeWave();
            _spawner.KillAll();
            _sut.Tick(_model);
            Assert.That(_model.Phase, Is.EqualTo(GamePhase.Preparation));

            _defeat.Met = true;
            _sut.Tick(_model);

            Assert.That(_model.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(_model.WavesCleared, Is.EqualTo(1));
            Assert.That(_gameOver.Messages[0].WavesCleared, Is.EqualTo(1));
            Assert.That(_model.TeamScore, Is.EqualTo(1000));
        }

        [Test]
        public void Tick_RestartRequestedAfterGameOver_ClearsZombiesAndStartsOverFromWaveOne()
        {
            _sut.Begin(_model);
            AdvancePastPreparation();
            _sut.Tick(_model);
            SpawnWholeWave();
            _defeat.Met = true;
            _sut.Tick(_model);
            _defeat.Met = false;

            _sut.RequestRestart();
            _sut.Tick(_model);

            Assert.That(_model.Phase, Is.EqualTo(GamePhase.Preparation));
            Assert.That(_model.WaveNumber, Is.EqualTo(0));
            Assert.That(_restarted.Messages.Count, Is.EqualTo(1));
            Assert.That(_spawner.DespawnAllCount, Is.EqualTo(1));
            Assert.That(_spawner.Spawned, Is.Empty);
        }

        [Test]
        public void RequestRestart_DuringWave_IsIgnored()
        {
            _sut.Begin(_model);
            AdvancePastPreparation();
            _sut.Tick(_model);

            _sut.RequestRestart();
            _sut.Tick(_model);
            _defeat.Met = true;
            _sut.Tick(_model);
            _sut.Tick(_model);

            Assert.That(_model.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(_restarted.Messages, Is.Empty);
        }

        [Test]
        public void AllPlayersDownedCondition_MetOnlyWhenEveryPlayerIsDown()
        {
            var players = new PlayerRosterModel();
            var sut = new AllPlayersDownedCondition(players);
            var first = new FakePlayer(1, health: 0);
            var second = new FakePlayer(2, health: 50);
            players.Register(first);
            players.Register(second);

            Assert.That(sut.IsMet(), Is.False);
            second.Health = 0;
            Assert.That(sut.IsMet(), Is.True);
        }

        private void AdvancePastPreparation()
        {
            _clock.Advance(FakeClock.TICK_RATE + 1);
        }

        private void SpawnWholeWave()
        {
            for (int tickIndex = 0; tickIndex < 4; tickIndex++)
            {
                _clock.Advance(1);
                _sut.Tick(_model);
            }
        }
    }
}
