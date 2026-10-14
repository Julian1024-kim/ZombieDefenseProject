// 웨이브 예산을, 등장 가능하고 살 수 있는 좀비 종류에 무작위로 써서 웨이브를 만든다.
// 고정된 웨이브 목록 대신이라 웨이브가 끝없이 이어진다.

using System.Collections.Generic;
using UnityEngine;
using ZD.Gameplay.Zombies;

namespace ZD.Gameplay.Waves
{
    public sealed class WaveBudgetGenerator
    {
        private readonly IRandomSource _random;
        private readonly List<ZombieDefinition> _affordable = new(8);

        public WaveBudgetGenerator(IRandomSource random)
        {
            _random = random;
        }

        public void Generate(WaveProfile profile, int waveNumber, List<ZombieDefinition> result)
        {
            result.Clear();
            int remainingBudget = profile.GetBudget(waveNumber);
            IReadOnlyList<ZombieDefinition> types = profile.ZombieTypes;

            while (remainingBudget > 0)
            {
                CollectAffordable(types, waveNumber, remainingBudget);
                if (_affordable.Count == 0)
                {
                    break;
                }

                ZombieDefinition picked = _affordable[_random.Next(_affordable.Count)];
                result.Add(picked);
                remainingBudget -= Mathf.Max(1, picked.SpawnCost);
            }

            _affordable.Clear();
        }

        private void CollectAffordable(IReadOnlyList<ZombieDefinition> types, int waveNumber, int remainingBudget)
        {
            _affordable.Clear();
            for (int typeIndex = 0; typeIndex < types.Count; typeIndex++)
            {
                ZombieDefinition type = types[typeIndex];
                if (type.FirstWave <= waveNumber && Mathf.Max(1, type.SpawnCost) <= remainingBudget)
                {
                    _affordable.Add(type);
                }
            }
        }
    }
}
