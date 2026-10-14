// 게임 오버 점수 테스트: 통계 집계, 결과 확정(팀 점수, 줄 정렬), 점수 규칙, 모드별 최고 점수.

using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using ZD.Core.Messages;
using ZD.Gameplay.Defense;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Players;
using ZD.Gameplay.Stats;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class ScoreAndStatsTests
    {
        private PlayerRosterModel _players;
        private CapturingSubscriber<ZombieKilledMessage> _killed;
        private CapturingSubscriber<PlayerRevivedMessage> _revived;
        private CapturingSubscriber<MatchRestartedMessage> _restarted;

        [SetUp]
        public void SetUp()
        {
            _players = new PlayerRosterModel();
            _killed = new CapturingSubscriber<ZombieKilledMessage>();
            _revived = new CapturingSubscriber<PlayerRevivedMessage>();
            _restarted = new CapturingSubscriber<MatchRestartedMessage>();
        }

        [Test]
        public void StatsSystem_CountsKillsHeadshotsAndRevives()
        {
            var shooter = new FakePlayer(0);
            var medic = new FakePlayer(1);
            _players.Register(shooter);
            _players.Register(medic);
            using var sut = new StatsSystem(_players, _killed, _revived, _restarted);

            _killed.Raise(new ZombieKilledMessage(100, 0, isHeadshot: true));
            _killed.Raise(new ZombieKilledMessage(101, 0, isHeadshot: false));
            _killed.Raise(new ZombieKilledMessage(102, 99, isHeadshot: true));
            _revived.Raise(new PlayerRevivedMessage(1, 0));

            Assert.That(shooter.Kills, Is.EqualTo(2));
            Assert.That(shooter.Headshots, Is.EqualTo(1));
            Assert.That(medic.Revives, Is.EqualTo(1));
            Assert.That(medic.Kills, Is.EqualTo(0));
        }

        [Test]
        public void StatsSystem_MatchRestarted_ResetsStats()
        {
            var player = new FakePlayer(0) { Kills = 5, Headshots = 2, Revives = 1 };
            _players.Register(player);
            using var sut = new StatsSystem(_players, _killed, _revived, _restarted);

            _restarted.Raise(new MatchRestartedMessage());

            Assert.That(player.Kills + player.Headshots + player.Revives, Is.EqualTo(0));
        }

        [Test]
        public void MatchResultSystem_RecordsTeamScoreAndSortedRows()
        {
            var second = new FakePlayer(1) { Kills = 3 };
            second.FakeWallet.PointsEarned = 400;
            var first = new FakePlayer(0) { Kills = 7, Headshots = 2 };
            first.FakeWallet.PointsEarned = 1200;
            first.FakeWallet.Points = 0;
            _players.Register(second);
            _players.Register(first);
            var result = new FakeGameFlow();

            new MatchResultSystem(_players, new BaseRosterModel()).Record(result, wavesCleared: 3);

            Assert.That(result.TeamScore, Is.EqualTo(3 * 1000 + 1200 + 400));
            Assert.That(result.WavesCleared, Is.EqualTo(3));
            Assert.That(result.PlayerCount, Is.EqualTo(2));
            Assert.That(result.GetPlayer(0).PlayerNumber, Is.EqualTo(0));
            Assert.That(result.GetPlayer(0).Kills, Is.EqualTo(7));
            Assert.That(result.GetPlayer(1).PointsEarned, Is.EqualTo(400));
            Assert.That(result.Version, Is.EqualTo(1));
        }

        [Test]
        public void MatchResultSystem_BaseDestroyed_RecordsBaseReason()
        {
            _players.Register(new FakePlayer(0, health: 0));
            var bases = new BaseRosterModel();
            bases.Register(new FakeBase(TestAssets.Base(), UnityEngine.Vector3.zero) { Health = 0 });
            var result = new FakeGameFlow();

            new MatchResultSystem(_players, bases).Record(result, wavesCleared: 0);

            Assert.That(result.IsBaseDestroyed, Is.True);
        }

        [Test]
        public void ScoreRules_DaysSurvived_SixWavesPerDay()
        {
            Assert.That(ScoreRules.DaysSurvived(5), Is.EqualTo(0));
            Assert.That(ScoreRules.DaysSurvived(6), Is.EqualTo(1));
            Assert.That(ScoreRules.DaysSurvived(13), Is.EqualTo(2));
        }

        [Test]
        public void HighScoreSystem_KeepsBestPerMode()
        {
            var repository = new FakeHighScoreRepository();
            var sut = new HighScoreSystem(repository);

            HighScoreResult first = sut.SubmitAsync(GameModeId.BaseDefense, 5000, CancellationToken.None).GetAwaiter().GetResult();
            HighScoreResult lower = sut.SubmitAsync(GameModeId.BaseDefense, 3000, CancellationToken.None).GetAwaiter().GetResult();
            HighScoreResult otherMode = sut.SubmitAsync(GameModeId.Survival, 1000, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(first.IsNewBest, Is.True);
            Assert.That(lower.IsNewBest, Is.False);
            Assert.That(lower.Best, Is.EqualTo(5000));
            Assert.That(otherMode.IsNewBest, Is.True);
            Assert.That(repository.Saved.BestByMode[(int)GameModeId.BaseDefense], Is.EqualTo(5000));
            Assert.That(repository.Saved.BestByMode[(int)GameModeId.Survival], Is.EqualTo(1000));
            Assert.That(repository.SaveCount, Is.EqualTo(2));
        }
    }
}
