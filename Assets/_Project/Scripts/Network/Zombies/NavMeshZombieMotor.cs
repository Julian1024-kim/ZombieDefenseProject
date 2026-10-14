// 호스트 전용.
// NavMeshAgent가 경로와 회피를 계획하고, 좀비는 네트워크 틱 안에서 움직인다 (agent.updatePosition 끔).
// 그래서 NetworkTransform이 틱에 맞춘 위치를 복제한다.

using Fusion;
using UnityEngine;
using UnityEngine.AI;
using ZD.Gameplay.Zombies;

namespace ZD.Network.Zombies
{
    public sealed class NavMeshZombieMotor : IZombieMotor
    {
        // 목표가 이만큼 움직였을 때만 경로를 다시 구해서, 틱마다 경로 계산을 하지 않는다.
        private const float REPATH_SQR_DISTANCE = 0.5f * 0.5f;
        private const float TURN_SPEED_DEGREES = 540f;
        private const float MIN_SQR_VELOCITY = 0.0001f;

        private static readonly Vector3 NoDestination = new(float.MaxValue, 0f, 0f);

        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private readonly NetworkRunner _runner;
        private Vector3 _lastDestination = NoDestination;
        private bool _isPathBlocked;

        public bool IsPathBlocked => _isPathBlocked;

        public NavMeshZombieMotor(NavMeshAgent agent, Transform transform, NetworkRunner runner)
        {
            _agent = agent;
            _transform = transform;
            _runner = runner;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        public void MoveTowards(Vector3 destination, float speed)
        {
            _agent.speed = speed;
            if ((destination - _lastDestination).sqrMagnitude > REPATH_SQR_DISTANCE)
            {
                _agent.SetDestination(destination);
                _lastDestination = destination;
            }

            // 경로 계산이 끝난 뒤에만 판정을 바꾼다. 목적지까지 닿지 못하는 경로(Partial)는 방어막에 길이 막혔다는 뜻이다.
            if (!_agent.pathPending)
            {
                _isPathBlocked = _agent.pathStatus != NavMeshPathStatus.PathComplete;
            }

            // agent.velocity에는 주변 회피가 들어 있어서, 좀비들이 한 점에 겹치지 않고 퍼진다.
            Vector3 velocity = _agent.velocity;
            if (velocity.sqrMagnitude < MIN_SQR_VELOCITY)
            {
                velocity = (_agent.steeringTarget - _transform.position).normalized * speed;
            }

            Vector3 current = _transform.position;
            Vector3 next = current + Vector3.ClampMagnitude(velocity, speed) * _runner.DeltaTime;
            RotateTowards(next - current);
            _transform.position = next;
            _agent.nextPosition = next;
        }

        public void Stop()
        {
            if (_agent.hasPath)
            {
                _agent.ResetPath();
            }

            _agent.velocity = Vector3.zero;
            _lastDestination = NoDestination;
        }

        public void FaceTowards(Vector3 point)
        {
            RotateTowards(point - _transform.position);
        }

        private void RotateTowards(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < MIN_SQR_VELOCITY)
            {
                return;
            }

            Quaternion target = Quaternion.LookRotation(direction);
            _transform.rotation = Quaternion.RotateTowards(_transform.rotation, target, TURN_SPEED_DEGREES * _runner.DeltaTime);
        }
    }
}
