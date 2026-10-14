// 총구 위치를 알려주는 쪽(손에 든 무기 모델).
// 사격 이펙트가 실제 총구에서 나가게 한다.

using UnityEngine;

namespace ZD.Gameplay.Players
{
    public interface IMuzzlePoint
    {
        bool TryGetMuzzlePosition(out Vector3 position);
    }
}
