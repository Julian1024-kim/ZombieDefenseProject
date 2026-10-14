// 사람이 입력해 다른 플레이어 화면에 보이는 글(닉네임, 방 이름) 정리 규칙.
// 앞뒤 공백과 줄바꿈을 지우고, TMP 서식 태그가 먹지 않게 꺾쇠()를 빼고, 길이를 자른다.
// 비면 기본값을 쓴다.

using System.Text;

namespace ZD.Gameplay.Lobby
{
    public static class TextRules
    {
        public const int MAX_NICKNAME_LENGTH = 12;
        public const int MAX_ROOM_NAME_LENGTH = 20;
        public const int MAX_PASSWORD_LENGTH = 16;
        public const string DEFAULT_NICKNAME = "Player";

        public static string SanitizeNickname(string input) => Sanitize(input, MAX_NICKNAME_LENGTH, DEFAULT_NICKNAME);

        public static string SanitizeRoomName(string input, string hostNickname) => Sanitize(input, MAX_ROOM_NAME_LENGTH, hostNickname + "'s Room");

        public static string SanitizePassword(string input)
        {
            string trimmed = (input ?? string.Empty).Trim();
            return trimmed.Length > MAX_PASSWORD_LENGTH ? trimmed.Substring(0, MAX_PASSWORD_LENGTH) : trimmed;
        }

        private static string Sanitize(string input, int maxLength, string fallback)
        {
            var builder = new StringBuilder(maxLength);
            string source = (input ?? string.Empty).Trim();
            for (int charIndex = 0; charIndex < source.Length && builder.Length < maxLength; charIndex++)
            {
                char character = source[charIndex];
                if (char.IsControl(character) || character == '<' || character == '>')
                {
                    continue;
                }

                builder.Append(character);
            }

            string result = builder.ToString().Trim();
            return result.Length == 0 ? fallback : result;
        }
    }
}
