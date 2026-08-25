using System.Linq;
using Game.GameEngine.Ecs;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zenject.Internal;
using Zenject;

namespace RolePlayingPrototype.Editor
{
    public static class RpgPrototypeAssetRepair
    {
        private const string Root = "Assets/RolePlayingPrototype/Game";
        private const string ControllerPath = Root + "/GameEngine/Animator/Animations/Character Runtime.controller";
        private const string CharacterPrefabPath = Root + "/Gameplay/Prefabs/Character.prefab";
        private const string ScenePath = Root + "/Scenes/Game.unity";

        public static void ValidateZenjectScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ZenUnityEditorUtil.ValidateCurrentSceneSetup();
            Debug.Log("RPG Zenject scene validation completed successfully.");
        }

        public static void SmokeTestZenjectScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ZenUnityEditorUtil.RunCurrentSceneSetup();

            var sceneContext = Object.FindObjectOfType<SceneContext>(true);
            var world = sceneContext.Container.Resolve<GameECS.EcsWorld>();
            var loop = sceneContext.Container.Resolve<EcsLoop>();
            var entities = Object.FindObjectsOfType<Entity>(true);

            if (entities.Length == 0 || entities.Any(entity => !entity.IsExists()))
            {
                throw new MissingReferenceException("One or more scene entities were not created in EcsWorld.");
            }

            loop.FixedTick();
            loop.Tick();
            loop.LateTick();

            if (world == null)
            {
                throw new MissingReferenceException("EcsWorld was not resolved from SceneContext.");
            }

            Debug.Log($"RPG ECS smoke test completed successfully. Entities: {entities.Length}.");
        }

        public static void RebuildCharacterController()
        {
            AssetDatabase.DeleteAsset(ControllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("State", AnimatorControllerParameterType.Int);

            var stateMachine = controller.layers[0].stateMachine;
            var idle = AddState(stateMachine, "Idle", "Character (Idle).anim", AnimatorStateId.IDLE);
            AddState(stateMachine, "Move", "Character (Run).anim", AnimatorStateId.MOVE);
            AddState(stateMachine, "Attack", "Character (Attack).anim", AnimatorStateId.ATTACK);
            AddState(stateMachine, "Gathering", "Character (Chop).anim", AnimatorStateId.GATHERING);
            stateMachine.defaultState = idle;

            var prefabRoot = PrefabUtility.LoadPrefabContents(CharacterPrefabPath);
            try
            {
                var animator = prefabRoot.GetComponentInChildren<Animator>(true);
                if (animator == null)
                {
                    throw new MissingComponentException("Character prefab does not contain an Animator component.");
                }

                animator.runtimeAnimatorController = controller;
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, CharacterPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("RPG prototype character controller rebuilt successfully.");
        }

        private static AnimatorState AddState(
            AnimatorStateMachine stateMachine,
            string stateName,
            string clipName,
            int stateId)
        {
            var clipPath = Root + "/GameEngine/Animator/Animations/" + clipName;
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                throw new MissingReferenceException("Animation clip was not found: " + clipPath);
            }

            var state = stateMachine.AddState(stateName);
            state.motion = clip;
            state.writeDefaultValues = true;

            var listener = state.AddStateMachineBehaviour<ScriptableAnimatorState>();
            var serializedListener = new SerializedObject(listener);
            serializedListener.FindProperty("stateId").intValue = stateId;
            serializedListener.ApplyModifiedPropertiesWithoutUndo();

            var transition = stateMachine.AddAnyStateTransition(state);
            transition.hasExitTime = false;
            transition.duration = 0.1f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.Equals, stateId, "State");
            return state;
        }
    }
}
