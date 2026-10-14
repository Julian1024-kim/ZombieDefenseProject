// 설정 저장 DTO(살아 있는 모델과 따로).
// 지금은 닉네임만 있고, 감도·그래픽 설정이 생기면 여기에 더한다(스키마 버전을 올려 옮김).

using System;

namespace ZD.Gameplay.Lobby
{
    [Serializable]
    public sealed class SettingsSaveData
    {
        public const int CURRENT_SCHEMA_VERSION = 1;

        public int SchemaVersion = CURRENT_SCHEMA_VERSION;
        public string Nickname = string.Empty;
    }
}
