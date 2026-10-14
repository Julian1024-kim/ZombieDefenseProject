// Mixamo에서 받은 캐릭터와 애니메이션의 임포트 설정을 맞춘다: 휴머노이드 리그, 내장 텍스처 꺼내기(2048 제한), 반복 여부, 루트 이동 처리.
// 몇 번을 다시 실행해도 결과가 같다.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZD.Editor
{
    internal static class MixamoAssetSetup
    {
        public const string ROOT = "Assets/_Project/Art/Mixamo";
        public const string CHARACTER_FOLDER = ROOT + "/Characters";
        public const string ANIMATION_FOLDER = ROOT + "/Animations";
        public const string AVATAR_SOURCE = CHARACTER_FOLDER + "/Player1.fbx";

        private const string TEXTURE_FOLDER = ROOT + "/Textures";
        private const int MAX_TEXTURE_SIZE = 2048;

        // 반복 재생하는 클립. 나머지(쓰러지기, 일어나기, 재장전, 공격, 죽음 등)는 한 번만 재생한다.
        private static readonly HashSet<string> LoopClips = new()
        {
            "Rifle idle", "Rifle Aiming Idle", "Firing Rifle",
            "Rifle Walk", "Rifle Walk Backward", "Rifle Strafe Left", "Rifle Strafe Right",
            "Rifle Run", "Rifle Run Backward", "Rifle Run Left", "Rifle Run Right",
            "Pistol Idle", "Pistol Walk", "Pistol Run", "Knife Idle",
            "Knocked Down Idle", "Drinking", "Building",
            // 좀비 공격은 공격 상태가 이어지는 동안 반복한다(1.2초 간격으로 한 번씩 휘두름).
            "Zombie Idle", "Zombie Walk", "Zombie Run", "Zombie Crawl", "Zombie Attack",
        };

        // 받을 때 생긴 오타를 바로잡는다(.meta와 GUID는 유지된다).
        private static readonly (string from, string to)[] Renames =
        {
            ("Player/Riring Rifle.fbx", "Firing Rifle.fbx"),
        };

        [MenuItem("ZD/Setup Mixamo Assets")]
        public static void SetupAll()
        {
            ApplyRenames();
            string[] characters = FindModels(CHARACTER_FOLDER);
            for (int characterIndex = 0; characterIndex < characters.Length; characterIndex++)
            {
                SetupCharacter(characters[characterIndex]);
            }

            Avatar sourceAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(AVATAR_SOURCE);
            string[] animations = FindModels(ANIMATION_FOLDER);
            for (int animationIndex = 0; animationIndex < animations.Length; animationIndex++)
            {
                SetupAnimation(animations[animationIndex], sourceAvatar);
            }

            AssetDatabase.SaveAssets();
        }

        public static string[] FindModels(string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folder });
            var paths = new string[guids.Length];
            for (int guidIndex = 0; guidIndex < guids.Length; guidIndex++)
            {
                paths[guidIndex] = AssetDatabase.GUIDToAssetPath(guids[guidIndex]);
            }

            return paths;
        }

        private static void ApplyRenames()
        {
            for (int renameIndex = 0; renameIndex < Renames.Length; renameIndex++)
            {
                string from = $"{ANIMATION_FOLDER}/{Renames[renameIndex].from}";
                if (AssetDatabase.LoadAssetAtPath<Object>(from) == null)
                {
                    continue;
                }

                string error = AssetDatabase.RenameAsset(from, Path.GetFileNameWithoutExtension(Renames[renameIndex].to));
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning($"{from} 이름 변경 실패: {error}");
                }
            }
        }

        private static void SetupCharacter(string path)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.SaveAndReimport();
            EnsureHumanAvatar(path, importer);

            string textureFolder = $"{TEXTURE_FOLDER}/{Path.GetFileNameWithoutExtension(path)}";
            if (!AssetDatabase.IsValidFolder(textureFolder))
            {
                EnsureFolder(TEXTURE_FOLDER);
                AssetDatabase.CreateFolder(TEXTURE_FOLDER, Path.GetFileNameWithoutExtension(path));
                // 내장 텍스처를 파일로 꺼내야 머티리얼이 텍스처를 찾는다.
                importer.ExtractTextures(textureFolder);
                AssetDatabase.Refresh();
                importer.SaveAndReimport();
            }

            SetupTextures(textureFolder);
        }

        private static void EnsureHumanAvatar(string path, ModelImporter importer)
        {
            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(path);
            if ((avatar != null && avatar.isHuman) || path == AVATAR_SOURCE)
            {
                return;
            }

            var source = (ModelImporter)AssetImporter.GetAtPath(AVATAR_SOURCE);
            var boneNames = new HashSet<string>();
            Transform[] bones = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<Transform>(true);
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                boneNames.Add(bones[boneIndex].name);
            }

            HumanBone[] sourceBones = source.humanDescription.human;
            var mapped = new List<HumanBone>(sourceBones.Length);
            for (int boneIndex = 0; boneIndex < sourceBones.Length; boneIndex++)
            {
                if (boneNames.Contains(sourceBones[boneIndex].boneName))
                {
                    mapped.Add(sourceBones[boneIndex]);
                }
            }

            HumanDescription description = importer.humanDescription;
            description.human = mapped.ToArray();
            importer.humanDescription = description;
            importer.SaveAndReimport();

            avatar = AssetDatabase.LoadAssetAtPath<Avatar>(path);
            if (avatar == null || !avatar.isHuman)
            {
                Debug.LogWarning($"{path}: 휴머노이드 아바타를 만들지 못했습니다.");
            }
        }

        private static void SetupTextures(string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            for (int guidIndex = 0; guidIndex < guids.Length; guidIndex++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[guidIndex]);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                bool isNormal = path.Contains("Normal");
                TextureImporterType type = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                if (importer.maxTextureSize == MAX_TEXTURE_SIZE && importer.textureType == type)
                {
                    continue;
                }

                importer.maxTextureSize = MAX_TEXTURE_SIZE;
                importer.textureType = type;
                importer.SaveAndReimport();
            }
        }

        private static void SetupAnimation(string path, Avatar sourceAvatar)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            bool hasSkin = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = hasSkin ? ModelImporterAvatarSetup.CreateFromThisModel : ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = hasSkin ? null : sourceAvatar;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importAnimation = true;

            string clipName = Path.GetFileNameWithoutExtension(path);
            ModelImporterClipAnimation[] defaults = importer.defaultClipAnimations;
            if (defaults.Length == 0)
            {
                importer.SaveAndReimport();
                defaults = importer.defaultClipAnimations;
            }

            ModelImporterClipAnimation clip = defaults[0];
            clip.name = clipName;
            clip.loopTime = LoopClips.Contains(clipName);
            clip.lockRootRotation = true;
            clip.keepOriginalOrientation = true;
            clip.lockRootHeightY = true;
            clip.keepOriginalPositionY = true;
            clip.lockRootPositionXZ = false;
            clip.keepOriginalPositionXZ = true;
            importer.clipAnimations = new[] { clip };
            importer.SaveAndReimport();
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            EnsureFolder(Path.GetDirectoryName(folder).Replace('\\', '/'));
            AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));
        }
    }
}
