// 개발 빌드 전용 명령줄 옵션을 읽는다.
// 봇·2인 테스트 빌드가 로비를 건너뛰게 한다.
// -zd-mode=base|survival: 그 모드의 개발용 세션(먼저 온 사람이 호스트)에 바로 들어간다.
// -zd-join=방코드 [-zd-password=비밀번호]: 그 방에 참가한다(방 만들기·비밀번호 확인 시험용).
// -zd-nickname=이름: 저장하지 않고 이번 실행에만 닉네임을 바꾼다(같은 컴퓨터의 에디터와 빌드가 설정 파일을 같이 쓰므로).

using System;
using UnityEngine;
using ZD.Gameplay.GameFlow;

namespace ZD.Bootstrap
{
    internal static class CommandLineMode
    {
        private const string MODE_PREFIX = "-zd-mode=";
        private const string JOIN_PREFIX = "-zd-join=";
        private const string PASSWORD_PREFIX = "-zd-password=";
        private const string NICKNAME_PREFIX = "-zd-nickname=";
        private const string BASE = "base";
        private const string SURVIVAL = "survival";

        public static bool TryRead(out GameModeId mode)
        {
            mode = GameModeId.Survival;
            if (!TryReadValue(MODE_PREFIX, out string value))
            {
                return false;
            }

            if (string.Equals(value, BASE, StringComparison.OrdinalIgnoreCase))
            {
                mode = GameModeId.BaseDefense;
                return true;
            }

            return string.Equals(value, SURVIVAL, StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryReadJoin(out string code, out string password)
        {
            password = TryReadValue(PASSWORD_PREFIX, out string value) ? value : string.Empty;
            return TryReadValue(JOIN_PREFIX, out code);
        }

        public static bool TryReadNickname(out string nickname) => TryReadValue(NICKNAME_PREFIX, out nickname);

        private static bool TryReadValue(string prefix, out string value)
        {
            value = string.Empty;
            if (!Debug.isDebugBuild)
            {
                return false;
            }

            string[] args = Environment.GetCommandLineArgs();
            for (int argIndex = 0; argIndex < args.Length; argIndex++)
            {
                if (args[argIndex].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    value = args[argIndex].Substring(prefix.Length);
                    return true;
                }
            }

            return false;
        }
    }
}
