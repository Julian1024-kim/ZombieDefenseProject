// 테스트용 매치 상태 모델(확정 결과 포함).

using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEditor;
using UnityEngine;
using ZD.Core;
using ZD.Gameplay.Combat;
using ZD.Gameplay.GameFlow;
using ZD.Gameplay.Players;
using ZD.Gameplay.Stats;
using ZD.Gameplay.Waves;
using ZD.Gameplay.Weapons;
using ZD.Gameplay.Zombies;

namespace ZD.Tests.EditMode.Fakes
{
    internal sealed class FakeGameFlow : IGameFlowModel, IMatchResultModel
    {
        public GameModeId Mode { get; set; }
        public GamePhase Phase { get; set; }
        public int PhaseEndTick { get; set; }
        public int WaveNumber { get; set; }
        public IMatchResultModel Result => this;
        public int Version { get; set; }
        public int TeamScore { get; set; }
        public int WavesCleared { get; set; }
        public int PlayerCount { get; set; }
        public bool IsBaseDestroyed { get; set; }
        private readonly PlayerResult[] _rows = new PlayerResult[MatchResultSystem.MAX_PLAYERS];

        public PlayerResult GetPlayer(int index) => _rows[index];

        public void SetPlayer(int index, in PlayerResult result) => _rows[index] = result;
    }
}
