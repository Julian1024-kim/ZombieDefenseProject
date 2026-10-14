// Fusion이 만드는 모든 NetworkObject에, Spawned()가 불리기 전에 GameLifetimeScope의 의존성을 주입한다.

using Fusion;
using VContainer;
using VContainer.Unity;

namespace ZD.Network.Session
{
    public sealed class InjectingObjectProvider : NetworkObjectProviderDefault
    {
        private IObjectResolver _resolver;

        public void SetResolver(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            NetworkObject instance = base.InstantiatePrefab(runner, prefab);
            _resolver.InjectGameObject(instance.gameObject);
            return instance;
        }
    }
}
