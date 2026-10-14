// System.Random으로 만든 실제 난수 공급자.

using System;

namespace ZD.Gameplay.Waves
{
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random = new();

        public int Next(int maxExclusive) => _random.Next(maxExclusive);
    }
}
