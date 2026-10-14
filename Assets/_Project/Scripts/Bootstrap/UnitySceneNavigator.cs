// Bootstrap 씬을 그대로 두고 Game 씬을 덧붙여 불러오거나 내린다.
// 시작 화면은 게임 중에 숨긴다.
// Game 씬의 LifetimeScope는 이 Bootstrap 씬의 RootLifetimeScope를 부모로 찾는다.

using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using ZD.Core;
using ZD.Gameplay.GameFlow;

namespace ZD.Bootstrap
{
    internal sealed class UnitySceneNavigator : ISceneNavigator
    {
        private const string GAME_SCENE = "Game";

        private readonly MenuModel _menu;

        public UnitySceneNavigator(MenuModel menu)
        {
            _menu = menu;
        }

        public async UniTask EnterGameAsync()
        {
            await SceneManager.LoadSceneAsync(GAME_SCENE, LoadSceneMode.Additive).ToUniTask();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(GAME_SCENE));
            _menu.IsVisible.Value = false;
        }

        public async UniTask ReturnToMenuAsync(MenuReturnReason reason)
        {
            Scene game = SceneManager.GetSceneByName(GAME_SCENE);
            if (game.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(game).ToUniTask();
            }

            DevLog.Info($"Returned to the menu ({reason})");
            _menu.Notice.Value = ToNotice(reason);
            _menu.IsVisible.Value = true;
        }

        private static MenuNotice ToNotice(MenuReturnReason reason)
        {
            return reason switch
            {
                MenuReturnReason.HostLeft => MenuNotice.HostLeft,
                MenuReturnReason.RoomNotFound => MenuNotice.RoomNotFound,
                MenuReturnReason.RoomFull => MenuNotice.RoomFull,
                MenuReturnReason.WrongPassword => MenuNotice.WrongPassword,
                MenuReturnReason.ConnectFailed => MenuNotice.ConnectFailed,
                _ => MenuNotice.None,
            };
        }
    }
}
