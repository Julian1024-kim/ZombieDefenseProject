// 쓰러진 동료 옆에서 F를 누르고 있으면 5초 뒤 체력 30으로 부활시킨다.
// 떼거나, 멀어지거나, 부활시키던 사람이 쓰러지면 취소된다.
// 체력은 호스트에서만 바꾼다.

using MessagePipe;
using UnityEngine;
using ZD.Core;
using ZD.Core.Messages;

namespace ZD.Gameplay.Players
{
    public sealed class ReviveSystem
    {
        public const int NO_TARGET = -1;

        private readonly INetworkClock _clock;
        private readonly PlayerRosterModel _players;
        private readonly PlayerDefinition _definition;
        private readonly IPublisher<PlayerRevivedMessage> _revivedPublisher;

        public ReviveSystem(INetworkClock clock, PlayerRosterModel players, PlayerDefinition definition, IPublisher<PlayerRevivedMessage> revivedPublisher)
        {
            _clock = clock;
            _players = players;
            _definition = definition;
            _revivedPublisher = revivedPublisher;
        }

        public static bool IsReviving(IReviveModel revive) => revive.ReviveTargetId != NO_TARGET;

        public void Tick(IPlayerModel reviver, bool isReviveHeld, bool canComplete)
        {
            IReviveModel revive = reviver.Revive;
            if (reviver.Health <= 0 || !isReviveHeld)
            {
                Cancel(revive);
                return;
            }

            if (!IsReviving(revive))
            {
                IPlayerModel candidate = FindRevivableTeammate(reviver);
                if (candidate != null)
                {
                    revive.ReviveTargetId = candidate.EntityId;
                    revive.ReviveEndTick = _clock.Tick + Mathf.Max(1, _clock.SecondsToTicks(_definition.ReviveSeconds));
                }

                return;
            }

            IPlayerModel target = _players.FindById(revive.ReviveTargetId);
            if (target == null || target.Health > 0 || !IsInRange(reviver, target))
            {
                Cancel(revive);
                return;
            }

            if (_clock.Tick < revive.ReviveEndTick)
            {
                return;
            }

            Cancel(revive);
            if (canComplete)
            {
                target.Health = Mathf.Min(target.MaxHealth, _definition.ReviveHealth);
                _revivedPublisher.Publish(new PlayerRevivedMessage(reviver.EntityId, target.EntityId));
            }
        }

        public void Cancel(IReviveModel revive)
        {
            revive.ReviveTargetId = NO_TARGET;
            revive.ReviveEndTick = 0;
        }

        public float GetProgress(IReviveModel revive)
        {
            if (!IsReviving(revive))
            {
                return -1f;
            }

            int totalTicks = Mathf.Max(1, _clock.SecondsToTicks(_definition.ReviveSeconds));
            int ticksLeft = revive.ReviveEndTick - _clock.Tick;
            return Mathf.Clamp01(1f - ticksLeft / (float)totalTicks);
        }

        public IPlayerModel FindRevivableTeammate(IPlayerModel reviver)
        {
            IPlayerModel nearest = null;
            float nearestSqr = float.MaxValue;
            for (int playerIndex = 0; playerIndex < _players.Count; playerIndex++)
            {
                IPlayerModel teammate = _players.Get(playerIndex);
                if (teammate == reviver || teammate.Health > 0 || !IsInRange(reviver, teammate))
                {
                    continue;
                }

                float sqr = (teammate.Position - reviver.Position).sqrMagnitude;
                if (sqr < nearestSqr)
                {
                    nearestSqr = sqr;
                    nearest = teammate;
                }
            }

            return nearest;
        }

        public IPlayerModel FindReviverOf(IPlayerModel downed)
        {
            for (int playerIndex = 0; playerIndex < _players.Count; playerIndex++)
            {
                IPlayerModel teammate = _players.Get(playerIndex);
                if (teammate != downed && teammate.Revive.ReviveTargetId == downed.EntityId)
                {
                    return teammate;
                }
            }

            return null;
        }

        private bool IsInRange(IPlayerModel reviver, IPlayerModel target)
        {
            Vector3 offset = target.Position - reviver.Position;
            offset.y = 0f;
            return offset.sqrMagnitude <= _definition.ReviveRange * _definition.ReviveRange;
        }
    }
}
