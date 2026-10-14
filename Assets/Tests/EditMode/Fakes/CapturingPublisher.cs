// 발행된 메시지를 목록에 모아두는 테스트용 발행자.

using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEditor;
using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Combat;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Players;
using ZD.Gameplay.Waves;
using ZD.Gameplay.Weapons;
using ZD.Gameplay.Zombies;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class CapturingPublisher<T> : IPublisher<T>
    {
        public List<T> Messages { get; } = new();

        public void Publish(T message) => Messages.Add(message);
    }
}
