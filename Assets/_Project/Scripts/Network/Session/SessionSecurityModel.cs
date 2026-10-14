// 호스트가 들고 있는 이 방 비밀번호의 토큰(방 코드 + 비밀번호 해시, 비밀번호가 없으면 null).
// 참가 요청을 확인하는 데만 쓰고 네트워크로 보내지 않는다.

namespace ZD.Network.Session
{
    public sealed class SessionSecurityModel
    {
        public byte[] ExpectedToken { get; private set; }

        public void SetPassword(string roomCode, string password) => ExpectedToken = SessionProperties.ToToken(roomCode, password);

        public bool Accepts(byte[] token) => ExpectedToken == null || SessionProperties.TokensEqual(ExpectedToken, token);
    }
}
