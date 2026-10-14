// 개발 빌드 전용: -zd-perf 옵션으로 실행하면 몇 초마다 관리 메모리 할당과 프레임 시간을 로그로 남긴다.
// 에디터 부담 없이 플레이어 자체를 측정한다(프레임당 0B 목표 확인용).

using System;
using Unity.Profiling;
using UnityEngine;
using ZD.Core;

namespace ZD.Presentation.Diagnostics
{
    [DisallowMultipleComponent]
    public sealed class DevPerfProbe : MonoBehaviour
    {
        private const string COMMAND_LINE_FLAG = "-zd-perf";
        private const float REPORT_PERIOD_SECONDS = 5f;

        private ProfilerRecorder _gcAllocRecorder;
        private ProfilerRecorder _gcCountRecorder;
        private float _windowStart;
        private int _frames;
        private int _framesWithAllocation;
        private long _maxBytes;
        private long _totalBytes;
        private float _maxFrameMs;

        private void Awake()
        {
            enabled = Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(), COMMAND_LINE_FLAG) >= 0;
        }

        private void OnEnable()
        {
            _gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            _gcCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocation In Frame Count");
            _windowStart = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            long bytes = _gcAllocRecorder.LastValue;
            _frames++;
            _totalBytes += bytes;
            _maxBytes = Math.Max(_maxBytes, bytes);
            _maxFrameMs = Mathf.Max(_maxFrameMs, Time.unscaledDeltaTime * 1000f);
            if (bytes > 0)
            {
                _framesWithAllocation++;
            }

            if (Time.realtimeSinceStartup - _windowStart < REPORT_PERIOD_SECONDS)
            {
                return;
            }

            DevLog.Info($"perf: {_frames} frames, GC avg {_totalBytes / _frames} B, max {_maxBytes} B, frames with alloc {_framesWithAllocation}, last alloc count {_gcCountRecorder.LastValue}, worst frame {_maxFrameMs:0.0} ms");
            _windowStart = Time.realtimeSinceStartup;
            _frames = 0;
            _framesWithAllocation = 0;
            _maxBytes = 0;
            _totalBytes = 0;
            _maxFrameMs = 0f;
        }

        private void OnDisable()
        {
            _gcAllocRecorder.Dispose();
            _gcCountRecorder.Dispose();
        }
    }
}
