// 플레이어와 좀비의 Animator 컨트롤러, 상체 마스크를 코드로 만든다.
// 다시 실행하면 같은 구성으로 새로 만든다.
// 플레이어: 하체 이동 블렌드 + 쓰러짐/부활/설치 전신 동작, 상체 레이어는 Upper 값 하나로 상태를 고른다.
// 좀비: State(추적/공격/죽음)와 Speed(걷기/달리기)로 고른다.

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ZD.Editor
{
    internal static class CharacterAnimatorBuilder
    {
        public const string ANIMATOR_FOLDER = "Assets/_Project/Art/Animators";
        public const string PLAYER_CONTROLLER = ANIMATOR_FOLDER + "/PlayerAnimator.controller";
        public const string ZOMBIE_CONTROLLER = ANIMATOR_FOLDER + "/ZombieAnimator.controller";

        private const string UPPER_MASK = ANIMATOR_FOLDER + "/UpperBody.mask";
        private const string PLAYER_CLIPS = MixamoAssetSetup.ANIMATION_FOLDER + "/Player/";
        private const string ZOMBIE_CLIPS = MixamoAssetSetup.ANIMATION_FOLDER + "/Zombie/";

        // 게임 이동 속도(5m/s)가 애니메이션 보폭보다 빨라서, 달리기는 방향마다 잰 보폭(앞 2.5, 뒤 3.1, 왼쪽 2.5, 오른쪽 3.2m/s)에 맞춰
        // 빠르게 재생해 발 미끄러짐을 줄인다. 너무 빨라 보이지 않게 1.8배에서 멈춘다.
        private const float RUN_FORWARD_TIME_SCALE = 1.8f;
        private const float RUN_BACKWARD_TIME_SCALE = 1.6f;
        private const float RUN_LEFT_TIME_SCALE = 1.8f;
        private const float RUN_RIGHT_TIME_SCALE = 1.55f;
        private const float MELEE_SECONDS = 0.6f;
        private const float RELOAD_SECONDS = 2f;
        private const float BLEND_SECONDS = 0.15f;
        private const float DOWNED_BLEND_SECONDS = 0.4f;

        // 좀비: 일반 좀비 속도 2.2m/s에서 달리기 보폭이 맞도록(모델을 0.9배로 줄인 것 포함), 공격은 1.2초 간격에 맞춘다.
        private const float ZOMBIE_WALK_SPEED = 0.6f;
        private const float ZOMBIE_RUN_SPEED = 2.2f;
        private const float ZOMBIE_RUN_TIME_SCALE = 1.45f;
        private const float ZOMBIE_ATTACK_SECONDS = 1.2f;

        // PlayerAnimatorView의 상체 상태 번호와 같아야 한다.
        private const int UPPER_RIFLE = 0;
        private const int UPPER_PISTOL = 1;
        private const int UPPER_KNIFE = 2;
        private const int UPPER_HEAL = 3;
        private const int UPPER_RELOAD = 4;
        private const int UPPER_RIFLE_FIRE = 5;
        private const int UPPER_UNARMED = 6;
        private const int UPPER_KNIFE_ATTACK = 7;

        [MenuItem("ZD/Build Character Animators")]
        public static void BuildAll()
        {
            EnsureFolder();
            AvatarMask upperMask = CreateUpperBodyMask();
            BuildPlayerController(upperMask);
            BuildZombieController();
            AssetDatabase.SaveAssets();
        }

        private static void BuildPlayerController(AvatarMask upperMask)
        {
            AnimatorController controller = CreateController(PLAYER_CONTROLLER);
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Downed", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Reviving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Building", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Upper", AnimatorControllerParameterType.Int);

            AnimatorStateMachine body = controller.layers[0].stateMachine;
            AnimatorState locomotion = controller.CreateBlendTreeInController("Locomotion", out BlendTree tree, 0);
            tree.blendType = BlendTreeType.FreeformDirectional2D;
            tree.blendParameter = "MoveX";
            tree.blendParameterY = "MoveY";
            tree.AddChild(PlayerClip("Rifle idle"), Vector2.zero);
            tree.AddChild(PlayerClip("Rifle Walk"), new Vector2(0f, 0.5f));
            tree.AddChild(PlayerClip("Rifle Walk Backward"), new Vector2(0f, -0.5f));
            tree.AddChild(PlayerClip("Rifle Strafe Left"), new Vector2(-0.5f, 0f));
            tree.AddChild(PlayerClip("Rifle Strafe Right"), new Vector2(0.5f, 0f));
            tree.AddChild(PlayerClip("Rifle Run"), new Vector2(0f, 1f));
            tree.AddChild(PlayerClip("Rifle Run Backward"), new Vector2(0f, -1f));
            tree.AddChild(PlayerClip("Rifle Run Left"), new Vector2(-1f, 0f));
            tree.AddChild(PlayerClip("Rifle Run Right"), new Vector2(1f, 0f));
            ScaleRunChildren(tree);
            body.defaultState = locomotion;

            AnimatorState knockedDown = AddState(body, "KnockedDown", PlayerClip("Knocked Down"));
            AnimatorState downedIdle = AddState(body, "DownedIdle", PlayerClip("Knocked Down Idle"));
            AnimatorState standUp = AddState(body, "StandUp", PlayerClip("Stand Up"));
            AnimatorState kneeling = AddState(body, "Kneeling", PlayerClip("Kneeling"));
            AnimatorState building = AddState(body, "Building", PlayerClip("Building"));

            AddBoolTransition(locomotion, knockedDown, "Downed", true, BLEND_SECONDS);
            AddBoolTransition(kneeling, knockedDown, "Downed", true, BLEND_SECONDS);
            AddBoolTransition(standUp, knockedDown, "Downed", true, BLEND_SECONDS);
            AddExitTransition(knockedDown, downedIdle, 0.95f, DOWNED_BLEND_SECONDS);
            AddBoolTransition(downedIdle, standUp, "Downed", false, DOWNED_BLEND_SECONDS);
            AddBoolTransition(knockedDown, standUp, "Downed", false, DOWNED_BLEND_SECONDS);
            AddExitTransition(standUp, locomotion, 0.9f, 0.3f);
            AddBoolTransition(locomotion, kneeling, "Reviving", true, 0.25f);
            AddBoolTransition(kneeling, locomotion, "Reviving", false, 0.3f);
            AddBoolTransition(locomotion, building, "Building", true, 0.25f);
            AddBoolTransition(building, locomotion, "Building", false, 0.3f);
            AddBoolTransition(building, knockedDown, "Downed", true, BLEND_SECONDS);

            controller.AddLayer("UpperBody");
            AnimatorControllerLayer[] layers = controller.layers;
            layers[1].avatarMask = upperMask;
            layers[1].blendingMode = AnimatorLayerBlendingMode.Override;
            layers[1].defaultWeight = 1f;
            controller.layers = layers;

            AnimatorStateMachine upper = controller.layers[1].stateMachine;
            AnimatorState rifle = AddUpperState(upper, "RifleHold", PlayerClip("Rifle Aiming Idle"), UPPER_RIFLE);
            // 권총은 뛸 때도 팔을 앞으로 겨눈 자세를 유지한다(Pistol Run 상체는 두 손이 얼굴 높이로 올라가 어색하다).
            AddUpperState(upper, "PistolHold", PlayerClip("Pistol Idle"), UPPER_PISTOL);
            AddUpperState(upper, "Unarmed", PlayerClip("Pistol Idle"), UPPER_UNARMED);
            AddUpperState(upper, "KnifeHold", PlayerClip("Knife Idle"), UPPER_KNIFE);
            AddUpperState(upper, "Heal", PlayerClip("Drinking"), UPPER_HEAL);
            AnimatorState reload = AddUpperState(upper, "Reload", PlayerClip("Reloading"), UPPER_RELOAD);
            reload.speed = PlayerClip("Reloading").length / RELOAD_SECONDS;
            AddUpperState(upper, "RifleFire", PlayerClip("Firing Rifle"), UPPER_RIFLE_FIRE);
            AnimatorState knifeAttack = AddUpperState(upper, "KnifeAttack", PlayerClip("Knife Attack"), UPPER_KNIFE_ATTACK);
            knifeAttack.speed = PlayerClip("Knife Attack").length / MELEE_SECONDS;
            // Mixamo Knife Attack은 왼손으로 찌른다. 칼은 오른손에 들므로 좌우를 뒤집는다(휴머노이드 미러).
            knifeAttack.mirror = true;
            upper.defaultState = rifle;

            EditorUtility.SetDirty(controller);
        }

        private static void BuildZombieController()
        {
            AnimatorController controller = CreateController(ZOMBIE_CONTROLLER);
            controller.AddParameter("State", AnimatorControllerParameterType.Int);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState locomotion = controller.CreateBlendTreeInController("Locomotion", out BlendTree tree, 0);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            tree.AddChild(ZombieClip("Zombie Idle"), 0f);
            tree.AddChild(ZombieClip("Zombie Walk"), ZOMBIE_WALK_SPEED);
            tree.AddChild(ZombieClip("Zombie Run"), ZOMBIE_RUN_SPEED);
            ChildMotion[] children = tree.children;
            children[2].timeScale = ZOMBIE_RUN_TIME_SCALE;
            tree.children = children;
            machine.defaultState = locomotion;

            AnimatorState attack = AddState(machine, "Attack", ZombieClip("Zombie Attack"));
            attack.speed = ZombieClip("Zombie Attack").length / ZOMBIE_ATTACK_SECONDS;
            AnimatorState dying = AddState(machine, "Dying", ZombieClip("Zombie Dying"));

            // 1: 플레이어 공격, 3: 방어막 공격. 둘 다 같은 공격 동작을 쓴다.
            AddIntTransition(locomotion, attack, "State", 1, BLEND_SECONDS);
            AddIntTransition(locomotion, attack, "State", 3, BLEND_SECONDS);
            AddIntTransition(attack, locomotion, "State", 0, 0.2f);
            AddIntTransition(locomotion, dying, "State", 2, BLEND_SECONDS);
            AddIntTransition(attack, dying, "State", 2, BLEND_SECONDS);

            EditorUtility.SetDirty(controller);
        }

        private static AvatarMask CreateUpperBodyMask()
        {
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(UPPER_MASK);
            if (mask == null)
            {
                mask = new AvatarMask();
                AssetDatabase.CreateAsset(mask, UPPER_MASK);
            }

            for (int partIndex = 0; partIndex < (int)AvatarMaskBodyPart.LastBodyPart; partIndex++)
            {
                var part = (AvatarMaskBodyPart)partIndex;
                bool isUpper = part == AvatarMaskBodyPart.Body || part == AvatarMaskBodyPart.Head
                    || part == AvatarMaskBodyPart.LeftArm || part == AvatarMaskBodyPart.RightArm
                    || part == AvatarMaskBodyPart.LeftFingers || part == AvatarMaskBodyPart.RightFingers;
                mask.SetHumanoidBodyPartActive(part, isUpper);
            }

            EditorUtility.SetDirty(mask);
            return mask;
        }

        private static void ScaleRunChildren(BlendTree tree)
        {
            ChildMotion[] children = tree.children;
            for (int childIndex = 0; childIndex < children.Length; childIndex++)
            {
                Vector2 position = children[childIndex].position;
                if (position.magnitude < 0.9f)
                {
                    continue;
                }

                children[childIndex].timeScale = Mathf.Abs(position.y) > Mathf.Abs(position.x)
                    ? (position.y > 0f ? RUN_FORWARD_TIME_SCALE : RUN_BACKWARD_TIME_SCALE)
                    : (position.x > 0f ? RUN_RIGHT_TIME_SCALE : RUN_LEFT_TIME_SCALE);
            }

            tree.children = children;
        }

        private static AnimatorState AddUpperState(AnimatorStateMachine machine, string name, AnimationClip clip, int upperValue)
        {
            AnimatorState state = AddState(machine, name, clip);
            AddUpperTransition(machine, state, upperValue);
            return state;
        }

        private static void AddUpperTransition(AnimatorStateMachine machine, AnimatorState state, int upperValue)
        {
            AnimatorStateTransition transition = machine.AddAnyStateTransition(state);
            transition.AddCondition(AnimatorConditionMode.Equals, upperValue, "Upper");
            transition.canTransitionToSelf = false;
            transition.hasExitTime = false;
            transition.duration = BLEND_SECONDS;
        }

        private static AnimatorState AddState(AnimatorStateMachine machine, string name, AnimationClip clip)
        {
            AnimatorState state = machine.AddState(name);
            state.motion = clip;
            return state;
        }

        private static void AddBoolTransition(AnimatorState from, AnimatorState to, string parameter, bool value, float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameter);
            transition.hasExitTime = false;
            transition.duration = duration;
        }

        private static void AddIntTransition(AnimatorState from, AnimatorState to, string parameter, int value, float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.AddCondition(AnimatorConditionMode.Equals, value, parameter);
            transition.hasExitTime = false;
            transition.duration = duration;
        }

        private static void AddExitTransition(AnimatorState from, AnimatorState to, float exitTime, float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
        }

        private static AnimatorController CreateController(string path)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
            {
                return AnimatorController.CreateAnimatorControllerAtPath(path);
            }

            for (int layerIndex = controller.layers.Length - 1; layerIndex >= 0; layerIndex--)
            {
                controller.RemoveLayer(layerIndex);
            }

            for (int parameterIndex = controller.parameters.Length - 1; parameterIndex >= 0; parameterIndex--)
            {
                controller.RemoveParameter(parameterIndex);
            }

            // 레이어를 지워도 블렌드 트리는 파일 안에 남으므로 직접 지운다.
            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int assetIndex = 0; assetIndex < subAssets.Length; assetIndex++)
            {
                if (subAssets[assetIndex] is BlendTree)
                {
                    AssetDatabase.RemoveObjectFromAsset(subAssets[assetIndex]);
                }
            }

            controller.AddLayer("Base Layer");
            return controller;
        }

        private static AnimationClip PlayerClip(string name) => LoadClip(PLAYER_CLIPS + name + ".fbx");

        private static AnimationClip ZombieClip(string name) => LoadClip(ZOMBIE_CLIPS + name + ".fbx");

        private static AnimationClip LoadClip(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int assetIndex = 0; assetIndex < assets.Length; assetIndex++)
            {
                if (assets[assetIndex] is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    return clip;
                }
            }

            Debug.LogWarning($"애니메이션 클립을 찾지 못했습니다: {path}");
            return null;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(ANIMATOR_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Art", "Animators");
            }
        }
    }
}
