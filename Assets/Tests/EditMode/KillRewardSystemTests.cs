// KillRewardSystem의 처치 Point 지급과 WalletSystem 테스트.

using MessagePipe;
using NUnit.Framework;
using ZD.Core.Messages;
using ZD.Gameplay.Economy;
using ZD.Gameplay.Players;
using ZD.Gameplay.Zombies;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class KillRewardSystemTests
    {
        private PlayerRosterModel _players;
        private ZombieRosterModel _zombies;
        private CapturingSubscriber<ZombieKilledMessage> _killed;
        private KillRewardSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _players = new PlayerRosterModel();
            _zombies = new ZombieRosterModel();
            _killed = new CapturingSubscriber<ZombieKilledMessage>();
            _sut = new KillRewardSystem(_players, _zombies, new WalletSystem(), _killed);
        }

        [TearDown]
        public void TearDown() => _sut.Dispose();

        [Test]
        public void ZombieKilled_PaysOnlyTheKillerItsKillPoints()
        {
            var killer = new FakePlayer(1);
            var bystander = new FakePlayer(2);
            _players.Register(killer);
            _players.Register(bystander);
            _zombies.Register(new FakeZombie(10, TestAssets.Zombie()), new FakeZombieMotor());

            _killed.Raise(new ZombieKilledMessage(10, 1, false));

            Assert.That(killer.FakeWallet.Points, Is.EqualTo(50));
            Assert.That(killer.FakeWallet.PointsEarned, Is.EqualTo(50));
            Assert.That(bystander.FakeWallet.Points, Is.EqualTo(0));
        }

        [Test]
        public void ZombieKilled_ByHeadshot_AddsTheBonus()
        {
            var killer = new FakePlayer(1);
            _players.Register(killer);
            _zombies.Register(new FakeZombie(10, TestAssets.Zombie()), new FakeZombieMotor());

            _killed.Raise(new ZombieKilledMessage(10, 1, true));

            Assert.That(killer.FakeWallet.Points, Is.EqualTo(70));
        }

        [Test]
        public void ZombieKilled_ByAnUnknownKiller_PaysNobody()
        {
            var player = new FakePlayer(1);
            _players.Register(player);
            _zombies.Register(new FakeZombie(10, TestAssets.Zombie()), new FakeZombieMotor());

            _killed.Raise(new ZombieKilledMessage(10, 99, false));

            Assert.That(player.FakeWallet.Points, Is.EqualTo(0));
        }

        [Test]
        public void WalletSystem_SpendingDoesNotLowerPointsEarned()
        {
            var wallet = new FakeWallet();
            var sut = new WalletSystem();
            sut.Earn(wallet, 500);

            Assert.That(sut.TrySpend(wallet, 300), Is.True);
            Assert.That(sut.TrySpend(wallet, 300), Is.False);
            Assert.That(wallet.Points, Is.EqualTo(200));
            Assert.That(wallet.PointsEarned, Is.EqualTo(500));
        }
    }
}
