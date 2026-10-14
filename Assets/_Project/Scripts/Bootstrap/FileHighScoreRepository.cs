// 최고 점수를 persistentDataPath/highscores.json에 JSON으로 저장한다.
// 파일에 손대는 유일한 곳이다.
// 파일이 없거나 깨졌으면 빈 기록으로 시작하고, 옛 스키마는 불러올 때 옮긴다.

using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZD.Gameplay.Stats;

namespace ZD.Bootstrap
{
    internal sealed class FileHighScoreRepository : IHighScoreRepository
    {
        private const string FILE_NAME = "highscores.json";

        private readonly string _filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);

        public async UniTask<HighScoreSaveData> LoadAsync(CancellationToken token)
        {
            if (!File.Exists(_filePath))
            {
                return new HighScoreSaveData();
            }

            string json = await File.ReadAllTextAsync(_filePath, token);
            HighScoreSaveData data = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<HighScoreSaveData>(json);
            return data == null ? new HighScoreSaveData() : MigrateIfNeeded(data);
        }

        public async UniTask SaveAsync(HighScoreSaveData data, CancellationToken token)
        {
            data.SchemaVersion = HighScoreSaveData.CURRENT_SCHEMA_VERSION;
            await File.WriteAllTextAsync(_filePath, JsonUtility.ToJson(data), token);
        }

        private static HighScoreSaveData MigrateIfNeeded(HighScoreSaveData data)
        {
            data.BestByMode ??= System.Array.Empty<int>();
            data.SchemaVersion = HighScoreSaveData.CURRENT_SCHEMA_VERSION;
            return data;
        }
    }
}
