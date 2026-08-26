using System;
using Game.GameEngine.Ecs;
using SampleProject;
using UnityEditor;
using UnityEngine;

namespace RolePlayingPrototype.Editor
{
    [CustomEditor(typeof(CommandController))]
    public sealed class CommandControllerEditor : UnityEditor.Editor
    {
        private Transform _point;
        private Entity _targetEntity;
        private Entity _resourceEntity;
        private Transform[] _patrolPoints = Array.Empty<Transform>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Commands", EditorStyles.boldLabel);
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Commands are available in Play Mode after Zenject initializes the scene.", MessageType.Info);
            }

            var controller = (CommandController)target;
            _point = (Transform)EditorGUILayout.ObjectField("Move Point", _point, typeof(Transform), true);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || _point == null))
            {
                if (GUILayout.Button("Move To Position")) controller.MoveToPosition(_point);
            }

            _targetEntity = (Entity)EditorGUILayout.ObjectField("Attack Target", _targetEntity, typeof(Entity), true);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || _targetEntity == null))
            {
                if (GUILayout.Button("Attack Target")) controller.AttackTarget(_targetEntity);
            }

            _resourceEntity = (Entity)EditorGUILayout.ObjectField("Resource", _resourceEntity, typeof(Entity), true);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || _resourceEntity == null))
            {
                if (GUILayout.Button("Gather Resource")) controller.GatherResource(_resourceEntity);
            }

            DrawPatrolPoints(ref _patrolPoints);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || _patrolPoints.Length == 0 || Array.Exists(_patrolPoints, item => item == null)))
            {
                if (GUILayout.Button("Patrol")) controller.Patrol(_patrolPoints);
            }

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Stop")) controller.Stop();
            }
        }

        internal static void DrawPatrolPoints(ref Transform[] points)
        {
            var size = Mathf.Max(0, EditorGUILayout.IntField("Patrol Point Count", points.Length));
            if (size != points.Length) Array.Resize(ref points, size);

            EditorGUI.indentLevel++;
            for (var i = 0; i < points.Length; i++)
            {
                points[i] = (Transform)EditorGUILayout.ObjectField("Point " + (i + 1), points[i], typeof(Transform), true);
            }
            EditorGUI.indentLevel--;
        }
    }

    [CustomEditor(typeof(AnimatorMachine))]
    public sealed class AnimatorMachineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var machine = (AnimatorMachine)target;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Toggle("Apply Root Motion", machine.IsRootMotion);
                EditorGUILayout.IntField("Current State", machine.CurrentState);
                EditorGUILayout.FloatField("Base Speed", machine.BaseSpeed);
            }
        }
    }
}
