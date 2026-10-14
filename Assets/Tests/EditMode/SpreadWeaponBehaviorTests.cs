// 샷건 산탄 패턴과 펠릿별 피해 테스트.

using NUnit.Framework;
using UnityEngine;
using ZD.Core.Messages;
using ZD.Gameplay.Combat;
using ZD.Gameplay.Weapons;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class SpreadWeaponBehaviorTests
    {
        [Test]
        public void Pattern_FirstPelletIsStraightAndAllStayInsideTheCone()
        {
            var sut = new SpreadWeaponBehavior(8, 6f);
            Quaternion aim = Quaternion.LookRotation(Vector3.forward);

            Assert.That(Vector3.Angle(sut.GetPelletDirection(aim, 0), Vector3.forward), Is.LessThan(0.01f));
            for (int pelletIndex = 0; pelletIndex < sut.PelletCount; pelletIndex++)
            {
                Assert.That(Vector3.Angle(sut.GetPelletDirection(aim, pelletIndex), Vector3.forward), Is.LessThanOrEqualTo(6.01f));
            }
        }

        [Test]
        public void Pattern_IsDeterministicSoClientAndHostAgree()
        {
            var first = new SpreadWeaponBehavior(8, 6f);
            var second = new SpreadWeaponBehavior(8, 6f);
            Quaternion aim = Quaternion.Euler(10f, 40f, 0f);

            for (int pelletIndex = 0; pelletIndex < first.PelletCount; pelletIndex++)
            {
                Assert.That(first.GetPelletDirection(aim, pelletIndex), Is.EqualTo(second.GetPelletDirection(aim, pelletIndex)));
            }
        }

        [Test]
        public void Fire_EveryPelletThatHitsDealsDamage()
        {
            WeaponDefinition shotgun = TestAssets.Primary("Shotgun", WeaponBehaviorType.Spread, pellets: 8, spread: 6f);
            var zombie = new FakeZombie(5, TestAssets.Zombie(health: 1000));
            var hits = new FakeHitQuery { RaycastHit = new HitResult(zombie, HitZone.Body, Vector3.zero) };
            var damage = new DamageSystem(new CapturingPublisher<PlayerDownedMessage>(), new CapturingPublisher<ZombieKilledMessage>());
            var command = new WeaponCommand(true, true, false, Vector3.zero, Vector3.forward);

            shotgun.CreateBehavior().Fire(shotgun, 1, command, hits, damage);

            Assert.That(hits.RaycastCount, Is.EqualTo(8));
            Assert.That(zombie.Health, Is.EqualTo(1000 - 8 * 10));
        }
    }
}
