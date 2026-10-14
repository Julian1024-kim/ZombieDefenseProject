// 피해 한 번의 양, 맞은 부위, 공격자를 담는 구조체.

namespace ZD.Gameplay.Combat
{
    public readonly struct DamageInfo
    {
        public readonly int Amount;
        public readonly HitZone Zone;
        public readonly int AttackerId;

        public DamageInfo(int amount, HitZone zone, int attackerId)
        {
            Amount = amount;
            Zone = zone;
            AttackerId = attackerId;
        }
    }
}
