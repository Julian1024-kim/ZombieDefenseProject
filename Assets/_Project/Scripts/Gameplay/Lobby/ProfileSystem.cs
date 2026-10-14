// 닉네임을 설정 파일에서 불러오고, 바꾸면 정리해서 저장한다.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZD.Gameplay.Lobby
{
    public sealed class ProfileSystem : IDisposable
    {
        private readonly PlayerProfileModel _profile;
        private readonly ISettingsRepository _repository;
        private readonly CancellationTokenSource _cts = new();

        public ProfileSystem(PlayerProfileModel profile, ISettingsRepository repository)
        {
            _profile = profile;
            _repository = repository;
        }

        public async UniTask LoadAsync()
        {
            SettingsSaveData data = await _repository.LoadAsync(_cts.Token);
            _profile.Nickname.Value = TextRules.SanitizeNickname(data.Nickname);
        }

        public string SetNickname(string input)
        {
            string nickname = TextRules.SanitizeNickname(input);
            if (nickname != _profile.Nickname.Value)
            {
                _profile.Nickname.Value = nickname;
                _repository.SaveAsync(new SettingsSaveData { Nickname = nickname }, _cts.Token).Forget();
            }

            return nickname;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
