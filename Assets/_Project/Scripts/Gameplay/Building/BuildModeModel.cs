// 로컬 전용 설치 모드 상태: 켜짐 여부, 고른 종류, 미리보기 위치와 판정 결과, 호스트의 거절 응답.

using R3;
using UnityEngine;

namespace ZD.Gameplay.Building
{
    public sealed class BuildModeModel
    {
        public ReactiveProperty<bool> IsActive { get; } = new(false);
        public ReactiveProperty<int> SelectedIndex { get; } = new(0);
        public ReactiveProperty<PlacementResult> PreviewResult { get; } = new(PlacementResult.Unavailable);
        public ReactiveProperty<Pose> PreviewPose { get; } = new(Pose.identity);

        public Subject<PlacementResult> Rejections { get; } = new();

        public IBuildRequestSender RequestSender { get; set; }
    }
}
