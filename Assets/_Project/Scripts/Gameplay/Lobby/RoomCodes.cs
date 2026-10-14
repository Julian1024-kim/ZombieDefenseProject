// 방 코드 규칙: 대문자와 숫자 6자리(헷갈리는 I, O, 0, 1은 뺌).
// 방 코드가 곧 세션 이름이다.

using System.Text;
using ZD.Gameplay.Waves;

namespace ZD.Gameplay.Lobby
{
    public static class RoomCodes
    {
        public const int LENGTH = 6;
        private const string ALPHABET = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static string Generate(IRandomSource random)
        {
            var builder = new StringBuilder(LENGTH);
            for (int charIndex = 0; charIndex < LENGTH; charIndex++)
            {
                builder.Append(ALPHABET[random.Next(ALPHABET.Length)]);
            }

            return builder.ToString();
        }

        public static bool TryNormalize(string input, out string code)
        {
            code = (input ?? string.Empty).Trim().ToUpperInvariant();
            if (code.Length != LENGTH)
            {
                return false;
            }

            for (int charIndex = 0; charIndex < code.Length; charIndex++)
            {
                if (ALPHABET.IndexOf(code[charIndex]) < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
