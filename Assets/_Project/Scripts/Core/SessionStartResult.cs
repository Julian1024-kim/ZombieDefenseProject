// 세션 시작 결과.
// 참가 실패는 이유별로 나눠 로비에서 알려 준다.

namespace ZD.Core
{
    public enum SessionStartResult : byte
    {
        Ok = 0,
        RoomNotFound = 1,
        RoomFull = 2,
        WrongPassword = 3,
        Failed = 4,
    }
}
