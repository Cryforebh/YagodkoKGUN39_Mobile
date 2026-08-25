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
        private Transform point;
        private Entity targetEntity;
        private Entity resourceEntity;
        private Transform[] patrolPoints = Array.Empty<Transform>();

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
            point = (Transform)EditorGUILayout.ObjectField("Move Point", point, typeof(Transform), true);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || point == null))
            {
                if (GUILayout.Button("Move To Position")) controller.MoveToPosition(point);
            }

            targetEntity = (Entity)EditorGUILayout.ObjectField("Attack Target", targetEntity, typeof(Entity), true);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || targetEntity == null))
            {
                if (GUILayout.Button("Attack Target")) controller.AttackTarget(targetEntity);
            }

            resourceEntity = (Entity)EditorGUILayout.ObjectField("Resource", resourceEntity, typeof(Entity), true);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || resourceEntity == null))
            {
                if (GUILayout.Button("Gather Resource")) controller.GatherResource(resourceEntity);
            }

            DrawPatrolPoints(ref patrolPoints);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || patrolPoints.Length == 0 || Array.Exists(patrolPoints, item => item == null)))
            {
                if (GUILayout.Button("Patrol")) controller.Patrol(patrolPoints);
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

    [CustomEditor(typeof(DataController))]
    public sealed class DataControllerEditor : UnityEditor.Editor
    {
        private Transform point;
        private Entity targetEntity;
        private Entity resourceEntity;
        private Transform[] patrolPoints = Array.Empty<Transform>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Commands", EditorStyles.boldLabel);
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Commands are available in Play Mode after Zenject initializes the scene.", MessageType.Info);
            }

            var controller = (DataController)target;
            point = (Transform)EditorGUILayout.ObjectField("Move Point", point, typeof(Transform), true);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || point == null))
            {
                if (GUILayout.Button("Move To Position")) controller.MoveToPosition(point);
            }

            targetEntity = (Entity)EditorGUILayout.ObjectField("Attack Target", targetEntity, typeof(Entity), true);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || targetEntity == null))
            {
                if (GUILayout.Button("Attack Target")) controller.AttackTarget(targetEntity);
            }

            resourceEntity = (Entity)EditorGUILayout.ObjectField("Resource", resourceEntity, typeof(Entity), true);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || resourceEntity == null))
            {
                if (GUILayout.Button("Gather Resource")) controller.GatherResource(resourceEntity);
            }

            CommandControllerEditor.DrawPatrolPoints(ref patrolPoints);
            using (new EditorGUI.DisabledScope(!Application.isPlaying || patrolPoints.Length == 0 || Array.Exists(patrolPoints, item => item == null)))
            {
                if (GUILayout.Button("Patrol")) controller.Patrol(patrolPoints);
            }
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
