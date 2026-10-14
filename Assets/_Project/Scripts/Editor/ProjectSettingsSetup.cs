// 설계 문서의 프로젝트 설정(레이어, 물리 충돌 매트릭스, GPU Resident Drawer, 그림자, 빌드 씬 목록)을 한 번에 맞춘다.
// 몇 번을 다시 실행해도 결과가 같다.

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ZD.Editor
{
    internal static class ProjectSettingsSetup
    {
        public const string ENVIRONMENT_LAYER = "Environment";
        public const string BARRICADE_LAYER = "Barricade";

        private const int ENVIRONMENT_LAYER_INDEX = 11;
        private const int BARRICADE_LAYER_INDEX = 12;
        private const string PC_PIPELINE_PATH = "Assets/Settings/PC_RPAsset.asset";
        private const string GAME_SCENE_PATH = "Assets/_Project/Scenes/Game.unity";
        private const float SHADOW_DISTANCE = 40f;
        private const int SHADOW_CASCADES = 2;
        private const int BRG_KEEP_ALL = 2;

        [MenuItem("ZD/Apply Project Settings")]
        public static void Apply()
        {
            EnsureLayers();
            ApplyCollisionMatrix();
            ApplyRendering();
            ApplyBuildScenes();
            AssetDatabase.SaveAssets();
        }

        public static void EnsureLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            SetLayerName(layers, ENVIRONMENT_LAYER_INDEX, ENVIRONMENT_LAYER);
            SetLayerName(layers, BARRICADE_LAYER_INDEX, BARRICADE_LAYER);
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyCollisionMatrix()
        {
            int hitbox = LayerMask.NameToLayer("Hitbox");
            int ui = LayerMask.NameToLayer("UI");
            int player = LayerMask.NameToLayer("Player");
            for (int layerIndex = 0; layerIndex < 32; layerIndex++)
            {
                Physics.IgnoreLayerCollision(hitbox, layerIndex, true);
                Physics.IgnoreLayerCollision(ui, layerIndex, true);
            }

            Physics.IgnoreLayerCollision(player, player, true);
        }

        private static void ApplyRendering()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PC_PIPELINE_PATH);
            pipeline.gpuResidentDrawerMode = GPUResidentDrawerMode.InstancedDrawing;
            pipeline.shadowDistance = SHADOW_DISTANCE;
            pipeline.shadowCascadeCount = SHADOW_CASCADES;
            EditorUtility.SetDirty(pipeline);

            // GPU Resident Drawer는 BatchRendererGroup 셰이더 변형이 빌드에 남아 있어야 동작한다.
            var graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            graphicsSettings.FindProperty("m_BrgStripping").intValue = BRG_KEEP_ALL;
            graphicsSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyBuildScenes()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(GAME_SCENE_PATH, true) };
        }

        private static void SetLayerName(SerializedProperty layers, int index, string layerName)
        {
            SerializedProperty slot = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(slot.stringValue))
            {
                slot.stringValue = layerName;
            }
            else if (slot.stringValue != layerName)
            {
                Debug.LogWarning($"레이어 {index}번에 이미 '{slot.stringValue}'가 있어 '{layerName}'을 넣지 않았습니다.");
            }
        }
    }
}
