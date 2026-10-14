// 앱 시작 때 저장된 닉네임을 불러오고, 개발 빌드를 -zd-join(방 참가)이나 -zd-mode(개발용 세션) 옵션으로 실행했으면 로비를 건너뛰고 바로 들어간다.

using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Lobby;

namespace ZD.Bootstrap
{
    internal sealed class CommandLineStartEntryPoint : IAsyncStartable
    {
        private readonly MenuSystem _menu;
        private readonly ProfileSystem _profile;
        private readonly PlayerProfileModel _profileModel;

        public CommandLineStartEntryPoint(MenuSystem menu, ProfileSystem profile, PlayerProfileModel profileModel)
        {
            _profileModel = profileModel;
            _menu = menu;
            _profile = profile;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            await _profile.LoadAsync();
            if (CommandLineMode.TryReadNickname(out string nickname))
            {
                _profileModel.Nickname.Value = TextRules.SanitizeNickname(nickname);
            }

            if (CommandLineMode.TryReadJoin(out string code, out string password))
            {
                _menu.JoinWithCode(code, password);
            }
            else if (CommandLineMode.TryRead(out GameModeId mode))
            {
                _menu.StartDev(mode);
            }
        }
    }
}
