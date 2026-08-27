using Entities;
using SampleProject;
using UnityEditor;
using UnityEngine;

namespace RolePlayingPrototype.Editor
{
    [CustomEditor(typeof(CharacterEntity))]
    public sealed class CharacterEntityEditor : UnityEditor.Editor
    {
        private const float DefaultPointSpacing = 1.5f;
        private static readonly Color RouteColor = new(0.15f, 0.8f, 1f, 0.9f);
        private SerializedProperty _initialPatrolPoints;

        private void OnEnable()
        {
            _initialPatrolPoints = serializedObject.FindProperty("_initialPatrolPoints");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "_initialPatrolPoints");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Initial Patrol Route", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Points are stored in world space. Select the unit and drag numbered handles in the Scene view.", MessageType.Info);

            var pointCount = EditorGUILayout.IntSlider("Point Count", _initialPatrolPoints.arraySize, 0, PatrolRouteEditor.MaximumPointCount);
            if (pointCount != _initialPatrolPoints.arraySize)
            {
                ResizeRoute(pointCount);
            }

            EditorGUI.indentLevel++;
            for (var i = 0; i < _initialPatrolPoints.arraySize; i++)
            {
                EditorGUILayout.PropertyField(_initialPatrolPoints.GetArrayElementAtIndex(i), new GUIContent("Point " + (i + 1)));
            }
            EditorGUI.indentLevel--;

            if (serializedObject.ApplyModifiedProperties())
            {
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            if (Application.isPlaying || _initialPatrolPoints == null)
            {
                return;
            }

            serializedObject.Update();
            Handles.color = RouteColor;
            for (var i = 0; i < _initialPatrolPoints.arraySize; i++)
            {
                var point = _initialPatrolPoints.GetArrayElementAtIndex(i);
                var position = point.vector3Value;
                var handleSize = HandleUtility.GetHandleSize(position) * 0.12f;
                Handles.SphereHandleCap(0, position, Quaternion.identity, handleSize, EventType.Repaint);
                Handles.Label(position + Vector3.up * handleSize, (i + 1).ToString());

                EditorGUI.BeginChangeCheck();
                var newPosition = Handles.PositionHandle(position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Move Patrol Point");
                    point.vector3Value = newPosition;
                }

                if (i > 0)
                {
                    Handles.DrawLine(_initialPatrolPoints.GetArrayElementAtIndex(i - 1).vector3Value, position, 3f);
                }
            }

            if (_initialPatrolPoints.arraySize > 1)
            {
                Handles.DrawLine(_initialPatrolPoints.GetArrayElementAtIndex(_initialPatrolPoints.arraySize - 1).vector3Value, _initialPatrolPoints.GetArrayElementAtIndex(0).vector3Value, 3f);
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void ResizeRoute(int pointCount)
        {
            var previousCount = _initialPatrolPoints.arraySize;
            _initialPatrolPoints.arraySize = pointCount;
            if (pointCount <= previousCount)
            {
                return;
            }

            var character = (CharacterEntity)target;
            for (var i = previousCount; i < pointCount; i++)
            {
                _initialPatrolPoints.GetArrayElementAtIndex(i).vector3Value = character.transform.position + character.transform.right * DefaultPointSpacing * (i + 1);
            }
        }
    }
}
