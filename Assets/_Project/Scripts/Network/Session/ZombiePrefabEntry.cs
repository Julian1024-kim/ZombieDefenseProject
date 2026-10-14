// 좀비 정의와 네트워크 프리팹(GUID 참조) 한 쌍.

using System;
using Fusion;
using UnityEngine;
using ZD.Gameplay.Zombies;

namespace ZD.Network.Session
{
    [Serializable]
    public struct ZombiePrefabEntry
    {
        [SerializeField] private ZombieDefinition _definition;
        [SerializeField] private NetworkPrefabRef _prefab;

        public ZombieDefinition Definition => _definition;
        public NetworkPrefabRef Prefab => _prefab;
    }
}
