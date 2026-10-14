// 방어막 정의와 네트워크 프리팹(GUID 참조) 한 쌍.

using System;
using Fusion;
using UnityEngine;
using ZD.Gameplay.Building;

namespace ZD.Network.Session
{
    [Serializable]
    public struct BarricadePrefabEntry
    {
        [SerializeField] private BarricadeDefinition _definition;
        [SerializeField] private NetworkPrefabRef _prefab;

        public BarricadeDefinition Definition => _definition;
        public NetworkPrefabRef Prefab => _prefab;
    }
}
