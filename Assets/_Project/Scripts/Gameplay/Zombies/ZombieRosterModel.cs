// 호스트의 활성 좀비 명단.
// AI 시스템이 루프 하나로 돌 수 있게 모델과 이동기를 나란한 리스트로 저장한다.

using System.Collections.Generic;

namespace ZD.Gameplay.Zombies
{
    public sealed class ZombieRosterModel
    {
        private const int INITIAL_CAPACITY = 128;

        private readonly List<IZombieModel> _models = new(INITIAL_CAPACITY);
        private readonly List<IZombieMotor> _motors = new(INITIAL_CAPACITY);

        public int Count => _models.Count;

        public int AliveCount
        {
            get
            {
                int alive = 0;
                for (int zombieIndex = 0; zombieIndex < _models.Count; zombieIndex++)
                {
                    if (_models[zombieIndex].Health > 0)
                    {
                        alive++;
                    }
                }

                return alive;
            }
        }

        public IZombieModel GetModel(int index) => _models[index];

        public IZombieModel FindById(int entityId)
        {
            for (int zombieIndex = 0; zombieIndex < _models.Count; zombieIndex++)
            {
                if (_models[zombieIndex].EntityId == entityId)
                {
                    return _models[zombieIndex];
                }
            }

            return null;
        }

        public IZombieMotor GetMotor(int index) => _motors[index];

        public void Register(IZombieModel model, IZombieMotor motor)
        {
            _models.Add(model);
            _motors.Add(motor);
        }

        public void Unregister(IZombieModel model)
        {
            int index = _models.IndexOf(model);
            if (index < 0)
            {
                return;
            }

            int lastIndex = _models.Count - 1;
            _models[index] = _models[lastIndex];
            _motors[index] = _motors[lastIndex];
            _models.RemoveAt(lastIndex);
            _motors.RemoveAt(lastIndex);
        }
    }
}
