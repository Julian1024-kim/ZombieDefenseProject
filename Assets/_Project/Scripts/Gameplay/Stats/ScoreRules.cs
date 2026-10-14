// 점수 규칙(설계 문서 추천안): 팀 점수 = 클리어한 웨이브 수 × 1,000 + 팀 전체 누적 획득 Point.
// 지갑 잔액이 아니라 누적 획득량을 쓰므로 무기를 산 플레이어가 손해 보지 않는다.
// 하루는 6웨이브다.

namespace ZD.Gameplay.Stats
{
    public static class ScoreRules
    {
        public const int POINTS_PER_WAVE = 1000;
        public const int WAVES_PER_DAY = 6;

        public static int TeamScore(int wavesCleared, int totalPointsEarned) => wavesCleared * POINTS_PER_WAVE + totalPointsEarned;

        public static int DaysSurvived(int wavesCleared) => wavesCleared / WAVES_PER_DAY;
    }
}
