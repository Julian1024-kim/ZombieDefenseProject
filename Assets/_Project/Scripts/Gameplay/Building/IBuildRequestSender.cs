// 로컬 설치 모드가 호스트에 설치를 요청하는 통로.
// 로컬 플레이어 오브젝트가 구현한다.

using UnityEngine;

namespace ZD.Gameplay.Building
{
    public interface IBuildRequestSender
    {
        void SendBuildRequest(int itemIndex, Vector3 target, float yaw);
    }
}
