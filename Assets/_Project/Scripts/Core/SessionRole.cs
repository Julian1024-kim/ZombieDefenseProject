// 세션에 어떤 역할로 들어가는지.
// Auto는 개발용(같은 이름 세션에 먼저 온 사람이 호스트).

namespace ZD.Core
{
    public enum SessionRole : byte
    {
        Auto = 0,
        Host = 1,
        Client = 2,
    }
}
