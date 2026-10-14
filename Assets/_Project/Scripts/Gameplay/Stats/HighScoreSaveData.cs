// 최고 점수 저장 DTO(살아 있는 모델과 따로).
// 모드별 최고 점수를 GameModeId 순서로 담고, 스키마 버전으로 옛 저장본을 옮긴다.

using System;

namespace ZD.Gameplay.Stats
{
    [Serializable]
    public sealed class HighScoreSaveData
    {
        public const int CURRENT_SCHEMA_VERSION = 1;

        public int SchemaVersion = CURRENT_SCHEMA_VERSION;
        public int[] BestByMode = Array.Empty<int>();
    }
}
