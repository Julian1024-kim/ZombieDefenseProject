// DamageSystem의 피해 적용과 쓰러짐/처치 메시지 테스트.

using NUnit.Framework;
using ZD.Core.Messages;
using ZD.Gameplay.Combat;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class DamageSystemTests
    {
        private CapturingPublisher<PlayerDownedMessage> _downed;
        private CapturingPublisher<ZombieKilledMessage> _killed;
        private DamageSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _downed = new CapturingPublisher<PlayerDownedMessage>();
            _killed = new CapturingPublisher<ZombieKilledMessage>();
            _sut = new DamageSystem(_downed, _killed);
        }

        [Test]
        public void ApplyDamage_BelowHealth_ReducesHealthWithoutMessages()
        {
            var player = new FakePlayer(1, health: 100);

            _sut.ApplyDamage(player, new DamageInfo(30, HitZone.Body, 7));

            Assert.That(player.Health, Is.EqualTo(70));
            Assert.That(_downed.Messages, Is.Empty);
        }

        [Test]
        public void ApplyDamage_LethalToPlayer_ClampsToZeroAndPublishesDowned()
        {
            var player = new FakePlayer(1, health: 20);

            _sut.ApplyDamage(player, new DamageInfo(50, HitZone.Body, 7));

            Assert.That(player.Health, Is.EqualTo(0));
            Assert.That(_downed.Messages.Count, Is.EqualTo(1));
            Assert.That(_downed.Messages[0].PlayerId, Is.EqualTo(1));
        }

        [Test]
        public void ApplyDamage_LethalHeadshotToZombie_PublishesKillWithKillerAndHeadshot()
        {
            var zombie = new FakeZombie(5, TestAssets.Zombie(health: 40));

            _sut.ApplyDamage(zombie, new DamageInfo(80, HitZone.Head, 2));

            Assert.That(_killed.Messages.Count, Is.EqualTo(1));
            Assert.That(_killed.Messages[0].ZombieId, Is.EqualTo(5));
            Assert.That(_killed.Messages[0].KillerId, Is.EqualTo(2));
            Assert.That(_killed.Messages[0].IsHeadshot, Is.True);
        }

        [Test]
        public void ApplyDamage_ToAlreadyDownedTarget_DoesNothing()
        {
            var player = new FakePlayer(1, health: 0);

            _sut.ApplyDamage(player, new DamageInfo(10, HitZone.Body, 7));

            Assert.That(_downed.Messages, Is.Empty);
        }
    }
}
