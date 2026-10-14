// 설정 저장소.
// 저장 방식은 이 계약 뒤에 숨긴다.
// 저장본이 없으면 빈 DTO를 돌려준다.

using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZD.Gameplay.Lobby
{
    public interface ISettingsRepository
    {
        UniTask<SettingsSaveData> LoadAsync(CancellationToken token);

        UniTask SaveAsync(SettingsSaveData data, CancellationToken token);
    }
}
