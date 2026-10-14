// 에디터와 개발 빌드에서만 존재하는 진단 로그.
// 릴리스 빌드에서는 호출 자체가 컴파일에서 빠진다.

using System.Diagnostics;

namespace ZD.Core
{
    public static class DevLog
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Info(string message)
        {
            UnityEngine.Debug.Log("[ZD] " + message);
        }
    }
}
