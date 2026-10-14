// 방 목록에 보이는 세션 속성 키와 비밀번호 토큰 변환.
// 호스트(방 만들기)와 로비(목록 읽기)가 같은 키를 쓴다.
// 비밀번호는 원문이 아니라 "방 코드 + 비밀번호"의 SHA-256 값으로 보낸다(같은 비밀번호라도 방마다 값이 다르다).

using System.Security.Cryptography;
using System.Text;

namespace ZD.Network.Session
{
    public static class SessionProperties
    {
        public const string ROOM_NAME = "n";
        public const string HOST_NAME = "h";
        public const string MODE = "m";
        public const string LOCKED = "l";

        public static byte[] ToToken(string roomCode, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return null;
            }

            using SHA256 sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(roomCode + "\n" + password));
        }

        public static bool TokensEqual(byte[] expected, byte[] actual)
        {
            if (expected == null || actual == null || expected.Length != actual.Length)
            {
                return false;
            }

            int difference = 0;
            for (int byteIndex = 0; byteIndex < expected.Length; byteIndex++)
            {
                difference |= expected[byteIndex] ^ actual[byteIndex];
            }

            return difference == 0;
        }
    }
}
