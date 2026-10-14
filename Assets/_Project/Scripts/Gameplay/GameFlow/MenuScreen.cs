// 시작 화면 안의 화면: 처음(닉네임·모드 고르기), 방 목록, 방 만들기 창, 비밀번호 입력 창.

namespace ZD.Gameplay.GameFlow
{
    public enum MenuScreen : byte
    {
        Main = 0,
        Lobby = 1,
        CreateRoom = 2,
        Password = 3,
    }
}
