// 설정(닉네임)을 persistentDataPath/settings.json에 JSON으로 저장한다.
// 파일에 손대는 유일한 곳이다.
// 파일이 없거나 깨졌으면 빈 설정으로 시작하고, 옛 스키마는 불러올 때 옮긴다.

using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZD.Gameplay.Lobby;

namespace ZD.Bootstrap
{
    internal sealed class FileSettingsRepository : ISettingsRepository
    {
        private const string FILE_NAME = "settings.json";

        private readonly string _filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);

        public async UniTask<SettingsSaveData> LoadAsync(CancellationToken token)
        {
            if (!File.Exists(_filePath))
            {
                return new SettingsSaveData();
            }

            string json = await File.ReadAllTextAsync(_filePath, token);
            SettingsSaveData data = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<SettingsSaveData>(json);
            return data == null ? new SettingsSaveData() : MigrateIfNeeded(data);
        }

        public async UniTask SaveAsync(SettingsSaveData data, CancellationToken token)
        {
            data.SchemaVersion = SettingsSaveData.CURRENT_SCHEMA_VERSION;
            await File.WriteAllTextAsync(_filePath, JsonUtility.ToJson(data), token);
        }

        private static SettingsSaveData MigrateIfNeeded(SettingsSaveData data)
        {
            data.Nickname ??= string.Empty;
            data.SchemaVersion = SettingsSaveData.CURRENT_SCHEMA_VERSION;
            return data;
        }
    }
}
