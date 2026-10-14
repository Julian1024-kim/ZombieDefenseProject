// 게임 오버 때 확정한 매치 결과.
// 네트워크 계층이 [Networked] 값으로 구현해 모든 피어가 같은 결과를 본다.
// Version은 결과를 확정할 때마다 오른다(각 피어가 새 결과인지 알아보는 데 쓴다).

namespace ZD.Gameplay.Stats
{
    public interface IMatchResultModel
    {
        int Version { get; set; }
        int TeamScore { get; set; }
        int WavesCleared { get; set; }
        int PlayerCount { get; set; }

        bool IsBaseDestroyed { get; set; }

        PlayerResult GetPlayer(int index);

        void SetPlayer(int index, in PlayerResult result);
    }
}
