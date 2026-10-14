// Fusion 네트워크 시뮬레이션(NetworkProjectConfig의 NetworkConditions) 프리셋 메뉴.
// MVP 기준인 핑 100ms에서 예측과 Lag Compensation 판정을 확인하는 데 쓴다.
// 지연은 각 피어가 보내는 쪽에 걸리므로 단방향 지연 = 핑 / 2다.

using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace ZD.Editor
{
    internal static class NetworkConditionsMenu
    {
        private const string CONFIG_PATH = "Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion";
        private const string MENU_ROOT = "ZD/Network Conditions/";

        [MenuItem(MENU_ROOT + "Off")]
        private static void Off() => Apply(false, 0.0, 0.0, 0.0);

        [MenuItem(MENU_ROOT + "Ping 100 ms")]
        private static void Ping100() => Apply(true, 0.05, 0.005, 0.0);

        [MenuItem(MENU_ROOT + "Ping 150 ms + 2% Loss")]
        private static void Ping150WithLoss() => Apply(true, 0.075, 0.005, 0.02);

        private static void Apply(bool isEnabled, double oneWayDelaySeconds, double jitterSeconds, double lossChance)
        {
            string json = File.ReadAllText(CONFIG_PATH);
            json = SetValue(json, "Enabled", isEnabled ? "true" : "false");
            json = SetValue(json, "DelayMin", Format(oneWayDelaySeconds));
            json = SetValue(json, "DelayMax", Format(oneWayDelaySeconds));
            json = SetValue(json, "AdditionalJitter", Format(jitterSeconds));
            json = SetValue(json, "LossChanceMin", Format(lossChance));
            json = SetValue(json, "LossChanceMax", Format(lossChance));
            File.WriteAllText(CONFIG_PATH, json);
            AssetDatabase.ImportAsset(CONFIG_PATH, ImportAssetOptions.ForceUpdate);
        }

        private static string SetValue(string json, string key, string value)
        {
            Match block = Regex.Match(json, "\"NetworkConditions\"\\s*:\\s*\\{[^}]*\\}");
            if (!block.Success)
            {
                throw new InvalidDataException("NetworkConditions block not found in " + CONFIG_PATH);
            }

            string updated = Regex.Replace(block.Value, $"(\"{key}\"\\s*:\\s*)[^,\\n}}]+", "${1}" + value);
            return json.Substring(0, block.Index) + updated + json.Substring(block.Index + block.Length);
        }

        private static string Format(double value) => value.ToString("0.0###", CultureInfo.InvariantCulture);
    }
}
