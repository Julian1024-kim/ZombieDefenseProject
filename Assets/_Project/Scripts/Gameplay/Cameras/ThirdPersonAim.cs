// 카메라와 조준 계산을 한곳에서 공유한다.
// 호스트가 쏜 사람의 카메라 레이를 위치와 시선으로 다시 계산해서, 탄이 어깨 오프셋만큼 빗나가지 않고 조준점에 맞는다.

using UnityEngine;

namespace ZD.Gameplay.Cameras
{
    public static class ThirdPersonAim
    {
        public static Pose GetCameraPose(CameraSettings settings, Vector3 playerPosition, float pitch, float yaw)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = playerPosition + settings.PivotOffset;
            return new Pose(pivot + rotation * settings.ShoulderOffset, rotation);
        }

        public static void GetAimRay(CameraSettings settings, Vector3 playerPosition, float pitch, float yaw, out Vector3 origin, out Vector3 direction)
        {
            Pose camera = GetCameraPose(settings, playerPosition, pitch, yaw);
            direction = camera.rotation * Vector3.forward;
            Vector3 eye = playerPosition + settings.PivotOffset;
            float distanceToEyePlane = Vector3.Dot(eye - camera.position, direction);
            origin = camera.position + direction * Mathf.Max(0f, distanceToEyePlane);
        }
    }
}
