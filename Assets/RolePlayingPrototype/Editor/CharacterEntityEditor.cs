using Entities;
using SampleProject;
using UnityEditor;
using UnityEngine;

namespace RolePlayingPrototype.Editor
{
    [CustomEditor(typeof(CharacterEntity))]
    [CanEditMultipleObjects]
    public sealed class CharacterEntityEditor : UnityEditor.Editor
    {
        private const float DefaultPointSpacing = 1.5f;
        private static readonly Color RouteColor = new(0.15f, 0.8f, 1f, 0.9f);
        private SerializedProperty _initialPatrolPoints;
        private SerializedProperty _initialPatrolGroup;

        private void OnEnable()
        {
            _initialPatrolPoints = serializedObject.FindProperty("_initialPatrolPoints");
            _initialPatrolGroup = serializedObject.FindProperty("_initialPatrolGroup");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "_initialPatrolPoints");
            EditorGUILayout.Space();
            if (targets.Length > 1)
            {
                DrawSharedPatrolGroupControls();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            if (_initialPatrolGroup.objectReferenceValue != null)
            {
                EditorGUILayout.HelpBox("This unit uses the shared patrol route. Individual patrol points are kept as a fallback and are currently inactive.", MessageType.Info);
                if (GUILayout.Button("Select Shared Patrol Group"))
                {
                    Selection.activeObject = _initialPatrolGroup.objectReferenceValue;
                }

                serializedObject.ApplyModifiedProperties();
                return;
            }

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
            if (Application.isPlaying || serializedObject.isEditingMultipleObjects || _initialPatrolPoints == null)
            {
                return;
            }

            serializedObject.Update();
            if (_initialPatrolGroup.objectReferenceValue != null)
            {
                return;
            }

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

        private void DrawSharedPatrolGroupControls()
        {
            EditorGUILayout.LabelField("Shared Initial Patrol", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Create one shared route for all selected CharacterEntity objects. The units will patrol and wait at route points as one group.", MessageType.Info);
            if (GUILayout.Button("Create Shared Patrol Group"))
            {
                serializedObject.ApplyModifiedProperties();
                CreateSharedPatrolGroup();
                serializedObject.Update();
            }
        }

        private void CreateSharedPatrolGroup()
        {
            var members = new CharacterEntity[targets.Length];
            var center = Vector3.zero;
            for (var i = 0; i < targets.Length; i++)
            {
                members[i] = (CharacterEntity) targets[i];
                center += members[i].transform.position;
            }

            center /= members.Length;
            var points = GetInitialPoints(members[0], center);
            var groupObject = new GameObject("Patrol Group Start");
            Undo.RegisterCreatedObjectUndo(groupObject, "Create Shared Patrol Group");
            var entitiesParent = FindEntitiesParent(members[0].transform);
            if (entitiesParent != null)
            {
                Undo.SetTransformParent(groupObject.transform, entitiesParent, "Parent Shared Patrol Group");
                GameObjectUtility.EnsureUniqueNameForSibling(groupObject);
            }

            groupObject.transform.position = center;
            var group = Undo.AddComponent<PatrolGroupAuthoring>(groupObject);
            PatrolGroupAuthoringEditor.AssignMembers(group, members);

            var groupSerializedObject = new SerializedObject(group);
            var pointsProperty = groupSerializedObject.FindProperty("_points");
            pointsProperty.arraySize = points.Length;
            for (var i = 0; i < points.Length; i++)
            {
                pointsProperty.GetArrayElementAtIndex(i).vector3Value = points[i];
            }

            groupSerializedObject.ApplyModifiedProperties();
            Selection.activeGameObject = groupObject;
            SceneView.RepaintAll();
        }

        private Transform FindEntitiesParent(Transform character)
        {
            var current = character.parent;
            while (current != null)
            {
                if (current.name == "Entities")
                {
                    return current;
                }

                current = current.parent;
            }

            var roots = character.gameObject.scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var entities = FindChild(roots[i].transform, "Entities");
                if (entities != null)
                {
                    return entities;
                }
            }

            return null;
        }

        private Transform FindChild(Transform parent, string objectName)
        {
            if (parent.name == objectName)
            {
                return parent;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var result = FindChild(parent.GetChild(i), objectName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private Vector3[] GetInitialPoints(CharacterEntity character, Vector3 center)
        {
            var characterSerializedObject = new SerializedObject(character);
            var pointsProperty = characterSerializedObject.FindProperty("_initialPatrolPoints");
            if (pointsProperty.arraySize > 0)
            {
                var points = new Vector3[pointsProperty.arraySize];
                for (var i = 0; i < points.Length; i++)
                {
                    points[i] = pointsProperty.GetArrayElementAtIndex(i).vector3Value;
                }

                return points;
            }

            return new[]
            {
                center + Vector3.right * DefaultPointSpacing,
                center - Vector3.right * DefaultPointSpacing
            };
        }
    }
}
