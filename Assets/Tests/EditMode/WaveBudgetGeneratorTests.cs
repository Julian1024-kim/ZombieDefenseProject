// WaveBudgetGenerator의 예산 사용과 좀비 해금 테스트.

using System.Collections.Generic;
using NUnit.Framework;
using ZD.Gameplay.Waves;
using ZD.Gameplay.Zombies;
using ZD.Tests.EditMode.Fakes;

namespace ZD.Tests.EditMode
{
    public sealed class WaveBudgetGeneratorTests
    {
        private readonly List<ZombieDefinition> _result = new();

        [Test]
        public void Generate_FirstWave_UsesOnlyUnlockedTypesAndSpendsWholeBudget()
        {
            ZombieDefinition normal = TestAssets.Zombie(cost: 1, firstWave: 1);
            ZombieDefinition tank = TestAssets.Zombie(cost: 5, firstWave: 3);
            WaveProfile profile = TestAssets.Waves(baseBudget: 6, perWave: 3, tank, normal);
            var sut = new WaveBudgetGenerator(new FixedRandomSource());

            sut.Generate(profile, 1, _result);

            Assert.That(_result.Count, Is.EqualTo(6));
            Assert.That(_result, Has.All.SameAs(normal));
        }

        [Test]
        public void Generate_LaterWave_BudgetGrowsAndUnlocksNewTypes()
        {
            ZombieDefinition normal = TestAssets.Zombie(cost: 1, firstWave: 1);
            ZombieDefinition tank = TestAssets.Zombie(cost: 5, firstWave: 3);
            WaveProfile profile = TestAssets.Waves(baseBudget: 6, perWave: 3, tank, normal);
            var sut = new WaveBudgetGenerator(new FixedRandomSource());

            sut.Generate(profile, 3, _result);

            // 예산 12: 고정 난수는 항상 살 수 있는 첫 종류(탱커, 5)를 고르다가 일반 좀비만 남으면 일반을 고른다.
            Assert.That(_result.Count, Is.EqualTo(4));
            Assert.That(_result[0], Is.SameAs(tank));
            Assert.That(_result[1], Is.SameAs(tank));
            Assert.That(_result[2], Is.SameAs(normal));
        }

        [Test]
        public void Generate_NoAffordableType_ReturnsEmptyWave()
        {
            ZombieDefinition tank = TestAssets.Zombie(cost: 10, firstWave: 1);
            WaveProfile profile = TestAssets.Waves(baseBudget: 3, perWave: 0, tank);
            var sut = new WaveBudgetGenerator(new FixedRandomSource());

            sut.Generate(profile, 1, _result);

            Assert.That(_result, Is.Empty);
        }
    }
}
